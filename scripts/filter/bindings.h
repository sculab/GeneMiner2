#ifndef INCLUDED_MainFilterNew_Bindings
#define INCLUDED_MainFilterNew_Bindings

#include <list>

#define BUFFER_MIN_SIZE 4096
#define BUFFER_MAX_SIZE 65536
#define CHUNK_SIZE 65536
#define EMH_EXT 1
#define EMH_INT_HASH 1

#include "haxe/io/Eof.h"
#include "haxe/io/Bytes.h"
#include "hx/StdString.h"
#include "hash_table7.hpp"
#include "hash_set2.hpp"
#include "zlib.h"

#pragma comment(lib, "zlib")

class GzipReader final {
public:
    static GzipReader* open(::String filename) {
        gzFile file = gzopen(filename.c_str(), "rb");

        if (file == NULL) {
            return NULL;
        }

        if (gzbuffer(file, CHUNK_SIZE) != 0) {
            gzclose(file);
            return NULL;
        }

        return new GzipReader(file);
    }

    void close() {
        if (!closed) {
            closed = true;
            gzclose(input);
        }
    }

    void destroy() {
        delete this;
    }

    int readBytes(haxe::io::Bytes buf, int pos, int len) {
        if (closed || pos < 0 || len <= 0 || pos > buf->length || pos + len > buf->length) {
            return 0;
        }

        hx::EnterGCFreeZone();

        int bytesRead = gzread(input, &buf->b[pos], len);

        if (bytesRead <= 0) {
            int errnum;
            const char* errstr = gzerror(input, &errnum);

            hx::ExitGCFreeZone();

            if (errnum == Z_OK) {
                hx::Throw(haxe::io::Eof_obj::__new());
            }
            else {
                hx::Throw(::String::create(errstr, strlen(errstr)));
            }

            return 0;
        }

        hx::ExitGCFreeZone();
        return bytesRead;
    }

protected:
    GzipReader(gzFile file) : input(file) {}

    virtual ~GzipReader() {
        close();
    }

private:
    bool closed = false;
    gzFile input;
};

class NativeFileOutputBuffer final {
public:
    explicit NativeFileOutputBuffer(int fileCount, bool compressed = false) :
    records(fileCount), fileBufferSize(fileCount, 0) {
        binaryFormat = compressed;
        totalBufferSize = 0;
    }

    virtual ~NativeFileOutputBuffer() {}

    void addEntry(int key, const char* pointer, size_t size) {
        size_t entrySize = size + !binaryFormat;
        std::list<std::string>& fileBuffer = records.at(key);

        bool filled = fileBuffer.empty() || fileBuffer.back().size() + entrySize > fileBuffer.back().capacity();
        bool newBlockNeeded = fileBuffer.empty() || (filled && fileBuffer.back().capacity() >= BUFFER_MAX_SIZE);

        if (newBlockNeeded) {
            fileBuffer.emplace_back();

            std::string& block = fileBuffer.back();

            block.reserve(std::max(entrySize, static_cast<size_t>(BUFFER_MIN_SIZE)));
            block.append(pointer, size);

            if (!binaryFormat) {
                block.append("\n", 1);
            }

            fileBufferSize[key] += block.capacity();
            totalBufferSize += block.capacity();
        }
        else {
            std::string& block = fileBuffer.back();

            fileBufferSize[key] -= block.capacity();
            totalBufferSize -= block.capacity();

            if (filled) {
                block.reserve(std::max(4 * block.capacity(), block.size() + entrySize));
            }

            block.append(pointer, size);

            if (!binaryFormat) {
                block.append("\n", 1);
            }

            fileBufferSize[key] += block.capacity();
            totalBufferSize += block.capacity();
        }
    }

    int addRecord(int key, ::Array<::String> record) {
        if (binaryFormat) {
            size_t compressedSize = compressRecord(record);

            if (compressedSize > 0) {
                addEntry(key, reinterpret_cast<char*>(binaryEntryBuffer.data()), compressedSize);
            }
        }
        else {
            for (int i = 0; i < record->length; i++) {
                const char* pointer = ::String(record->__GetItem(i)).c_str();
                addEntry(key, pointer, std::strlen(pointer));
            }
        }

        return fileBufferSize[key];
    }

