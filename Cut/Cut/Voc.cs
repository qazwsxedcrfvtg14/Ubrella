using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Text.RegularExpressions;
using System.Net;
using System.Diagnostics;
namespace Cut
{
    public static class Voc
    {
        public static XLabs.Platform.Services.Geolocation.IGeolocator _geolocator = DependencyService.Get<XLabs.Platform.Services.Geolocation.IGeolocator>();
        public static string sites_map;
        public static Maps 
            words = new Maps(), 
            root = new Maps(), 
            prefix = new Maps(), 
            suffix = new Maps(),
            note = new Maps(),
            setting = new Maps(), 
            favorite = new Maps();
        public static SortedDictionary<string, SortedSet<string>> rtp=null;
        public static async Task DumpAppFileAsync(string fil)
        {
            var fileService = DependencyService.Get<ISaveAndLoad>();
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var tmp=assembly.GetManifestResourceNames();
            Stream stream = assembly.GetManifestResourceStream("Cut.data."+fil);
            using (var reader = new System.IO.StreamReader(stream))
            {
                await fileService.SaveTextAsync(fil, reader.ReadToEnd());
            }
        }
        public static async Task SavingSettingAsync()
        {
            string ot="";
            foreach (var x in setting.data)
		        ot += x.Key + "," + x.Value + "\n";
            var fileService = DependencyService.Get<ISaveAndLoad>();
            await fileService.SaveTextAsync("setting.txt",ot);
        }
        public static async Task<Maps> GetDocAsync(string inp,bool user=true){
            Maps data=new Maps();
            data.clear();
            var fileService = DependencyService.Get<ISaveAndLoad>();
            if (!fileService.FileExists(inp + ".txt"))
                return data;
            string st= await fileService.LoadTextAsync(inp+".txt");
            if (user && fileService.FileExists(inp + "_user.txt"))
                st += "\n" + await fileService.LoadTextAsync(inp + "_user.txt");
            byte[] byteArray = Encoding.UTF8.GetBytes(st);
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            MemoryStream stream = new MemoryStream(byteArray);
            StreamReader str = new StreamReader(stream);
            string s;
            while (true)
            {
                s = str.ReadLine();
                if (s == null) break;
                List<string> a = new List<string>
                {
                    ""
                };
                string b = "", d = "";
                int l = s.Length;
                bool tag = false, star = false, dis = false;
                for (int i = 0; i < l; i++)
                    if (tag) b += s[i];
                    else if (s[i] == '$') dis = true;
                    else if (s[i] == '*') star = true;
                    else if (star && s[i] == ' ') d += ' ';
                    else if (s[i] == ',') tag = true;
                    else if (s[i] == '/') a.Add("");
                    else
                    {
                        a[a.Count - 1] += s[i];
                        d += s[i];
                    }
                foreach (var c in a)
                {
                    if (c.Trim() == "") continue;
                    if (dis)
                    {
                        data.remove(c);
                        continue;
                    }
                    data.add(c, b);
                    if(star)
                        data.add_ok(c, d);
                }
            }
            if(user)
                data.file_name = inp + "_user.txt";
            else
                data.file_name = inp + ".txt";
            return data;
        }
        public static List<string> Match(string match, int beg = 0,string st="")
        {
            List<string> ve = new List<string>();
            ve.Clear();
            string reg_string = "";
            string lb="",ub="";
            bool noteng=false;
            if (match.Length != 0 && match[0] == '^') {
                noteng = true;
                match = match.Substring(1);
            }
            bool tg = true;
            for (int i = 0; i < match.Length; i++)
            {
                if (match[i] == '*')
                {
                    reg_string += ".*";
                    ub += (char)0xEFFF;
                    tg = false;
                }
                else if (match[i] == '?')
                {
                    reg_string += ".";
                    if(tg)lb += (char)0;
                    ub += (char)0xEFFF;
                }
                else if ((match[i] == '[' || match[i] == ']' || match[i] == '.'))
                {
                    reg_string += "\\" + match[i];
                    if (tg) lb += match[i];
                    ub += match[i];
                }
                else if (match[i] >= 0 && match[i] <= 128)
                {
                    reg_string += match[i];
                    if (tg) lb += match[i];
                    ub += match[i];
                }
                else
                {
                    noteng = true;
                    reg_string += match[i];
                }
            }
            Regex reg = new Regex("^"+reg_string, RegexOptions.IgnoreCase);
            Regex reg2 = new Regex("^.*" + reg_string + ".*");
            int cnt = 0;
            if (noteng)
            {
                foreach (var x in words.data)
                {
                    if (reg.IsMatch(x.Key))
                    {
                        if (cnt >= beg)
                            ve.Add(x.Key);
                        if ((++cnt) - beg == 30) break;
                    }
                    else if (noteng && reg2.IsMatch(x.Value))
                    {
                        if (cnt >= beg)
                            ve.Add(x.Key);
                        if ((++cnt) - beg == 30) break;
                    }
                }
            }
            else
            {
                var be = words.data.lower_bound(lb);
                var ed = words.data.upper_bound(ub);
                if (st != "")
                {
                    be = words.data.upper_bound(st);
                    beg = 0;
                }
                while (be != ed) {
                    if (reg.IsMatch(be.val.Key))
                    {
                        if (cnt >= beg)
                            ve.Add(be.val.Key);
                        if ((++cnt) - beg == 30) break;
                    }
                    be = be.next();
                }
            }
            return ve;
        }
        public static List<string> Match_rot(string match, string beg = "")
        {
            bool noteng = false;
            if (match.Length != 0 && match[0] == '^')
            {
                noteng = true;
                match = match.Substring(1);
            }
            string reg_string = "";
            for (int i = 0; i < match.Length; i++)
            {
                if (match[i] == '*')
                    reg_string += ".*";
                else if (match[i] == '?')
                    reg_string += ".";
                else if (match[i] >= 0 && match[i] <= 128)
                    reg_string += match[i];
                else
                {
                    noteng = true;
                    reg_string += match[i];
                }
            }
            Regex reg = new Regex("^"+reg_string, RegexOptions.IgnoreCase);
            Regex reg2 = new Regex("^.*" + reg_string + ".*", RegexOptions.IgnoreCase);
            List<string> ve = new List<string>();
            int cnt = 0;
            foreach (var x in prefix.data)
            {
                if (reg.IsMatch(x.Key))
                {
                    ve.Add(x.Key + "-");
                    if (++cnt == 25) break;
                }
                else if (noteng && reg2.IsMatch(x.Value))
                {
                        ve.Add(x.Key + "-");
                        if (++cnt == 25) break;
                }
            }
            foreach (var x in root.data)
            {
                if (reg.IsMatch(x.Key))
                {
                    ve.Add("-"+x.Key + "-");
                    if (++cnt == 50) break;
                }
                else if (noteng && reg2.IsMatch(x.Value))
                {
                    ve.Add("-" + x.Key + "-");
                    if (++cnt == 50) break;
                }
            }
            foreach (var x in suffix.data)
            {
                if (reg.IsMatch(x.Key))
                {
                    ve.Add("-"+x.Key);
                    if (++cnt == 75) break;
                }
                else if (noteng && reg2.IsMatch(x.Value))
                {
                    ve.Add("-" + x.Key);
                    if (++cnt == 75) break;
                }
            }
            ve.Sort(delegate (string x, string y)
            {
                if (x[0] == '-' && y[0] == '-') return x.CompareTo(y);
                else if (x[0] == '-') return x.Substring(1).CompareTo(y);
                else if (y[0] == '-') return x.CompareTo(y.Substring(1));
                else return x.CompareTo(y);
            });
            if(ve.Count>25)
                ve.RemoveRange(25,ve.Count-25);
            return ve;
        }

