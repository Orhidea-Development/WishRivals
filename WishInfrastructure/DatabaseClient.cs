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
        private string _playerTableName;
        private string _clanTableName;
        private readonly RustPlugin _plugin;
        private readonly DatabaseConfig _databaseConfig;
        MySql SqlLib = Interface.GetMod().GetLibrary<MySql>();
        Connection Sql_conn;

        private List<string> _playerColumns = new List<string>();
        private List<string> _clanColumns = new List<string>();
        private List<string> _changedPlayersData = new List<string>();
        private List<string> _changedClansData = new List<string>();


        Dictionary<string, Hash<string, object>> _playerData = new Dictionary<string, Hash<string, object>>();
        Dictionary<string, Hash<string, object>> _clanData = new Dictionary<string, Hash<string, object>>();


        public DatabaseClient(string playerTableName, string clanTableName, RustPlugin plugin, DatabaseConfig databaseConfig)
        {
            _playerTableName = playerTableName;
            _clanTableName = clanTableName;
            _plugin = plugin;
            _databaseConfig = databaseConfig;
        }
        public void SetupDatabase()
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
        void LoadClans()
        {
            foreach (string clanid in KnownClans())
            {
                try
                {
                    LoadClan(clanid);
                }
                catch
                {
                    Interface.Oxide.LogWarning("Couldn't load " + clanid);
                }
            }
        }

        private void LoadClan(string clanid)
        {
            try
            {
                if (!_clanData.ContainsKey(clanid)) _clanData.Add(clanid, new Hash<string, object>());
                bool newplayer = true;
                SqlLib.Query(Sql.Builder.Append($"SELECT * from {_clanTableName} WHERE `clanid` = '{clanid}'"), Sql_conn, list =>
                {
                    if (list != null)
                    {
                        foreach (var entry in list)
                        {
                            foreach (var p in entry)
                            {
                                if (p.Value is string)
                                {
                                    _clanData[clanid][p.Key] = (string)p.Value;
                                }
                                else if (p.Value is int)
                                {
                                    _clanData[clanid][p.Key] = (int)p.Value;

                                }
                            }
                            newplayer = false;
                        }
                    }
                    if (newplayer)
                    {
                        _clanData[clanid]["clanid"] = clanid;
                        SqlLib.Insert(Sql.Builder.Append($"INSERT IGNORE INTO {_clanTableName} ( clanid ) VALUES ( {clanid} )"), Sql_conn);

                        _changedClansData.Add(clanid);
                    }
                });
            }
            catch (Exception e)
            {
                Interface.Oxide.LogError(string.Format("Loading {0} got this error: {1}", clanid, e.Message));
            }
        }

        public void LoadPlayer(string userid)
        {
            try
            {
                if (!_playerData.ContainsKey(userid)) _playerData.Add(userid, new Hash<string, object>());
                bool newplayer = true;
                SqlLib.Query(Sql.Builder.Append($"SELECT * from {_playerTableName} WHERE `userid` = '{userid}'"), Sql_conn, list =>
                {
                    if (list != null)
                    {
                        foreach (var entry in list)
                        {
                            foreach (var p in entry)
                            {
                                if (p.Value is string)
                                {
                                    _playerData[userid][p.Key] = (string)p.Value;
                                }
                                else if (p.Value is int)
                                {
                                    _playerData[userid][p.Key] = (int)p.Value;

                                }
                            }
                            newplayer = false;
                        }
                    }
                    if (newplayer)
                    {
                        _playerData[userid]["userid"] = userid;
                        SqlLib.Insert(Sql.Builder.Append($"INSERT IGNORE INTO {_playerTableName} ( userid ) VALUES ( {userid} )"), Sql_conn);

                        _changedPlayersData.Add(userid);
                    }
                });
            }
            catch (Exception e)
            {
                Interface.Oxide.LogError(string.Format("Loading {0} got this error: {1}", userid, e.Message));
            }
        }
        public void SetPlayerData(string userid, string key, string data)
        {
            if (!IsKnownPlayer(userid)) LoadPlayer(userid);

            if (!(_playerColumns.Contains(key)))
            {
                SqlLib.Insert(Sql.Builder.Append($"ALTER TABLE `{_playerTableName}` ADD `{key}` LONGTEXT"), Sql_conn);
                _playerColumns.Add(key);
            }

            _playerData[userid][key] = data.ToString();

            if (!_changedPlayersData.Contains(userid))
                _changedPlayersData.Add(userid);

        }
        public void SetPlayerData(string userid, string key, int data)
        {
            if (!IsKnownPlayer(userid)) LoadPlayer(userid);


            if (!(_playerColumns.Contains(key)))
            {
                SqlLib.Insert(Sql.Builder.Append($"ALTER TABLE `{_playerTableName}` ADD `{key}` INT"), Sql_conn);
                _playerColumns.Add(key);
            }

            _playerData[userid][key] = data;

            if (!_changedPlayersData.Contains(userid))
                _changedPlayersData.Add(userid);

        }

        public void SetPlayerDataSerialized<T>(string clanid, string key, T data)
        {
            string serilized = JsonConvert.SerializeObject(data);
            SetPlayerData(clanid, key, serilized);
        }
        public void SetClanData(string clanid, string key, string data)
        {
            if (!IsKnownClan(clanid)) LoadClan(clanid);

            if (!(_clanColumns.Contains(key)))
            {
                SqlLib.Insert(Sql.Builder.Append($"ALTER TABLE `{_clanTableName}` ADD `{key}` LONGTEXT"), Sql_conn);
                _clanColumns.Add(key);
            }

            _clanData[clanid][key] = data.ToString();

            if (!_changedClansData.Contains(clanid))
                _changedClansData.Add(clanid);

        }
        public void SetClanData(string clanid, string key, int data)
        {
            if (!IsKnownClan(clanid)) LoadClan(clanid);


            if (!(_clanColumns.Contains(key)))
            {
                SqlLib.Insert(Sql.Builder.Append($"ALTER TABLE `{_clanTableName}` ADD `{key}` INT"), Sql_conn);
                _clanColumns.Add(key);
            }

            _clanData[clanid][key] = data;

            if (!_changedClansData.Contains(clanid))
                _changedClansData.Add(clanid);

        }

        public void SetClanDataSerialized<T>(string clanid, string key, T data)
        {
            string serilized = JsonConvert.SerializeObject(data);
            SetClanData(clanid, key, serilized);
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
                InitPlayers();
                InitClans();
                Interface.Oxide.LogInfo($"Loading database for {_playerTableName} and {_clanTableName}");
            }
            catch (Exception e)
            {
                FatalError(e.Message);
            }
        }

        private void InitClans()
        {
            SqlLib.Insert(Sql.Builder.Append("SET NAMES utf8mb4"), Sql_conn);
            SqlLib.Insert(Sql.Builder.Append($"CREATE TABLE IF NOT EXISTS {_clanTableName} ( `clanid` VARCHAR(30) NOT NULL );"), Sql_conn);
            SqlLib.Query(Sql.Builder.Append($"desc {_clanTableName};"), Sql_conn, list =>
            {
                if (list == null)
                {
                    FatalError("Couldn't get columns. Database might be corrupted.");
                    return;
                }
                foreach (var entry in list)
                {
                    _clanColumns.Add((string)entry["Field"]);
                }

            });
            SqlLib.Query(Sql.Builder.Append($"SELECT clanid from {_clanTableName}"), Sql_conn, list =>
            {
                if (list == null) return;
                foreach (var entry in list)
                {
                    string clanid = (string)entry["clanid"];
                    if (clanid != "0")
                        _clanData.Add(clanid, new Hash<string, object>());
                }
                LoadClans();
            });
        }

        private void InitPlayers()
        {
            SqlLib.Insert(Sql.Builder.Append("SET NAMES utf8mb4"), Sql_conn);
            SqlLib.Insert(Sql.Builder.Append($"CREATE TABLE IF NOT EXISTS {_playerTableName} ( `id` int(11) NOT NULL, `userid` VARCHAR(17) NOT NULL );"), Sql_conn);
            SqlLib.Query(Sql.Builder.Append($"desc {_playerTableName};"), Sql_conn, list =>
            {
                if (list == null)
                {
                    FatalError("Couldn't get columns. Database might be corrupted.");
                    return;
                }
                foreach (var entry in list)
                {
                    _playerColumns.Add((string)entry["Field"]);
                }

            });
            SqlLib.Query(Sql.Builder.Append($"SELECT userid from {_playerTableName}"), Sql_conn, list =>
            {
                if (list == null) return;
                foreach (var entry in list)
                {
                    string steamid = (string)entry["userid"];
                    if (steamid != "0")
                        _playerData.Add(steamid, new Hash<string, object>());
                }
                LoadPlayers();
            });
        }

        public void SaveDatabase()
        {
            Interface.Oxide.LogDebug($"Saving");

            SavePlayerData();
            SaveClanData();
        }

        private void SaveClanData()
        {
            foreach (string clanid in _changedClansData)
            {
                try
                {
                    Interface.Oxide.LogDebug($"  Saving {clanid}");

                    var values = _clanData[clanid];

                    string arg = string.Empty;
                    var parms = new List<object>();
                    foreach (var c in values)
                    {
                        Interface.Oxide.LogDebug($"      Value: {c.Value}");

                        arg += string.Format("{0}`{1}` = @{2}", arg == string.Empty ? string.Empty : ",", c.Key, parms.Count.ToString());
                        parms.Add(c.Value);
                    }

                    SqlLib.Insert(Sql.Builder.Append($"UPDATE {_clanTableName} SET {arg} WHERE clanid = {clanid}", parms.ToArray()), Sql_conn);

                }
                catch (Exception e)
                {
                    Interface.Oxide.LogWarning(e.Message);
                    Interface.Oxide.LogWarning(e.StackTrace);
                }
            }
            _changedClansData.Clear();
        }

        private void SavePlayerData()
        {
            foreach (string userid in _changedPlayersData)
            {
                try
                {
                    Interface.Oxide.LogDebug($"  Saving {userid}");

                    var values = _playerData[userid];

                    string arg = string.Empty;
                    var parms = new List<object>();
                    foreach (var c in values)
                    {
                        Interface.Oxide.LogDebug($"      Value: {c.Value}");

                        arg += string.Format("{0}`{1}` = @{2}", arg == string.Empty ? string.Empty : ",", c.Key, parms.Count.ToString());
                        parms.Add(c.Value);
                    }

                    SqlLib.Insert(Sql.Builder.Append($"UPDATE {_playerTableName} SET {arg} WHERE userid = {userid}", parms.ToArray()), Sql_conn);

                }
                catch (Exception e)
                {
                    Interface.Oxide.LogWarning(e.Message);
                    Interface.Oxide.LogWarning(e.StackTrace);

                }
            }
            _changedPlayersData.Clear();
        }

        public List<KeyValuePair<string, T>> GetLeaderboard<T>(string key, int take = 5)
        {
            var returnValues = new List<KeyValuePair<string, T>>();
            var query = $"Select ifnull({key}, 0) as {key}, ifnull(name,'None') as name from {_playerTableName} ORDER BY {key} DESC LIMIT {take}";

            SqlLib.Query(Sql.Builder.Append(query), Sql_conn, list =>
            {
                foreach (var x in list)
                {
                    object possibleName = null;

                    if (x.TryGetValue("name", out possibleName))
                    {
                        if (possibleName != null)
                        {


                            object possibleValue = null;
                            if (x.TryGetValue(key, out possibleValue))
                            {
                                if (possibleValue != null)
                                {
                                    returnValues.Add(new KeyValuePair<string, T>((string)possibleName, (T)possibleValue));

                                    continue;
                                }
                            }
                            returnValues.Add(new KeyValuePair<string, T>((string)possibleName, default(T)));
                            continue;
                        }
                    }
                    returnValues.Add(new KeyValuePair<string, T>("Nobody", default(T)));

                }
                if (returnValues.Count < take)
                {
                    for (int i = 0; i < take - returnValues.Count; i++)
                    {
                        returnValues.Add(new KeyValuePair<string, T>("Nobody", default(T)));
                    }
                }
            });


            return returnValues;
        }

        public T GetClanDataRaw<T>(string clandid, string key)
        {
            if (!IsKnownClan(clandid)) return default(T);

            if (!_clanColumns.Contains(key)) return default(T);
            if (_clanData[clandid] == null) return default(T);
            if (_clanData[clandid][key] == null) return default(T);
            Interface.Oxide.LogDebug("Getting clan data: " + key + " " + _clanData[clandid][key] +" type " + _clanData[clandid][key].GetType().Name);
            return (T)_clanData[clandid][key];
        }

        public T GetClanDataDeserialized<T>(string clandid, string key)
        {
            if (!IsKnownClan(clandid)) return default(T);

            if (!_clanColumns.Contains(key)) return default(T);
            if (_clanData[clandid] == null) return default(T);
            if (_clanData[clandid][key] == null) return default(T);
            return JsonConvert.DeserializeObject<T>((string)_clanData[clandid][key]);
        }
        public T GetPlayerDataRaw<T>(string userid, string key)
        {
            if (!IsKnownPlayer(userid)) return default(T);

            if (!_playerColumns.Contains(key)) return default(T);
            if (_playerData[userid] == null) return default(T);
            if (_playerData[userid][key] == null) return default(T);
            return (T)_playerData[userid][key];
        }

        public T GetPlayerDataDeserialized<T>(string userid, string key)
        {
            if (!IsKnownPlayer(userid)) return default(T);

            if (!_playerColumns.Contains(key)) return default(T);
            if (_playerData[userid] == null) return default(T);
            if (_playerData[userid][key] == null) return default(T);
            return JsonConvert.DeserializeObject<T>((string)_playerData[userid][key]);
        }

        public List<string> KnownPlayers()
        {
            return _playerData.Keys.ToList();
        }
        public List<string> KnownClans()
        {
            return _clanData.Keys.ToList();
        }
        public bool IsKnownPlayer(string userid)
        {
            return _playerData.ContainsKey(userid);
        }
        public bool IsKnownClan(string userid)
        {
            return _clanData.ContainsKey(userid);
        }
        void FatalError(string msg)
        {
            Interface.Oxide.LogError(msg);
        }
    }
}
