
EXE = wincompose.exe

CS = main.cs types.cs compose.cs keyboardhook.cs settings.cs \
     about.cs about.designer.cs \
     gui.cs gui.designer.cs \
     properties/assemblyinfo.cs \
     properties/resources.designer.cs \
     properties/settings.designer.cs

RESX = properties/resources.resx
RES = properties/wincompose.gui.resources

REFS = System.Drawing System.Windows.Forms System.Data

all: $(EXE)

clean:
	rm -f $(RES) $(EXE)

$(EXE): $(CS) $(RES)
	gmcs $(CS) $(REFS:%=-r:%) $(RES:%=-resource:%) -out:$@

$(RES): $(RESX)
	resgen2 $^ $@

