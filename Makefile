PY_SRC := build_consensus main_assembler main_refilter_new merge_seq unix_command
PY_BIN := $(patsubst %,cli/bin/%,$(PY_SRC))

.PHONY: build clean

build: cli/bin/MainFilterNew $(PY_BIN)
	for target in $(PY_SRC); do cp -L -r -t cli/bin --reflink=auto --update=none scripts/dist/$$target/_internal; done
	cd cli && ln -f -r -s bin/unix_command geneminer2

clean:
	rm -f -r scripts/filter/bin
	rm -f -r scripts/build
	rm -f -r scripts/dist

distclean: clean
	for target in $(PY_SRC); do rm -f scripts/$$target.spec; done
	rm -f -r cli/bin
	rm -f cli/geneminer2

cli/bin/MainFilterNew: scripts/filter/*.h scripts/filter/*.hpp scripts/filter/*.hx
	cd scripts/filter && haxe -cpp bin -dce full -D analyzer-optimize -D HXCPP_GC_BIG_BLOCKS -D HXCPP_GC_MOVING -D HXCPP_M64 -D HXCPP_OPTIMIZE_LINK -D HXCPP_SINGLE_THREADED_APP -D HXCPP_VISIT_ALLOCS -main MainFilterNew.hx
	install -D -t cli/bin scripts/filter/bin/MainFilterNew

$(PY_BIN): cli/bin/%: scripts/%.py
	cd scripts && pyinstaller -D -y --optimize 1 $(notdir $<)
	install -D -t cli/bin scripts/dist/$(notdir $@)/$(notdir $@)
