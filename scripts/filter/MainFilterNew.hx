import haxe.Exception;
import haxe.Int64;
import haxe.Rest;
import haxe.ds.Vector;
import haxe.io.Bytes;
import haxe.io.Eof;
import haxe.io.Input;
import haxe.io.Path;
import haxe.io.UInt16Array;
import haxe.macro.Expr;
import sys.FileSystem;
import sys.io.File;
import sys.io.FileOutput;

using StringTools;

#if cpp
import cpp.UInt32;
import cpp.StdString;
import cpp.VarArg;
#else
typedef UInt32 = Int;
#end

typedef Arguments = {
    reference: String,
    fastq1: Array<String>,
    fastq2: Array<String>,
    kmer: Int,
    step: Int,
    maxReads: Int,
    output: String,
    outSubdir: String,
    kmerDict: String,
    getReverse: Bool,
    useCompositionPattern: Bool,
    mode: Int
}

typedef OptionInfo = {
    arity: Int,
    ?convert: Dynamic,
    ?target: String
}

enum abstract FileType(Int) to Int {
    var Gzip;
    var Fastq;
    var Fasta;
}

class ArgumentError extends Exception {}
class EncodingError extends Exception {}

#if cpp

@:buildXml('<target id="haxe" unless="windows"><lib name="-lz"/></target>')
@:include('../../bindings.h')
@:native('::cpp::Reference<::GzipReader>')
extern class GzipReader {
    @:native('::GzipReader::open')
    public static function open(filename: StdString): GzipReader;
    public function close(): Void;
    public function destroy(): Void;
    public function readBytes(buf: Bytes, pos: Int, len: Int): Int;
}

@:include('../../bindings.h')
@:native('::cpp::Reference<::NativeFileOutputBuffer>')
extern class NativeFileOutputBuffer {
    @:native('new ::NativeFileOutputBuffer')
    public static function create(size: Int): NativeFileOutputBuffer;
    public function addRecord(key: Int, record: StdString): Int;
    public function destroy(): Void;
    public function flushBuffer(key: Int, filename: StdString): Void;
    public function getTotalSize(): Int;
}

@:include('../../bindings.h')
@:native('::UnorderedMapPointer')
extern class UnorderedMapPointer<K, V> {
    public function destroy(): Void;
    public function foreach(fn: Dynamic, extras: cpp.Rest<VarArg>): Void;
    public function get(key: K, def: V): V;
    public function reserve(count: Int): Void;
    public function set(key: K, value: V): Void;
}

@:include('../../bindings.h')
@:native('::UnorderedMapReference')
extern class UnorderedMapReference<K, V> extends UnorderedMapPointer<K, V> {
    public function new(create: Bool);
}

@:include('../../bindings.h')
@:native('::UnorderedSetPointer')
extern class UnorderedSetPointer<T> {
    public function destroy(): Void;
    public function clear(): Void;
    public function empty(): Bool;
    public function foreach(fn: Dynamic, extras: cpp.Rest<VarArg>): Void;
    public function insert(value: T): Void;
}

@:include('../../bindings.h')
@:native('::UnorderedSetReference')
extern class UnorderedSetReference<T> extends UnorderedSetPointer<T> {
    public function new(create: Bool);
}

class GzipFileInput extends Input {
    var pointer: GzipReader;

    public function new(filename: String) {
        pointer = GzipReader.open(StdString.ofString(filename));

        if (pointer == null)
        {
            throw new ArgumentError('Cannot decompress gzipped file $filename');
        }

        cpp.NativeGc.addFinalizable(this, false);
    }

    public function finalize() pointer.destroy();
    public override function close() pointer.close();
    public override function readBytes(buf: Bytes, pos: Int, len: Int) return pointer.readBytes(buf, pos, len);
}

class FileOutputBuffer {
    var pointer: NativeFileOutputBuffer;

    public function new(size: Int) {
        pointer = NativeFileOutputBuffer.create(size);
        cpp.NativeGc.addFinalizable(this, false);
    }

    public function finalize() pointer.destroy();
    public inline function addRecord(key: Int, record: String) return pointer.addRecord(key, StdString.ofString(record));
    public inline function flushBuffer(key: Int, filename: String) pointer.flushBuffer(key, StdString.ofString(filename));
    public inline function getTotalSize() return pointer.getTotalSize();
}

@:generic
@:remove
class UnorderedMap<K, V> {
    var pointer: UnorderedMapReference<K, V>;

    public function new() {
       pointer = new UnorderedMapReference<K, V>(true);
       cpp.NativeGc.addFinalizable(this, false);
    }

    public function finalize() pointer.destroy();
    public inline function foreach(fn: K -> V -> Void) pointer.foreach(fn);
    public inline function get(key: K, def: V) return pointer.get(key, def);
    public inline function reference() return pointer;
    public inline function reserve(count: Int) pointer.reserve(count);
    public inline function set(key: K, value: V) return pointer.set(key, value);
}

@:generic
@:remove
class UnorderedSet<T> {
    var pointer: UnorderedSetReference<T>;

    public function new() {
       pointer = new UnorderedSetReference<T>(true);
       cpp.NativeGc.addFinalizable(this, false);
    }

    public function finalize() pointer.destroy();
    public inline function clear() pointer.clear();
    public inline function empty() return pointer.empty();
    public inline function foreach(fn: T -> Void) pointer.foreach(fn);
    public inline function insert(value: T) return pointer.insert(value);
    public inline function reference() return pointer;
}

#else

class GzipFileInput extends Input {
    public function new(filename: String) throw new EncodingError('Gzip decompression is not supported on this target');
}

class FileOutputBuffer {
    var bufferedRecords: Array<Array<String>>;
    var bufferedRecordSize: Array<Int>;
    var totalRecordSize: Int = 0;

    public function new(size: Int) {
        bufferedRecords = [for (_ in 0...size) []];
        bufferedRecordSize = [for (_ in 0...size) 0];
    }

    public function addRecord(key: Int, record: String) {
        bufferedRecords[key].push(record);
        bufferedRecordSize[key] += record.length;
        totalRecordSize += record.length;
        return bufferedRecordSize[key];
    }

    public function flushBuffer(key: Int, filename: String) {
        var output = File.append(filename);
        output.writeString(bufferedRecords[key].join("\n"));
        output.writeString("\n");
        output.close();

        totalRecordSize -= bufferedRecordSize[key];
        bufferedRecords[key] = [];
        bufferedRecordSize[key] = 0;
    }

    public function getTotalSize() return totalRecordSize;
}

@:forward.new
abstract UnorderedMap<K, V>(Map<K, V>) {
    public inline function foreach(fn: Dynamic, extras: Rest<Any>) {
        for (k => v in this.keyValueIterator()) {
            Reflect.callMethod(null, fn.bind(k, v), extras.toArray());
        }
    }

    public inline function get(key: K, def: V) return this.get(key) ?? def;
    public inline function reference() return abstract;
    public inline function reserve(count: Int) {}
    public inline function set(key: K, value: V) this.set(key, value);
}

@:forward.new
abstract UnorderedSet<T>(Map<T, Bool>) {
    public inline function foreach(fn: Dynamic, extras: Rest<Any>) {
        for (k in this.keys()) {
            Reflect.callMethod(null, fn.bind(k), extras.toArray());
        }
    }

    public inline function clear() this.clear();
    public inline function empty() return Lambda.count(cast this) == 0;
    public inline function insert(value: T) this.set(value, true);
    public inline function reference() return abstract;
}

typedef UnorderedMapPointer<K, V> = UnorderedMap<K, V>;
typedef UnorderedSetPointer<T> = UnorderedSet<T>;

#end

