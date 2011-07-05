thisdir = class/Microsoft.Build.Tasks.Cpp
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.Tasks.Cpp.dll
BUILD_FRAMEWORK = Microsoft.Build.Framework.dll
BUILD_ENGINE = Microsoft.Build.Engine.dll

ifeq (3.5, $(FRAMEWORK_VERSION))
NAME_SUFFIX = .v3.5
BUILD_FRAMEWORK := $(topdir)/class/lib/$(PROFILE)/$(BUILD_FRAMEWORK)
BUILD_ENGINE := $(topdir)/class/lib/$(PROFILE)/$(BUILD_ENGINE)
else
ifeq (4.0, $(FRAMEWORK_VERSION))
NAME_SUFFIX = .v4.0
endif
endif

LIBRARY_NAME = Microsoft.Build.Tasks.Cpp$(NAME_SUFFIX).dll

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Core.dll			\
	/r:System.Xml.dll			\
	/r:System.Windows.Forms.dll		\
	/r:Microsoft.Build.Utilities$(NAME_SUFFIX).dll	\
	/r:$(BUILD_FRAMEWORK)	\
	/r:$(BUILD_ENGINE)


ifeq (net_4_0, $(PROFILE))
copy-config:
	cp Test/test-config-file-net-4.0 $(test_lib).config
else
ifeq (net_3_5, $(PROFILE))
copy-config:
	cp Test/test-config-file-net-3.5 $(test_lib).config
else
copy-config:
endif
endif

export TESTING_MONO=a
XBUILD_DIR=../../tools/xbuild
include $(XBUILD_DIR)/xbuild_targets.make

include ../../build/library.make
