using System;
using Npgsql;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace MewPewServerNew
{
    class DBController
    {
        public static string connString = $"Host={Program.config["dbHost"]};" +
            $"Port={Program.config["dbPort"]};" +
            $"Username={Program.config["dbUser"]};" +
            $"Password={Program.config["dbPass"]};" +
            $"Database={Program.config["dbName"]}";
        public static async Task<bool> InitDb()
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    await conn.OpenAsync();

                    NpgsqlCommand usersCmd = new NpgsqlCommand($"CREATE TABLE IF NOT EXISTS users(" +
                        $"name varchar," +
                        $"mail varchar," +
                        $"password varchar," +
                        $"verified boolean," +
                        $"verification varchar," +
                        $"createdat bigint)", conn);

                    await usersCmd.ExecuteNonQueryAsync();

                    NpgsqlCommand mewpewCmd = new NpgsqlCommand($"CREATE TABLE IF NOT EXISTS mewpew(" +
                        $"id varchar," +
                        $"mail varchar," +
                        $"active boolean," +
                        $"credits integer," +
                        $"inventory varchar," +
                        $"ship varchar," +
                        $"location varchar," +
                        $"joinedat bigint)", conn);

                    await mewpewCmd.ExecuteNonQueryAsync();
                    await conn.CloseAsync();

                    Console.WriteLine("Database initiated successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        public static async Task<bool> UpdateOne(string table, string values, string query)
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    await conn.OpenAsync();

                    NpgsqlCommand cmd = new NpgsqlCommand($"UPDATE {table} " +
                        $"SET {values} " +
                        $"WHERE {query}", conn);

                    await cmd.ExecuteNonQueryAsync();
                    

                    await conn.CloseAsync();
                    return true;
                }
            } catch (Exception e)
            {
                Console.Write(e);
                return false;
            }

        }
        public static async Task<bool> InsertOne(string table, string columns, string values)
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    await conn.OpenAsync();

                    NpgsqlCommand cmd = new NpgsqlCommand($"INSERT INTO {table} {columns} VALUES {values}", conn);
                    await cmd.ExecuteNonQueryAsync();
                    
                    await conn.CloseAsync();
                    return true;
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

        }
        public static async Task<object[]> SelectOne(string columns, string table, string query, int length)
        {
            try
            {
                
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    await conn.OpenAsync();

                    NpgsqlCommand cmd = new NpgsqlCommand($"SELECT {columns} " +
                        $"FROM {table} " +
                        $"WHERE {query}", conn);

                    object[] newArr = new object[length];
                    await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            for (int i = 0; i < length; i++)
                            {
                                newArr[i] = reader.GetValue(i);
                            }
                        }

                        reader.Close();
                    }

                    await conn.CloseAsync();
                    return newArr;
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
                return new object[0];
            }
        }

        public static async void SelectShip(MewPewSocket socket, Message[] parameters)
        {
            int shipIdx = Message.IntValueOfKey(parameters, "idx");
            socket.UserData.Ship = new Sparrow(socket.UserData.Id);
            bool success = await UpdateOne("mewpew", $"ship = '{JsonConvert.SerializeObject(socket.UserData.Ship)}'", $"mail = '{socket.UserData.Username}'");
        }
        public static async void Login(MewPewSocket socket, Message[] parameters)
        {
            string mail = Message.StringValueOfKey(parameters, "mail");
            string pass = Message.StringValueOfKey(parameters, "pass");

            if (string.IsNullOrWhiteSpace(mail) || string.IsNullOrEmpty(mail) || string.IsNullOrWhiteSpace(pass) || string.IsNullOrEmpty(pass))
            {
                socket.SendData("loginfailed?err=data", true);
                return;
            }

            object[] query = await SelectOne("password", "users", $"mail = '{mail}'", 1);
            if(query.Length == 0 || (string)query[0] != pass)
            {
                socket.SendData("loginfailed?err=auth", true);
                return;
            }

            long joinedAt = -1;
            query = await SelectOne("joinedat", "mewpew", $"mail = '{mail}'", 1);
            if (query.Length > 0 && query[0] != null)
                joinedAt = (long)query[0];

            string res = "loginsuccess?firsttime=";
            User newUser = new User(socket, mail);
            bool success;

            if (joinedAt == -1)
            {
                res += "true";

                success = await InsertOne("mewpew", "(id, mail, joinedat, credits, ship, inventory, location)", 
                    $"('{newUser.Id}', '{mail}', {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}, {10000}, '', '', 'Alpha Centauri')");
            }
            else
            {
                res += "false";

                query = await SelectOne("id, credits, ship, inventory", "mewpew", $"mail = '{mail}'", 4);
                if (query.Length > 0)
                {
                    newUser.Id = (string)query[0];
                    newUser.Credits = (int)query[1];
                    newUser.Ship = JsonConvert.DeserializeObject<PlayerShip>((string)query[2]);
                    newUser.Inventory = JsonConvert.DeserializeObject<Item[]>((string)query[3]);
                }
            }

            success = await UpdateOne("mewpew", "active=true", $"id = '{newUser.Id}'");

            socket.UserData = newUser;
            Program.activeUsers.Add(newUser);
            socket.SendData(res, true);
        }
        public static async void ChangeMap(MewPewSocket socket, Message[] parameters)
        {
            string status = Message.StringValueOfKey(parameters, "status");
            string mapName = Message.StringValueOfKey(parameters, "map");
            bool success = await UpdateOne("mewpew", $"location='{mapName}'", $"id = '{socket.UserData.Id}'");
            Map oldMap = Program.maps.Find(element => element.Name == socket.UserData.Location);
            oldMap.Players.Remove(socket.UserData);

            socket.UserData.Location = mapName;

            Map mapItem = Program.maps.Find(element => element.Name == mapName);
            mapItem.Players.Add(socket.UserData);
            socket.UserData.Status = status;
            mapItem.SendScene(socket);
        }
    }
}
