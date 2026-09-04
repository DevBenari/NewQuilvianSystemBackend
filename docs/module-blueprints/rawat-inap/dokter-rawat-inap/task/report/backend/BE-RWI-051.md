# Laporan Perubahan Backend — `BE-RWI-051`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-051` |
| Judul | Tindakan dokter tercatat dan tagihannya tidak pernah ganda |
| Slice | `DOK-MVP-5` — resep, tindakan, penunjang |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-051` |
| Trace | `FR-DOK-033`, `FR-DOK-034`; `INV-DOK-09`; `AC-CAP024-01`, `AC-CAP024-02`; `contracts/integration-contract.md` §3; `contracts/api-contract.md` §5; `contracts/state-transition-matrix.md` §4.1; `VAL-DOK-26` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-040` 🟡 **sebagian** ([laporan](BE-RWI-040.md)) — kolom konteks, tautan visite, dan kunci permintaan sudah ada. `BE-RWI-044` **selesai** ([laporan](BE-RWI-044.md)). `BE-RWI-048` 🟡 **sebagian** ([laporan](BE-RWI-048.md)) |
| Klasifikasi | `MEDIUM`, skor 9: repository 0, berkas diperiksa 1, berkas diubah 2, logika bisnis 3, kontrak API 2, database 0, keamanan/auth 1, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `ClinicalManagement`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9be5526d248d9813a4044f063e43066a2364dd7d` pada branch `MHamzah` |
| Tanggal | 4 September 2026 |
| Status | 🟡 **Sebagian.** Kelima acceptance criteria terbukti, termasuk uji kegagalan Billing. Nol migration. Verifikasi yang diminta roadmap menyebut **PostgreSQL** untuk percobaan ulang; yang dijalankan SQLite, karena tidak ada database uji yang tersedia — lihat bagian 5.1 |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Cli` — `ACTIVE / LEGACY` |
| Applicability | `TOUCHED LEGACY`. Controller dan DTO tindakan adalah kode lama |
| QBE berlaku | `QBE-API-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-TXN-001`, `QBE-PAGE-001`, `QBE-PERM-001` |
| Entity operasional baru | `NONE`. Kolom `InpEpisodeId`, `PhysicianVisitId`, dan `IdempotencyKey` sudah dibuat `BE-RWI-040` |
| Archetype | Transaksi. Satu endpoint baca baru ber-scope perawatan; nol endpoint baru yang mengubah status |
| Database authority | `NOT APPLICABLE`. Nol perubahan model, nol migration, nol eksekusi database |
| Frontend | Diperiksa read-only. Tidak ada berkas frontend yang diubah |

---

## 1. Masalah yang diperbaiki

**Tindakan rawat inap tidak dapat dibuktikan miliknya perawatan mana, dan percobaan ulang
berujung pada tagihan ganda.**

Tiga hal yang belum ada sebelum task ini:

1. **Penanda perawatan tidak pernah diisi.** Kolomnya sudah dibuat `BE-RWI-040`, tetapi jalur
   pembuatan tindakan belum menstempelnya, sehingga tindakan rawat inap tidak dapat dibaca per
   perawatan.
2. **Tidak ada penjaga salah pasien.** Layar dokter membuka satu pasien lalu mengirim tindakan;
   bila pasien pada layar dan pasien pada kunjungan berbeda, tindakan tercatat pada rekam medis
   orang lain.
3. **Tidak ada kunci permintaan, dan penandaan dikerjakan dapat diulang.** Keduanya berujung
   sama: fakta klinis kedua diterbitkan ke Billing, dan pasien membayar dua kali untuk satu
   tindakan.

---

## 2. Proses bisnis

### 2.1 Urutan yang tidak boleh dibalik

`INV-DOK-09`. Catatan klinis disimpan **lebih dulu**; fakta ke Billing diterbitkan sesudahnya.

Membalik urutannya berarti tindakan yang sudah dikerjakan pada pasien hilang dari rekam medis
hanya karena jaringan ke Billing sedang putus. Kegagalan sistem keuangan **tidak boleh** menghapus
bukti bahwa tindakan medis benar-benar terjadi.

