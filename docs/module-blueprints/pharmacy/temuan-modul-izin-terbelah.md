# Temuan — modul izin Farmasi terbelah menjadi dua kode

| Hal | Isi |
|---|---|
| Ditemukan | 5 Oktober 2026, saat menyiapkan matriks izin Farmasi tingkat HTTP |
| Sifat | **Ketidakrapian konfigurasi dengan akibat operasional**, bukan cacat kode |
| Dampak hari ini | Nol endpoint rusak. Seluruh 137 endpoint dijaga dan seluruh izinnya terdaftar aktif |
| Dampak bagi administrator | Farmasi muncul sebagai **dua modul** pada layar pemberian izin; memberi izin satu modul hanya memberi sebagian Farmasi |
| Pemilik keputusan | pemilik modul Farmasi bersama pemilik registry izin |

## Apa yang ditemukan

23 controller Farmasi memasang `[AccessController(...)]`, tetapi menyebut **dua kode modul yang
berbeda**:

| Kode modul | Jumlah controller |
|---|---|
| `HEALTH_SERVICE_PHARMACY` | 12 |
| `HEALTH_SERVICE_PHARMACY_MANAGEMENT` | 11 |

Pembelahannya tidak mengikuti pola apa pun yang dapat dibaca. Beberapa contoh yang berdekatan
secara domain justru terpisah:

| Controller | Kode modul |
|---|---|
| `PrescriptionController` | `HEALTH_SERVICE_PHARMACY` |
| `PrescriptionItemController` | `HEALTH_SERVICE_PHARMACY` |
| `PrescriptionReviewController` | `HEALTH_SERVICE_PHARMACY_MANAGEMENT` |
| `PrescriptionPreparationController` | `HEALTH_SERVICE_PHARMACY_MANAGEMENT` |
| `PrescriptionCompoundController` | `HEALTH_SERVICE_PHARMACY` |
| `PrescriptionDispensingController` | `HEALTH_SERVICE_PHARMACY_MANAGEMENT` |

Jadi satu resep berjalan melewati **dua modul izin** dalam satu alur: dibuat di bawah
`HEALTH_SERVICE_PHARMACY`, ditelaah dan disiapkan di bawah `HEALTH_SERVICE_PHARMACY_MANAGEMENT`,
diracik kembali di bawah `HEALTH_SERVICE_PHARMACY`, lalu diserahkan di bawah
`HEALTH_SERVICE_PHARMACY_MANAGEMENT`.

Operasi sebagai pembanding memakai **satu** kode untuk seluruh modulnya:
`HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT`.

## Yang sudah dipastikan TIDAK rusak

Dibaca **read-only** dari `localhost / QuilvianNewDevIkbalFr`; nol perintah tulis.

| Pemeriksaan | Hasil |
|---|---|
| Kedua kode modul ada pada `SysApplicationModule` | ✅ keduanya `IsActive = true`, `IsDelete = false` |
| Seluruh controller terdaftar | ✅ 12 + 11 = **23**, tidak ada yang tercecer |
| Pasangan `resource:action` diminta atribut | **85** unik |
| Pasangan aktif pada registry | **85** |
| Cocok dua arah | **85 / 85** |
| Diminta atribut tetapi tidak aktif di registry | **0** |
| Aktif di registry tetapi tak dipakai endpoint | **0** |

Nol pada baris keenam yang penting: kalau satu pasangan saja tidak terdaftar aktif,
`HasAccessAsync` menolak **semua** orang pada endpoint itu dan ia mati total walaupun pemetaan
perannya benar. Tidak ada yang seperti itu, pada kedua kode modul.

Jadi pembelahan ini **tidak** menyebabkan endpoint mana pun kehilangan izin.

## Mengapa tetap perlu dibereskan

Akibatnya jatuh pada orang yang memberi izin, bukan pada kode.

Administrator yang diminta "beri peran ini akses Farmasi" akan melihat dua modul bernama mirip
dan wajar menganggap salah satunya sudah cukup. Bila yang dipilih `HEALTH_SERVICE_PHARMACY`,
peran itu dapat membuat resep tetapi **tidak** dapat menelaah, menyiapkan, atau menyerahkannya.
Bila yang dipilih `HEALTH_SERVICE_PHARMACY_MANAGEMENT`, peran itu dapat menyerahkan obat tetapi
tidak dapat membuat resepnya.

Kegagalannya akan muncul sebagai `403` yang membingungkan di tengah alur klinis, dan dugaan
pertama orang hampir pasti tertuju pada kode, bukan pada modul mana yang tercentang.

## Yang dibutuhkan

Satu keputusan: **kode mana yang dipertahankan.** Sesudah itu pekerjaannya mekanis:

1. seragamkan `ModuleCode` pada 23 controller menjadi satu kode;
2. pindahkan baris `SysControllerAccess` yang menunjuk modul yang ditinggalkan ke modul yang
   dipertahankan — **memindahkan**, bukan membuat ulang, supaya `SysAccessPolicy` yang sudah
   menunjuk `ActionAccessId` lama tidak kehilangan acuannya;
3. tandai modul yang ditinggalkan `IsActive = false` setelah nol controller menunjuknya.

Langkah 2 dan 3 menyentuh data izin bersama, jadi keduanya milik pemilik registry izin dan
**tidak** dikerjakan dari scope Farmasi.

`HEALTH_SERVICE_PHARMACY_MANAGEMENT` yang lebih sejalan dengan pola modul lain
(`HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT`), tetapi itu usulan, bukan keputusan.

## Yang dijaga uji

`Tests/QuilvianSystemBackend.PharmacyTests/PermissionMatrixTests.cs` menerima **kedua** kode
sebagai sah untuk sekarang, dan menyatakan alasannya di tempat. Uji itu tidak dipakai untuk
memaksa keseragaman yang belum diputuskan — tetapi ia akan menolak kode modul **ketiga** yang
masuk tanpa keputusan.