    size_t compressRecord(::Array<::String> record) {
        if (record->length < 2) {
            return 0;
        }

        const char* sp = ::String(record->__GetItem(1)).c_str();
        const char* qp = nullptr;
        size_t      sl = std::strlen(sp);

        if (sl == 0) {
            return 0;
        }

        if (record->length >= 4) {
            qp = ::String(record->__GetItem(3)).c_str();

            if (std::strlen(qp) < sl) {
                qp = nullptr;
            }
        }

        if (sl > 0x7fffff) {
            sl = 0x7fffff;
        }

        if (binaryEntryBuffer.size() < 2 * sl + 6) {
            binaryEntryBuffer.resize(2 * sl + 6);
        }

        size_t endIndex = 6;
        unsigned char lastChunk = 0;
        unsigned char lastCount = 0;
        unsigned char lastValue = sp[0] - 64;

        for (size_t i = 1; i < sl; i++) {
            unsigned char value = sp[i] - 64;

            if (value < 1 || value > 26) {
                value = 'N' - 64;
            }

            if (value != lastValue || lastCount == 3) {
                unsigned char chunk = (lastCount << 5) + lastValue;
                binaryEntryBuffer[endIndex] = chunk ^ lastChunk;

                if (endIndex > 6 && binaryEntryBuffer[endIndex] == binaryEntryBuffer[endIndex - 1]) {
                    binaryEntryBuffer[endIndex - 1] |= 0x80;
                }
                else {
                    endIndex++;
                }

                lastChunk = chunk;
                lastCount = 0;
            }
            else {
                lastCount++;
            }

            lastValue = value;
        }

        unsigned char chunk = (lastCount << 5) + lastValue;
        binaryEntryBuffer[endIndex] = chunk ^ lastChunk;

        if (endIndex > 6 && binaryEntryBuffer[endIndex] == binaryEntryBuffer[endIndex - 1]) {
            binaryEntryBuffer[endIndex - 1] |= 0x80;
        }
        else {
            endIndex++;
        }

        if (qp) {
            lastCount = 0;
            lastValue = qp[0] & 0x7f;

            for (size_t i = 1; i < sl; i++) {
                unsigned char value = qp[i] & 0x7f;

                if (value != lastValue || lastCount == 255) {
                    if (lastCount == 0) {
                        binaryEntryBuffer[endIndex] = lastValue;
                        endIndex++;
                    }
                    else {
                        binaryEntryBuffer[endIndex] = lastCount | 0x80;
                        endIndex++;
                        binaryEntryBuffer[endIndex] = lastValue | (lastCount & 0x80);
                        endIndex++;
                        lastCount = 0;
                    }
                }
                else {
                    lastCount++;
                }

                lastValue = value;
            }

            if (lastCount == 0) {
                binaryEntryBuffer[endIndex] = lastValue;
                endIndex++;
            }
            else {
                binaryEntryBuffer[endIndex] = lastCount | 0x80;
                endIndex++;
                binaryEntryBuffer[endIndex] = lastValue | (lastCount & 0x80);
                endIndex++;
                lastCount = 0;
            }
        }

        binaryEntryBuffer[0] = ((endIndex - 6) >> 16) & 0xff;
        binaryEntryBuffer[1] = ((endIndex - 6) >>  8) & 0xff;
        binaryEntryBuffer[2] =  (endIndex - 6)        & 0xff;
        binaryEntryBuffer[3] =  (sl            >> 16) & 0xff;
        binaryEntryBuffer[4] =  (sl            >>  8) & 0xff;
        binaryEntryBuffer[5] =   sl                   & 0xff;

        if (qp) {
            binaryEntryBuffer[3] |= 0x80;
        }

        return endIndex;
    }

    void destroy() {
        delete this;
    }