abstract CuckooSet(Vector<Int64>) {
#if cpp
    public static inline var ITEM_MASK: Int64 = 0xffff;
#else
    public static final ITEM_MASK: Int64 = 0xffff;
#end

    public inline function new(size = 1, ?source = null, ?offset = 0) {
        if (source != null) {
            if (offset + size > source.length || size % 8 != 0) {
                throw new ArgumentError('Invalid Cuckoo set data size $size');
            }

            size = Std.int(size / 8);

            if (size != 1 && ((size - 1) & (size - 2)) != 0) {
                throw new ArgumentError('Invalid Cuckoo set bucket count $size');
            }

            this = new Vector<Int64>(size);

            for (i in 0...size) {
                this[i] = source.getInt64(offset + 8 * i);
            }
        }
        else {
            if ((size & (size - 1)) != 0) {
                throw new ArgumentError('Invalid Cuckoo set bucket count $size');
            }

            size |= 1;

            this = new Vector<Int64>(size, 0);
        }
    }

    private inline function bucketCount() return this.length - (this.length == 1 ? 0 : 1);

    public static function fingerprint(x: UInt32): UInt32 {
        // 16-bit fingerprints
        x *= 108301;
        x &= 0xffff;
        x = (x >>> 9) ^ x;
        x *= 108301;
        x &= 0xffff;
        x = (x >>> 11) ^ x;
        return x;
    }

    public static function fingerprintInverse(x: UInt32): UInt32 {
        x = (x >>> 11) ^ x;
        x *= 32709;
        x &= 0xffff;
        x = (x >>> 9) ^ x;
        x *= 32709;
        x &= 0xffff;
        return x;
    }

    private function indexHash(a: UInt32): Int {
        a += ~(a << 15);
        a ^= (a >>> 10);
        a += (a << 3);
        a ^= (a >>> 6);
        a += ~(a << 11);
        a ^= (a >>> 16);
        return a;
    }

    public inline function set(item) {
#if debug
        if (item < 2 || item > 0x1ffff) throw new ArgumentError('Value $item is not representable');
#end

        var item = tryInsert(item);

        if (item == 0) {
            return 0;
        }

        item = tryInsertStash(item);

        if (item == 0) {
            return 0;
        }

        var failureCount = grow();

        item = tryInsert(item);

        if (item == 0) {
            return failureCount;
        }

        item = tryInsertStash(item);

        if (item != 0) {
            failureCount++;
        }

        return failureCount;
    }

    private inline function grow() {
        var count = bucketCount();

        var multiplier = switch (count) {
            case 1: 2;
            case 2: 4;
            case 8: 4;
            default: 2;
        };

        var failureCount = 0;
        var newSet = new CuckooSet(multiplier * count);

        CuckooSet.foreach(toVector(), function(item) {
            item = newSet.tryInsert(item);

            if (item == 0) {
                return;
            }

            if (newSet.tryInsertStash(item) != 0) {
                failureCount++;
            }
        });

        this = newSet.toVector();

        return failureCount;
    }

    private function tryInsert(item) {
        var fp: UInt32 = fingerprint(item >>> 1);
        var lb: Int    = item & 1;

        var bm = bucketCount() - 1;
        var i1 = indexHash(item) & bm;
        var i2 = (i1 ^ indexHash(fp)) & bm;

        if (tryInsertBucket(i1, fp, lb) || tryInsertBucket(i2, fp, lb)) {
            return 0;
        }

        var ki = Std.random(200) >= 100 ? i1 : i2;

        for (_ in 0...28) {
            fp = swapSlot(ki, Std.random(2), fp, lb);

            var nki = ki ^ (indexHash(fp) & bm);

            if (nki == ki) {
                break;
            }

            if (tryInsertBucket(nki, fp, lb)) {
                return 0;
            }

            ki = nki;
        }

        return (fingerprintInverse(fp) << 1) | lb;
    }

    private function tryInsertStash(item) {
        var fp: UInt32 = fingerprint(item >>> 1);
        var lb: Int    = item & 1;

        var bc = bucketCount();

        if (bc > 1 && tryInsertBucket(bc, fp, lb)) {
            return 0;
        }

        return item;
    }

    private function tryInsertBucket(idx: Int, fp: Int64, lb: Int) {
        var bucket: Int64 = this[idx];
        var slotOffset    = 32 * lb;
        var slot:   Int64 = (bucket >>> slotOffset) & ITEM_MASK;

        if (slot != 0) {
            if (slot == fp) {
                return true;
            }

            slotOffset += 16;
            slot = (bucket >>> slotOffset) & ITEM_MASK;

            if (slot != 0) {
                return slot == fp;
            }
        }

        bucket &= ~(ITEM_MASK << slotOffset);
        bucket |= fp << slotOffset;
        this[idx] = bucket;

        return true;
    }

    private inline function swapSlot(idx: Int, slot: Int, fp: Int64, lb: Int): UInt32 {
        var offset = 32 * lb + 16 * slot;
        var kicked = ((this[idx] >>> offset) & ITEM_MASK).low;
        this[idx] &= ~(ITEM_MASK << offset);
        this[idx] |= fp << offset;
        return kicked;
    }

    public macro static function foreach(vector: ExprOf<Vector<Int64>>, e: Expr) {
        return macro {
            for (i in 0...$vector.length) {
                var bucket: Int64 = $vector[i];
                var slot0:  Int64 = bucket & CuckooSet.ITEM_MASK;
                var slot1:  Int64 = (bucket >>> 32) & CuckooSet.ITEM_MASK;

                if (slot0 != 0) {
                    $e(inline CuckooSet.fingerprintInverse(slot0.low) << 1);

                    slot0 = (bucket >>> 16) & CuckooSet.ITEM_MASK;

                    if (slot0 != 0) {
                        $e(inline CuckooSet.fingerprintInverse(slot0.low) << 1);
                    }
                }

                if (slot1 != 0) {
                    $e((inline CuckooSet.fingerprintInverse(slot1.low) << 1) | 1);

                    slot1 = (bucket >>> 48) & CuckooSet.ITEM_MASK;

                    if (slot1 != 0) {
                        $e((inline CuckooSet.fingerprintInverse(slot1.low) << 1) | 1);
                    }
                }
            }
        };
    }

    public inline function getStorageSize() return 8 * this.length;

    public inline function writeToBytes(buffer, offset) {
        for (i in 0...this.length) {
            buffer.setInt64(offset + 8 * i, this[i]);
        }
    }

    public inline function toVector() return this;
}

@:unreflective
class FastaHelper {
    static final nuclChars = ["A".code, "C".code, "G".code, "T".code];

    public static inline function intToNucl(n) return nuclChars[n];

    public static inline function nuclToInt(charCode: Int) {
        var twoBits = (charCode >>> 1) & 3;
        return twoBits ^ (twoBits >>> 1);
    }

    public static inline function nuclToPattern(charCode: Int) return (charCode >>> 1) & 1;

    public static inline function convertToDna(buf: Bytes, seq: String) {
#if debug
        if (buf.length < seq.length) throw new ArgumentError('Buffer size ${buf.length} is too small for a string of length ${seq.length}');
#end

        var i = 0;
        var j = 0;

        while (i < seq.length) {
            var c = getChar(seq, i);
            i++;

            // Not an alphabetic letter
            if (c < 65) {
                continue;
            }

            // Convert to upper case first
            if (c >= 97) {
                c -= 32;
            }

            // Fold U..Z to T..Y, such that U is normalized as T
            if (c >= 85) {
                c -= 1;
            }

            switch (c) {
                case "A".code | "C".code | "G".code | "T".code: setChar(buf, j, c);
                default: continue;
            }

            j++;
        }

        return j;
    }

