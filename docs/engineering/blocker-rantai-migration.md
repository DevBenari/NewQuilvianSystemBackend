# Blocker rantai migration dan deployment

Dicatat 29 September 2026. Pemilik: repositori/deployment, **bukan** modul Operasi.

Catatan ini ada supaya keadaan di bawah tidak dibaca sebagai pekerjaan modul yang tertinggal.
`BE-OPR-009` tidak tertahan olehnya, dan tidak boleh diselesaikan dengan merapikan rantai
migration secara sepihak.

## Keadaan

Working tree memuat tiga hal yang berasal dari pekerjaan squash baseline pada 21 September 2026
dan belum di-commit:

| Hal | Jumlah | Status git |
|---|---|---|
| Berkas migration lama terhapus | 409 | `D`, belum di-commit |
| Baseline hasil squash `20260921051447_InitialQuilvianV2` (+ Designer) | 2 | `??`, belum ditambahkan |
| Snapshot model | 1 | `M`, isinya sama dengan HEAD selain bidang `BE-OPR-009` |

Sementara itu basis data pengembangan `localhost/QuilvianNewDevIkbalFr` memuat **47 baris**
`__EFMigrationsHistory`, seluruhnya migration lama; baseline hasil squash itu **tidak ada** di
sana.

## Mengapa ini berbahaya

Rantai migration di repositori dan rantai yang benar-benar diterapkan basis data sudah tidak
sama, dan keduanya sama-sama masuk akal bila dibaca sendiri-sendiri.

1. **Basis data baru.** `dotnet ef database update` akan menjalankan `InitialQuilvianV2` lebih
   dulu, yaitu membangun seluruh skema dari nol. Itu memang yang dikehendaki untuk basis data
   kosong.
2. **Basis data yang sudah ada.** Basis data pengembangan dan basis data mana pun yang memuat 47
   migration lama tidak mengenali `InitialQuilvianV2`. EF akan menganggapnya belum diterapkan
   lalu mencoba membangun ulang ratusan tabel yang sudah ada. Yang terjadi bukan "tidak ada
   perubahan", melainkan kegagalan atau kerusakan.
3. **Commit sebagian memperburuknya.** Meng-commit penghapusan 409 berkas tanpa menambahkan
   baseline hasil squash menghasilkan repositori yang tidak dapat memigrasi apa pun.

## Yang harus diputuskan pemilik repositori

- Apakah squash baseline itu memang hendak diteruskan, dan bila ya, bagaimana basis data yang
  sudah berjalan diselaraskan — lazimnya dengan menyisipkan baris `InitialQuilvianV2` ke
  `__EFMigrationsHistory` tanpa menjalankan skripnya.
- Apakah penghapusan 409 berkas itu di-commit bersama baseline-nya dalam satu commit, supaya
  repositori tidak pernah berada dalam keadaan setengah jadi.
- Bagaimana lingkungan lain — staging, produksi — diperlakukan.

Tidak satu pun dari itu boleh diputuskan dari dalam task modul.

## Hubungan dengan `BE-OPR-009`

`BE-OPR-009` menambahkan satu migration, `20260929000000_AddOperatingRoomOutboxEventContract`,
yang seluruhnya additive: lima kolom dan satu indeks unik pada `OprIntegrationDelivery`. Ia sudah
diterapkan ke basis data pengembangan lewat `ALTER TABLE` eksplisit beserta baris riwayatnya,
sehingga konsisten dengan rantai yang benar-benar dipakai basis data itu.

Migration itu duduk di atas keadaan yang dijelaskan di atas. Ia tidak menyebabkannya dan tidak
memperburuknya, tetapi ia juga tidak memperbaikinya.
