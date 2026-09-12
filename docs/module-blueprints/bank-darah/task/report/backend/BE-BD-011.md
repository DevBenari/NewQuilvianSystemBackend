# Laporan Perubahan Backend — `BE-BD-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-011` |
| Judul | Konflik golongan darah diselesaikan validator klinis |
| Slice | `MVP-0` — jalur terbuka, tidak menyentuh number-series |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md` §5 |
| Trace | `DEC-BD-026`, `DEC-BD-031`, `DEC-BD-039` · `BD-DOM-21`, `BD-DOM-22` · `INV-BD-016`, `INV-BD-018`, `INV-BD-022` · `contracts/api-contract.md` §Blood Group Exam · `contracts/state-transition-matrix.md` §4 · `contracts/validation-matrix.md` (`VAL-BD-051`, `VAL-BD-054`, `VAL-BD-069`) |
| Contract version | `v4` — **`approved`** (`Sukmagp` / `2026-09-03`) |
| Dependency | `G1` ✅, `G2b` ✅, `BE-BD-005` ✅ — diselesaikan berurutan pada sesi yang sama |
| Klasifikasi | `MEDIUM` — satu entity append-only, satu endpoint, aturan penutupan lintas-baris dalam satu transaksi, risiko klinis tinggi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/BloodBankManagement/**`, `Repositories/**`, `Migrations/**`, `Tests/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `f0d6855` cabang `sukmagp` |
| Tanggal | `2026-09-09` |
| Status | **`SELESAI`** untuk scope task. Satu gap kontrak dicatat di bagian 7 |

---

## 1. Masalah yang diperbaiki

`BE-BD-005` sengaja berhenti di tengah jalan. Ia menyanggupi mendeteksi perbedaan hasil golongan
darah dan menahannya — tetapi **tidak menyediakan satu pun cara membuka penahanan itu**.

Akibatnya nyata: begitu seorang pasien punya dua hasil tervalidasi yang berbeda, ia **selamanya**
tercatat tidak punya golongan darah sah. Setiap gerbang klinis Bank Darah yang menuntut golongan
darah tertutup untuk pasien itu, dan tidak ada tindakan apa pun di sistem yang dapat membukanya
kembali.

**Contoh konkretnya.** Pasien punya hasil sah **O Positif**. Sampel baru diperiksa, hasilnya **A
Positif**, lalu divalidasi. Sejak saat itu pasien tertahan. Petugas boleh mengambil sampel ketiga,
mencatat hasilnya, bahkan memvalidasinya — perbedaan **tetap tertahan**, karena memvalidasi
pemeriksaan ulang memang tidak menutup perbedaan. Tanpa task ini, jalan keluarnya tidak ada.

**Kenapa jalan keluarnya tidak boleh berupa penimpaan data.** Godaan yang paling wajar adalah
membiarkan seseorang menyunting hasil yang salah, atau membiarkan sistem memilih hasil terbaru.
Keduanya ditolak `DEC-BD-031`: perbedaan hasil ABO adalah tanda ada yang keliru secara fisik —
tabung tertukar, label salah, atau pemeriksaan meleset — dan menghapus salah satu hasil menghapus
tanda itu bersamaan. Yang benar adalah memeriksa ulang, lalu meminta manusia berwenang menyatakan
hasil mana yang berlaku.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Hal | Isi |
| --- | --- |
| Tujuan | Mengakhiri keadaan tertahan sehingga pasien kembali punya **tepat satu** golongan darah sah, tanpa satu pun hasil dihapus atau ditimpa |
| Pelaku | **Validator klinis yang ditunjuk** — Dokter BDRS atau penanggung jawab klinis (`DEC-BD-039`) |
| Pemicu | Pasien sedang menahan perbedaan hasil, dan sudah ada pemeriksaan ulang yang tervalidasi |
| Tempat | Di dalam layar pemeriksaan golongan darah, **bukan** daftar kerja keempat (`DEC-BD-033`) |
| Hasil akhir | Satu hasil sah kembali berlaku; pelaku, alasan, dan waktu tersimpan permanen; seluruh hasil tetap terbaca |

### 2.2 Langkah normal, berurutan

| No | Langkah | Pelaku | Akibat |
| ---: | --- | --- | --- |
| 1 | Perbedaan tertahan sejak validasi di `BE-BD-005` | Sistem | Pasien tidak punya golongan darah sah |
| 2 | Ambil sampel ulang | Petugas Bank Darah | Pemeriksaan baru `SampleTaken` |
| 3 | Catat hasil pemeriksaan ulang | Pemeriksa | `ResultRecorded` |
| 4 | Validasi hasil pemeriksaan ulang | Petugas berwenang validasi | `Validated` — **perbedaan masih tertahan** |
| 5 | Nyatakan hasil ulang itu yang berlaku | **Validator klinis** | Perbedaan ditutup; satu hasil sah kembali |

Langkah 4 dan 5 **sengaja terpisah**, dan dijaga dua butir hak akses yang berbeda. Memvalidasi hasil
pemeriksaan ulang tidak dengan sendirinya menutup perbedaan — penutupannya adalah keputusan klinis
tersendiri.

### 2.3 Yang terjadi pada data saat perbedaan ditutup

Seluruhnya dalam **satu transaksi database**:

| Baris | Sebelum | Sesudah |
| --- | --- | --- |
| Hasil lama O Positif | tertahan, tidak berlaku | **tidak** tertahan, tidak berlaku · nilai **tetap** O Positif |
| Hasil baru A Positif | tertahan, tidak berlaku | **tidak** tertahan, tidak berlaku · nilai **tetap** A Positif |
| Pemeriksaan ulang | tervalidasi, belum berlaku | **berlaku** sebagai hasil sah |
| Catatan penyelesaian | belum ada | **baru** — pasien, pemeriksaan pemutus, validator, alasan, waktu |

Yang berubah hanyalah **penanda**. Nilai `AboRhesusResult` setiap pemeriksaan tidak disentuh, dan
tidak satu baris pun dihapus — karena itu riwayat kedua hasil yang bentrok tetap terbaca sesudahnya
(`AC-BD-036`, `AC-BD-079`).

### 2.4 Jalur tidak normal

| Keadaan | Akibat | Kode |
| --- | --- | --- |
| Menutup tanpa menunjuk pemeriksaan ulang | Ditolak `422` | `VAL-BD-051` |
| Menunjuk salah satu **pihak** perbedaan | Ditolak `422` — pemeriksaan yang sedang bertentangan bukan "pemeriksaan ulang" | `VAL-BD-051` |
| Menunjuk pemeriksaan ulang yang **belum** divalidasi | Ditolak `422` | `VAL-BD-051` |
| Meminta sistem memilih hasil "mayoritas" | **Tidak mungkin diminta** — permintaannya tidak punya field untuk itu | `VAL-BD-054` |
| Alasan berupa ketikan bebas | Ditolak `400` | `INV-BD-016` |
| Pasien sedang tidak menahan perbedaan | Ditolak `422` | — |
| Pelaku bukan validator klinis | Ditolak `403` oleh `AccessPermissionFilter` | `VAL-BD-069` |

**Wewenang tidak menggantikan prasyarat.** Validator klinis sekalipun tidak dapat menutup perbedaan
tanpa menunjuk pemeriksaan ulang tervalidasi (`AC-BD-080`). Kedua hal itu diperiksa terpisah:
kewenangan oleh hak akses, prasyarat oleh service.

**Hasil ulang boleh bernilai ketiga.** Bila pemeriksaan ulang menghasilkan nilai yang berbeda dari
**kedua** hasil yang bentrok — misalnya B Negatif ketika yang bentrok O Positif dan A Positif — hasil
itu **tetap boleh** dinyatakan berlaku (`AC-BD-053`). Sistem tidak memaksa hasil baru cocok dengan
salah satu hasil lama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Sama dengan `BE-BD-005` — lihat laporannya bagian 3.1 — ditambah pendalaman pada
`00-interview-decisions.md` §`DEC-BD-031` (Model C), `contracts/state-transition-matrix.md` §4 baris
penyelesaian, dan `MstBloodBankReason.cs` beserta kesepuluh kategori alasannya.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodGroupConflictResolution.cs` | Entity append-only `BD-DOM-22`. Dibuat pada migration `BE-BD-005`; **aturan dan pemakaiannya milik task ini** |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodGroupExamDtos.cs` | `ResolveConflictRequest` ditambahkan — tiga field saja, **nol** mode pemilihan otomatis |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodGroupExamService.cs` | Method `ResolveConflictAsync` — prasyarat, alasan terkendali, penutupan lintas-baris dalam satu transaksi |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodGroupExamController.cs` | Endpoint `POST /conflict-resolution` dengan butir `BloodGroupExam : ResolveConflict` |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodGroupConflictResolutionConfiguration.cs` | FK `Restrict` ke pemeriksaan pemutus dan ke `MstPatient`; dua index |
| `Tests/.../BankDarah/BloodGroupExam/BloodGroupExamServiceTests.cs` | 9 pengujian penyelesaian perbedaan |
| `Tests/.../BankDarah/MasterData/BloodBankRoleAccessContractTests.cs` | Butir `ResolveConflict` masuk cakupan; pengujian penjaga `DEC-BD-039` ditambahkan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif murni.** Satu endpoint baru, persis seperti daftar kontrak `v4`. Nol endpoint existing berubah |
| Database | Tabel `BbkBloodGroupConflictResolution` ikut pada migration `20260909015603_AddBbkBloodGroupExam`. **Belum dijalankan.** Nol tabel existing diubah |
| Keamanan/Auth | Butir `BloodGroupExam : ResolveConflict` — **terpisah** dari `Validate` sesuai `DEC-BD-039`. Penegakannya murni lewat hak akses; **nol** pemeriksaan nama peran, nama jabatan, atau `UserType` di dalam kode. Validator pelaku diturunkan dari pengguna terautentikasi, tidak pernah dari isian permintaan |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `BloodBankManagement` / `NOT APPLICABLE` |
| Pemilik / prefix registry | `BloodBankManagement / Blood Bank` → **`Bbk`**, status **`ACTIVE`** |
| Keberlakuan | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-004`, `QBE-SVC-001` |
| `QBE-CODE-001/002/003` | **Tidak berlaku** — nol nomor bisnis dialokasikan |
| Gerbang `G4` | **Tidak mengenai task ini** — nol field number-series pada seluruh slice |

---

## 4. Dokumentasi endpoint

#### Health Services / Blood Bank Management / Blood Group Exam

Base URL: `api/v1/health-services/blood-bank-management/blood-group-exams`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/conflict-resolution` | Menutup perbedaan hasil golongan darah dengan menunjuk pemeriksaan ulang tervalidasi | `BloodGroupExam : ResolveConflict` |