    public static inline function fromBytes(b: Bytes, pos: Int, len: Int): String {
#if cpp
        var src: cpp.RawConstPointer<cpp.Char> = cast cpp.NativeArray.address(b.getData(), pos).rawCast();
        var dst: cpp.Pointer<cpp.Char> = cast cpp.NativeGc.allocGcBytes(len + 1).reinterpret();
        cpp.Native.memcpy(dst.get_raw(), src, len);
        dst.setAt(len, 0);
        return cpp.NativeString.fromGcPointer(dst, len);
#else
        return b.getString(pos, len);
#end
    }

    public static inline function getChar(s: String, i: Int): Int {
#if cpp
        return cpp.ConstPointer.fromRaw(cpp.NativeString.raw(s)).at(i);
#else
        return s.unsafeCodeAt(i);
#end
    }

    public static inline function setChar(b: Bytes, i: Int, c: Int) {
#if cpp
        cpp.NativeArray.unsafeSet(b.getData(), i, c);
#else
        b.set(i, c);
#end
    }
}

abstract ShortKmer(Int64) {
    public inline function new(value: Int64) {
        this = value;
    }

    public static inline function fromString(kmer: String, complement = false) {
#if debug
        if (kmer.length < 0 || kmer.length > 32) throw new ArgumentError('Invalid short k-mer size ${kmer.length}');
#end

        var value: Int64 = 0;

        for (i in 0...kmer.length) {
            var shift: Int;
            var n: Int64;

            if (complement) {
                shift = 2 * (i + 32 - kmer.length);
                n = 3 - FastaHelper.nuclToInt(FastaHelper.getChar(kmer, i));
            }
            else {
                shift = 2 * (31 - i);
                n = FastaHelper.nuclToInt(FastaHelper.getChar(kmer, i));
            }

            value |= n << shift;
        }

        return new ShortKmer(value);
    }

    public inline function toInt64(): Int64 return this;

#if debug
    public inline function toString(size: Int) {
        if (size < 0 || size > 32) throw new ArgumentError('Invalid short k-mer size $size');

        var buf = new StringBuf();

        for (i in 0...size) {
            var shift = 2 * (31 - i);
            buf.addChar(FastaHelper.intToNucl(((this >>> shift) & 0x3).low));
        }

        return buf.toString();
    }
#end
}

abstract LongKmer(String) {
    public inline function new(kmer: String, complement = false) {
#if debug
        if (kmer.length <= 32) throw new ArgumentError('Invalid long k-mer size ${kmer.length}');
        if (kmer.toUpperCase() != kmer) throw new EncodingError('Non-normalized long k-mer string');
#end

        if (complement) {
            var buf = Bytes.alloc(kmer.length);

            for (i in 0...kmer.length) {
                FastaHelper.setChar(buf, i, FastaHelper.intToNucl(3 - FastaHelper.nuclToInt(FastaHelper.getChar(kmer, kmer.length - i - 1))));
            }

            this = FastaHelper.fromBytes(buf, 0, kmer.length);
        }
        else {
            this = kmer;
        }
    }

    public inline function toString() return this;
}

@:unreflective
class KmerMap {
    public static inline var minMaxPatternSize = 12;
    public static inline var minMaxMapSize = 1 << minMaxPatternSize;

    static inline var initialBufferSize = 64 * 1024;
    static inline var maxBufferSize = 16 * 1024 * 1024;

    public var kmerLength(default, null): Int; // actual length
    public var useLongKmer(default, null): Bool;

    var kmerStorageSize: Int; // in bytes
    var referenceCount: Int;

    var cuckooSetList: Array<CuckooSet>;
    var minMaxMap: UInt16Array;

    var shortKmerMap: UnorderedMap<Int64, Int>;
    var longKmerMap: UnorderedMap<String, Int>;

    var skipLength: Int;
    var expectedSkipLength: Float;

    private inline function new(kmer, refNum) {
        kmerLength = kmer;
        kmerStorageSize = kmer <= 32 ? 8 : kmer;
        useLongKmer = kmer > 32;
        referenceCount = refNum;

        cuckooSetList = [];

        if (useLongKmer) {
            longKmerMap = new UnorderedMap<String, Int>();
        }
        else {
            shortKmerMap = new UnorderedMap<Int64, Int>();
        }

        skipLength = kmer - minMaxPatternSize - 3;
        expectedSkipLength = 0.0;
    }

    private inline function addShortKmer(index, kmer) {
        if (cuckooSetList.length > 2147483645) {
            throw new ArgumentError('Overflow in k-mer list');
        }

        var failure = 0;
        var pos     = shortKmerMap.get(kmer, -1);

        if (pos == -1) {
            shortKmerMap.set(kmer, index);
        }
        else if (pos < referenceCount) {
            var cs = new CuckooSet();
            failure += cs.set(pos + 2);
            failure += cs.set(index + 2);
            shortKmerMap.set(kmer, cuckooSetList.length);
            cuckooSetList.push(cs);
        }
        else {
            var cs = cuckooSetList[pos];
            failure += cs.set(index + 2);
            cuckooSetList[pos] = cs;
        }

        return failure;
    }

    private inline function addLongKmer(index, kmer) {
#if debug
        if (kmer.length != kmerLength) throw new ArgumentError('Mismatched k-mer length ${kmer.length}');
#end

        if (cuckooSetList.length > 2147483645) {
            throw new ArgumentError('Overflow in k-mer list');
        }

        var failure = 0;
        var pos     = longKmerMap.get(kmer, -1);

        if (pos == -1) {
            longKmerMap.set(kmer, index);
        }
        else if (pos < referenceCount) {
            var cs = new CuckooSet();
            failure += cs.set(pos + 2);
            failure += cs.set(index + 2);
            longKmerMap.set(kmer, cuckooSetList.length);
            cuckooSetList.push(cs);
        }
        else {
            var cs = cuckooSetList[pos];
            failure += cs.set(index + 2);
            cuckooSetList[pos] = cs;
        }

        return failure;
    }

    public static function fromReference(kmer, refPathList: Array<String>, getReverse = false) {
        var refCount = refPathList.length;
        var object = new KmerMap(kmer, refCount);

        object.cuckooSetList.resize(refCount);
        object.minMaxMap = UInt16Array.fromBytes(Bytes.alloc(2 * minMaxMapSize), 0, minMaxMapSize);

        for (i in 0...refCount) {
            var cs = new CuckooSet();
            cs.set(i + 2);
            object.cuckooSetList[i] = cs;
        }

        for (i in 0...minMaxMapSize) {
            object.minMaxMap[i] = 0xff00;
        }

        var seqBuffer = Bytes.alloc(initialBufferSize);
        var shortKmerSet: UnorderedSet<Int64> = null;
        var longKmerSet: Map<String, Bool> = null;

        if (object.useLongKmer) {
            longKmerSet = new Map<String, Bool>();
        }
        else {
            shortKmerSet = new UnorderedSet<Int64>();
        }

        var failureCount = 0;
        var kmerCount = 0;

        for (i => path in refPathList) {
            // writeLog('Reading k-mers from ${Path.withoutDirectory(path)}');

            if (object.useLongKmer) {
                longKmerSet.clear();
            }
            else {
                shortKmerSet.clear();
            }

            var inFile = new SeqFileInput(path, FileType.Fasta);

#if cpp
            cpp.vm.Gc.enable(false);
#end

            while (!inFile.eof()) {
                var record = inFile.readSequence();

                if (record.length < 2) {
                    continue;
                }

                var rawSeqLen = record[1].length;

                if (seqBuffer.length < rawSeqLen) {
                    if (rawSeqLen > maxBufferSize) {
                        throw new EncodingError('Sequence is too long (buffer size is ${maxBufferSize} bytes)');
                    }

                    seqBuffer = Bytes.alloc(rawSeqLen);
                }

                var seqEnd = FastaHelper.convertToDna(seqBuffer, record[1]);
                var seq = FastaHelper.fromBytes(seqBuffer, 0, seqEnd);

                if (seq.length < object.kmerLength) {
                    continue;
                }

                object.updateMinMaxMap(seq);

                if (object.useLongKmer) {
                    for (j in 0...(seq.length - object.kmerLength + 1)) {
                        longKmerSet[seq.substr(j, object.kmerLength)] = true;
                    }
                }
                else {
                    for (j in 0...(seq.length - object.kmerLength + 1)) {
                        var kmerSeq = seq.substr(j, object.kmerLength);

                        shortKmerSet.insert(ShortKmer.fromString(kmerSeq).toInt64());

                        if (getReverse) {
                            shortKmerSet.insert(ShortKmer.fromString(kmerSeq, true).toInt64());
                        }
                    }
                }
            }

            if (object.useLongKmer) {
                for (kmer in longKmerSet.keys()) {
                    failureCount += object.addLongKmer(i, kmer);
                    kmerCount++;

                    if (getReverse) {
                        failureCount += object.addLongKmer(i, new LongKmer(kmer, true).toString());
                        kmerCount++;
                    }
                }
            }
            else {
                shortKmerSet.foreach(function(kmer) {
                    failureCount += object.addShortKmer(i, kmer);
                    kmerCount++;
                });
            }

#if cpp
            cpp.vm.Gc.enable(true);
#end

            inFile.close();
        }

#if debug
        var totalElements = 0;
        var totalSlots = 0;

        for (cs in object.cuckooSetList) {
            CuckooSet.foreach(cs.toVector(), function(_) { totalElements++; });
            totalSlots += 4 * cs.toVector().length;
        }

        return {dict: object, failure: failureCount, count: kmerCount, occupancy: totalElements / totalSlots};
#else
        return {dict: object, failure: failureCount, count: kmerCount};
#end
    }

