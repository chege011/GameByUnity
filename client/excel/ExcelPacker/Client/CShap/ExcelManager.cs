#define _CLIENT_DEFAULT_LOADER_

using System.Collections.Generic;
using System;
using System.IO;

static public class ExcelManager
{
    public delegate Stream ExcelLoader(string name);
#if _CLIENT_DEFAULT_LOADER_
    static private ExcelLoader loader = delegate (string name)
    {
        UnityEngine.TextAsset asset = UnityEngine.Resources.Load<UnityEngine.TextAsset>(name);
        if (asset != null)
            return new System.IO.MemoryStream(asset.bytes);
        return null;
    };
#else
    static private ExcelLoader loader;
#endif
    static public void InitLoader(ExcelLoader l)
    {
        loader = l;
    }
    static private bool _checkLoader()
    {
        if (loader == null)
        {
            throw new System.Exception("Call InitLoader To Set Loader!");
        }
        return true;
    }

	static private List<_footMan_client_._Human_> _footMan_Human_list_;
    static public List<_footMan_client_._Human_> footMan_Human_list 
    { 
        private set
        {
            _footMan_Human_list_ = value; 
        }
        get {return _footMan_Human_list_;} 
    }
	static private Dictionary<uint, _footMan_client_._Human_> _footMan_Human_;
    static public Dictionary<uint, _footMan_client_._Human_> footMan_Human 
    { 
        private set
        {
            _footMan_Human_ = value; 
        }
        get {return _footMan_Human_;} 
    }
	static private List<_footMan_client_._Demon_> _footMan_Demon_list_;
    static public List<_footMan_client_._Demon_> footMan_Demon_list 
    { 
        private set
        {
            _footMan_Demon_list_ = value; 
        }
        get {return _footMan_Demon_list_;} 
    }
	static private Dictionary<uint, _footMan_client_._Demon_> _footMan_Demon_;
    static public Dictionary<uint, _footMan_client_._Demon_> footMan_Demon 
    { 
        private set
        {
            _footMan_Demon_ = value; 
        }
        get {return _footMan_Demon_;} 
    }

    static private bool Load_footMan()
    {
        Stream s = loader("Data/footMan");
        if (s != null)
        {
             _footMan_client_._Excel_ excel = ProtoBuf.Serializer.Deserialize<_footMan_client_._Excel_>(s);
             if (excel != null)
             {
				footMan_Human_list = excel.HumanData;
				footMan_Human = new Dictionary<uint, _footMan_client_._Human_>();
				foreach (_footMan_client_._Human_ item in excel.HumanData)
				{
					if (footMan_Human.ContainsKey(item.id)) continue;
					footMan_Human.Add(item.id, item);
				}
				footMan_Demon_list = excel.DemonData;
				footMan_Demon = new Dictionary<uint, _footMan_client_._Demon_>();
				foreach (_footMan_client_._Demon_ item in excel.DemonData)
				{
					if (footMan_Demon.ContainsKey(item.id)) continue;
					footMan_Demon.Add(item.id, item);
				}

                return true;
            }
        }
        return false;
    }
    static public void LoadAll()
    {
        if (_checkLoader())
        {
			Load_footMan();
        }
    }

    static public System.Collections.IEnumerator LoadAll_Enum()
    {
        yield return LoadAll_Enum(null);
    }
    static public System.Collections.IEnumerator LoadAll_Enum(Action<float> progress)
    {
        if (_checkLoader())
        {
			Load_footMan();
            if (progress != null)
                progress.Invoke(1f);
            yield return null;

        }
    }

    static public void Unload()
    {
    
		footMan_Human_list.Clear();
		footMan_Human_list = null;
		footMan_Human.Clear();
		footMan_Human = null;
		footMan_Demon_list.Clear();
		footMan_Demon_list = null;
		footMan_Demon.Clear();
		footMan_Demon = null;
	}
}