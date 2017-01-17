using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using HtmlAgilityPack;
using System.Xml;

namespace SWD_AHP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<Preference> preferences = new List<Preference>();
        List<Laptop> laptops = new List<Laptop>();
        Dictionary<string, double> cpulistparsed = new Dictionary<string, double>();
        Dictionary<string, double> gpulistparsed = new Dictionary<string, double>();
        double[] RI = { 0, 0, 0.58, 0.9, 1.12, 1.24, 1.32, 1.41, 1.45, 1.49 };
        double[,] matcpu = null;
        double[,] matgpu = null;
        double[,] matprice = null;
        double[,] matram = null;

        private void add_Click(object sender, RoutedEventArgs e)
        {
            bool ex = false;
            foreach (Preference p in preferences)
            {
                if (p.id == combo.SelectedIndex)
                {
                    ex = true;
                    p.val = Double.Parse(ist.Text);
                    prefList.Items[preferences.IndexOf(p)] = new { name = combo.Text, val = ist.Text };
                }
                Console.WriteLine(p.val);
            }
            if (!ex)
            {
                Preference temp = new Preference(combo.SelectedIndex, combo.Text, Double.Parse(ist.Text));
                preferences.Add(temp);
                prefList.Items.Add(new { name = combo.Text, val = ist.Text });
            }

        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmatpref_Click(object sender, RoutedEventArgs e)
        {
            double[,] matpref = createPreferenceMatrix();
            printMatrix(matpref);
            lista.Items.Add("======");
            printMatrix(normalizeMatrix(matpref));
            lista.Items.Add(consistencyCheck(matpref, normalizeMatrix(matpref)));
        }

        private double[,] createPreferenceMatrix()
        {
            int prfs = preferences.Count;
            double[,] matpref = new double[prfs, prfs];
            for (int i = 0; i < prfs; i++)
                for (int j = 0; j < prfs; j++)
                {
                    if (j > i)
                    {
                        double tmp = (preferences[j].val - (preferences[i].val - 1.0));
                        if (tmp == 0)
                            tmp = 1;
                        double val = 1/tmp;
                        if (val < 0)
                            val = 1 / Math.Abs(val);
                        
                        matpref[i, j] = val;
                    }
                    else if (i == j)
                        matpref[i, j] = 1;
                    else
                        matpref[i, j] = 1 / matpref[j, i];
                }
            return matpref;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AddLaptop addLaptop = new AddLaptop();
            addLaptop.ShowDialog();
            Laptop tmp = addLaptop.laptop;
            laptops.Add(tmp);
            laptopList.Items.Add(new { man = tmp.manufacturer, name = tmp.name, cpu = tmp.cpu, gpu = tmp.gpu, ram = tmp.ram, price = tmp.price });
        }

        private double[,] createCriteriumMatrix(double[] cpuscore)
        {
            double[,] cpumat = new double[cpuscore.Length, cpuscore.Length];

            double max = cpuscore.Max();
            double min = cpuscore.Min();

            for (int i = 0; i < cpuscore.Length; i++)
                cpuscore[i] = 11 - ConvertRange(min, max, cpuscore[i]);

            for (int i = 0; i < cpuscore.Length; i++)
                for (int j = 0; j < cpuscore.Length; j++)
                {
                    if (j > i)
                    {
                        double val = cpuscore[j] - (cpuscore[i] + 1.0);
                        cpumat[j, i] = Math.Abs(val);
                    }
                    else if (i == j)
                        cpumat[i, j] = 1;
                    else
                        cpumat[j, i] = 1 / cpumat[i, j];
                }


            return cpumat;
        }

        private double[,] createCriteriumMatrixPrice(double[] cpuscore)
        {
            double[,] cpumat = new double[cpuscore.Length, cpuscore.Length];

            double max = cpuscore.Max();
            double min = cpuscore.Min();

            for (int i = 0; i < cpuscore.Length; i++)
            {
                cpuscore[i] = ConvertRange(min, max, cpuscore[i]);
            }



            for (int i = 0; i < cpuscore.Length; i++)
                for (int j = 0; j < cpuscore.Length; j++)
                {
                    if (j > i)
                    {
                        double val = cpuscore[j] - (cpuscore[i] - 1.0);
                        if (val < 0)
                            val = 1 / Math.Abs(val);
                        if (val == 0)
                            val = 1;
                        cpumat[i, j] = val;
                    }
                    else if (i == j)
                        cpumat[i, j] = 1;
                    else
                        cpumat[i, j] = 1 / cpumat[j, i];
                }


            return cpumat;
        }

        private double[,] createCriteriumMatrixCPUGPU(string[] vars, string t)
        {
            double[,] cpumat = new double[vars.Length, vars.Length];
            double[] cpuscore = new double[vars.Length];

            if (t.Equals("cpu"))
            {
                if (cpulistparsed.Keys.Count == 0)
                    parseCPU();
            }
            else
                if (gpulistparsed.Keys.Count == 0)
                    parseGPU();


            for (int i = 0; i < vars.Length; i++)
            {
                if (t.Equals("cpu"))
                {
                    cpuscore[i] = cpulistparsed[vars[i]];
                }
                else
                {
                    cpuscore[i] = gpulistparsed[vars[i]];
                }
            }

            double max = cpuscore.Max();
            double min = cpuscore.Min();

            for (int i = 0; i < cpuscore.Length; i++)
            {
               // Console.WriteLine("sc before: " + cpuscore[i]);
                cpuscore[i] = 11 - ConvertRange(min, max, cpuscore[i]);
               // Console.WriteLine("sc after: " + cpuscore[i]);
            }

            for (int i = 0; i < cpuscore.Length; i++)
                for (int j = 0; j < cpuscore.Length; j++)
                {
                    if (j > i)
                    {
                        
                        double val = cpuscore[j] - (cpuscore[i]+1);
                        cpumat[j, i] = Math.Abs(val);
                    }
                    else if (i == j)
                        cpumat[i, j] = 1;
                    else
                        cpumat[j, i] = 1 / cpumat[i, j];
                }
            return cpumat;
        }


        private void parseCPU()
        {
            string html = "";
            using (WebClient client = new WebClient())
            {
                html = client.DownloadString("http://www.cpubenchmark.net/cpu_list.php");
            }
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            doc.LoadHtml(html);

            HtmlNodeCollection cpulist = doc.DocumentNode.SelectNodes("//table[@id='cputable']/tbody/tr");
            foreach (HtmlNode n in cpulist)
            {

                cpulistparsed.Add(n.ChildNodes[0].InnerText, double.Parse(n.ChildNodes[1].InnerText));

                //Console.WriteLine(n.ChildNodes[0].InnerText);
                //Console.WriteLine(n.ChildNodes[1].InnerText);
            }
            Console.WriteLine("PARSED");
        }

        private void parseGPU()
        {
            string html = "";
            using (WebClient client = new WebClient())
            {
                html = client.DownloadString("http://www.videocardbenchmark.net/gpu_list.php");
            }
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            doc.LoadHtml(html);

            HtmlNodeCollection gpulist = doc.DocumentNode.SelectNodes("//table[@id='cputable']/tbody/tr");

            foreach (HtmlNode n in gpulist)
            {
                try
                {
                    gpulistparsed.Add(n.ChildNodes[0].InnerText, double.Parse(n.ChildNodes[1].InnerText));
                }
                catch (Exception e)
                {
                }
                //Console.WriteLine(n.ChildNodes[0].InnerText);
                //Console.WriteLine(n.ChildNodes[1].InnerText);
            }
            Console.WriteLine("PARSED");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string[] vars = new string[laptops.Count];
            for (int i = 0; i < vars.Length; i++)
                vars[i] = laptops[i].cpu;
            printMatrix(createCriteriumMatrixCPUGPU(vars, "cpu"));
        }

        private void printMatrix(double[,] mat)
        {
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                string st = "";
                for (int j = 0; j < mat.GetLength(0); j++)
                {
                    st += mat[i, j] + " ";
                }
                lista.Items.Add(st);
            }

        }

        private double[,] normalizeMatrix(double[,] mat)
        {
            int len = mat.GetLength(0);
            double[,] tmp = new double[len, len];

            for (int i = 0; i < len; i++)
            {
                double sum = 0;
                for (int j = 0; j < len; j++)
                {
                    sum += mat[j, i];
                }

                for (int j = 0; j < len; j++)
                {
                    tmp[j, i] = mat[j, i] / sum;
                }


            }
            return tmp;
        }

        private double[] rowAvg(double[,] norm)
        {
            double[] avg = new double[norm.GetLength(0)];
            for (int i = 0; i < avg.Length; i++)
            {
                double sum = 0;
                for (int j = 0; j < avg.Length; j++)
                {
                    sum += norm[i, j];
                    avg[i] = sum / avg.Length;
                }
            }
            return avg;
        }

        private double[] colSum(double[,] mat)
        {
            double[] sum = new double[mat.GetLength(0)];
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                double s = 0;
                for (int j = 0; j < mat.GetLength(0); j++)
                {
                    s += mat[j, i];
                }
                sum[i] = s;
            }

            return sum;
        }

        private bool consistencyCheck(double[,] mat, double[,] norm)
        {
            int len = mat.GetLength(0);
            double[] col = colSum(mat);
            double[] row = rowAvg(norm);
            double val = 0;
            for (int i = 0; i < len; i++)
                val += col[i] * row[i];

            double ci = (val - len) / (len - 1);

            double cr = ci / RI[len];
            lista.Items.Add("CI = "+ci);
            lista.Items.Add("RI = " + RI[len]);
            lista.Items.Add("CR = " + cr);
            return cr < 0.1;
        }

        private bool checkPreference(int id)
        {
            foreach (Preference p in preferences)
                if (p.id == id)
                    return true;
            return false;
        }

        private double[] eval()
        {
            //uruchom proces
            lista.Items.Clear();
            //tworzenie macierzy
            double[,] matpref = createPreferenceMatrix();
            //printMatrix(matpref);
            //lista.Items.Add("Macierz preferencji: " + (consistencyCheck(matpref, normalizeMatrix(matpref)) ? "Spójna" : "Niespójna"));
            
            List<double[,]> tmp = new List<double[,]>();

            if (checkPreference(0) && matcpu==null)
            {
                string[] vars = new string[laptops.Count];
                for (int i = 0; i < vars.Length; i++)
                    vars[i] = laptops[i].cpu;
                matcpu = createCriteriumMatrixCPUGPU(vars, "cpu");
                //lista.Items.Add("Kryterium CPU: " + (consistencyCheck(matcpu, normalizeMatrix(matcpu)) ? "Spójne" : "Niespójne"));
                //printMatrix(matcpu);
            }

            if (checkPreference(1) && matgpu == null)
            {
                string[] vars = new string[laptops.Count];
                for (int i = 0; i < vars.Length; i++)
                    vars[i] = laptops[i].gpu;
                matgpu = createCriteriumMatrixCPUGPU(vars, "gpu");
                //lista.Items.Add("Kryterium GPU: " + (consistencyCheck(matgpu, normalizeMatrix(matgpu)) ? "Spójne" : "Niespójne"));
                //printMatrix(matgpu);
            }
            if (checkPreference(2) && matram == null)
            {
                double[] nvars = new double[laptops.Count];
                for (int i = 0; i < nvars.Length; i++)
                    nvars[i] = laptops[i].ram;
                matram = createCriteriumMatrix(nvars);
                //lista.Items.Add("Kryterium RAM: " + (consistencyCheck(matram, normalizeMatrix(matram)) ? "Spójne" : "Niespójne"));
                //printMatrix(matram);
            }
            if (checkPreference(3) && matprice == null)
            {
                double[] nvars = new double[laptops.Count];
                for (int i = 0; i < nvars.Length; i++)
                    nvars[i] = laptops[i].price;
                matprice = createCriteriumMatrixPrice(nvars);
                //lista.Items.Add("Kryterium Cena: " + (consistencyCheck(matprice, normalizeMatrix(matprice)) ? "Spójne" : "Niespójne"));
                //printMatrix(matprice);
            }

            foreach (Preference p in preferences)
            {
                if (p.id == 0)
                    tmp.Add(matcpu);
                if (p.id == 1)
                    tmp.Add(matgpu);
                if (p.id == 2)
                    tmp.Add(matram);
                if (p.id == 3)
                    tmp.Add(matprice);
            }

            double[] avgPref = rowAvg(normalizeMatrix(matpref));

            double[] rankVal = new double[laptops.Count];

            for (int i = 0; i < rankVal.Length; i++)
            {
                double val = 0;

                for (int j = 0; j < avgPref.Length; j++)
                {
                    double[] tempAvg = rowAvg(normalizeMatrix(tmp[j]));
                    val += avgPref[j] * tempAvg[i];
                }
                rankVal[i] = val;
                //Console.WriteLine(val);
            }

            double[] ranking = new double[rankVal.Length];
            Array.Copy(rankVal, ranking, rankVal.Length);
            Array.Sort(ranking);
            Array.Reverse(ranking);
            //Console.WriteLine("========");
            //lista.Items.Add("Ranking: ");
            for (int i = 0; i < ranking.Length; i++)
            {
                //Console.WriteLine(laptops[indexOf(ranking[i], rankVal)].name + " " + ranking[i]);
                lista.Items.Add(laptops[indexOf(ranking[0], rankVal)].manufacturer + " " + laptops[indexOf(ranking[i], rankVal)].name + " - " + ((Math.Round(ranking[i], 5)) * 100) + "%");
            }
            //lista.Items.Add("Najlepszy: " + laptops[indexOf(ranking[0], rankVal)].manufacturer + " " + laptops[indexOf(ranking[0], rankVal)].name);
            return rankVal;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            eval();

        }

        private int indexOf(double d, double[] t)
        {
            int i = -1;
            for (int j = 0; j < t.Length; j++)
                if (t[j] == d)
                    i = j;
            return i;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML Files (*.xml)|*.xml";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                Console.WriteLine(filename);
                XmlDocument doc = new XmlDocument();
                doc.Load(@filename);
                Console.WriteLine(doc.SelectNodes("//Laptop").Count);
                foreach (XmlNode xn in doc.SelectNodes("//Laptop"))
                {
                    Console.WriteLine(xn.InnerXml);
                    string manu = xn.ChildNodes[0].InnerText;
                    string model = xn.ChildNodes[1].InnerText;
                    string cpu = xn.ChildNodes[2].InnerText;
                    string gpu = xn.ChildNodes[3].InnerText;
                    string ram = xn.ChildNodes[4].InnerText;
                    string price = xn.ChildNodes[5].InnerText;
                    laptops.Add(new Laptop(manu, model, cpu, gpu, double.Parse(ram), double.Parse(price)));
                    laptopList.Items.Add(new { man = manu, name = model, cpu = cpu, gpu = gpu, ram = ram, price = price });
                }
            }
        }

        public static double ConvertRange(double originalStart, double originalEnd, double value)
        {
            double scale = (double)(10 - 1) / (originalEnd - originalStart);
            return (1 + ((value - originalStart) * scale));
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            System.IO.StreamWriter file;
            
            for(int l = 0 ; l < 3 ; l++){
                file = new System.IO.StreamWriter("d:\\t3"+laptops[l].manufacturer+".dat");
                for (int i = 0; i < 10; i++)
                {
                    preferences[0].val = 1 + i;
                    preferences[1].val = 10 - i;
                    for (int j = 1; j <= 10; j++)
                    {
                        preferences[2].val = j;
                        double[] rank = eval();
                        string s = (i + 1) + " " + j + " " + Math.Round(rank[l]*100,2);
                        file.WriteLine(s);
                    }
                    file.WriteLine();
                }
                file.Close();
            }
            
            
        }
    }
}