    public static function fromPrebuiltFile(kmer, dictPath) {
        var inFile = File.read(dictPath);

        var kmerSize = inFile.readInt32();

        if (kmerSize != kmer) {
            throw new ArgumentError('Requested to read ${kmer}-mer, but dictionary contains ${kmerSize}-mer!');
        }

        var patternSize = inFile.readInt32();

        if (patternSize != minMaxPatternSize) {
            throw new ArgumentError('This program is compiled with pattern size $minMaxPatternSize, but dictionary is built with size $patternSize');
        }

        var refCount = inFile.readInt32();

        if (refCount > 131068) {
            throw new ArgumentError('Overflow in reference list');
        }

        var cuckooSetCount = inFile.readInt32();

        var basenameList = [];
        basenameList.resize(refCount);

        for (i in 0...refCount) {
            basenameList[i] = inFile.readUntil(0);
        }

        var minMaxMapBuf = Bytes.alloc(2 * minMaxMapSize);
        inFile.readFullBytes(minMaxMapBuf, 0, minMaxMapBuf.length);

        var object = new KmerMap(kmerSize, refCount);
        object.cuckooSetList.resize(cuckooSetCount);
        object.minMaxMap = UInt16Array.fromBytes(minMaxMapBuf, 0, minMaxMapSize);

        if (object.useLongKmer) {
            object.longKmerMap.reserve(Std.int(cuckooSetCount / 0.75));
        }
        else {
            object.shortKmerMap.reserve(Std.int(cuckooSetCount / 0.75));
        }

        final kmerHeaderSize = object.kmerStorageSize + 4;

        var buf = Bytes.alloc(2 * 1024 * 1024);
        var dataEnd = 0;

        var count = 0;
        var insertionIndex = 0;
        var singletons = new UnorderedMap<Int64, Int>();

        while (true) {
            try {
                dataEnd += inFile.readBytes(buf, dataEnd, buf.length - dataEnd);
            }
            catch (e: Eof) {
                break;
            }

            var i = 0;
            var resize = 0;

            // The minimum size of an item is 20 bytes (4 bytes size + 8 bytes kmer + 8 bytes cuckoo set)
            while (i + 20 <= dataEnd) {
                var itemSize = buf.getInt32(i);

                if (itemSize > buf.length) {
                    resize = 2 * itemSize;
                    break;
                }

                if (i + itemSize > dataEnd) {
                    break;
                }

                if (itemSize < kmerHeaderSize) {
                    throw new ArgumentError('Malformed k-mer');
                }

                var csLen = itemSize - kmerHeaderSize;
                var isSingleton = csLen == 8;
                var singletonElement = buf.getInt64(i + kmerHeaderSize);
                var index = isSingleton ? singletons.get(singletonElement, -1) : -1;

                if (index < 0) {
                    if (insertionIndex >= cuckooSetCount || insertionIndex > 2147483645) {
                        throw new ArgumentError('Overflow in k-mer list');
                    }

                    object.cuckooSetList[insertionIndex] = new CuckooSet(csLen, buf, i + kmerHeaderSize);
                    index = insertionIndex;
                    insertionIndex++;

                    if (isSingleton) {
                        singletons.set(singletonElement, index);
                    }
                }

                if (object.useLongKmer) {
                    var lk = FastaHelper.fromBytes(buf, i + 4, object.kmerStorageSize);
                    object.longKmerMap.set(lk, index);
                }
                else {
                    var sk = buf.getInt64(i + 4);
                    object.shortKmerMap.set(sk, index);
                }

                count++;
                i += itemSize;
            }

            var srcBuf = buf;

            if (resize > 0) {
                buf = Bytes.alloc(resize);
                resize = 0;
            }

            if (i < dataEnd) {
                dataEnd -= i;
                buf.blit(0, srcBuf, i, dataEnd);
            }
            else {
                dataEnd = 0;
            }
        }

        return {ref: basenameList, dict: object, count: count};
    }

    private inline function addMinMaxTetramer(pattern, minimizer, maximizer) {
        var originalMaximizer = minMaxMap[pattern] & 0xff;
        var originalMinimizer = (minMaxMap[pattern] >>> 8) & 0xff;
        maximizer = (maximizer > originalMaximizer) ? maximizer : originalMaximizer;
        minimizer = (minimizer < originalMinimizer) ? minimizer : originalMinimizer;
        minMaxMap[pattern] = (minimizer << 8) | maximizer;
    }

    private function updateMinMaxMap(seq: String) {
        var forwardPattern = 0;
        var forwardTetramer = 0;
        var backwardPattern = 0;
        var backwardTetramer = 0;

        var bit: Int;

        for (i in 0...minMaxPatternSize) {
            bit = FastaHelper.nuclToPattern(FastaHelper.getChar(seq, i));
            forwardPattern <<= 1;
            forwardPattern |= bit;
            backwardPattern >>>= 1;
            backwardPattern |= bit << (minMaxPatternSize - 1);
        }

        for (i in minMaxPatternSize...(minMaxPatternSize + 4)) {
            forwardTetramer <<= 2;
            forwardTetramer |= FastaHelper.nuclToInt(FastaHelper.getChar(seq, i));
        }

        addMinMaxTetramer(forwardPattern, forwardTetramer, forwardTetramer);
        addMinMaxTetramer(backwardPattern, 0, 0xff);

        for (i in 1...(seq.length - minMaxPatternSize + 1)) {
            bit = FastaHelper.nuclToPattern(FastaHelper.getChar(seq, minMaxPatternSize + i - 1));
            forwardPattern <<= 1;
            forwardPattern |= bit;
            forwardPattern &= (1 << minMaxPatternSize) - 1;
            backwardPattern >>>= 1;
            backwardPattern |= bit << (minMaxPatternSize - 1);

            forwardTetramer <<= 2;
            forwardTetramer &= 0xff;

            if (i <= seq.length - minMaxPatternSize - 4) {
                forwardTetramer |= FastaHelper.nuclToInt(FastaHelper.getChar(seq, minMaxPatternSize + i + 3));
                addMinMaxTetramer(forwardPattern, forwardTetramer, forwardTetramer);
            }
            else {
                var j = i - (seq.length - minMaxPatternSize - 4);
                addMinMaxTetramer(forwardPattern, forwardTetramer & ((0xff << (2 * j)) & 0xff), forwardTetramer | (0xff >>> (2 * (4 - j))));
            }

            backwardTetramer >>>= 2;
            backwardTetramer |= (3 - FastaHelper.nuclToInt(FastaHelper.getChar(seq, i - 1))) << 6;

            if (i >= 4) {
                addMinMaxTetramer(backwardPattern, backwardTetramer, backwardTetramer);
            }
            else {
                addMinMaxTetramer(backwardPattern, backwardTetramer & ((0xff << (2 * (4 - i))) & 0xff), backwardTetramer | (0xff >>> (2 * i)));
            }
        }
    }

