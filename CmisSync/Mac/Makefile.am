SUBDIRS = Pixmaps

ASSEMBLY = CmisSync
TARGET = exe

LIBS = $(REF_CMISSYNC) $(LOG4NET_LIBS) $(NOTIFY_SHARP_LIBS) $(MONOMAC_LIBS) $(DRAWING)

SOURCES =  \
	../AboutController.cs \
	../CertPolicyHandler.cs \
	../ControllerBase.cs \
	../EditController.cs \
	../Program.cs \
	../Properties/Resources.Designer.cs \
	../SettingController.cs \
	../SetupController.cs \
	../StatusIconController.cs \
	../Windows/FolderTreeMVC/IgnoredFolderLoader.cs \
	../Windows/FolderTreeMVC/LoadingStatusModel.cs \
	../Windows/FolderTreeMVC/LocalFolderLoader.cs \
	../Windows/FolderTreeMVC/NodeLoader.cs \
	../Windows/FolderTreeMVC/NodeModel.cs \
	../Windows/FolderTreeMVC/NodeModelUtils.cs \
	../TransmissionController.cs \
	About.cs \
	AppDelegate.cs \
	Badger.cs \
	CertPolicyWindow.cs \
	ClientBrand.cs \
	CmisTree/CmisOutline.cs \
	CmisTree/CmisOutline.designer.cs \
	CmisTree/CmisOutlineController.cs \
	CmisTree/CmisTreeDataSource.cs \
	Controller.cs \
	EditWizard.cs \
	EditWizard.designer.cs \
	EditWizardController.cs \
	GeneralSettings.cs \
	GeneralSettings.designer.cs \
	GeneralSettingsController.cs \
	MacWatcher.cs \
	Notifications/Credentials.cs \
	Notifications/FileSystemProgress.cs \
	Notifications/TransmissionProgress.cs \
	OutlineViewDelegate.cs \
	ProxySettings.cs \
	ProxySettings.designer.cs \
	ProxySettingsController.cs \
	RepositoryMenuItem.cs \
	SetupSubCustomize.cs \
	SetupSubCustomize.designer.cs \
	SetupSubCustomizeController.cs \
	SetupSubFinished.cs \
	SetupSubFinished.designer.cs \
	SetupSubFinishedController.cs \
	SetupSubLogin.cs \
	SetupSubLogin.designer.cs \
	SetupSubLoginController.cs \
	SetupSubRepoSelect.cs \
	SetupSubRepoSelect.designer.cs \
	SetupSubRepoSelectController.cs \
	SetupSubTutorial.cs \
	SetupSubTutorial.designer.cs \
	SetupSubTutorialBegin.cs \
	SetupSubTutorialBegin.designer.cs \
	SetupSubTutorialBeginController.cs \
	SetupSubTutorialController.cs \
	SetupSubTutorialEnd.cs \
	SetupSubTutorialEnd.designer.cs \
	SetupSubTutorialEndController.cs \
	SetupSubWelcome.cs \
	SetupSubWelcome.designer.cs \
	SetupSubWelcomeController.cs \
	SetupWizard.cs \
	SetupWizard.designer.cs \
	SetupWizardController.cs \
	StatusIcon.cs \
	TransmissionWidget.cs \
	TransmissionWidget.designer.cs \
	TransmissionWidgetController.cs \
	TextFieldDelegate.cs \
	UI.cs \
	UIHelpers.cs

RESOURCES = $(RESOURCES_RESOURCES)

RESOURCES_RESX = $(wildcard $(srcdir)/../Properties/*.resx)
RESOURCES_RESOURCES = $(addprefix CmisSync.,$(patsubst %.resx,%.resources,$(notdir $(RESOURCES_RESX))))

RESOURCES_RESX_XAMARIN =  \
	CmisSync../Properties/Properties.Resources.cs.resx,CmisSync.Properties.Resources.cs.resources \
	CmisSync../Properties/Properties.Resources.de.resx,CmisSync.Properties.Resources.de.resources \
	CmisSync../Properties/Properties.Resources.es-ES.resx,CmisSync.Properties.Resources.es-ES.resources \
	CmisSync../Properties/Properties.Resources.fr.resx,CmisSync.Properties.Resources.fr.resources \
	CmisSync../Properties/Properties.Resources.ja.resx,CmisSync.Properties.Resources.ja.resources \
	CmisSync../Properties/Properties.Resources.nl.resx,CmisSync.Properties.Resources.nl.resources \
	CmisSync../Properties/Properties.Resources.resx,CmisSync.Properties.Resources.resources \
	CmisSync../Properties/Properties.Resources.tlh-AA.resx,CmisSync.Properties.Resources.tlh-AA.resources \
	CmisSync../Properties/Properties.Resources.tlh-KX.resx,CmisSync.Properties.Resources.tlh-KX.resources \
	CmisSync../Properties/Properties.Resources.uk.resx,CmisSync.Properties.Resources.uk.resources      

CmisSync.%.resources: $(srcdir)/../Properties/%.resx
	resgen2 $< $@

EXTRA_DIST = README.md

include $(top_srcdir)/build/build.mk

CLEANFILES += $(RESOURCES_RESOURCES)
