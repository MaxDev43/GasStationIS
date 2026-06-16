using System;
using GasStationIS.Core;
using GasStationIS.Models;

namespace GasStationIS.Services
{
    public static class StationInfoService
    {
        public static StationInfo Get()
        {
            var list = DatabaseService.ExecuteReader(
                "SELECT id, name, address, phone, director, inn, license_number " +
                "FROM station_info WHERE id = 1;",
                r => new StationInfo
                {
                    Id            = Convert.ToInt32(r["id"]),
                    Name          = r["name"].ToString(),
                    Address       = r["address"]?.ToString(),
                    Phone         = r["phone"]?.ToString(),
                    Director      = r["director"]?.ToString(),
                    Inn           = r["inn"]?.ToString(),
                    LicenseNumber = r["license_number"]?.ToString(),
                });
            return list.Count > 0 ? list[0] : new StationInfo { Id = 1, Name = "АЗС №1" };
        }

        public static void Save(StationInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            DatabaseService.ExecuteNonQuery(
                "INSERT OR REPLACE INTO station_info " +
                "(id, name, address, phone, director, inn, license_number) " +
                "VALUES (1, @name, @addr, @phone, @dir, @inn, @lic);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@name",  info.Name          ?? "");
                    cmd.Parameters.AddWithValue("@addr",  info.Address       ?? "");
                    cmd.Parameters.AddWithValue("@phone", info.Phone         ?? "");
                    cmd.Parameters.AddWithValue("@dir",   info.Director      ?? "");
                    cmd.Parameters.AddWithValue("@inn",   info.Inn           ?? "");
                    cmd.Parameters.AddWithValue("@lic",   info.LicenseNumber ?? "");
                });
            Logger.Info("Настройки АЗС сохранены.");
        }
    }
}