    public function checkPatternUsage() {
        var sum = 0.0;
        var c = 0.0;

        for (i in minMaxMap) {
            var skipProb = 1.0;

            if (i != 0xff00) {
                var maximizer = i & 0xff;
                var minimizer = (i >>> 8) & 0xff;
                skipProb = (255 - maximizer + minimizer) / 256;
            }

            var value = skipProb * skipLength / minMaxMapSize;

            var y = value - c;
            var t = sum + y;
            c = (t - sum) - y;
            sum = t;
        }

        expectedSkipLength = sum;
    }

    private static function setMatches(list: Array<CuckooSet>, pos: Int, match: UnorderedSetPointer<Int>) {
#if cpp
        var cs: Vector<Int64> = untyped __cpp__("{0}.StaticCast<::Array<::cpp::Int64>>()", cpp.NativeArray.unsafeGet(list, pos));
#else
        var cs = list[pos].toVector();
#end

        CuckooSet.foreach(cs, match.insert);
    }

    private inline function markHitsShort(map: UnorderedMapPointer<Int64, Int>, match: UnorderedSetPointer<Int>, kmer: ShortKmer) {
        var pos = map.get(kmer.toInt64(), -1);

        if (pos != -1) {
            setMatches(cuckooSetList, pos, match);
            return true;
        }

        return false;
    }

    private inline function markHitsLong(map: UnorderedMapPointer<String, Int>, match: UnorderedSetPointer<Int>, kmer: LongKmer) {
#if debug
        if (kmer.toString().length != kmerLength) throw new ArgumentError('Mismatched k-mer length ${kmer.toString().length}');
#end

        var pos = map.get(kmer.toString(), -1);

        if (pos != -1) {
            setMatches(cuckooSetList, pos, match);
            return true;
        }

        return false;
    }

    public function markHits(match: UnorderedSetPointer<Int>, seq: Bytes, seqEnd: Int, step: Int, getReverse: Bool) {
#if debug
        if (seqEnd < kmerLength) throw new ArgumentError('Read is too short ($seqEnd bp)');
#end

        final longMap     = longKmerMap?.reference();
        final shortMap    = shortKmerMap?.reference();
        final usePatterns = expectedSkipLength >= 0.5 && skipLength > step;
        final tailOffset  = seqEnd - kmerLength;

        var checkPattern  = usePatterns;
        var offset        = 0;

        while (offset <= tailOffset) {
            var kmer = FastaHelper.fromBytes(seq, offset, kmerLength);

            if (checkPattern) {
                var pattern = 0;
                var tetramer = 0;

                for (i in (kmerLength - minMaxPatternSize - 4)...(kmerLength - 4)) {
                    pattern <<= 1;
                    pattern |= FastaHelper.nuclToPattern(FastaHelper.getChar(kmer, i));
                }

                for (i in (kmerLength - 4)...kmerLength) {
                    tetramer <<= 2;
                    tetramer |= FastaHelper.nuclToInt(FastaHelper.getChar(kmer, i));
                }

                var maximizer = minMaxMap[pattern] & 0xff;
                var minimizer = (minMaxMap[pattern] >>> 8) & 0xff;

                if (tetramer > maximizer || tetramer < minimizer) {
                    offset += skipLength;
                    continue;
                }
            }

            var hit = false;

            if (useLongKmer) {
                hit = markHitsLong(longMap, match, new LongKmer(kmer));

                if (getReverse) {
                    hit = markHitsLong(longMap, match, new LongKmer(kmer, true)) || hit;
                }
            }
            else {
                hit = markHitsShort(shortMap, match, ShortKmer.fromString(kmer));

                if (getReverse) {
                    hit = markHitsShort(shortMap, match, ShortKmer.fromString(kmer, true)) || hit;
                }
            }

            checkPattern = usePatterns && !hit;
            offset += step;
        }

        if (offset - step < tailOffset) {
            var kmer = FastaHelper.fromBytes(seq, tailOffset, kmerLength);

            if (useLongKmer) {
                markHitsLong(longMap, match, new LongKmer(kmer));

                if (getReverse) {
                    markHitsLong(longMap, match, new LongKmer(kmer, true));
                }
            }
            else {
                markHitsShort(shortMap, match, ShortKmer.fromString(kmer));

                if (getReverse) {
                    markHitsShort(shortMap, match, ShortKmer.fromString(kmer, true));
                }
            }
        }
    }

    public function markHitsFast(match: UnorderedSetPointer<Int>, seq: Bytes, seqEnd: Int, step: Int, getReverse: Bool) {
#if debug
        if (useLongKmer || step >= kmerLength) throw new ArgumentError('KmerMap.markHitsFast supports short k-mers only');
        if (seqEnd < kmerLength) throw new ArgumentError('Read is too short ($seqEnd bp)');
#end

        final map: UnorderedMapPointer<Int64, Int> = shortKmerMap.reference();

        final kmerOffset  = 64 - 2 * kmerLength;
        final kmerMask    = ~((1 << kmerOffset) - 1);
        final usePatterns = expectedSkipLength >= 0.5 && skipLength > step;
        final tailOffset  = seqEnd - kmerLength;

        var actualStep   = step;
        var checkPattern = usePatterns;
        var offset       = 0;
        var pattern      = 0;
        var sk:  Int64   = 0;
        var skr: Int64   = 0;

        for (i in (kmerLength - minMaxPatternSize - 4)...(kmerLength - 4)) {
            pattern <<= 1;
            pattern |= FastaHelper.nuclToPattern(Bytes.fastGet(seq.getData(), i));
        }

        pattern >>>= actualStep;

        for (i in 0...(kmerLength - step)) {
            var nuc:  Int64 = FastaHelper.nuclToInt(Bytes.fastGet(seq.getData(), i));
            var nucc: Int64 = 3 - nuc;
            sk  |= nuc  << (2 * (31 - step - i));
            skr |= nucc << (2 * (32 - kmerLength + step + i));
        }

        while (offset <= tailOffset) {
            pattern  <<= actualStep;
            sk       <<= 2 * actualStep;
            skr     >>>= 2 * actualStep;

            do {
                var stride = kmerLength - actualStep;
                var bit = FastaHelper.nuclToPattern(Bytes.fastGet(seq.getData(), offset + stride - 4));
                var nuc:  Int64 = FastaHelper.nuclToInt(Bytes.fastGet(seq.getData(), offset + stride));
                var nucc: Int64 = 3 - nuc;
                pattern |= bit  << (actualStep - 1);
                sk      |= nuc  << (64 - 2 * (stride + 1));
                skr     |= nucc << (64 - 2 * actualStep);
                actualStep--;
            } while (actualStep > 0);

            pattern &= (1 << minMaxPatternSize) - 1;
            sk      &= kmerMask;
            skr     &= kmerMask;

            if (checkPattern) {
                var tetramer = (sk >>> kmerOffset) & 0xff;
                var maximizer = minMaxMap[pattern] & 0xff;
                var minimizer = (minMaxMap[pattern] >>> 8) & 0xff;

                if (tetramer > maximizer || tetramer < minimizer) {
                    actualStep = skipLength;
                    offset += skipLength;
                    continue;
                }
            }

            var hit = markHitsShort(map, match, new ShortKmer(sk));

            if (getReverse) {
                hit = markHitsShort(map, match, new ShortKmer(skr)) || hit;
            }

            checkPattern = usePatterns && !hit;
            actualStep = step;
            offset += step;
        }

        offset -= actualStep;

        if (offset != tailOffset) {
            actualStep = tailOffset - offset;
            offset     = tailOffset;
            sk       <<= 2 * actualStep;
            skr     >>>= 2 * actualStep;

            do {
                var stride = kmerLength - actualStep;
                var nuc:  Int64 = FastaHelper.nuclToInt(Bytes.fastGet(seq.getData(), offset + stride));
                var nucc: Int64 = 3 - nuc;
                sk  |= nuc  << (64 - 2 * (stride + 1));
                skr |= nucc << (64 - 2 * actualStep);
                actualStep--;
            } while (actualStep > 0);

            markHitsShort(map, match, new ShortKmer(sk));

            if (getReverse) {
                markHitsShort(map, match, new ShortKmer(skr));
            }
        }
    }

