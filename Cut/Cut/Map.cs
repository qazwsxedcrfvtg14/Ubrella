using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Cut
{
    public class Maps
    {
        //public SortedDictionary<string, string> data = new SortedDictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        //public SortedDictionary<string, string> ok = new SortedDictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        public Trie<string> data = new Trie<string>();
        public Trie<string> ok = new Trie<string>();
        public string file_name;
        ISaveAndLoad fileService = DependencyService.Get<ISaveAndLoad>();
        public void add(string key, string value = "")
        {
            data.add(key,value);
            //data[key] = value;
            if (file_name != null) {
                if(exists_ok(key))
                    fileService.AppendTextAsync(file_name, "*" + val_ok(key) + "," + val(key) + "\n");
                else
                    fileService.AppendTextAsync(file_name, key + "," + val(key) + "\n");
            }
        }
        public void add_ok(string key, string value = "")
        {
            ok.add(key, value);
            //ok[key] = value;
            if (file_name != null)
            {
                if (exists_ok(key))
                    fileService.AppendTextAsync(file_name, "*" + val_ok(key) + "," + val(key) + "\n");
                else
                    fileService.AppendTextAsync(file_name, key + "," + val(key) + "\n");
            }
        }
        public void remove(string key)
        {
            data.remove(key);
            data.remove(key);
            //data.Remove(key);
            //ok.Remove(key);
            if (file_name != null)
            {
                    fileService.AppendTextAsync(file_name, "$" + key + "\n");
            }
        }
        public bool exists(string key)
        {
            return data.exists(key);
            /*string value;
            if (data.TryGetValue(key, out value)) return true;
            else return false;*/
        }
        public bool exists_ok(string key)
        {
            return ok.exists(key);
            /*string value;
            if (ok.TryGetValue(key, out value)) return true;
            else return false;*/
        }
        public string val(string key)
        {
            var res = data.find(key);
            if (res == null) return null;
            return res.val.Value;
            /*string value;
            if (data.TryGetValue(key, out value)) return value;
            else return null;*/
        }
        public string this[string i]
        {
            get { return val(i); }
            set { add(i, value); }
        }
        public string val_ok(string key)
        {
            var res=ok.find(key);
            if (res == null) return null;
            return res.val.Value;
            /*string value;
            if (ok.TryGetValue(key, out value)) return value;
            else return null;*/
        }
        public void clear()
        {
            data=new Trie<string>();
            ok=new Trie<string>();
        }
    }
}
