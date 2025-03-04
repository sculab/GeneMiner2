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
    static GzipReader* open(std::string filename) {
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
    explicit NativeFileOutputBuffer(int size) : records(size), fileBufferSize(size, 0) {
        totalBufferSize = 0;
    }

    virtual ~NativeFileOutputBuffer() {}

    int addRecord(int key, const std::string& record) {
        std::list<std::string>& fileBuffer = records.at(key);

        bool filled = fileBuffer.empty() || fileBuffer.back().size() + record.size() + 1 > fileBuffer.back().capacity();
        bool newBlockNeeded = fileBuffer.empty() || (filled && fileBuffer.back().capacity() >= BUFFER_MAX_SIZE);

        if (newBlockNeeded) {
            fileBuffer.emplace_back();

            std::string& block = fileBuffer.back();

            block.reserve(std::max(record.size() + 1, static_cast<decltype(record.size())>(BUFFER_MIN_SIZE)));
            block.append(record);
            block.append("\n", 1);

            fileBufferSize[key] += block.capacity();
            totalBufferSize += block.capacity();
        }
        else {
            std::string& block = fileBuffer.back();

            fileBufferSize[key] -= block.capacity();
            totalBufferSize -= block.capacity();

            if (filled) {
                block.reserve(std::max(4 * block.capacity(), block.size() + record.size() + 1));
            }

            block.append(record);
            block.append("\n", 1);

            fileBufferSize[key] += block.capacity();
            totalBufferSize += block.capacity();
        }

        return fileBufferSize[key];
    }

    void destroy() {
        delete this;
    }

    void flushBuffer(int key, const std::string& filename) {
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

        for (std::string& block : fileBuffer) {
            std::fputs(block.c_str(), file);

            fileBufferSize[key] -= block.capacity();
            totalBufferSize -= block.capacity();
        }

        fileBuffer.clear();

        std::fclose(file);
        hx::ExitGCFreeZone();
    }

    int getTotalSize() {
        return totalBufferSize;
    }

private:
    std::vector<std::list<std::string>> records;
    std::vector<int> fileBufferSize;
    int totalBufferSize;
};

template <typename T>
class OwningReference final : public ::cpp::Reference<T> {
public:
    OwningReference(const null& value) : ::cpp::Reference<T>(value) {}
    explicit OwningReference(bool create = false) : ::cpp::Reference<T>(create ? new T() : nullptr) {}
};

template <typename Derived, typename K, typename V>
class UnorderedMapBase {
public:
    UnorderedMapBase() {}

    void destroy() {
        delete this;
    }

    static inline K cast_key(const K& key) {
        return key;
    }

    static inline V cast_value(const V& value) {
        return value;
    }

    static inline K dynamic_key(const K& key) {
        return key;
    }

    static inline V dynamic_value(const V& value) {
        return value;
    }

    template <typename... Ts>
    void foreach(::Dynamic fn, Ts... extras) const {
        for (auto it = map.cbegin(); it != map.cend(); it++) {
            fn(static_cast<const Derived*>(this)->dynamic_key(it->first), static_cast<const Derived*>(this)->dynamic_value(it->second), extras...);
        }
    }

    template <typename Kd, typename Vd>
    Vd get(Kd key, Vd def) const {
        const V* ptr = map.try_get(static_cast<const Derived*>(this)->cast_key(key));
        return ptr ? static_cast<const Derived*>(this)->dynamic_value(*ptr) : def;
    }

    void reserve(int count) {
        map.rehash(count);
    }

    template <typename Kd, typename Vd>
    void set(Kd key, Vd value) {
        map[static_cast<const Derived*>(this)->cast_key(key)] = static_cast<const Derived*>(this)->cast_value(value);
    }

protected:
    virtual ~UnorderedMapBase() {}

private:
    emhash7::HashMap<K, V> map;
};

template <typename K, typename V>
class UnorderedMapType final : public UnorderedMapBase<UnorderedMapType<K, V>, K, V> {
    using UnorderedMapBase<UnorderedMapType<K, V>, K, V>::UnorderedMapBase;
};

template <typename V>
class UnorderedMapType<::String, V> final : public UnorderedMapBase<UnorderedMapType<::String, V>, std::string, V> {
    using UnorderedMapBase<UnorderedMapType<::String, V>, std::string, V>::UnorderedMapBase;

public:
    static inline std::string cast_key(::String key) {
        return ::hx::StdString(key);
    }

    static inline ::String dynamic_key(std::string key) {
        char* s = hx::NewString(key.size());
        ::memcpy(s, key.c_str(), key.size() * sizeof(char));
        return ::String(s, key.size());
    }
};

template <typename K, typename V>
using UnorderedMapPointer = UnorderedMapType<K, V>*;

template <typename K, typename V>
using UnorderedMapReference = OwningReference<UnorderedMapType<K, V>>;

template <typename Derived, typename T>
class UnorderedSetBase {
public:
    UnorderedSetBase() {}

    void destroy() {
        delete this;
    }

    static inline T cast_value(const T& value) {
        return value;
    }

    static inline T dynamic_value(const T& value) {
        return value;
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
            fn(static_cast<const Derived*>(this)->dynamic_value(*it), extras...);
        }
    }

    void insert(T value) {
        set.insert(static_cast<const Derived*>(this)->cast_value(value));
    }

protected:
    virtual ~UnorderedSetBase() {}

private:
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
