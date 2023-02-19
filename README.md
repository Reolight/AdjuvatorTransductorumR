# AdjuvatorTransductorumR
## Background
AdjuvatorTransductorumR (nearly like "helper for translators" from latin) is an application for providing support for localization and translation of applications. I decided to make it due to next reasons:

I was making application on React with i18next and I saw there are: backend library retrieving translational keys from files and a paid site for continuous localization in a comfortable way. If you decide to make it yourself, you'll encounter with multiple files opened per language. So, I have tried to google a little for applications like Translator++ and while I was doing this I decided to try make it myself to gain an interesting expirience of making real application from the beginning.

## How to use
There is no release version yet, but it is possible to test current commit (though there is may be some not working features due to development and reimplementing some of them in a way more easier to use or more convinient for application itself [e.g. Data Model Change Tracker for traking all changes made]):
1. Clone/download the project,
2. Compile the solution (if tests failed to build, than build projects separatedly. Tests are temporally not work with current plugin interface),
3. Compiled plugin place in the Plugin folder of an application (if there is no such folder, create it manually or launch the application and it will create one automatically).
4. Loaded plugins are displayed in extraction/injection menu. If plugin is red than it is not compatible with current core version and thus is not supported.

## About application

The application supports:
1. Multiple data sources to extract from or inject into. (**Different sources** have been made by plugin extensibility.)
2. Saving it as a project in XML file and loading from it,
3. **Workspace** for editing files (with file, folder, new languages, addition and so on). **Workspace** (is under development in current time) WPF was chosen. It allows to edit files in separate tabs and interact with main features of an application

Data representation devided into 3 layers:
*Raw data* which are processed by a plugin into *data model* displaying as a *view model* to user.

## About plugins
There is one plugin by default. It extracts data from 'locales' folder created for ReactJs i18next library and provides ability to edit it in the Workspace.

Plugin is loaded by core of an Adjuvator and due to core may be slightly changed in a future it has CorVersion.

Plugin has to implement 2 interfaces: 
1. **IDataProviderInfo** - contains detail about plugin and CorVersion. If CorVersion is not equal to applications one the plugin is considered unsupported thus the 2nd interface is not loaded to avoid errors.
2. **IDataProvider** - contains 2 windows defined for translation extraction and injection. All internal logic of the plugin is implemented by developer and hooks to those windows.

The windows must be definded with help of ViewDescriber. It contains basic tools for form description and provides ability to define custom event handlers.

Front-end project (which is WPF in the current solution) in it turn must have reader for this ViewDescriber to read and display properly those windows. In the current version PluginViewTranslator does this. It connects WPF events to ViewDescriber events, binds DependencyProperty of WPF forms to a ViewProp of described forms and allows to use command interface though CanExecute and Execute events defined in ViewForm. Due to this approach achieved next goals: independency of plugin custom windows from front-end part of an application, uniform windows description, abstraction from logic of front-end application (it is achieved by data simplification upon handling front-end events: front-end form transmits associated ViewForm to custom event handler as sender object). Some types of events use their own EventArgs realisation: **_ViewEventArgsCanExecute_** has field *CanExecute*, which should be changed to true if Execute event may be executed; and **_ViewEventArgsDataAction_** which contains such fields as *DataExtracted* (nullable, if contains DataModel extraction window will be closed as it function was done) and *Injected" (if true, DataModel considered injected).
