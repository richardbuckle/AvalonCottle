using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace AvalonCottle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CompletionWindow completionWindow;
        private string currentFilePath;

        #region setup
        public MainWindow()
        {
            RegisterCottleDefinition();
            InitializeComponent();
            ConfigfureTextEditor();
            LoadDefaultText();
        }

        private void RegisterCottleDefinition()
        {
            IHighlightingDefinition cottleHighlightingDef;
            using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("AvalonCottle.Cottle.xshd"))
            {
                using (XmlReader reader = new XmlTextReader(s))
                {
                    cottleHighlightingDef = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            // Tweak a HighlightingColor at runtime to prove that we can
            SetBackgroundColor(cottleHighlightingDef, "Comment", Colors.Bisque);

            // Register our definition against the file extension
            HighlightingManager.Instance.RegisterHighlighting("Cottle", new string[] { ".cottle" }, cottleHighlightingDef);
        }

        private static void SetBackgroundColor(IHighlightingDefinition definition, string colorName, Color newColor)
        {
            HighlightingColor color = definition.GetNamedColor(colorName);
            color.Background = new SimpleHighlightingBrush(newColor);
        }

        private void ConfigfureTextEditor()
        {
            SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
            textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
            SearchPanel.Install(textEditor);
        }

        private void LoadDefaultText()
        {
            using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("AvalonCottle.Data voucher redeemed.cottle"))
            {
                if (s == null) {return;}
                using (StreamReader reader = new StreamReader(s))
                {
                    textEditor.Text = reader.ReadToEnd();
                }
            }
        }
        #endregion

        #region File handling
        void openFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Cottle files (*.cottle)|*.cottle|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                CheckFileExists = true
            };
            if (dlg.ShowDialog() ?? false)
            {
                currentFilePath = dlg.FileName;
                refreshWindowTitle();
                textEditor.Load(currentFilePath);
                textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(currentFilePath));
            }
        }

        void saveFileClick(object sender, EventArgs e)
        {
            if (currentFilePath == null)
            {
                SaveFileDialog dlg = new SaveFileDialog
                {
                    FileName = "Document",
                    DefaultExt = ".cottle",
                    Filter = "Cottle files (*.cottle)|*.cottle|Text files (*.txt)|*.txt",
                };
                if (dlg.ShowDialog() ?? false)
                {
                    currentFilePath = dlg.FileName;
                }
                else
                {
                    return;
                }
            }
            textEditor.Save(currentFilePath);
            refreshWindowTitle();
        }

        private void refreshWindowTitle()
        {
            Title = Path.GetFileName(currentFilePath);
        }
        #endregion

        #region Input handling
        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            // open code completion after the user has pressed dot:
            if (e.Text == ".")
            {
                completionWindow = new CompletionWindow(textEditor.TextArea);

                // provide AvalonEdit with the data:
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.Add(new MyCompletionData("alpha"));
                data.Add(new MyCompletionData("beta"));
                data.Add(new MyCompletionData("gamma"));

                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
        }

        void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // do not set e.Handled=true - we still want to insert the character that was typed
        }
        #endregion
    }
}