        public static List<string> Show2(string s)
        {
            s = s.Trim();
            int len = s.Length;
            //s.ToLower();
            bool sp = false;
            for (int i = 0; i < len; i++)
                if ((s[i] < 'A' || s[i] > 'Z') && (s[i] < 'a' || s[i] > 'z')) {
                    sp = true;
                    s.Remove(i,1);
                    s.Insert(i," ");
                }
            if (sp)
            {
                var str = s.Split(' ');
                List<string> ret = new List<string>();
                foreach (var voc in str)
                    ret.Add(voc);
                return ret;
            }
            else if (words.val_ok(s) != null)
            {
                var str = words.val_ok(s).Split(' ');
                List<string> ret = new List<string>();
                int size = str.Length;
                if (size == 0) return ret;
                else if (size == 1)
                {
                    ret.Add(str[0]);
                    return ret;
                }
                else {
                    for (int i = 0; i < size; i++)
                        if (i == 0 && prefix.exists(str[i]))
                            ret.Add(str[i] + "-");
                        else if (i == size - 1 && suffix.exists(str[i]))
                            ret.Add("-" + str[i]);
                        else if (root.exists(str[i]))
                            ret.Add("-" + str[i] + "-");
                        else if (words.exists(str[i]))
                            ret.Add(str[i]);
                        else if (words.exists(str[i] + "e"))
                            ret.Add(str[i]);
                        else if (prefix.exists(str[i]))
                            ret.Add(str[i] + "-");
                        else if (suffix.exists(str[i]))
                            ret.Add("-" + str[i]);
                        else
                            ret.Add(str[i]);
                    return ret;
                }
            }
            else
            {

                List<string> ret = new List<string>();
                if (s.Length >= 5 && s.Substring(s.Length - 3, 3) == "ing") {
                    if (words.exists(s.Substring(0, s.Length - 3) + "e"))
                    {
                        ret.Add(s.Substring(0, s.Length - 3));
                        ret.Add("-ing");
                        return ret;
                    }
                    else if (words.exists(s.Substring(0, s.Length - 3)))
                    {
                        ret.Add(s.Substring(0, s.Length - 3));
                        ret.Add("-ing");
                        return ret;
                    }
                    else if (s.Length >= 6 && words.exists(s.Substring(0, s.Length - 4)))
                    {
                        ret.Add(s.Substring(0, s.Length - 4));
                        ret.Add(s.Substring(s.Length - 4, 1));
                        ret.Add("-ing");
                        return ret;
                    }
                }
                int[,] dp = new int[len, len];
                string[,] dps = new string[len, len];
                for (int i = 0; i < len; i++)
                    for (int j = i; j < len; j++) {
                        string now = s.Substring(i, j - i + 1);
                        dp[i, j] = j - i - 1;
                        dps[i, j] = now;
                        if (i == 0 && j == len - 1)
                        {
                            dp[i, j] = j - i + Math.Max(17 / (j - i + 1), 1);
                            dps[i, j] = now;
                            continue;
                        }
                        if ((i != 0 || j != len - 1) && prefix.exists(now))
                        {
                            int score = i == 0 ? (j - i) * 2 + 1 : (j - i - 1) * 2;
                            if (dp[i, j] < score) {
                                dp[i, j] = score;
                                dps[i, j] = now + "-";
                            }
                        }
                        if ((i != 0 || j != len - 1) && suffix.exists(now))
                        {
                            int score = j == len - 1 ? (j - i) * 2 + 1 : (j - i - 1) * 2;
                            if (dp[i, j] < score) {
                                dp[i, j] = score;
                                dps[i, j] = "-" + now;
                            }
                        }
                        if ((i != 0 || j != len - 1) && root.exists(now))
                        {
                            int score = (j - i) * 2;
                            if (dp[i, j] < score) {
                                dp[i, j] = score;
                                dps[i, j] = "-" + now + "-";
                            }
                        }
                        if ((i != 0 || j != len - 1) && (j - i >= 3 || (j - i == 2 && (j == len - 1 || i == 0))) && words.exists(now))
                        {
                            int score = (j - i) * 2 ;
                            if (dp[i, j] < score) {
                                dp[i, j] = score;
                                dps[i, j] = now;
                            }
                        }
                        if ((i != 0 || j != len - 2) && (j - i >= 3 || (j - i == 2 && (j == len - 1 || i == 0))) && words.exists(now + "e"))
                        {
                            int score = (j - i) * 2 - 1;
                            if (dp[i, j] < score) {
                                dp[i, j] = score;
                                dps[i, j] = now;
                            }
                        }
                        if ((i != 0 || j != len - 1) && (j - i >= 3 || (j - i == 2 && (j == len - 1 || i == 0))) && now[now.Length-1] == 'i' && words.exists(now.Substring(0, now.Length - 1) + "y"))
                        {
                            int score = (j - i) * 2 - 1;
                            if (dp[i, j] < score) {
                                dp[i, j] = score;
                                dps[i,j] = now;
                            }
                        }
                    }
                int[] d = new int[len];
                int[] f = new int[len];
                /*string ot="";
                for (int i = 0; i < len; i++) {
                    for (int j = 0; j < len; j++)
				        ot += dp[i,j].ToString() + ' ';
                    ot+= '\n';
                }*/
                for (int i = 0; i < len; i++)
                {
                    d[i] = dp[0, i];
                    f[i] = -1;
                    for (int j = 0; j < i; j++)
                        if (d[i] <= d[j] + dp[j + 1, i]) {
                            d[i] = d[j] + dp[j + 1, i];
                            f[i] = j;
                        }
                }
                int nw = len - 1;
                while (nw != -1) {
                    ret.Add(dps[f[nw] + 1, nw]);
                    nw = f[nw];
                }
                ret.Reverse();
                return ret;
            }
        }
        public static List<Tuple<string, string>> Show(string s)
        {
            var ve = Show2(s);
            List<Tuple<string, string>> ret=new List<Tuple<string, string>>();
            foreach (var x in ve)
            {
                int len = x.Length;
                if (x[0] == '-' || x[len - 1] == '-')
                {
                    string now = x;
                    if (now[0] == '-')
                        now = now.Substring(1);
                    if (now[now.Length - 1] == '-')
                        now = now.Substring(0, now.Length - 1);
                    ret.Add(Tuple.Create(GetRootExp(x),now));
                }
                else
                {
                    var now = x;
                    ret.Add(Tuple.Create(WordRotToExp(now).Item1, now));
                }
            }
            return ret;
        }
        public static Tuple<string, string> WordRotToExp(string now)
        {
            string nw= now.Substring(0, now.Length - 1) + "y";
            if (words.exists(now))
                return (Tuple.Create(words.val(now), now));
            else if (now.Length > 2 && words.exists(now + "e"))
                return (Tuple.Create(words.val(now + "e"), now + "e"));
            else if (now.Length > 2 && now[now.Length-1] == 'i' && words.exists(nw))
		        return (Tuple.Create(words.val(nw), nw));
            else
                return (Tuple.Create(now, now));
        }
        public static Tuple<string, List<int>> GetExp(string s)
        {
            var pos = s.IndexOf(",");
            s.IndexOf(",");
            if (pos == -1) return Tuple.Create(s, new List<int>());
            var str=s.Substring(pos + 1).Split(' ');
            List<int> ve=new List<int>();
            foreach (var i in str)
                ve.Add(Convert.ToInt32(i));
            s = s.Substring(0, pos);
            if (s.Length > 1 && s[0] == '=' && words.exists(s.Substring(1).Trim()))
                return Tuple.Create(GetExpSimple(words.val(s.Substring(1).Trim())), ve);

            return Tuple.Create(s, ve);
        }
        public static string GetExpSimple(string s, int cnt = 0)
        {
            var pos = s.IndexOf(",");
            if (pos == -1) return s;
            s = s.Substring(0, pos);
            if (cnt < 20 && s.Length > 1 && s[0] == '=' && words.exists(s.Substring(1).Trim()))
                return GetExpSimple(words.val(s.Substring(1).Trim()), cnt + 1);
            return s;
        }
        public static string GetExpSimpleOrg(string s)
        {
            var pos = s.IndexOf(",");
            if (pos == -1) return s;
            return s.Substring(0, pos);
        }
        public static List<string> CutExp(string s)
        {
            s = GetExp(s).Item1;
            string s2 = s;
            List<string> ve=new List<string>();
            int pos;
            while ((pos = s.IndexOf("/")) != -1)
            {
                if (pos!=0)
                    ve.Add((s.Substring(0, pos)).Trim());
                while (pos + 1 < s.Length && s[pos + 1] == '/') pos++;
                if (pos + 1 != s.Length)
                    s = s.Substring(pos + 1);
                else
                    s = "";
            }
            if (s != "")
		        ve.Add(s.Trim());
            if (ve.Count == 1 && s[0] == '[')
            {
                s = s2;
                ve.Clear();
                while (s.Length > 1 && (pos = s.IndexOf("[", 1)) != -1)
                {
                    ve.Add((s.Substring(0, pos)).Trim());
                    s = s.Substring(pos);
                }
                if (s != "")
			        ve.Add(s.Trim());
                return ve;
            }
            return ve;
        }