    public function writeToFile(basenameList: Array<String>, dictPath) {
        var outFile = File.write(dictPath);

        outFile.writeInt32(kmerLength);
        outFile.writeInt32(minMaxPatternSize);
        outFile.writeInt32(basenameList.length);
        outFile.writeInt32(cuckooSetList.length);

        for (basename in basenameList) {
            outFile.writeString(basename);
            outFile.writeByte(0);
        }

        var minMaxMapData = minMaxMap.getData();
        outFile.writeFullBytes(minMaxMapData.bytes, minMaxMapData.byteOffset, minMaxMapData.byteLength);

        var buf = Bytes.alloc(2 * 1024 * 1024);
        var i = 0;

        if (useLongKmer) {
            longKmerMap.foreach(function(kmer, pos) {
                var kmerBytes = Bytes.ofString(kmer);
                var cuckooSet = cuckooSetList[pos];
                var itemSize = 4 + kmerBytes.length + cuckooSet.getStorageSize();

                if (i + itemSize > buf.length) {
                    outFile.writeFullBytes(buf, 0, i);
                    i = 0;

                    if (itemSize > buf.length) {
                        buf = Bytes.alloc(2 * itemSize);
                    }
                }

                buf.setInt32(i, itemSize);
                buf.blit(i + 4, kmerBytes, 0, kmerStorageSize);
                cuckooSet.writeToBytes(buf, i + kmerStorageSize + 4);

                i += itemSize;
            });
        }
        else {
            shortKmerMap.foreach(function(kmer, pos) {
                var cuckooSet = cuckooSetList[pos];
                var itemSize = 12 + cuckooSet.getStorageSize();

                if (i + itemSize > buf.length) {
                    outFile.writeFullBytes(buf, 0, i);
                    i = 0;

                    if (itemSize > buf.length) {
                        buf = Bytes.alloc(itemSize);
                    }
                }

                buf.setInt32(i, itemSize);
                buf.setInt64(i + 4, kmer);
                cuckooSet.writeToBytes(buf, i + 12);

                i += itemSize;
            });
        }

        if (i != 0) {
            outFile.writeFullBytes(buf, 0, i);
        }

        outFile.flush();
        outFile.close();
    }
}

class SeqFileInput {
    static inline var initialBufferSize = 64 * 1024;
    static inline var maxBufferSize = 16 * 1024 * 1024;
    static inline var pageSize = 4 * 1024;

    var fileInput: Input;
    var isFasta: Bool;

    var buffer: Bytes;
    var isEof = false;
    var posStart = 0;
    var posEnd = 0;

    var linesRead = 0;
    var pendingRecord = new Array<String>();

    public function new(path, type: Int) {
        fileInput = switch (type) {
            case FileType.Gzip: new GzipFileInput(path);
            case FileType.Fasta | FileType.Fastq: File.read(path);
            default: throw new ArgumentError('Unrecognized file type for $path');
        };

        buffer = Bytes.alloc(initialBufferSize);
        isFasta = type == FileType.Fasta;
    }

    public inline function eof() return isEof && posStart == 0 && posEnd == 0;

    private inline function finalizeRecord(record) return isFasta ? [record[0], record.slice(1).join("")] : record;

    public function readSequence() {
        while (true) {
            if (!isEof && (posStart < pageSize || posEnd <= 3 * (buffer.length >> 2))) {
                try {
                    posEnd += fileInput.readBytes(buffer, posEnd, buffer.length - posEnd);
                }
                catch (e: Eof) {
                    isEof = true;
                }
            }

            var descriptionLine = isFasta || linesRead & 1 == 0;

            if (descriptionLine) {
                while (posStart < posEnd && Bytes.fastGet(buffer.getData(), posStart) <= 32) {
                    posStart++;
                }
            }

            var foundNewLine = false;
            var lineEnd = posStart + 1;

            while (lineEnd <= posEnd) {
                foundNewLine = switch (Bytes.fastGet(buffer.getData(), lineEnd - 1)) {
                    case 0 | "\n".code: true;
                    default: false;
                };

                if (foundNewLine) {
                    lineEnd--;
                    break;
                }

                if (lineEnd == posEnd) {
                    break;
                }

                foundNewLine = switch (Bytes.fastGet(buffer.getData(), lineEnd)) {
                    case 0 | "\n".code: true;
                    default: false;
                };

                if (foundNewLine) {
                    break;
                }

                lineEnd += 2;
            }

            if (foundNewLine) {
                if (!descriptionLine) {
                    while (posStart < lineEnd && Bytes.fastGet(buffer.getData(), posStart) <= 32) {
                        posStart++;
                    };
                }

                var nextPosStart = lineEnd + 1;

                while (posStart < lineEnd && Bytes.fastGet(buffer.getData(), lineEnd - 1) <= 32) {
                    lineEnd--;
                };

                var line = FastaHelper.fromBytes(buffer, posStart, lineEnd - posStart);
                linesRead++;
                posStart = nextPosStart;

                if ((isFasta ? line.startsWith(">") : (linesRead & 3) == 1) || eof()) {
                    var record = pendingRecord;
                    pendingRecord = [];

#if cpp
                    cpp.NativeArray.reserve(pendingRecord, isFasta ? 2 : 4);
#end

                    if (record.length != 0) {
                        pendingRecord.push(line);
                        return finalizeRecord(record);
                    }
                }

                pendingRecord.push(line);
            }
            else {
                if (posEnd >= buffer.length && posStart < pageSize) {
                    if (buffer.length >= maxBufferSize) {
                        throw new EncodingError('Sequence is too long (buffer size is ${buffer.length} bytes)');
                    }

                    var oldBuffer = buffer;
                    var oldDataLength = oldBuffer.length - posStart;

                    buffer = Bytes.alloc(2 * oldBuffer.length);
                    buffer.blit(0, oldBuffer, posStart, oldDataLength);
                    posStart = 0;
                    posEnd = oldDataLength;
                }
                else if (posStart != 0) {
                    var dataLength = posEnd - posStart;

                    buffer.blit(0, buffer, posStart, dataLength);
                    posStart = 0;
                    posEnd = dataLength;
                }

                if (!isEof) {
                    continue;
                }

                while (posStart < posEnd && Bytes.fastGet(buffer.getData(), posStart) <= 32) {
                    posStart++;
                };

                while (posStart < posEnd && Bytes.fastGet(buffer.getData(), posEnd - 1) <= 32) {
                    posEnd--;
                };

                pendingRecord.push(FastaHelper.fromBytes(buffer, posStart, posEnd));
                posStart = 0;
                posEnd = 0;

                var record = pendingRecord;
                pendingRecord = [];
                return finalizeRecord(record);
            }
        }
    }

