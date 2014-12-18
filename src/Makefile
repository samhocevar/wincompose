
EXE = wincompose.exe

CS = about.cs about.designer.cs \
     compose.cs \
     gui/App.xaml.cs gui/mainwindow.xaml.cs \
     keyboardhook.cs main.cs \
     properties/assemblyinfo.cs \
     properties/resources.designer.cs \
     properties/settings.designer.cs \
     settings.cs types.cs

RESX = properties/resources.resx
RES = properties/wincompose.gui.resources

REFS = System.Drawing System.Windows.Forms System.Data

all: $(EXE)

clean:
	rm -f $(RES) $(EXE)

$(EXE): $(CS) $(RES)
	gmcs $(CS) $(REFS:%=-r:%) $(RES:%=-resource:%) -out:$@

$(RES): $(RESX)
	resgen2 -useSourcePath $^ $@