        public static string MakeExp(Tuple<string, List<int>> p)
        {
            string ret=(p.Item1 + ",");
            bool nf = false;
            foreach (var  x in p.Item2)
            {
                if (nf)
                    ret += " ";
                nf = true;
                ret += x.ToString();
            }
            return ret;
        }
        public static string MergeExp(List<string> v)
        {
            string ret="";
            bool nf = false;
            foreach (var x in v)
            {
                if (nf) ret += "/";
                nf = true;
                ret += x;
            }
            return ret;
        }
        public static string GetRootExp(string s, int cnt=0)
        {
            if (s == "")return "";
            if (s[0] == '-' & s[s.Length-1] == '-')
            {
                s = s.Substring(1, s.Length - 2);
                if (root.exists(s) )
                {
                    string exp = root.val(s);
                    if (cnt < 20 && exp.Length > 1 && exp[0] == '=' && root.exists(exp.Substring(1).Trim()))
                        return GetRootExp("-" + exp.Substring(1).Trim() + "-", cnt + 1);
                    return exp;
                }
                else
                    return "";
            }
            else if (s[0] == '-')
            {
                s = s.Substring(1, s.Length - 1);
                if (suffix.exists(s))
                {
                    string exp = suffix.val(s);
                    if (cnt < 20 && exp.Length > 1 && exp[0] == '=' && suffix.exists(exp.Substring(1).Trim()))
                        return GetRootExp("-" + exp.Substring(1).Trim(), cnt + 1);
                    return exp;
                }
                else
                    return "";
            }
            else if (s[s.Length-1] == '-')
            {
                s = s.Substring(0, s.Length - 1);
                if (prefix.exists(s))
                {
                    string exp = prefix.val(s);
                    if (cnt < 20 && exp.Length > 1 && exp[0] == '=' && prefix.exists((exp.Substring(1).Trim())))
                        return GetRootExp("-" + exp.Substring(1).Trim(), cnt + 1);
                    return exp;
                }
                else
                    return "";
            }
            else
            {
                return GetExpSimple(s);
            }
        }
        public static string GetRootExpOrg(string s)
        {
            if (s == "")return "";
            if (s[0] == '-' && s[s.Length-1] == '-')
            {
                s = s.Substring(1, s.Length - 2);
                if (root.exists(s) )
                {
                    return root.val(s);
                }
                else
                    return "";
            }
            else if (s[0] == '-')
            {
                s = s.Substring(1, s.Length - 1);
                if (suffix.exists(s))
                {
                    return suffix.val(s);
                }
                else
                    return "";
            }
            else if (s[s.Length-1] == '-')
            {
                s = s.Substring(0, s.Length - 1);
                if (prefix.exists(s))
                {
                    return prefix.val(s);
                }
                else
                    return "";
            }
            else
            {
                return GetExpSimpleOrg(s);
            }
        }
        public static StackLayout ExpStack(string s,int size=20)
        {
            var expst = new StackLayout { Orientation = StackOrientation.Vertical };
            s = s.Trim();
            string pre = "";
            bool iv = false;
            var stp = new StackLayout { Orientation = StackOrientation.Horizontal };
            foreach (var x in s)
                if (!iv && x == '[')
                {
                    pre = pre.Trim();
                    if (pre != "")
                    {
                        var tmp = new Label
                        {
                            Text = pre,
                            FontSize=size,
                            Margin = new Thickness(10, 0, 0, 0),
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center
                        };
                        stp.Children.Add(tmp);
                        expst.Children.Add(stp);
                        stp = new StackLayout { Orientation = StackOrientation.Horizontal };
                        pre = "";
                    }
                    iv = true;
                }
                else if (iv && x == ']')
                {
                    pre = pre.Trim();
                    var tmp = new Label
                    {
                        Text = pre,
                        FontSize = size,
                        LineBreakMode = LineBreakMode.WordWrap,
                        //FontAttributes = FontAttributes.Bold,
                        Margin = new Thickness(3, 2, 3, 2),
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                    };
                    var bor = new Frame
                    {
                        //MinimumWidthRequest = 50,
                        Margin = new Thickness(10, 0, 0, 4),
                        //MinimumWidthRequest = 600,
                        HasShadow = false,
                        OutlineColor = Color.Silver, 
                        Content = tmp,
                        Padding = 0
                    };
                    tmp.SizeChanged += (object sender, EventArgs e) => {
                        if (tmp.Width < 57)
                        {
                            tmp.WidthRequest = 57;
                            bor.IsVisible = false;
                            Task.Run(() => {
                                Device.BeginInvokeOnMainThread(() => {
                                    bor.IsVisible = true;
                                });
                            });
                        }
                    };
                    stp.Children.Add(bor);
                    pre = "";
                    iv = false;
                }
                else
                    pre += x;
            if (pre != "")
            {
                pre = pre.Trim();
                var tmp = new Label
                {
                    Text = pre,
                    //FontSize = 20;
                    FontSize = size,
                    Margin = new Thickness(10, 0, 0, 0),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };
                stp.Children.Add(tmp);
                expst.Children.Add(stp);
            }
            return expst;
        }

