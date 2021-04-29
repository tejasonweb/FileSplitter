using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Timers;

namespace FileSplitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int DefaultSplitSize = 100;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (filetosplit.Text.Trim() != "")
            {
                if (mbsize.Text.Trim() != "")
                    int.TryParse(mbsize.Text,out DefaultSplitSize);

                if (DefaultSplitSize > 0)
                {
                    if (new FileInfo(filetosplit.Text).Length > DefaultSplitSize * 1048576)
                    {
                        try
                        {
                            int i = 1;
                            string infilename = filetosplit.Text;
                            string extension = System.IO.Path.GetExtension(filetosplit.Text);
                            string outfilename = (infilename.Replace(extension, "") + "_Split_" + i + extension);

                            DateTime before = DateTime.Now;
                            disableForm();
                            bool result = await Task.Factory.StartNew(()=>  SplitFile(infilename, outfilename, DefaultSplitSize));
                            DateTime after = DateTime.Now;

                            string timetaken = Convert.ToInt32((after - before).TotalSeconds) > 60 ? ((after - before).TotalMinutes > 60 ? (Math.Round((after - before).TotalHours)).ToString() + " Hours" : Math.Round((after - before).TotalMinutes).ToString() + " Minutes") : Math.Round((after - before).TotalSeconds).ToString() + " Seconds";

                            if (result)
                                System.Windows.Forms.MessageBox.Show("File Split Completed, Took " + timetaken + ".");
                            else
                                System.Windows.Forms.MessageBox.Show("Failed to Split");
                            enableForm();
                        }
                        catch(Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show("Exception Occurred: " + ex.Message, "Exception");
                            enableForm();
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("File is already smaller than the specified split size (Default 100 MB).");

                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Incorrect Split Size provided!", "Error");
                }
            }
        }

        

        private void disableForm()
        {
            //System.Windows.Forms.MessageBox.Show("Disabling form");
            
            filetosplit.IsEnabled = false;
            //System.Windows.Forms.MessageBox.Show(filetosplit.IsEnabled.ToString());
            splitbutton.IsEnabled = false;
            pickinputfile.IsEnabled = false;
            mbsize.IsEnabled = false;
            splitprogress.Visibility = Visibility.Visible;
        }

        private void enableForm()
        {
            filetosplit.IsEnabled = true;
            splitbutton.IsEnabled = true;
            pickinputfile.IsEnabled = true;
            mbsize.IsEnabled = true;
            splitprogress.Visibility = Visibility.Collapsed;
        }

        private bool SplitFile(string filetosp, string outfile, int DefaultSpSize)
        {
            int i = 1, readcount;
            char[] buffer = new char[1050576];
            string extension = System.IO.Path.GetExtension(filetosp);
            
            using(StreamReader sr = new StreamReader(filetosp))
            {
                StreamWriter sw = new StreamWriter(outfile);
                while ((readcount = sr.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (readcount < buffer.Length)
                    {
                        char[] tempbuffer = new char[readcount];
                        Array.Copy(buffer, tempbuffer, readcount);
                        buffer = tempbuffer;
                        //System.Windows.Forms.MessageBox.Show("bufferlen " + buffer.Length + " and readcount is "+readcount);
                    }
                    //buffer = buffer.ToList().ToArray();
                    //buffer = buffer.Where(b => b != null).ToArray();
                    sw.Write(buffer);
                    //sw.Write("\nreadcount :: " + readcount + "\n");
                    Array.Clear(buffer, 0, buffer.Length);
                    //System.Windows.Forms.MessageBox.Show("FileSize : "+ (new FileInfo(outfile)).Length);
                    if ((new FileInfo(outfile)).Length >= (DefaultSpSize * 1048576))
                    {
                        //System.Windows.Forms.MessageBox.Show("Max splitsize occurred..");
                        char[] temp = new char[1];
                        int tempcount = 0;
                        while (true)
                        {
                            var tempdata = sr.Read(temp, 0, temp.Length);
                            sw.Write(temp);
                            if ((temp[0] == '\n' || tempcount > 5000))
                            {
                                //System.Windows.Forms.MessageBox.Show("Completed writting to the old split file");
                                break;
                            }
                            tempcount++;
                        }
                        //System.Windows.Forms.MessageBox.Show("Tempcount : "+ tempcount);
                        sw.Close();
                        i++;
                        outfile = filetosp.Replace(extension, "") + "_Split_" + i + extension;
                        sw = new StreamWriter(outfile);
                        //System.Windows.Forms.MessageBox.Show("Created new splitfile..");
                    }
                }
                sw.Close();
            }



            //using (StreamReader sr = new StreamReader(filetosp))
            //{
            //    try
            //    {
            //        StreamWriter sw = new StreamWriter(outfile);
            //        do
            //        {
            //            //System.Windows.Forms.MessageBox.Show(sr.ReadLine());
            //            if (count % 10 == 0)
            //            {
            //                if (new FileInfo(outfile).Length >= (DefaultSpSize * 1048576) && datatowrite == buffer.Length)
            //                {
            //                    // file size exceeded the limit, time to create a new file
            //                    // however before that, make sure to write the data until the newline

            //                    char[] temp = new char[1];
            //                    int tempcount = 0;
            //                    do
            //                    {
            //                        var tempdata = sr.ReadBlock(temp, 0, temp.Length);
            //                        sw.Write(temp);
            //                        if (temp[0] == '\n' || tempcount > 5000 || temp[0]!='\0')
            //                        {
            //                            break;
            //                        }
            //                        tempcount++;
            //                    }
            //                    while (true);

            //                    sw.Close();
            //                    i++;
            //                    outfile = (filetosp.Replace(extension, "") + "Split_" + i + extension);
            //                    sw = new StreamWriter(outfile);
            //                }
            //            }
            //            //System.Windows.Forms.MessageBox.Show(buffer.ToString());
            //            datatowrite = sr.ReadBlock(buffer, 0, buffer.Length);
            //            sw.Write(buffer);
            //            Array.Clear(buffer,0,buffer.Length);
            //            //await sw.WriteLineAsync(sr.ReadLine());
            //            count++;
            //        }
            //        while (datatowrite == buffer.Length);       // if it is not equal, there is no more data to read
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Windows.MessageBox.Show(ex.Message, "Exception");
            //    }
            //}
            return true;
        }

        private void pickinputfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select File to Split";
            ofd.Filter = "Log Files(*.log)|*.log|Text Files(*.txt)|*.txt";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filetosplit.Text = ofd.FileName;
            }
        }
    }
}
