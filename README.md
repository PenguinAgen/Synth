When cloning use "git clone --recursive" to ensure you also get the "Stuff" submodule.

The WPF application currently has more functionality than the console. It is therefore the recommended startup project.

Any Setup of a board should happen in the Synth.Setup method defined in the .gitignored file SynthSetup.cs. This is to make sure your hard work designing the board doesn't get overwritten with every pull (and you don't overwrite everyone else's).