        public static async Task<string> GetAsync(string uri)
        {
            try
            {
                var req = HttpWebRequest.Create(uri);
                req.Method = "GET";
                using (WebResponse response = await req.GetResponseAsync())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new System.IO.StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
                
            }
            catch
            {
                Debug.WriteLine("Error!", "Get");
                return null;
            }
        }
        public static EeekSoft.Text.StringSearch ChtAC1 ;
        public static EeekSoft.Text.StringSearch ChtAC2 ;
        public static Dictionary<string, string> ChtDict1 = new Dictionary<string, string>();
        public static Dictionary<string, string> ChtDict2 = new Dictionary<string, string>();
        public static async Task InitAsync() {

            return;
            ChtAC1 = new EeekSoft.Text.StringSearch();
            ChtAC2 = new EeekSoft.Text.StringSearch();
            var fileService = DependencyService.Get<ISaveAndLoad>();
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var tmp = assembly.GetManifestResourceNames();
            Stream stream = assembly.GetManifestResourceStream("Cut.data.STPhrases.txt");
            string s;
            List<string> lis = new List<string>();
            using (var reader = new System.IO.StreamReader(stream))
            {
                while ((s = await reader.ReadLineAsync()) != null)
                {
                    var str = s.Split('	');
                    lis.Add(str[0]);
                    ChtDict1[str[0]] = str[1].Split(' ')[0];
                }
                ChtAC1.Keywords = lis.ToArray();
            }
            stream = assembly.GetManifestResourceStream("Cut.data.STCharacters.txt");
            lis = new List<string>();
            using (var reader = new System.IO.StreamReader(stream))
            {
                while ((s = await reader.ReadLineAsync()) != null)
                {
                    var str = s.Split('	');
                    lis.Add(str[0]);
                    ChtDict2[str[0]] = str[1].Split(' ')[0];
                }
                ChtAC2.Keywords = lis.ToArray();
            }
        }
        public static string s2t(string s)
        {
            var res1 = ChtAC1.FindAll(s);
            var res2 = ChtAC2.FindAll(s);
            string ret = "";
            int p1 = 0, p2 = 0;
            for (int i = 0; i < s.Length; i++) {
                while (p1 < res1.Count && res1[p1].Index < i) p1++;
                while (p2 < res2.Count && res2[p2].Index < i) p2++;
                if (p1 < res1.Count && res1[p1].Index == i)
                {
                    ret += ChtDict1[res1[p1].Keyword];
                    i += res1[p1].Keyword.Length-1;
                }
                else if (p2 < res2.Count && res2[p2].Index == i)
                {
                    ret += ChtDict2[res2[p2].Keyword];
                    i += res2[p2].Keyword.Length-1;
                }
                else
                    ret += s[i];
            }
            return ret;
        }

