using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Database;
using Oxide.Core.MySql.Libraries;
using Oxide.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using WishInfrastructure.Models;

namespace WishInfrastructure
{
    public class DatabaseClient
    {
        private string _tableName;
        private readonly RustPlugin _plugin;
        private readonly DatabaseConfig _databaseConfig;
        MySql SqlLib = Interface.GetMod().GetLibrary<MySql>();
        Connection Sql_conn;

        private List<string> _sqlColumns = new List<string>();
        private List<string> _changedPlayersData = new List<string>();

        Dictionary<string, Hash<string, dynamic>> _sqlData = new Dictionary<string, Hash<string, dynamic>>();

        public DatabaseClient(string tableName, RustPlugin plugin, DatabaseConfig databaseConfig)
        {
            _tableName = tableName;
            _plugin = plugin;
            _databaseConfig = databaseConfig;
        }
        void SetupDatabase()
        {
            LoadData();
        }
        void LoadPlayers()
        {
            foreach (string userid in KnownPlayers())
            {
                try
                {
                    LoadPlayer(userid);
                }
                catch
                {
                    Interface.Oxide.LogWarning("Couldn't load " + userid);
                }
            }
        }
        void LoadPlayer(string userid)
        {
            try
            {
                if (!_sqlData.ContainsKey(userid)) _sqlData.Add(userid, new Hash<string, dynamic>());
                bool newplayer = true;
                SqlLib.Query(Sql.Builder.Append($"SELECT * from {_tableName} WHERE `userid` = '{userid}'"), Sql_conn, list =>
                {
                    if (list != null)
                    {
                        foreach (var entry in list)
                        {
                            foreach (var p in entry)
                            {
                                if (p.Value is string)
                                {
                                    _sqlData[userid][p.Key] = (string)p.Value;
                                }
                                else if (p.Value is int)
                                {
                                    _sqlData[userid][p.Key] = (int)p.Value;

                                }
                            }
                            newplayer = false;
                        }
                    }
                    if (newplayer)
                    {
                        _sqlData[userid]["userid"] = userid;
                        SqlLib.Insert(Sql.Builder.Append($"INSERT IGNORE INTO {_tableName} ( userid ) VALUES ( {userid} )"), Sql_conn);

                        _changedPlayersData.Add(userid);
                    }
                });
            }
            catch (Exception e)
            {
                Interface.Oxide.LogError(string.Format("Loading {0} got this error: {1}", userid, e.Message));
            }
        }
        void SetPlayerData(string userid, string key, string data)
        {
            if (!IsKnownPlayer(userid)) LoadPlayer(userid);

            SqlLib.Insert(Sql.Builder.Append($"ALTER TABLE `{_tableName}` ADD `{key}` LONGTEXT"), Sql_conn);
            _sqlColumns.Add(key);

            _sqlData[userid][key] = data.ToString();

            if (!_changedPlayersData.Contains(userid))
                _changedPlayersData.Add(userid);

        }
        void SetPlayerData(string userid, string key, int data)
        {
            if (!IsKnownPlayer(userid)) LoadPlayer(userid);



            SqlLib.Insert(Sql.Builder.Append($"ALTER TABLE `{_tableName}` ADD `{key}` INT"), Sql_conn);
            _sqlColumns.Add(key);

            _sqlData[userid][key] = data;

            if (!_changedPlayersData.Contains(userid))
                _changedPlayersData.Add(userid);

        }

        void SetPlayerDataSerialized<T>(string userid, string key, T data)
        {
            string serilized = JsonConvert.SerializeObject(data);
            SetPlayerData(userid, key, serilized);
        }

        void LoadData()
        {
            try
            {
                Sql_conn = SqlLib.OpenDb(_databaseConfig.sql_host,
                    _databaseConfig.sql_port,
                    _databaseConfig.sql_db,
                    _databaseConfig.sql_user,
                    _databaseConfig.sql_pass,
                    _plugin);

                if (Sql_conn == null || Sql_conn.Con == null)
                {
                    FatalError("Couldn't open the SQLite PlayerDatabase: " + Sql_conn.Con.State.ToString());
                    return;
                }
                SqlLib.Insert(Sql.Builder.Append("SET NAMES utf8mb4"), Sql_conn);
                SqlLib.Insert(Sql.Builder.Append($"CREATE TABLE IF NOT EXISTS {_tableName} ( `id` int(11) NOT NULL, `userid` VARCHAR(17) NOT NULL );"), Sql_conn);
                SqlLib.Query(Sql.Builder.Append($"desc {_tableName};"), Sql_conn, list =>
                {
                    if (list == null)
                    {
                        FatalError("Couldn't get columns. Database might be corrupted.");
                        return;
                    }
                    foreach (var entry in list)
                    {
                        _sqlColumns.Add((string)entry["Field"]);
                    }

                });
                SqlLib.Query(Sql.Builder.Append($"SELECT userid from {_tableName}"), Sql_conn, list =>
                {
                    if (list == null) return;
                    foreach (var entry in list)
                    {
                        string steamid = (string)entry["userid"];
                        if (steamid != "0")
                            _sqlData.Add(steamid, new Hash<string, dynamic>());
                    }
                    LoadPlayers();
                });
                Interface.Oxide.LogInfo($"Loading database for {_tableName}");
            }
            catch (Exception e)
            {
                FatalError(e.Message);
            }
        }
        void SavePlayerDatabase()
        {
            foreach (string userid in _changedPlayersData)
            {
                try
                {
                    var values = _sqlData[userid];

                    string arg = string.Empty;
                    var parms = new List<object>();
                    foreach (var c in values)
                    {
                        arg += string.Format("{0}`{1}` = @{2}", arg == string.Empty ? string.Empty : ",", c.Key, parms.Count.ToString());
                        parms.Add(c.Value);
                    }

                    SqlLib.Insert(Sql.Builder.Append($"UPDATE {_tableName} SET {arg} WHERE userid = {userid}", parms.ToArray()), Sql_conn);

                }
                catch (Exception e)
                {
                    Interface.Oxide.LogWarning(e.Message);
                }
            }
            _changedPlayersData.Clear();
        }

        dynamic GetPlayerDataRaw(string userid, string key)
        {
            if (!IsKnownPlayer(userid)) return null;

            if (!_sqlColumns.Contains(key)) return null;
            if (_sqlData[userid] == null) return null;
            if (_sqlData[userid][key] == null) return null;
            return _sqlData[userid][key];
        }

        T GetPlayerDataDeserialized<T>(string userid, string key)
        {
            if (!IsKnownPlayer(userid)) return default(T);

            if (!_sqlColumns.Contains(key)) return default(T);
            if (_sqlData[userid] == null) return default(T);
            if (_sqlData[userid][key] == null) return default(T);
            return JsonConvert.DeserializeObject<T>(_sqlData[userid][key]);
        }

        public List<string> KnownPlayers()
        {
            return _sqlData.Keys.ToList();
        }

        private bool IsKnownPlayer(string userid)
        {
            return _sqlData.ContainsKey(userid);
        }

        void FatalError(string msg)
        {
            Interface.Oxide.LogError(msg);
        }
    }
}