Urutan itu sudah benar pada source sebelum task ini; yang ditambahkan adalah pendaftaran
keutuhan rekam medis pada `SaveChanges` yang sama — lihat [laporan `BE-RWI-038`](BE-RWI-038.md).

### 2.2 Keadaan pengiriman bukan status tindakan

| Keadaan pengiriman | Yang terjadi pada catatan klinis |
| --- | --- |
| Diterbitkan | Tidak berubah |
| Diputar ulang | Tidak berubah |
| Hasil tidak diketahui | **Tetap `Completed`** |
| Ditolak Billing | **Tetap `Completed`** |

Menyimpan keadaan pengiriman sebagai status tindakan akan membuat kegagalan sistem keuangan
terlihat seperti tindakan medis yang batal.

### 2.3 Dua jalur pencatatan, keduanya dipertahankan

| Jalur | Kapan dipakai | Status awal |
| --- | --- | --- |
| Direncanakan lebih dulu | Tindakan besar yang dijadwalkan | `Planned` |
| Langsung dicatat dikerjakan | Tindakan kecil di samping tempat tidur | `Completed` |

Memaksa salah satu jalur akan membuat dokter memalsukan alur demi bisa menyimpan.

### 2.4 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Pasien pada tindakan tidak cocok dengan pasien kunjungan | Ditolak beserta arahan memeriksa pasien yang sedang dibuka | `400` |
| Penanda perawatan tidak cocok dengan kunjungannya | Ditolak | `400` |
| Tautan visite bukan milik kunjungan yang sama | Ditolak | `400` |
| Kunci permintaan sama dengan yang sudah tersimpan | **Bukan galat.** Tindakan yang sudah ada dikembalikan | `200` |
| Tindakan yang sudah ditandai dikerjakan ditandai lagi | **Bukan galat.** Tidak ada fakta kedua yang diterbitkan | `200` |
| Billing gagal dihubungi | Tindakan **tetap tersimpan**; keadaan pengiriman tercatat tidak berhasil | `200` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` | Acceptance criteria dan DoD |
| `contracts/api-contract.md` §5 | Bentuk endpoint dan urutan yang mengikat |
| `contracts/state-transition-matrix.md` §4, §4.1 | Mesin status tindakan dan keadaan penerbitan fakta |
| `contracts/integration-contract.md` §3 | Kontrak penyerahan fakta klinis ke Billing |
| `Areas/HealthServices/ClinicalManagement/Services/ClinicalMilestoneFactProducer.cs` | Pendeteksian penerbitan identik dan penanganan kegagalan pengiriman |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs` | Kolom dari `BE-RWI-040` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientProcedureDtos.cs` | Permintaan pembuatan menerima pasien, penanda perawatan, tautan visite, dan kunci permintaan — keempatnya **opsional** |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` | Pemeriksaan kunci permintaan paling awal; penjaga penanda pasien, perawatan, dan visite; penstempelan ketiganya; penjaga penandaan dikerjakan berulang; endpoint `GET /episodes/{episodeId}`; pembacaan ringkasan tanpa menulis |
| `Tests/.../ClinicalManagement/InpatientProcedureTests.cs` | **Baru.** Tujuh uji |
| `Tests/.../Infrastructure/TindakanTestData.cs` | Menambahkan tarif rumah sakit pada tindakan master uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Empat ruas **opsional** pada `POST /` sesuai `api-contract.md` §5. Satu endpoint baca baru `GET /episodes/{episodeId}`. Satu perilaku baru pada `PATCH /{id}/execute`: penandaan berulang dijawab `200` tanpa menerbitkan fakta kedua |
| Database | Nol perubahan schema, nol migration, nol eksekusi database |
| Keamanan/Auth | Nol butir hak akses baru; endpoint baru memakai `PatientProcedure : Read` yang sudah ada. Penjaga salah pasien dan salah perawatan adalah aturan bisnis, bukan hak akses |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Procedure

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Mencatat tindakan. **Perubahan:** menerima pasien, penanda perawatan, tautan visite, dan kunci permintaan | `PatientProcedure : Create` |
| `PATCH` | `/{id}/execute` | Menandai tindakan dikerjakan lalu menerbitkan fakta klinis. **Perubahan:** penandaan berulang tidak menerbitkan fakta kedua | `PatientProcedure : Update` |
| `GET` | `/episodes/{episodeId}` | Tindakan satu perawatan, terurut waktu tindakan | `PatientProcedure : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | `0 Error(s)`, `185 Warning(s)` | `PASS` | Keluaran perintah |
| `dotnet test` project uji SQLite | `Failed: 0, Passed: 320` | `PASS` | Keluaran perintah |
| Pasien dan kunjungan tidak cocok ditolak | `400`, pesan memuat "tidak sesuai dengan pasien"; nol tindakan tersimpan | `PASS` | `PasienDanKunjunganTidakCocok_Ditolak400` |
| Penanda perawatan tidak cocok ditolak | `400`; nol tindakan tersimpan | `PASS` | `PenandaPerawatanTidakCocok_Ditolak400` |
| Percobaan ulang tidak menggandakan tindakan maupun fakta | Dua `200` dengan identitas identik; satu baris tindakan; **satu** baris fakta klinis setelah dua kali penandaan dikerjakan | `PASS` | `PercobaanUlangBerkunciSama_TidakMenghasilkanTindakanMaupunFaktaGanda`, dijalankan terhadap **SQLite** |
| **Percobaan ulang terhadap PostgreSQL sungguhan** | **Belum dijalankan** | `NOT RUN` | Tidak ada database uji yang tersedia: `BLOCKED_BY_TEST_DB_CONFIGURATION`. Unique index parsial `IdempotencyKey` pada `TrxPatientProcedure` sudah ada sejak `BE-RWI-040`, tetapi belum pernah diuji terhadap PostgreSQL |
| Billing gagal dihubungi → catatan tetap tersimpan, penerbitan tercatat gagal | Tindakan tetap `Completed` dan tetap ditandai dikerjakan; baris fakta berstatus bukan terkirim, dengan kode hasil terisi | `PASS` | `BillingGagalDihubungi_CatatanTindakanTetapTersimpanDanPenerbitanTercatatGagal` |
| Kedua jalur pencatatan dipertahankan | Jalur rencana menghasilkan `Planned`; jalur langsung menghasilkan `Completed`; dua baris tersimpan | `PASS` | `KeduaJalurPencatatanTindakan_TetapBerjalan` |
| Tautan visite opsional, wajib cocok ketika dikirim | Tanpa tautan diterima; tautan milik kunjungan sama tersimpan; tautan milik kunjungan lain ditolak `400` | `PASS` | `TautanKejadianVisite_OpsionalTetapiWajibCocokKetikaDikirim` |
| Tindakan satu perawatan terbaca dari konteks perawatannya | Tiga tindakan terbaca, seluruhnya milik kunjungan yang sama | `PASS` | `TindakanSatuPerawatan_TerbacaDariKonteksPerawatan` |
| `dotnet test` project uji InMemory | `Failed: 1, Passed: 908` | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan `BillingFinalizationServiceTests`, berkas tidak disentuh task ini |
| `dotnet test` project uji PostgreSQL | `Failed: 54, Passed: 34` | `EXISTING / ENVIRONMENT ISSUE` | Satu sebab: `BLOCKED_BY_TEST_DB_CONFIGURATION` |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** migration dan perintah basis data apa pun.

### 5.1 Kenapa task ini ditandai sebagian

Verification pada roadmap menyebut "integration test terhadap **PostgreSQL** untuk percobaan
ulang". Yang dijalankan adalah uji terhadap SQLite, sehingga butir verifikasi itu **belum
terpenuhi apa adanya**.

Yang sudah terbukti pada SQLite adalah pencegahan penggandaan di **lapisan aplikasi**:
pemeriksaan kunci permintaan sebelum pembuatan, dan penjaga penandaan dikerjakan berulang.
Keduanya berperilaku sama pada SQLite maupun PostgreSQL.

Yang **belum** terbukti adalah lapisan basis datanya: dua permintaan yang tiba benar-benar
bersamaan hanya dapat ditolak unique index parsial `IdempotencyKey`, dan index itu memakai
filter khas PostgreSQL. Pengujiannya terhalang `BLOCKED_BY_TEST_DB_CONFIGURATION`, sama seperti
pada [`BE-RWI-048`](BE-RWI-048.md).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tindakan untuk pasangan pasien dan kunjungan yang tidak cocok ditolak `400` | Terpenuhi | `PasienDanKunjunganTidakCocok_Ditolak400` |
| 2. Percobaan ulang tidak menghasilkan tindakan maupun fakta klinis ganda | Terpenuhi | `PercobaanUlangBerkunciSama_TidakMenghasilkanTindakanMaupunFaktaGanda` |
| 3. Saat Billing gagal dihubungi, catatan tindakan **tetap tersimpan** dan hasil penerbitannya tercatat gagal | Terpenuhi | `BillingGagalDihubungi_CatatanTindakanTetapTersimpanDanPenerbitanTercatatGagal` |
| 4. Kedua jalur pencatatan dipertahankan | Terpenuhi | `KeduaJalurPencatatanTindakan_TetapBerjalan` |
| 5. Tautan ke kejadian visite bersifat opsional | Terpenuhi | `TautanKejadianVisite_OpsionalTetapiWajibCocokKetikaDikirim` |

### Definition of Done

| Butir | Status |
| --- | --- |
| Kelima acceptance criteria terbukti | ✅ |
| Test kegagalan Billing hijau | ✅ `BillingGagalDihubungi_...` |
| Verifikasi percobaan ulang terhadap PostgreSQL | ⛔ **Belum.** Lihat bagian 5.1 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan build baru |
| **Cara kegagalan Billing dipaksa** | Jalur menuju Billing diputus dengan menyuntikkan konteks basis data yang sudah ditutup ke layanan folio, sehingga pemanggilannya melempar. Mesin fakta menangkapnya, menandai hasil pengiriman tidak diketahui, dan menyimpan barisnya. Yang dibuktikan bukan pesan galatnya, melainkan keadaan sesudahnya |
| **Perilaku baru yang dilaporkan** | Penandaan tindakan dikerjakan kini **idempoten**: tindakan yang sudah `Completed` dijawab `200` tanpa menerbitkan fakta kedua. Sebelum task ini, penandaan berulang menerbitkan revisi fakta baru setiap kali. Perubahan ini diperlukan kriteria 2 dan tidak merugikan pemanggil lama, karena hasilnya tetap `200` |
| **Perubahan pada data uji bersama** | `TindakanTestData.BuatTindakanMaster` kini membuat kategori tarif dan tarif rumah sakit. Tanpa keduanya, pembuatan tindakan ditolak "Tarif rumah sakit untuk tindakan belum dikonfigurasi", dan penolakan itu menyamarkan hal yang sedang diuji |
| Masalah yang diketahui | Kunci permintaan bersifat **opsional**, sama seperti pada resep. Pemanggil yang tidak mengirimnya tetap dapat melahirkan tindakan ganda saat jaringan terputus, walaupun penandaan dikerjakan yang berulang sudah aman |
| Risiko tersisa | Penjaga salah pasien hanya berlaku ketika layar **mengirim** penanda pasien. Layar yang tidak mengirimnya tetap dilayani, dengan pasien diturunkan dari kunjungannya — perilaku lama yang sengaja tidak diubah supaya pemanggil poliklinik dan IGD tidak terputus |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Bersih sebelum task; tidak ada stage, commit, maupun push |
| Langkah berikutnya | Menyediakan `QUILVIAN_BILLING_TEST_DB` yang menunjuk database uji tersendiri, lalu menjalankan uji percobaan ulang terhadap PostgreSQL untuk menaikkan task ini menjadi ✅. Layar rawat inap dianjurkan selalu mengirim penanda pasien, penanda perawatan, dan kunci permintaan |