        public static int LevenshteinDistance(string s, string t)
        {
            if (s == t) return 0;
            int n = s.Length, m = t.Length;
            if (n == 0) return m;
            if (m == 0) return n;

            // create two work vectors of integer distances

            int[] v0 = new int[m + 1];
            int[] v1 = new int[m + 1];
            
            for (int i = 0; i <= m; i++)
                v0[i] = i;

            for (int i = 0; i < n; i++)
            {
                v1[0] = i + 1;
                for (int j = 0; j < m; j++)
                {
                    int cost = (s[i] == t[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(Math.Min(v1[j] + 1, v0[j + 1] + 1), v0[j] + cost);
                }
                for (int j = 0; j <= m; j++)
                    v0[j] = v1[j];
            }

            return v1[m];
        }

        static Dictionary<string, string> NAMED_ENTITIES = new Dictionary<string, string>(){
            { "AElig;", "Æ" },
            { "Aacute;", "Á" },
            { "Acirc;", "Â" },
            { "Agrave;", "À" },
            { "Alpha;", "Α" },
            { "Aring;", "Å" },
            { "Atilde;", "Ã" },
            { "Auml;", "Ä" },
            { "Beta;", "Β" },
            { "Ccedil;", "Ç" },
            { "Chi;", "Χ" },
            { "Dagger;", "‡" },
            { "Delta;", "Δ" },
            { "ETH;", "Ð" },
            { "Eacute;", "É" },
            { "Ecirc;", "Ê" },
            { "Egrave;", "È" },
            { "Epsilon;", "Ε" },
            { "Eta;", "Η" },
            { "Euml;", "Ë" },
            { "Gamma;", "Γ" },
            { "Iacute;", "Í" },
            { "Icirc;", "Î" },
            { "Igrave;", "Ì" },
            { "Iota;", "Ι" },
            { "Iuml;", "Ï" },
            { "Kappa;", "Κ" },
            { "Lambda;", "Λ" },
            { "Mu;", "Μ" },
            { "Ntilde;", "Ñ" },
            { "Nu;", "Ν" },
            { "OElig;", "Œ" },
            { "Oacute;", "Ó" },
            { "Ocirc;", "Ô" },
            { "Ograve;", "Ò" },
            { "Omega;", "Ω" },
            { "Omicron;", "Ο" },
            { "Oslash;", "Ø" },
            { "Otilde;", "Õ" },
            { "Ouml;", "Ö" },
            { "Phi;", "Φ" },
            { "Pi;", "Π" },
            { "Prime;", "″" },
            { "Psi;", "Ψ" },
            { "Rho;", "Ρ" },
            { "Scaron;", "Š" },
            { "Sigma;", "Σ" },
            { "THORN;", "Þ" },
            { "Tau;", "Τ" },
            { "Theta;", "Θ" },
            { "Uacute;", "Ú" },
            { "Ucirc;", "Û" },
            { "Ugrave;", "Ù" },
            { "Upsilon;", "Υ" },
            { "Uuml;", "Ü" },
            { "Xi;", "Ξ" },
            { "Yacute;", "Ý" },
            { "Yuml;", "Ÿ" },
            { "Zeta;", "Ζ" },
            { "aacute;", "á" },
            { "acirc;", "â" },
            { "acute;", "´" },
            { "aelig;", "æ" },
            { "agrave;", "à" },
            { "alefsym;", "ℵ" },
            { "alpha;", "α" },
            { "amp;", "&" },
            { "and;", "∧" },
            { "ang;", "∠" },
            { "apos;", "'" },
            { "aring;", "å" },
            { "asymp;", "≈" },
            { "atilde;", "ã" },
            { "auml;", "ä" },
            { "bdquo;", "„" },
            { "beta;", "β" },
            { "brvbar;", "¦" },
            { "bull;", "•" },
            { "cap;", "∩" },
            { "ccedil;", "ç" },
            { "cedil;", "¸" },
            { "cent;", "¢" },
            { "chi;", "χ" },
            { "circ;", "ˆ" },
            { "clubs;", "♣" },
            { "cong;", "≅" },
            { "copy;", "©" },
            { "crarr;", "↵" },
            { "cup;", "∪" },
            { "curren;", "¤" },
            { "dArr;", "⇓" },
            { "dagger;", "†" },
            { "darr;", "↓" },
            { "deg;", "°" },
            { "delta;", "δ" },
            { "diams;", "♦" },
            { "divide;", "÷" },
            { "eacute;", "é" },
            { "ecirc;", "ê" },
            { "egrave;", "è" },
            { "empty;", "∅" },
            { "epsilon;", "ε" },
            { "equiv;", "≡" },
            { "eta;", "η" },
            { "eth;", "ð" },
            { "euml;", "ë" },
            { "euro;", "€" },
            { "exist;", "∃" },
            { "fnof;", "ƒ" },
            { "forall;", "∀" },
            { "frac12;", "½" },
            { "frac14;", "¼" },
            { "frac34;", "¾" },
            { "frasl;", "⁄" },
            { "gamma;", "γ" },
            { "ge;", "≥" },
            { "gt;", ">" },
            { "hArr;", "⇔" },
            { "harr;", "↔" },
            { "hearts;", "♥" },
            { "hellip;", "…" },
            { "iacute;", "í" },
            { "icirc;", "î" },
            { "iexcl;", "¡" },
            { "igrave;", "ì" },
            { "image;", "ℑ" },
            { "infin;", "∞" },
            { "int;", "∫" },
            { "iota;", "ι" },
            { "iquest;", "¿" },
            { "isin;", "∈" },
            { "iuml;", "ï" },
            { "kappa;", "κ" },
            { "lArr;", "⇐" },
            { "lambda;", "λ" },
            { "lang;", "〈" },
            { "laquo;", "«" },
            { "larr;", "←" },
            { "lceil;", "⌈" },
            { "ldquo;", "“" },
            { "le;", "≤" },
            { "lfloor;", "⌊" },
            { "lowast;", "∗" },
            { "loz;", "◊" },
            { "lsaquo;", "‹" },
            { "lsquo;", "‘" },
            { "lt;", "<" },
            { "macr;", "¯" },
            { "mdash;", "—" },
            { "micro;", "µ" },
            { "middot;", "·" },
            { "minus;", "−" },
            { "mu;", "μ" },
            { "nabla;", "∇" },
            { "ndash;", "–" },
            { "ne;", "≠" },
            { "ni;", "∋" },
            { "not;", "¬" },
            { "notin;", "∉" },
            { "nsub;", "⊄" },
            { "ntilde;", "ñ" },
            { "nu;", "ν" },
            { "oacute;", "ó" },
            { "ocirc;", "ô" },
            { "oelig;", "œ" },
            { "ograve;", "ò" },
            { "oline;", "‾" },
            { "omega;", "ω" },
            { "omicron;", "ο" },
            { "oplus;", "⊕" },
            { "or;", "∨" },
            { "ordf;", "ª" },
            { "ordm;", "º" },
            { "oslash;", "ø" },
            { "otilde;", "õ" },
            { "otimes;", "⊗" },
            { "ouml;", "ö" },
            { "para;", "¶" },
            { "part;", "∂" },
            { "permil;", "‰" },
            { "perp;", "⊥" },
            { "phi;", "φ" },
            { "pi;", "π" },
            { "piv;", "ϖ" },
            { "plusmn;", "±" },
            { "pound;", "£" },
            { "prime;", "′" },
            { "prod;", "∏" },
            { "prop;", "∝" },
            { "psi;", "ψ" },
            { "quot;", "\"" },
            { "rArr;", "⇒" },
            { "radic;", "√" },
            { "rang;", "〉" },
            { "raquo;", "»" },
            { "rarr;", "→" },
            { "rceil;", "⌉" },
            { "rdquo;", "”" },
            { "real;", "ℜ" },
            { "reg;", "®" },
            { "rfloor;", "⌋" },
            { "rho;", "ρ" },
            { "rsaquo;", "›" },
            { "rsquo;", "’" },
            { "sbquo;", "‚" },
            { "scaron;", "š" },
            { "sdot;", "⋅" },
            { "sect;", "§" },
            { "sigma;", "σ" },
            { "sigmaf;", "ς" },
            { "sim;", "∼" },
            { "spades;", "♠" },
            { "sub;", "⊂" },
            { "sube;", "⊆" },
            { "sum;", "∑" },
            { "sup1;", "¹" },
            { "sup2;", "²" },
            { "sup3;", "³" },
            { "sup;", "⊃" },
            { "supe;", "⊇" },
            { "szlig;", "ß" },
            { "tau;", "τ" },
            { "there4;", "∴" },
            { "theta;", "θ" },
            { "thetasym;", "ϑ" },
            { "thorn;", "þ" },
            { "tilde;", "˜" },
            { "times;", "×" },
            { "trade;", "™" },
            { "uArr;", "⇑" },
            { "uacute;", "ú" },
            { "uarr;", "↑" },
            { "ucirc;", "û" },
            { "ugrave;", "ù" },
            { "uml;", "¨" },
            { "upsih;", "ϒ" },
            { "upsilon;", "υ" },
            { "uuml;", "ü" },
            { "weierp;", "℘" },
            { "xi;", "ξ" },
            { "yacute;", "ý" },
            { "yen;", "¥" },
            { "yuml;", "ÿ" },
            { "zeta;", "ζ" }
        };
        public static string html_decode(string input)
        {
            string ret = "";
            int len = input.Length;
            for (int i = 0; i < len; i++)
            {
                if (input[i] == '&')
                {
                    if (i == len - 1)
                        ret += '&';
                    else if (input[i + 1] == '#')
                    {
                        int pos = input.IndexOf(';', i);
                        if (pos == -1)
                            ret += '&';
                        else
                        {
                            string s = input.Substring(i + 2, pos - i - 2);
                            ret += (Char)int.Parse(s);
                            i += s.Length + 2;
                        }
                    }
                    else
                    {
                        int pos = input.IndexOf(';', i);
                        if (pos == -1)
                            ret += '&';
                        else
                        {
                            string s = input.Substring(i + 1, pos - i);
                            if (NAMED_ENTITIES.ContainsKey(s))
                            {
                                ret += NAMED_ENTITIES[s];
                                i += s.Length;
                            }
                            else
                                ret += '&';
                        }
                    }

                }
                else
                    ret += input[i];
            }
            return ret;
        }

        public static double rad(double d)
        {
            return d * Math.PI / 180.0;
        }

        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            var radLat1 = rad(lat1);
            var radLat2 = rad(lat2);
            var a = radLat1 - radLat2;
            var b = rad(lng1) - rad(lng2);
            var s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
            Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * 6378.137;// EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }
    }
}
