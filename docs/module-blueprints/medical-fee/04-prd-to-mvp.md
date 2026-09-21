# Medical Fee — PRD ke MVP

| Field | Nilai |
|---|---|
| Kontrak | `MDF-MVP-1.0` — `locked` 20 September 2026 |
| Blueprint ID | `MF-BP-001` |
| Masukan | 17 keputusan bisnis `approved`, `MDF-DES-001`..`018` `draft` |
| Tanggal | 20 September 2026 |

---

## 1. Masalah yang dipecahkan, dalam angka

Dari rangkuman transcript meeting RS MMC 15 Juli 2026:

| Keadaan sekarang | Angka |
|---|---|
| Tenaga medis yang jasanya dihitung tiap bulan | ±220 orang |
| Waktu yang dihabiskan tiap bulan | ±4 hari kerja |
| Tempat perhitungan sebenarnya terjadi | Excel "satelit", di luar sistem |
| Kelompok yang seluruhnya manual | Laboratorium, Radiologi, Anestesi, Digestive, Urologi |
| Risiko yang diakui sendiri oleh tim | Dokter dibayar dobel, atau justru terlewat |

Modul ini menghentikan dua risiko terakhir dengan membuat setiap rupiah dapat ditelusuri ke
layanan, peran, dan baris tarif asalnya — dan membuat layanan yang **tidak** dapat dihitung
terlihat, alih-alih hilang.

## 2. Batas rilis pertama

| Masuk rilis pertama | Tidak masuk |
|---|---|
| Daftar peran seragam lintas sumber | Jasa radiologi (`MF-DEC-015`) |
| Kesepakatan tarif sharing menunjuk kontrak HR | Potongan PPh 21, kasbon, iuran (milik Finance) |
| Perhitungan jasa dari kamar operasi — tim penuh | Pembayaran ke tenaga medis (milik Finance) |
| Perhitungan dari tindakan klinis dan laboratorium — pelaksana tunggal | Pembagian tim di luar kamar operasi (`MF-CQ-07`) |
| Daftar layanan yang belum dapat dihitung | Jasa dari entri bebas kasir (`MF-CQ-05`) |
| Periode: buka, hitung, verifikasi, setujui, tutup | Alir nilai jasa ke `BilInvoiceItem.DoctorShare` (`MF-CQ-08`) |
| Koreksi berjenjang maker-checker | Portal mandiri tenaga medis |
| Penyerahan ke Finance | Laporan analitik |

## 3. Epic

| Epic | Judul | Status | Menunggu |
|---|---|---|---|
| `MDF-01` | Fondasi modul dan pendaftaran registry | Siap | — |
| `MDF-02` | Daftar peran seragam | Siap | — |
| `MDF-03` | Kesepakatan tarif sharing dan baris tarif berversi | Siap | — |
| `MDF-04` | Periode dan perhitungan dari kamar operasi | Siap | — |
| `MDF-05` | Perhitungan dari tindakan klinis dan laboratorium | **Sebagian** | Pelaksana tunggal siap; pembagian tim menunggu `MF-CQ-07` |
| `MDF-06` | Daftar layanan yang belum dapat dihitung | Siap | — |
| `MDF-07` | Jasa dari entri bebas kasir | **`OPEN DECISION`** | `MF-CQ-05` |
| `MDF-08` | Verifikasi, persetujuan, dan penutupan periode | Siap | — |
| `MDF-09` | Koreksi berjenjang | Siap | — |
| `MDF-10` | Penyerahan ke Finance | Siap | — |
| `MDF-11` | Alir nilai jasa ke `DoctorShare` | **`OPEN DECISION`** | `MF-CQ-08` |
| `MDF-12` | Tenaga medis melihat jasanya sendiri | Siap | — |

Dua epic `OPEN DECISION` MUST NOT masuk gelombang pengerjaan mana pun sebelum jawabannya turun.

## 4. Gelombang pengerjaan