    public function close() {
        fileInput.close();
    }
}

class OutputBuffer {
    static inline var defaultFileMemoryBudget = 128 * 1024;
    static inline var totalMemoryBudget = 64 * 1024 * 1024;

    var writePairedEnds = false;

    var basenameList: Array<String>;
    var filteredPathList: Array<String>;

    var fileMemoryBudget: Int;
    var fileOutputBuffer: FileOutputBuffer;

    public function new(outputPath, refNames, fileType: Int, mode, outSubdir) {
        basenameList = refNames;
        filteredPathList = [];

        writePairedEnds = mode == 4;

        var filteredFileExt = fileType == FileType.Fasta ? '.fasta' : '.fq';

        for (basename in basenameList) {
            if (mode == 0) {
                filteredPathList.push(Path.join([outputPath, outSubdir, basename + filteredFileExt]));
            }
            else if (mode == 4) {
                filteredPathList.push(Path.join([outputPath, outSubdir, basename + "_1" + filteredFileExt]));
                filteredPathList.push(Path.join([outputPath, outSubdir, basename + "_2" + filteredFileExt]));
            }
        }

        if (mode == 1) {
            filteredPathList.push(Path.join([outputPath, outSubdir, "all_1.fq"]));
            filteredPathList.push(Path.join([outputPath, outSubdir, "all_2.fq"]));
        }

        var fewFile = defaultFileMemoryBudget * filteredPathList.length < totalMemoryBudget;
        fileMemoryBudget = if (fewFile) Std.int(totalMemoryBudget / filteredPathList.length) else defaultFileMemoryBudget;
        fileOutputBuffer = new FileOutputBuffer(filteredPathList.length);
    }

    public function getBasenames() return basenameList;
    public function getOutputPaths() return filteredPathList;

    public function flush() {
        for (key => path in filteredPathList) {
            fileOutputBuffer.flushBuffer(key, path);
        }
    }

    public function writeRecord(key, record: Array<String>, isReverse) {
        if (writePairedEnds) {
            key = 2 * key + (isReverse ? 1 : 0);
        }

        var fileBufferSize = 0;

        for (line in record) {
            fileBufferSize = fileOutputBuffer.addRecord(key, line);
        }

        if (fileBufferSize >= fileMemoryBudget) {
            fileOutputBuffer.flushBuffer(key, filteredPathList[key]);
        }
        else if (fileOutputBuffer.getTotalSize() >= totalMemoryBudget) {
            flush();
        }
    }
}

class MainFilterNew {
    static inline var initialBufferSize = 16 * 1024;
    static inline var maxBufferSize = 256 * 1024;

    static function judgeType(path) return
        switch (Path.extension(path)) {
            case 'gz':                   FileType.Gzip;
            case 'fq' | 'fastq':         FileType.Fastq;
            case 'fa' | 'fas' | 'fasta': FileType.Fasta;
            default:                     -1;
        };

    static function parseOrThrow(s) {
        var v = Std.parseInt(s);
        return v == null ? throw new ArgumentError('Cannot convert $s to integer') : v;
    }

    static function main() return new MainFilterNew().run();

    var command: Arguments;
    var logStream: FileOutput;

    var kmerDict: KmerMap;
    var outputBuffer: OutputBuffer;

    var refHitsList: Array<Int>;

    function new() {
        command = {
            reference: "",
            fastq1: [],
            fastq2: [],
            kmer: 31,
            step: 3,
            maxReads: -1,
            output: "",
            outSubdir: "filtered",
            kmerDict: "",
            getReverse: false,
            useCompositionPattern: false,
            mode: 0
            /* 0: filter reads and write paired ends into one file;
               1: filter reads for NOVOPlasty;
               2: build k-mer dictionary only;
               3: do not write filtered reads to disk;
               4: filter reads and write paired ends into separate files */
        };
    }

    function writeLog(logStr) {
        logStream.writeString(logStr);
        logStream.writeString("\n");
        logStream.flush();
        Sys.println(logStr);
    }

    function run() {
        try {
            parseArgs();
        }
        catch (e: Exception) {
            Sys.println('Invalid argument: $e');
            return;
        }

        Sys.println("Do not close this window manually, please!");

#if cpp
        cpp.vm.Gc.setTargetFreeSpacePercentage(90);
#end

        FileSystem.createDirectory(command.output);

        logStream = File.append(Path.join([command.output, "log.txt"]), false);

        var fileType = command.fastq1.length == 0 ? FileType.Fastq : judgeType(command.fastq1[0]);
        var t0 = Sys.time();

        if (command.kmerDict.length != 0 && FileSystem.exists(command.kmerDict)) {
            try {
                var loader = KmerMap.fromPrebuiltFile(command.kmer, command.kmerDict);

                kmerDict = loader.dict;
                outputBuffer = new OutputBuffer(command.output, loader.ref, fileType, command.mode, command.outSubdir);

                writeLog('The dictionary file of length ${loader.count} has been successfully loaded.');
            }
            catch (e: Exception) {
                writeLog('Failed to read k-mer dictionary: $e');
                return;
            }
        }
        else {
            writeLog("Getting information from references...");

            var refPathList = getRefInfo();

            if (refPathList.length == 0) {
                writeLog("No reference found.");
                return;
            }

            if (refPathList.length > 131068) {
                writeLog('The number of reference files shall not exceed 131068. Found ${refPathList.length}.');
                return;
            }

            if (refPathList.length > 2) {
                var i = refPathList.length - 1;

                while (i > 0) {
                    var j = Std.random(i + 1);

                    var t = refPathList[j];
                    refPathList[j] = refPathList[i];
                    refPathList[i] = t;

                    i--;
                }
            }

            var basenameList = [for (path in refPathList) new Path(path).file];
            outputBuffer = new OutputBuffer(command.output, basenameList, fileType, command.mode, command.outSubdir);

            writeLog("================== Building k-mer dict ==================");

            var count = makeKmerDict(refPathList);

            if (count != 0 && command.kmerDict.length != 0) {
                var parentDir = Path.directory(command.kmerDict);

                if (parentDir.length != 0) {
                    FileSystem.createDirectory(parentDir);
                }

                kmerDict.writeToFile(outputBuffer.getBasenames(), command.kmerDict);
            }
        }

        var t1 = Sys.time();

        writeLog('Step 1 took ${t1 - t0} seconds.');

        if (command.mode == 2) {
            return;
        }

        FileSystem.createDirectory(Path.join([command.output, command.outSubdir]));

        if (command.fastq1.length == 0) {
            writeLog("At least one sequencing file is required.");
            return;
        }

        if (command.fastq2.length == 0) {
            command.fastq2 = command.fastq1;
        }
        else if (command.fastq2.length < command.fastq1.length) {
            command.fastq2.resize(command.fastq1.length);
        }

        for (path in outputBuffer.getOutputPaths()) {
            if (FileSystem.exists(path)) {
                FileSystem.deleteFile(path);
            }
        }

        var t2 = Sys.time();

        writeLog("======================== Filter =========================");

        refHitsList = [for (_ in outputBuffer.getBasenames()) 0];

        if (command.useCompositionPattern) {
            kmerDict.checkPatternUsage();
        }

        filterReads(command.fastq1, command.fastq2, !command.getReverse);

        outputBuffer.flush();

        var t3 = Sys.time();

        writeLog('Step 2 took ${t3 - t2} seconds.');
        writeLog('Ran for ${t3 - t0} seconds in total.');

        var refCountDict = File.write(Path.join([command.output, "ref_reads_count_dict.txt"]), false);

        for (i => basename in outputBuffer.getBasenames()) {
            if (refHitsList[i] > 0) {
                refCountDict.writeString([Std.string(basename), Std.string(refHitsList[i]), "\n"].join(","));
            }
        }

        refCountDict.flush();
        refCountDict.close();
    }

