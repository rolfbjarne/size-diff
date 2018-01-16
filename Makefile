all:
	msbuild

install: all ~/bin/size-diff

~/bin/size-diff:
	sed 's@%DIR%@$(abspath $(CURDIR))@' size-diff.in > $@
	chmod +x $@

.PHONY: ~/bin/size-diff