| Gelombang | Epic | Hasil yang dapat diuji |
|---|---|---|
| **MVP-0 — Fondasi** | `MDF-01`, `MDF-02` | Modul terdaftar; daftar peran dapat diisi dan dipetakan ke peran kamar operasi |
| **MVP-1 — Aturan tarif** | `MDF-03` | Kesepakatan tarif tersusun, menunjuk kontrak HR, berversi menurut masa berlaku |
| **MVP-2 — Perhitungan** | `MDF-04`, `MDF-05` (sebagian), `MDF-06` | Satu periode dapat dihitung dari kamar operasi, tindakan klinis, dan laboratorium; yang tidak dapat dihitung terlihat sebagai daftar |
| **MVP-3 — Persetujuan** | `MDF-08`, `MDF-09` | Periode dapat diverifikasi, disetujui, dan ditutup dengan pemisahan wewenang; koreksi berjalan |
| **MVP-4 — Penyerahan** | `MDF-10` | Hasil jasa yang disetujui terbaca Finance dan di-ACK |
| **MVP-5 — Keterbukaan** | `MDF-12` | Tenaga medis melihat jasanya sendiri beserta rinciannya |
| **Tertahan** | `MDF-07`, `MDF-11`, sisa `MDF-05` | Menunggu `MF-CQ-05`, `MF-CQ-08`, `MF-CQ-07` |

MVP-0 MUST selesai lebih dulu — tanpa pendaftaran registry, file model pertama pun tidak boleh
ditulis (`MDF-DES-002`).

MVP-1 MUST mendahului MVP-2: tanpa baris tarif yang terisi, perhitungan tidak menghasilkan apa
pun selain daftar `RULE_MISSING` sepanjang jumlah layanan.

## 5. Rincian epic

### `MDF-01` — Fondasi modul

| Aspek | Isi |
|---|---|
| Keluaran | Pendaftaran registry, struktur folder, 9 `DbSet`, 7 `AddScoped`, 3 migration aditif |
| Prasyarat | Persetujuan `MDF-DES-001`..`007`; otorisasi migration terpisah |
| Selesai bila | Migration berjalan di lingkungan pengembangan; seluruh check constraint terpasang |

### `MDF-02` — Daftar peran

| Aspek | Isi |
|---|---|
| Keluaran | `MstMedicalFeeRole`, CRUD, pemetaan ke `OprTeamRole` |
| Aturan kunci | Satu nilai `OprTeamRole` dipetakan paling banyak satu peran; peran terpakai dinonaktifkan bukan dihapus |
| Selesai bila | Lima nilai `OprTeamRole` yang ada terpetakan, ditambah satu peran utama umum |

### `MDF-03` — Kesepakatan tarif sharing

| Aspek | Isi |
|---|---|
| Keluaran | `MdfSharingAgreement`, `MdfSharingRule`, pemilihan empat tingkat kekhususan, penggantian berversi |
| Aturan kunci | Menunjuk `WfpContractHistory`, tidak menyalin; perubahan tarif membuat baris baru |
| Selesai bila | Mengubah tarif tidak menggeser hasil periode yang sudah dihitung |

### `MDF-04` — Periode dan perhitungan kamar operasi

| Aspek | Isi |
|---|---|
| Keluaran | `MdfFeePeriod`, `MdfServiceFee`, `MdfServiceFeeDetail`, adapter sumber, perhitungan `Serializable` |
| Aturan kunci | Basis nilai kotor; porsi per peran; snapshot persentase dan `SharingRuleId` |
| Selesai bila | Satu tindakan operasi dengan tim tiga orang menghasilkan tiga rincian dengan porsi sesuai tarif |

### `MDF-05` — Tindakan klinis dan laboratorium

| Aspek | Isi |
|---|---|
| Keluaran siap | Pelaksana tunggal dari `TrxPatientProcedure.DoctorId` dan `LabOrder.ExaminerDoctorId` |
| Keluaran tertahan | Pembagian tim beserta peran — menunggu `MF-CQ-07` |
| Aturan kunci | Pemeriksa laboratorium kosong → `PERFORMER_MISSING`, bukan dilewati |

### `MDF-06` — Layanan belum dapat dihitung