    function parseArgs() {
        var currOpt : Null<String> = null;

        var desc : Map<String, OptionInfo> = [
            "-r"       => {arity: 1, target: "reference"},
            "-q1"      => {arity: 2, convert: command.fastq1.push},
            "-q2"      => {arity: 2, convert: command.fastq2.push},
            "-kf"      => {arity: 1, convert: parseOrThrow, target: "kmer"},
            "-s"       => {arity: 1, convert: parseOrThrow, target: "step"},
            "-m_reads" => {arity: 1, convert: parseOrThrow, target: "maxReads"},
            "-o"       => {arity: 1, target: "output"},
            "-subdir"  => {arity: 1, target: "outSubdir"},
            "-lkd"     => {arity: 1, target: "kmerDict"},
            "-gr"      => {arity: 0, target: "getReverse"},
            "-lb"      => {arity: 0, target: "useCompositionPattern"},
            "-m"       => {arity: 1, convert: parseOrThrow, target: "mode"}
        ];

        var checkArguments = function() {
            if (currOpt == null) {
                return;
            }

            var info = desc[currOpt];

            if (info.arity == 0) {
                Reflect.setField(command, info.target, true);
                currOpt = null;
            }
            else if (info.arity == 1) {
                throw new ArgumentError('Option $currOpt requires an argument');
            }
        }

        for (arg in Sys.args()) {
            arg = arg.trim();

            if (arg.startsWith('-')) {
                checkArguments();
                currOpt = desc.exists(arg) ? arg : throw new ArgumentError('Unknown option $arg');
                continue;
            }

            if (currOpt == null) {
                throw new ArgumentError('Unexpected argument $arg');
            }

            var info = desc[currOpt];

            if (info.arity == 1) {
                Reflect.setField(command, info.target, (info.convert ?? (x -> x))(arg));
                currOpt = null;
            }
            else {
                info.convert(arg);
            }
        }

        checkArguments();

        if (command.kmer < KmerMap.minMaxPatternSize + 4) {
            throw new ArgumentError('K-mer size must be at least ${KmerMap.minMaxPatternSize + 4}');
        }

        if (command.reference.length == 0) {
            throw new ArgumentError('Reference sequences must be supplied');
        }

        if (command.output.length == 0) {
            throw new ArgumentError('Output directory must be supplied');
        }
    }

    function getRefInfo() {
        if (FileSystem.isDirectory(command.reference)) {
            var refPathList = [];

            for (ent in FileSystem.readDirectory(command.reference)) {
                var entPath = Path.join([command.reference, ent]);
                if (!FileSystem.isDirectory(entPath) && judgeType(entPath) == FileType.Fasta) {
                    refPathList.push(entPath);
                }
            }

            return refPathList;
        }
        else if (FileSystem.exists(command.reference)) {
            return [command.reference];
        }

        return [];
    }

    function makeKmerDict(refPathList: Array<String>) {
        var loader = KmerMap.fromReference(command.kmer, refPathList, command.getReverse);

        var failureCount = loader.failure;

        if (failureCount != 0) {
            var be = "is";
            var plurality = "";

            if (failureCount > 1) {
                be = "are";
                plurality = "s";
            }

            writeLog('Encountered $failureCount Cuckoo filter insertion failure${plurality}!');
            writeLog('This means $failureCount k-mer${plurality} ${be} dropped.');
            writeLog('Generally, dropping one k-mer or two will not affect filtering.');
            writeLog('If some many k-mers are lost, please file a bug!');
        }

        kmerDict = loader.dict;

#if debug
        writeLog('Cuckoo filter load factor: ${Std.string(loader.occupancy)}');
#end

        return loader.count;
    }

    function filterReads(fastq1: Array<String>, fastq2: Array<String>, getReverse: Bool) {
        var mode = command.mode;
        var kmerSize = command.kmer;
        var stepSize = command.step;
        var useFastIteration = stepSize < kmerSize && !kmerDict.useLongKmer;

        var matchCallback: Int -> Bool -> Array<String> -> Array<String> -> Void;

        if (mode == 0 || mode == 4) {
            matchCallback = function(key: Int, hasRecord2: Bool, record1: Array<String>, record2: Array<String>) {
                var trueKey = key - 2;

                outputBuffer.writeRecord(trueKey, record1, false);
                refHitsList[trueKey]++;

                if (hasRecord2) {
                    outputBuffer.writeRecord(trueKey, record2, true);
                    refHitsList[trueKey]++;
                }
            };
        }
        else {
            matchCallback = function(key: Int, hasRecord2: Bool, record1: Array<String>, record2: Array<String>) {
                refHitsList[key - 2] += hasRecord2 ? 2 : 1;
            };
        }

        var matchSet = new UnorderedSet<Int>();
        var seqBuffer = Bytes.alloc(initialBufferSize);

        for (i => file1 in fastq1) {
            var file2 = fastq2[i];

            var t0 = Sys.time();

            var readCount = 0;
            var pairedReads = file1 != file2 && file2 != null;
            var isFasta = judgeType(file1) == FileType.Fasta;
            var recordLen = isFasta ? 2 : 4;

            var input1 = new SeqFileInput(file1, judgeType(file1));
            var input2 = pairedReads ? new SeqFileInput(file2, judgeType(file2)) : null;

            var match: UnorderedSetPointer<Int> = matchSet.reference();

            while (!input1.eof()) {
                var record1 = input1.readSequence();
                var record2 = pairedReads ? input2.readSequence() : null;

                if (record1.length < recordLen) {
                    continue;
                }

                var hasRecord2 = record2 != null && record2.length >= recordLen;
                var maxRawSeqLen = (hasRecord2 && record2[1].length > record1[1].length) ? record2[1].length : record1[1].length;

                if (seqBuffer.length < maxRawSeqLen) {
                    if (maxRawSeqLen > maxBufferSize) {
                        throw new EncodingError('Read is too long (buffer size is ${maxBufferSize} bytes)');
                    }

                    seqBuffer = Bytes.alloc(maxRawSeqLen);
                }

                match.clear();
                readCount++;

                var seqEnd = FastaHelper.convertToDna(seqBuffer, record1[1]);

                if (seqEnd >= kmerSize) {
                    if (useFastIteration) {
                        kmerDict.markHitsFast(match, seqBuffer, seqEnd, stepSize, getReverse);
                    }
                    else {
                        kmerDict.markHits(match, seqBuffer, seqEnd, stepSize, getReverse);
                    }
                }

                if (hasRecord2) {
                    seqEnd = FastaHelper.convertToDna(seqBuffer, record2[1]);

                    if (seqEnd >= kmerSize) {
                        if (useFastIteration) {
                            kmerDict.markHitsFast(match, seqBuffer, seqEnd, stepSize, getReverse);
                        }
                        else {
                            kmerDict.markHits(match, seqBuffer, seqEnd, stepSize, getReverse);
                        }
                    }
                }

                if (!match.empty()) {
                    if (mode == 1) {
                        outputBuffer.writeRecord(0, record1, false);

                        if (hasRecord2) {
                            outputBuffer.writeRecord(1, record2, true);
                        }
                    }

                    match.foreach(matchCallback, hasRecord2, record1, record2);
                }

                if (readCount & 1048575 == 0) {
                    var mReads = readCount >> 20;
                    var t1 = Sys.time();

                    writeLog('Handled $mReads m reads, ${1048576 / (t1 - t0)} reads/s');

                    t0 = t1;

                    if (command.maxReads > 0 && mReads >= command.maxReads) {
                        break;
                    }
                }
            }

            input1.close();

            if (pairedReads) {
                input2.close();
            }
        }
    }
}