Kode balasan gagal: `400` alasan tidak sah · `403` bukan validator klinis (`VAL-BD-069`, oleh
`AccessPermissionFilter`) · `404` pemeriksaan pemutus tidak ada pada pasien itu · `422` prasyarat
tidak terpenuhi (`VAL-BD-051`, `VAL-BD-054`).

Delapan endpoint lain pada controller yang sama adalah lingkup `BE-BD-005`; lihat laporannya.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | Berhasil — `0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet test --filter BankDarah` | `Failed: 0, Passed: 134, Total: 134` | `PASS` | Keluaran perintah |
| Menutup perbedaan lewat pemeriksaan ulang tervalidasi | Tepat satu hasil sah kembali; catatan tersimpan | `PASS` | `Penyelesaian_MenunjukPemeriksaanUlangTervalidasi_MengembalikanTepatSatuHasilSah` |
| Hasil ulang bernilai ketiga | Diterima | `PASS` | `Penyelesaian_HasilUlangBernilaiKetiga_TetapDiterima` |
| Menutup tanpa menunjuk pemeriksaan ulang | Ditolak; nol catatan tertulis; perbedaan tetap tertahan | `PASS` | `Penyelesaian_TanpaMenunjukPemeriksaanUlang_Ditolak` |
| Menunjuk salah satu pihak perbedaan | Ditolak | `PASS` | `Penyelesaian_MenunjukSalahSatuPihakKonflik_Ditolak` |
| Menunjuk pemeriksaan ulang yang belum divalidasi | Ditolak | `PASS` | `Penyelesaian_PemeriksaanUlangBelumTervalidasi_Ditolak` |
| Sistem menghitung mayoritas | Tidak terjadi walaupun mayoritas 2:1 tersedia | `PASS` | `Penyelesaian_SistemTidakPernahMenghitungMayoritas` |
| Permintaan tidak punya mode otomatis | Terbukti hanya tiga field | `PASS` | `Penyelesaian_PermintaanTidakPunyaModePemilihanOtomatis` |
| Alasan berupa ketikan bebas | Ditolak | `PASS` | `Penyelesaian_AlasanDiLuarDaftarTerkendali_Ditolak` |
| Pasien tidak sedang menahan perbedaan | Ditolak | `PASS` | `Penyelesaian_PasienTidakSedangMenahanPerbedaan_Ditolak` |
| `UnitTests.InMemory` | `Failed: 9, Passed: 896` | `EXISTING / ENVIRONMENT ISSUE` | Kesembilannya milik `BillingManagement`, bawaan — bukti tiga lapis di laporan `BE-BD-005` bagian 5 |
| Eksekusi migration ke database | Tidak dijalankan | `NOT RUN` | Wewenang terpisah |

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta database; migration belum dijalankan.