| Aspek | Isi |
|---|---|
| Keluaran | `MdfUnresolvedService`, layar daftar, rekap per alasan, pengesampingan bercatatan |
| Aturan kunci | Tidak ada endpoint penghapus; menahan penutupan periode |
| Selesai bila | Periode dengan satu baris `Open` menolak ditutup dengan pesan yang menyebut jumlahnya |

### `MDF-08` — Verifikasi dan persetujuan

| Aspek | Isi |
|---|---|
| Keluaran | Siklus status periode dan hasil jasa, pemisahan wewenang tiga lapis |
| Selesai bila | Pengguna yang memverifikasi ditolak saat mencoba menyetujui, walau izinnya diberikan paksa |

### `MDF-09` — Koreksi berjenjang

| Aspek | Isi |
|---|---|
| Keluaran | `MdfServiceFeeAdjustment`, pengajuan, persetujuan, penolakan |
| Aturan kunci | `RequestedBy <> ApprovedBy`; koreksi final tidak dapat diubah; `FinalAmount` tidak boleh negatif |

### `MDF-10` — Penyerahan ke Finance

| Aspek | Isi |
|---|---|
| Keluaran | `MdfFinanceHandoff`, dibuat di transaksi persetujuan, ACK, kegagalan, pengiriman ulang |
| Aturan kunci | Nilai kotor; `HandoffKey` tetap saat dikirim ulang |

### `MDF-12` — Tenaga medis melihat jasanya

| Aspek | Isi |
|---|---|
| Keluaran | Penyaringan data di service terhadap identitas pengguna |
| Aturan kunci | Pembatasan data, bukan sekadar pembatasan endpoint |
| Selesai bila | Dokter A yang memanggil endpoint dengan `payeeReferenceId` dokter B menerima daftar kosong, bukan data B |

## 6. UAT

| # | Skenario | Hasil yang diharapkan | Epic |
|---:|---|---|---|
| 1 | Susun peran dan petakan `PrimarySurgeon` | Tersimpan; pemetaan kedua ke nilai sama ditolak | `MDF-02` |
| 2 | Susun kesepakatan tarif menunjuk kontrak yang sudah berakhir | Ditolak `MDF_AGREEMENT_CONTRACT_MISMATCH` | `MDF-03` |
| 3 | Aktifkan kesepakatan tanpa baris tarif | Ditolak `MDF_AGREEMENT_NO_RULES` | `MDF-03` |
| 4 | Tambah baris tarif yang membuat jumlah peran melebihi 100% | Ditolak `MDF_RULE_PERCENTAGE_EXCEEDED` | `MDF-03` |
| 5 | Ganti tarif 40% menjadi 45% mulai bulan depan, lalu buka hasil bulan lalu | Angka bulan lalu tidak berubah | `MDF-03` |
| 6 | Hitung periode dengan satu operasi bertim tiga orang | Tiga rincian, porsi sesuai peran, jumlah = `GrossAmount` | `MDF-04` |
| 7 | Buka rincian satu hasil jasa | Setiap baris menyebut layanan, peran, persentase, dan nomor kesepakatannya | `MDF-04` |
| 8 | Hitung periode dengan `LabOrder` tanpa pemeriksa | Muncul di daftar belum terhitung `PERFORMER_MISSING` | `MDF-05`, `MDF-06` |
| 9 | Tutup periode yang masih punya satu baris `Open` | Ditolak, pesan menyebut jumlahnya | `MDF-06`, `MDF-08` |
| 10 | Lengkapi pemeriksa lalu hitung ulang | Jasa terbit; baris belum terhitung tidak lagi menahan | `MDF-05`, `MDF-06` |
| 11 | Hitung ulang periode dua kali berturut-turut | Hasil identik; tidak ada rincian ganda | `MDF-04` |
| 12 | Layanan dengan diskon promo pasien | `BaseAmount` tetap nilai kotor, tidak berkurang | `MDF-04` |
| 13 | Verifikasi lalu setujui dengan pengguna yang sama | Ditolak `MDF_MAKER_CHECKER_VIOLATION` | `MDF-08` |
| 14 | Ajukan lalu setujui koreksi dengan pengguna yang sama | Ditolak di service **dan** di database | `MDF-09` |
| 15 | Koreksi pengurang melebihi nilai jasa | Ditolak `422` | `MDF-09` |
| 16 | Setujui periode | Satu handoff per hasil jasa, nilainya kotor | `MDF-10` |
| 17 | Kirim ulang handoff yang gagal | `HandoffKey` sama persis | `MDF-10` |
| 18 | Ulangi perhitungan dengan `Idempotency-Key` sama | Hasil sama, tidak menggandakan | `MDF-04` |
| 19 | Dua pengguna menyetujui periode bersamaan | Satu berhasil, satu `409` | `MDF-08` |
| 20 | Dokter A meminta hasil jasa dokter B | Kosong, bukan data B | `MDF-12` |
| 21 | Kesampingkan layanan belum terhitung tanpa catatan | Ditolak `MDF_WAIVER_NOTE_REQUIRED` | `MDF-06` |
| 22 | Hapus peran yang sudah dipakai baris tarif | Ditolak `MDF_ROLE_IN_USE` | `MDF-02` |