    void flushBuffer(int key, ::String filename) {
        std::list<std::string>& fileBuffer = records.at(key);

        if (fileBuffer.empty()) {
            return;
        }

        hx::EnterGCFreeZone();

        FILE* file = std::fopen(filename.c_str(), "a+b");

        if (!file) {
            hx::ExitGCFreeZone();
            hx::Throw(haxe::io::Eof_obj::__new());
        }

        std::setbuf(file, nullptr);

        bool failure = false;

        for (std::string& block : fileBuffer) {
            if (binaryFormat) {
                failure |= std::fwrite(block.c_str(), sizeof(char), block.size(), file) < block.size();
            }
            else {
                failure |= std::fputs(block.c_str(), file) < 0;
            }

            fileBufferSize[key] -= block.capacity();
            totalBufferSize -= block.capacity();
        }

        failure |= std::fclose(file) < 0;

        fileBuffer.clear();
        hx::ExitGCFreeZone();

        if (failure) {
            hx::Throw(haxe::io::Eof_obj::__new());
        }
    }

    int getTotalSize() {
        return totalBufferSize;
    }

private:
    std::vector<std::list<std::string>> records;
    std::vector<int> fileBufferSize;
    int totalBufferSize;

    std::vector<unsigned char> binaryEntryBuffer;
    bool binaryFormat;
};

template <typename T>
class OwningReference final : public ::cpp::Reference<T> {
public:
    OwningReference(const null& value) : ::cpp::Reference<T>(value) {}
    explicit OwningReference(bool create = false) : ::cpp::Reference<T>(create ? new T() : nullptr) {}
};

template <typename Derived>
class UnorderedMapBase {
public:
    UnorderedMapBase() {}
    virtual ~UnorderedMapBase() {}

    void destroy() {
        delete this;
    }

    void reserve(int count) {
        static_cast<Derived*>(this)->map.rehash(count);
    }
};

template <typename K, typename V>
class UnorderedMapType final : public UnorderedMapBase<UnorderedMapType<K, V>> {
public:
    using UnorderedMapBase<UnorderedMapType<K, V>>::UnorderedMapBase;

    template <typename... Ts>
    void foreach(::Dynamic fn, Ts... extras) const {
         for (auto it = map.cbegin(); it != map.cend(); it++) {
            fn(it->first, it->second, extras...);
         }
     }

    V get(K key, V def) const {
        const V* ptr = map.try_get(key);
        return ptr ? *ptr : def;
    }

    void set(K key, V value) {
        map[key] = value;
    }

    emhash7::HashMap<K, V> map;
};

template <typename V>
class UnorderedMapType<::String, V> final : public UnorderedMapBase<UnorderedMapType<::String, V>> {
public:
    using UnorderedMapBase<UnorderedMapType<::String, V>>::UnorderedMapBase;

    struct hx_equal_to final {
        bool operator()(const std::string& lhs, const std::string& rhs) const {
            return lhs == rhs;
        }

        bool operator()(const ::String& lhs, const std::string& rhs) const {
            return !lhs.isUTF16Encoded() && lhs.length == rhs.size() && memcmp(lhs.raw_ptr(), rhs.data(), rhs.size()) == 0;
        }
    };

    struct hx_hash final {
        uint64_t lu64(const char* data) const {
            uint64_t value;
            memcpy(&value, data, 8);
            return value;
        }

