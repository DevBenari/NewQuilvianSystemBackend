namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums
{
    /// <summary>
    /// Arah wing kamar pada denah gedung (RWI-DEC-313). Tanpa nilai bawaan: <c>null</c> pada
    /// <c>MstRoom.WingDirection</c> berarti kamar tidak tergambar di denah.
    /// </summary>
    public enum RoomWingDirection
    {
        Utara = 1,
        TimurLaut = 2,
        Timur = 3,
        Tenggara = 4,
        Selatan = 5,
        BaratDaya = 6,
        Barat = 7,
        BaratLaut = 8
    }
}
