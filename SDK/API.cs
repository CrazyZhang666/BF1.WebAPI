namespace BF1.WebAPI.SDK;

public static class API
{
    private const int MaxPlayer = 74;

    private static List<PlayerInfo> AllPlayers = new();

    private static List<string> WeaponSlot = new();

    public static object GetPlayerList()
    {
        if (Memory.Bf1ProHandle != IntPtr.Zero)
        {
            AllPlayers.Clear();

            WeaponSlot.Clear();
            for (int j = 0; j < 8; j++)
            {
                WeaponSlot.Add("");
            }

            for (int i = 0; i < MaxPlayer; i++)
            {
                var pPlayer = Player.GetPlayerById(i);
                if (!Memory.IsValid(pPlayer))
                    continue;

                var oMark = Memory.Read<byte>(pPlayer + 0x1D7C);
                var oTeamId = Memory.Read<int>(pPlayer + 0x1C34);
                var oSpectator = Memory.Read<byte>(pPlayer + 0x1C31);
                var oPersonaId = Memory.Read<long>(pPlayer + 0x38);
                var oSquadId = Memory.Read<int>(pPlayer + 0x1E50);
                var oClan = Memory.ReadString(pPlayer + 0x2151, 64);
                var oName = Memory.ReadString(pPlayer + 0x40, 64);
                if (string.IsNullOrEmpty(oName))
                    continue;

                var pClientVehicleEntity = Memory.Read<long>(pPlayer + 0x1D38);
                var pClientSoldierEntity = Memory.Read<long>(pPlayer + 0x1D48);

                if (Memory.IsValid(pClientVehicleEntity))
                {
                    var _pVehicleEntityData = Memory.Read<long>(pClientVehicleEntity + 0x30);
                    var _pVehicleName = Memory.Read<long>(_pVehicleEntityData + 0x2F8);
                    WeaponSlot[0] = Memory.ReadString(_pVehicleName, 64);
                }
                else if (Memory.IsValid(pClientSoldierEntity))
                {
                    var pClientSoldierWeaponComponent = Memory.Read<long>(pClientSoldierEntity + 0x698);
                    var pHandler = Memory.Read<long>(pClientSoldierWeaponComponent + 0x8A8);

                    for (int j = 0; j < 8; j++)
                    {
                        var offset0 = Memory.Read<long>(pHandler + j * 0x8);

                        offset0 = Memory.Read<long>(offset0 + 0x4A30);
                        offset0 = Memory.Read<long>(offset0 + 0x20);
                        offset0 = Memory.Read<long>(offset0 + 0x38);
                        offset0 = Memory.Read<long>(offset0 + 0x20);

                        WeaponSlot[j] = Memory.ReadString(offset0, 64);
                    }
                }

                var index = AllPlayers.FindIndex(val => val.Name == oName);
                if (index == -1)
                {
                    AllPlayers.Add(new PlayerInfo()
                    {
                        Mark = oMark,
                        TeamId = oTeamId,
                        Spectator = oSpectator,
                        Clan = oClan,
                        Name = oName,
                        PersonaId = oPersonaId,
                        SquadId = oSquadId,

                        Rank = 0,
                        Kill = 0,
                        Dead = 0,
                        Score = 0,

                        KD = 0,
                        KPM = 0,

                        WeaponS0 = WeaponSlot[0],
                        WeaponS1 = WeaponSlot[1],
                        WeaponS2 = WeaponSlot[2],
                        WeaponS3 = WeaponSlot[3],
                        WeaponS4 = WeaponSlot[4],
                        WeaponS5 = WeaponSlot[5],
                        WeaponS6 = WeaponSlot[6],
                        WeaponS7 = WeaponSlot[7],
                    });
                }
            }

            //////////////////////////////// 得分板数据 ////////////////////////////////

            var pClientScoreBA = Memory.Read<long>(Memory.Bf1ProBaseAddress + 0x39EB8D8);
            pClientScoreBA = Memory.Read<long>(pClientScoreBA + 0x68);

            for (int i = 0; i < MaxPlayer; i++)
            {
                pClientScoreBA = Memory.Read<long>(pClientScoreBA);
                var _pClientScoreOffset = Memory.Read<long>(pClientScoreBA + 0x10);
                if (!Memory.IsValid(pClientScoreBA))
                    continue;

                var oMark = Memory.Read<byte>(_pClientScoreOffset + 0x300);
                int oRank = Memory.Read<int>(_pClientScoreOffset + 0x304);
                if (oRank == 0)
                    continue;
                var oKill = Memory.Read<int>(_pClientScoreOffset + 0x308);
                var oDead = Memory.Read<int>(_pClientScoreOffset + 0x30C);
                var oScore = Memory.Read<int>(_pClientScoreOffset + 0x314);

                var index = AllPlayers.FindIndex(val => val.Mark == oMark);
                if (index != -1)
                {
                    AllPlayers[index].Rank = oRank;
                    AllPlayers[index].Kill = oKill;
                    AllPlayers[index].Dead = oDead;
                    AllPlayers[index].Score = oScore;
                    AllPlayers[index].KD = 0;
                    AllPlayers[index].KPM = 0;
                }
            }

            Results.StatusCode(200);
            return AllPlayers;
        }
        else
        {
            Results.StatusCode(500);
            return "未发现《BF1》进程";
        }
    }
}

public class PlayerInfo
{
    public byte Mark { get; set; }
    public int TeamId { get; set; }
    public byte Spectator { get; set; }
    public string Clan { get; set; }
    public string Name { get; set; }
    public long PersonaId { get; set; }
    public int SquadId { get; set; }

    public int Rank { get; set; }
    public int Kill { get; set; }
    public int Dead { get; set; }
    public int Score { get; set; }

    public float KD { get; set; }
    public float KPM { get; set; }

    public string WeaponS0 { get; set; }
    public string WeaponS1 { get; set; }
    public string WeaponS2 { get; set; }
    public string WeaponS3 { get; set; }
    public string WeaponS4 { get; set; }
    public string WeaponS5 { get; set; }
    public string WeaponS6 { get; set; }
    public string WeaponS7 { get; set; }
}
