using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SQL_IN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window , INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            //Defaults
            SelectedDelimiter = @"\r\n";
        }

        #region Properties

        private List<string> _DelimiterList;
        public List<string> DelimiterList
        {
            get
            {
                if (_DelimiterList == null)
                {
                    _DelimiterList = new List<string>();
                    _DelimiterList.Add(",");
                    _DelimiterList.Add("<space>");
                    _DelimiterList.Add(@"\r\n");
                    _DelimiterList.Add("|");
                    _DelimiterList.Add(";");
                }
                return _DelimiterList;
            }
        }
                
        private string _CustomDelimiter;
        public string CustomDelimiter
        {
            get
            {
                return _CustomDelimiter;
            }
            set
            {
                _CustomDelimiter = value;
                OnPropertyChanged("CustomDelimiter");
            }
        }

        
        private bool _UseCustomDelimiter = false;
        public bool UseCustomDelimiter
        {
            get
            {
                return _UseCustomDelimiter;
            }
            set
            {
                _UseCustomDelimiter = value;
                OnPropertyChanged("UseCustomDelimiter");
            }
        }
        
        private bool _DeleteDuplicates = true;
        public bool DeleteDuplicates
        {
            get
            {
                return _DeleteDuplicates;
            }
            set
            {
                _DeleteDuplicates = value;
                OnPropertyChanged("DeleteDuplicates");
            }
        }	
        
        private bool _UseBuiltInDelimiter = true;
        public bool UseBuiltInDelimiter
        {
            get
            {
                return _UseBuiltInDelimiter;
            }
            set
            {
                _UseBuiltInDelimiter = value;
                OnPropertyChanged("UseBuiltInDelimiter");
            }
        }

        //UseQuotes
        private bool _UseQuotes = true;
        public bool UseQuotes
        {
            get
            {
                return _UseQuotes;
            }
            set
            {
                _UseQuotes = value;
                OnPropertyChanged("UseQuotes");
            }
        }
      
      
        public string SelectedDelimiter { get; set; }


        public string Input { get; set; }

        
        private string _Output;
        public string Output
        {
            get
            {
                return _Output;
            }
            set
            {
                _Output = value;
                OnPropertyChanged("Output");
            }
        }	
      
                       
        private bool _CanModifyOutput;
        public bool CanModifyOutput
        {
            get
            {
                return _CanModifyOutput;
            }
            set
            {
                _CanModifyOutput = value;
                OnPropertyChanged("CanModifyOutput");                
            }
        }

        #endregion

        #region Methods

        private void SQLInIfy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                processInput();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }            
        }

        private void processInput()
        {
            //pick the delimiter
            string Delimiter = UseCustomDelimiter == true ? CustomDelimiter : SelectedDelimiter;
            Delimiter = Delimiter ?? ",";
            Delimiter = Delimiter == "<space>" ? " " : Delimiter;
            
            //explode the input
            List<string> inputList = new List<string>();
            if (Delimiter != "|")
            {
                inputList = Regex.Split(Input, Delimiter).ToList();
            }
            else
            {
                inputList = Input.Split('|').ToList();
            }

            //create the output
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
                        
            inputList = RemoveDuplicates(inputList);
            inputList = CleanupInputList(inputList);
            int words = inputList.Count();
            int count = 0;

            foreach (string item in inputList)
            {
                if (item == string.Empty || item == null)
                    continue;
                string formattedItem = item;
                formattedItem = formattedItem.Replace('"', ' ');
                formattedItem = formattedItem.Replace(System.Environment.NewLine, string.Empty);
                formattedItem = formattedItem.Replace("\r\n", string.Empty);
                if (UseQuotes)
                {
                    sb.Append("'");
                }
                sb.Append(formattedItem.Trim());

                //Last item in list?
                if (!(count == words - 1))
                {
                    if (UseQuotes)
                    {
                        sb.Append("',");
                    }
                    else
                    {
                        sb.Append(",");
                    }
                }
                else
                {
                    if (UseQuotes)
                    {
                        sb.Append("'");
                    }
                }
                count++;
            } 
            
            sb.Append(")");            
            Output = sb.ToString();            
        }

        /// <summary>
        /// Cleans up the list by removing empty strings and such
        /// </summary>
        /// <param name="inputList">The list to clean</param>
        private List<string> CleanupInputList(List<string> inputList)
        {
            List<string> tempInputList = new List<string>();
            foreach (var item in inputList)
            {
                if(!string.IsNullOrEmpty(item) && !string.IsNullOrWhiteSpace(item))
                {
                    tempInputList.Add(item);
                }
            }

            return tempInputList;
        }

        private List<string> RemoveDuplicates(List<string> input)
        {
            return DeleteDuplicates ? input.Distinct().ToList() : input;
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");
            
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void txtOutput_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txtOutput.SelectAll();
            Clipboard.SetText(txtOutput.Text);
        }
                       
    }

    #region Value Converters

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return (bool)value;
        }

        #endregion
    }

    #endregion
}