**Tidak dijalankan:** `IntegrationTests.Postgres` (menuntut database berjalan) dan uji lewat Swagger
(`HasAccessAsync` memulangkan `true` untuk SuperAdmin sebelum satu baris hak akses dibaca, sehingga
tidak membuktikan apa pun tentang pemisahan butir).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-036` — perbedaan diselesaikan peran validator; tepat satu hasil sah kembali; pelaku, alasan, waktu tersimpan; riwayat kedua hasil tetap terbaca | **Terpenuhi** | `Penyelesaian_MenunjukPemeriksaanUlangTervalidasi_MengembalikanTepatSatuHasilSah` — memeriksa keempat tuntutan itu satu per satu, termasuk kedua nilai lama masih utuh |
| `AC-BD-037` — perbedaan dicoba diselesaikan oleh peran yang bukan validator; ditolak | **Terpenuhi pada tingkat penegakan atribut** | Endpoint dijaga `[AccessPermission("BloodGroupExam", "ResolveConflict")]`; penolakan `403` dilakukan `AccessPermissionFilter` sebelum service dipanggil. Batas pembuktiannya disebut di bagian 8 |
| `AC-BD-051` — diselesaikan tanpa mencatat pemeriksaan ulang baru; ditolak | **Terpenuhi** | `Penyelesaian_TanpaMenunjukPemeriksaanUlang_Ditolak` dan `Penyelesaian_MenunjukSalahSatuPihakKonflik_Ditolak` |
| `AC-BD-053` — hasil ulang bernilai ketiga dinyatakan berlaku; diterima | **Terpenuhi** | `Penyelesaian_HasilUlangBernilaiKetiga_TetapDiterima` |
| `AC-BD-054` — ditutup dengan sistem memilih "mayoritas" otomatis; ditolak | **Terpenuhi** | `Penyelesaian_SistemTidakPernahMenghitungMayoritas` membuktikannya pada keadaan yang **punya** mayoritas 2:1, dan `Penyelesaian_PermintaanTidakPunyaModePemilihanOtomatis` membuktikan permintaannya memang tidak punya field untuk memintanya |
| `AC-BD-079` — validator klinis menutup perbedaan dengan menunjuk pemeriksaan ulang tervalidasi; berhasil, seluruh hasil tetap terbaca | **Terpenuhi** | Pengujian yang sama dengan `AC-BD-036` |
| `AC-BD-080` — validator klinis mencoba menutup **tanpa** pemeriksaan ulang tervalidasi; ditolak | **Terpenuhi** | `Penyelesaian_TanpaMenunjukPemeriksaanUlang_Ditolak` — pelakunya adalah validator klinis, dan tetap ditolak. Inilah bukti bahwa wewenang tidak menggantikan prasyarat |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Seluruh AC lulus | **Terpenuhi**, dengan batas `AC-BD-037` yang disebut apa adanya di atas |
| Butir hak akses `ResolveConflict` terdaftar dan terpisah dari `Validate` | **Terpenuhi** — `ValidasiRutinDanPenyelesaianKonflik_DijagaDuaButirBerbedaPadaController` |
| Membuka `FE-BD-009` | **Terpenuhi** — endpoint dan kontrak balasannya tersedia |
| Laporan tracked ditulis | **Terpenuhi** — berkas ini |

---

## 7. Delta kontrak

| Delta | Isi | Alasan |
| --- | --- | --- |
| **Kategori alasan belum ditetapkan** | `BbkBloodGroupConflictResolution.ReasonCode` wajib dan merujuk `MstBloodBankReason.ReasonCode`, tetapi kontrak `v4` **tidak menetapkan kategori mana** yang berlaku untuk penyelesaian perbedaan golongan darah. Kesepuluh kategori yang ada seluruhnya menyangkut order dan kantong | Service menuntut kode alasan yang **ada dan aktif**, tanpa memaksakan kategori. Menambah kategori ke-11 adalah keputusan pemilik proses, bukan keputusan builder — mengarangnya berarti menetapkan kebijakan yang belum diputuskan siapa pun |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan baru dari berkas task ini |
| Masalah yang diketahui | `AC-BD-037` dan `AC-BD-078` terbukti pada tingkat **atribut hak akses**, bukan pada percobaan panggilan ujung-ke-ujung oleh pengguna berperan berbeda. Pembuktian penuh menuntut aplikasi berjalan beserta database dan dua akun ber-hak-akses berbeda — dan **tidak boleh** memakai SuperAdmin, karena `HasAccessAsync` meloloskannya sebelum satu baris hak akses dibaca. Disarankan menjadi butir uji penerimaan saat migration dijalankan |
| Risiko tersisa | **Migration belum dijalankan**, sehingga endpoint ini belum dapat dipakai. Sampai kategori alasan diputuskan, petugas akan memilih alasan dari kategori yang secara makna tidak persis menggambarkan penyelesaian perbedaan golongan darah |
| Perubahan sampingan | `NONE` |
| Interupsi | Sama dengan `BE-BD-005`, dikerjakan pada sesi yang sama — lihat laporannya bagian 8 |
| Status Git | Sama dengan `BE-BD-005` bagian 9; kedua task berbagi satu kumpulan berkas |
| Langkah berikutnya | **(1)** `FE-BD-009` kini terbuka. **(2)** Putuskan kategori alasan penyelesaian perbedaan golongan darah. **(3)** Jadwalkan eksekusi migration Bank Darah secara terkoordinasi, lalu buktikan `AC-BD-037`/`AC-BD-078` ujung-ke-ujung dengan akun non-SuperAdmin |