        size_t hash(const char* data, size_t size) const {
            /* Hyper-simplified komihash without 128-bit multiplication
             * Checkerboard patterns are not a concern here
             * https://github.com/avaneev/komihash */

            uint64_t seed1 = 0x243F6A8885A308D3ULL ^ 0x5555555555555555ULL;
            uint64_t seed2 = 0x13198A2E03707344ULL ^ 0x55556A8885A3AAAAULL;
            uint64_t seed5 = 0x452821E638D01377ULL ^ 0xAAAAAAAAAAAAAAAAULL;
            uint64_t seed6 = 0xBE5466CF34E90C6CULL ^ 0xAAAA21E638D05555ULL;

            size_t index = 0;

            while (size >= 32) {
                uint64_t word0   = lu64(data + index)      ^ seed1;
                uint64_t word8   = lu64(data + index + 8)  ^ seed2;
                uint64_t word16  = lu64(data + index + 16) ^ seed5;
                uint64_t word32  = lu64(data + index + 24) ^ seed6;
                uint64_t temp1   = seed1;
                seed1 = ((word0  * 0xcc9e2d51) << 15) ^ seed2;
                seed2 = ((word8  * 0x1b873593) >> 19) ^ seed5;
                seed5 = ((word16 * 0xcc9e2d51) << 15) ^ seed6;
                seed6 = ((word32 * 0x1b873593) >> 19) ^ temp1;
                index += 32;
                size  -= 32;
            }

            if (size >= 16) {
                uint64_t word0   = lu64(data + index)      ^ seed1;
                uint64_t word8   = lu64(data + index + 8)  ^ seed5;
                uint64_t temp1   = seed1;
                seed1 = ((word0  * 0xcc9e2d51) << 15) ^ seed5;
                seed5 = ((word8  * 0x1b873593) >> 19) ^ temp1;
                index += 16;
                size  -= 16;
            }

            if (size > 8) {
                uint64_t word0   = lu64(data + index) ^ seed1;
                uint64_t temp1   = seed2;
                seed2 = ((word0  * 0xcc9e2d51) << 15) ^ seed6;
                seed6 = ((seed2  * 0x1b873593) >> 19) ^ temp1;
                index += 8;
                size  -= 8;
            }

            seed1 ^= (seed2 * 0x1b873593) ^ seed5 ^ (seed6 * 0xcc9e2d51);

            // Use FNA-1a for the remaining bytes
            while (size > 0) {
                seed1 ^= (unsigned)data[index];
                seed1 *= 0x100000001b3ULL;
                index += 1;
                size  -= 1;
            }

            return seed1;
        }

        size_t operator()(const std::string& value) const {
            return hash(value.data(), value.size());
        }

        size_t operator()(const ::String value) const {
            return hash(value.raw_ptr(), value.length);
        }
    };

    static inline ::String hx_new_string(std::string key) {
        char* s = hx::NewString(key.size());
        ::memcpy(s, key.c_str(), key.size() * sizeof(char));
        return ::String(s, key.size());
    }

    template <typename... Ts>
    void foreach(::Dynamic fn, Ts... extras) const {
         for (auto it = map.cbegin(); it != map.cend(); it++) {
            fn(hx_new_string(it->first), it->second, extras...);
         }
     }

    V get(::String key, V def) const {
        const auto it = map.find(key);
        return it == map.cend() ? def : it->second;
    }

    void set(::String key, V value) {
        map[::hx::StdString(key)] = value;
    }

    emhash7::HashMap<std::string, V, hx_hash, hx_equal_to> map;
};

template <typename K, typename V>
using UnorderedMapPointer = UnorderedMapType<K, V>*;

template <typename K, typename V>
using UnorderedMapReference = OwningReference<UnorderedMapType<K, V>>;

template <typename Derived, typename T>
class UnorderedSetBase {
public:
    UnorderedSetBase() {}
    virtual ~UnorderedSetBase() {}

    void destroy() {
        delete this;
    }

    void clear() {
        set.clear();
    }

    bool empty() const {
        return set.empty();
    }

    template <typename... Ts>
    void foreach(::Dynamic fn, Ts... extras) const {
        for (auto it = set.cbegin(); it != set.cend(); it++) {
            fn(*it, extras...);
        }
    }

    void insert(T value) {
        set.insert(value);
    }

    emhash2::HashSet<T> set;
};

template <typename T>
class UnorderedSetType final : public UnorderedSetBase<UnorderedSetType<T>, T> {
    using UnorderedSetBase<UnorderedSetType<T>, T>::UnorderedSetBase;
};

template <typename T>
using UnorderedSetPointer = UnorderedSetType<T>*;

template <typename T>
using UnorderedSetReference = OwningReference<UnorderedSetType<T>>;

#endif /* INCLUDED_MainFilterNew_Bindings */