## 7. Definition of Done

| # | Kriteria |
|---:|---|
| 1 | `MedicalFeeManagement` dan prefix `Mdf` terdaftar di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` **sebelum** file model pertama |
| 2 | Seluruh 9 entity mewarisi `IdentityModel`; tidak ada hard delete |
| 3 | Seluruh status `string` + `static class ...Statuses` + check constraint |
| 4 | Seluruh aggregate root punya `Guid RowVersion`; perintah pengubah memeriksanya |
| 5 | Perintah pengubah nilai uang menerima `Idempotency-Key` |
| 6 | Perhitungan periode berjalan `IsolationLevel.Serializable` |
| 7 | Maker-checker ditegakkan di service **dan** check constraint pada tiga pasangan di `permission-audit-matrix.md` bagian 3 |
| 8 | Seluruh partial unique index memakai `WHERE "IsDelete" = false` |
| 9 | Seluruh kolom uang `HasPrecision(18, 2)`; persentase `HasPrecision(5, 2)` |
| 10 | `MdfServiceFeeDetail.SharingRuleId` wajib — setiap rupiah tertelusur |
| 11 | Seluruh endpoint memakai `ApiResponse<T>`, route hyphenated, `[AccessPermission]` |
| 12 | Penyaringan data untuk tenaga medis diuji, bukan hanya izin endpoint |
| 13 | 22 skenario UAT lulus |
| 14 | Migration aditif; nol tabel modul lain diubah |
| 15 | Tidak ada kode untuk `MDF-07`, `MDF-11`, atau radiologi |

Kriteria 15 sengaja ditulis sebagai kriteria selesai: mengerjakan yang tertahan lebih awal
adalah kegagalan, bukan kelebihan — jawabannya bisa membatalkan bentuk yang dipilih.

## 8. Risiko

| Risiko | Dampak | Mitigasi |
|---|---|---|
| `MF-CQ-08` dijawab dengan bentuk selain rekomendasi | `MDF-11` dirancang ulang | Epic itu sudah dikeluarkan dari seluruh gelombang |
| Dokter tamu tidak punya `WfpContractHistory` | Kelompok yang tarifnya paling beragam tidak dapat dihitung | Pertanyaan sudah dikirim ke owner HR; jawabannya menentukan apakah `MdfSharingAgreement` perlu jalur tanpa kontrak |
| Kesepakatan tarif ±220 orang belum terisi saat modul aktif | Seluruh layanan jatuh ke `RULE_MISSING` | Pengisian dijadikan pekerjaan terjadwal, bukan efek samping MVP-1 |
| Daftar belum terhitung terlalu panjang di bulan pertama | Periode tidak dapat ditutup | Pengesampingan bercatatan tersedia, dan justru untuk inilah ia ada |
| Clinical dan Laboratory tidak menambahkan pencatatan tim | Jasa kelompok itu tetap dihitung sebagai pelaksana tunggal | Perilaku pelaksana tunggal sudah dirancang dan berjalan tanpa `MF-CQ-07` |
