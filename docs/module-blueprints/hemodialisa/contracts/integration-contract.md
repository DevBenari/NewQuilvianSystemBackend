# Hemodialisa — Kontrak Integrasi

| Field | Value |
|---|---|
| Blueprint ID | `HMD-BP-001` |
| Contract version | `HMD-CONTRACT-v1` — `last_changed_in: HMD-CONTRACT-v1`, status `approved` |
| Owner | Muhammad Hamzah |
| `approved_by` / `approved_at` | Muhammad Hamzah / 2026-09-18T15:40:07+07:00 |
| `input_revision` | `01-existing-capability-map.md` r2; `02-backend-architecture.md` r1 |

Dokumen ini menetapkan **arah baca dan tulis antar modul**. Ia tidak mengulang daftar endpoint;
itu milik `contracts/api-contract.md`.

**Hemodialisa tidak memanggil satu pun sistem di luar rumah sakit pada Phase 1.** Tidak ada
SATUSEHAT, tidak ada BPJS, tidak ada integrasi perangkat mesin. Ketiganya berada di Phase 2 dan
Phase 3 (`HMD-DEC-002`). Seluruh isi dokumen ini adalah integrasi **antar modul di dalam satu
aplikasi**.

---

## 1. Ringkasan arah data

| Modul lawan | Hemodialisa membaca | Hemodialisa menulis | Sifat |
|---|---|:---:|---|
| Patient Management | Identitas pasien | Tidak | Sinkron, hanya baca |
| Registration Management | Kunjungan, penjamin | Tidak | Sinkron, hanya baca |
| Inpatient Management | Episode rawat inap | Tidak | Sinkron, hanya baca, opsional |
| Human Resource | Dokter, profil petugas, kewenangan klinis | Tidak | Sinkron, hanya baca |
| Master Data | Unit layanan, ruang, tindakan | Tidak | Sinkron, hanya baca |
| Clinical Management | Persetujuan tindakan, tanda vital | **Ya** — tanda vital dan tindakan pasien | Sinkron, baca dan tulis |
| Laboratorium | Hasil pemeriksaan | Tidak | Sinkron, hanya baca rujukan |
| Farmasi | — | **Ya** — pemakaian obat | Sinkron |
| Rekam Medis | Status keutuhan dokumen, koreksi | **Ya** — pendaftaran dan penandatanganan dokumen | Sinkron, baca dan tulis |
| Billing dan Kasir | Status penyerahan | **Ya** — fakta tindakan selesai | **Asinkron terhadap finalisasi** |

Satu-satunya baris yang **asinkron** adalah Billing, dan itu disengaja. Alasannya ada di
bagian 6.

---

## 2. Registration Management — konteks kunjungan

**Arah:** Hemodialisa membaca. Tidak pernah menulis.

| Yang dibaca | Dipakai untuk |
|---|---|
| `RegPatientEncounter` beserta `PatientId`, `EncounterType`, `EncounterStatus` | Memastikan pasien memang sedang berkunjung, dan jenis kunjungannya apa |
| `RegPatientEncounterGuarantor` | Menampilkan penjamin pada kepala layar, sebagai informasi |

**Aturan yang mengikat:**

- Hemodialisa **tidak** membuat jenis kunjungan baru. Pasien HD tetap masuk sebagai rawat jalan,
  rawat inap, atau gawat darurat sesuai konteksnya.
- Sesi yang baru berupa jadwal **boleh** belum punya kunjungan. Kunjungan **wajib** ada sebelum
  sesi dimulai, dan diperiksa **ulang** tepat sebelum tombol Mulai dijalankan.
- Bila kunjungan gagal diverifikasi, berlaku `HMD-VAL-900`: tidak ada penulisan data klinis yang
  diterima, dan data pasien sebelumnya tidak boleh tetap tampil.

**Contoh mengapa pemeriksaan ulang penting:** koordinator menjadwalkan Bapak Darma pukul 07.00.
Pukul 06.50 petugas pendaftaran membatalkan kunjungannya karena salah input. Pukul 07.05 perawat
menekan Mulai. Tanpa pemeriksaan ulang, sesi akan berjalan di atas kunjungan yang sudah batal,
dan tagihannya kelak tidak punya tempat menempel.

---

## 3. Clinical Management — tanda vital, persetujuan, dan tindakan

**Arah:** dua arah.

| Yang dibaca | Dipakai untuk |
|---|---|
| `TrxPatientConsent` | Memeriksa apakah ada persetujuan tindakan yang sah untuk konteks sesi ini |
| `TrxPatientVitalSign` | Menampilkan tanda vital pasien |

| Yang ditulis | Kapan |
|---|---|
| `TrxPatientVitalSign` | Saat penilaian Pra-HD dan Pasca-HD disimpan, untuk tanda vital klinis |
| `TrxPatientProcedure` | Satu baris per sesi, dibuat saat sesi dimulai |

**Aturan yang mengikat:**

- Parameter mesin — kecepatan aliran darah, kecepatan dialisat, tekanan transmembran, tekanan
  vena dan arteri — **tidak** dipaksakan masuk ke `TrxPatientVitalSign`. Keduanya konsep berbeda;
  parameter mesin tinggal di `HmdSessionObservation`.
- Hemodialisa **tidak** membuat entity persetujuan sendiri. Yang ditanyakan hanya: apakah
  tersedia persetujuan yang sah untuk konteks sesi ini.
- Masa berlaku persetujuan **tidak** ditetapkan otomatis pada Phase 1, karena kebijakan rumah
  sakit tentang itu belum ada.

### Pemetaan dokter pada `TrxPatientProcedure`

Ini wujud `HMD-DEC-009`, dan menjadi kontrak yang mengikat:

| Kolom `TrxPatientProcedure` | Diisi dari | Wajib |
|---|---|:---:|
| `DoctorId` | `HmdSession.ResponsibleDoctorId` — dokter penanggung jawab sesi | Ya |
| `InstructingDoctorId` | `HmdPrescription.PrescribingDoctorId` — dokter pembuat resep | Tidak, tetapi selalu diisi oleh Hemodialisa |
| `OrderedByUserId` | Pengguna yang memulai sesi | Tidak |
| `EncounterId` | `HmdSession.EncounterId` | Ya |
| `IsBillable` | `false` bila sesi berakhir `Stopped`; `true` bila `Completed` | Ya |

Dua kolom dokter itu memang disediakan repository untuk keadaan "perawat mengerjakan atas
instruksi dokter", yang persis bentuk kerja Hemodialisa.

---

## 4. Laboratorium — hasil serologi

**Arah:** Hemodialisa membaca rujukan. Tidak pernah menulis, dan tidak pernah menyalin.

| Yang disimpan Hemodialisa | Yang **tidak** disimpan |
|---|---|
| Rujukan ke hasil pemeriksaan, tanggal hasil, ringkasan hasil sebagaimana dibaca petugas, status tinjauan, dan keputusan operasional yang diturunkan | Nilai hasil sebagai sumber kebenaran baru, rentang normal, status validasi laboratorium |

**Keterbatasan yang berlaku pada Phase 1:** Laboratorium hanya menyediakan pembacaan hasil per
pesanan dan per spesimen. Pembacaan per pasien belum ada (`HMD-DEP-001`). Akibatnya petugas HD
mencatat rujukan hasil secara manual ke `HmdSerologyReview`.

**Yang dilarang tegas:** membuat tabel salinan hasil laboratorium di Hemodialisa agar modul
terasa mandiri. Hasil laboratorium yang dikoreksi Laboratorium harus tetap terbaca sebagai
koreksi, bukan menjadi dua angka yang berbeda di dua modul.

---

## 5. Farmasi — pemakaian obat

**Arah:** Hemodialisa menulis fakta klinis, Farmasi memiliki stok.

| Langkah | Pemilik |
|---|---|
| Perawat mencatat obat apa, berapa, lewat jalur mana, oleh siapa, pukul berapa | Hemodialisa — `HmdSessionMedication` |
| Pengurangan stok, batch, dan penyaluran | Farmasi — `PhmDrugUsage` |
| Keputusan penagihan obat | Billing |

**Aturan yang mengikat:** Hemodialisa **tidak** mengurangi stok sendiri. Setelah catatan
pemberian tersimpan, faktanya diteruskan ke Farmasi. Bila penerusan gagal, catatan klinis
pemberian obat **tetap tersimpan** — obatnya memang sudah masuk ke tubuh pasien, dan menghapus
catatannya karena kegagalan teknis adalah kesalahan yang jauh lebih berbahaya.

---

## 6. Billing dan Kasir — penyerahan tindakan selesai

**Arah:** Hemodialisa menyerahkan fakta. Billing memiliki angka.

### Yang diserahkan

| Hal | Nilai |
|---|---|
| Sumber | `Procedure` — sumber yang **sudah terdaftar** pada kontrak Billing |
| Jenis efek | `ProcedureCharge` |
| Penanda sumber | Id `TrxPatientProcedure` milik sesi |
| Waktu kejadian | Waktu sesi difinalisasi |
| Jumlah dan satuan | Diambil dari `TrxPatientProcedure` |

**Hemodialisa tidak membuat sumber tagihan baru.** Usulan `SourceDomain = HEMODIALYSIS` ditolak;
lihat `02-backend-architecture.md` bagian *Yang sengaja tidak dibuat*.

### Kapan tagihan terbit, dan kapan tidak

Ini wujud `HMD-DEC-012`:

| Keadaan sesi | Tindakan terbentuk | Tagihan terbit | Sebabnya |
|---|:---:|:---:|---|
| `Completed` lalu `Finalized` | Ya, `IsBillable = true` | **Ya** | Cuci darah berjalan sampai selesai |
| `Stopped` lalu `Finalized` | Ya, `IsBillable = false` beserta alasan penghentian | **Tidak** | Layanan tidak tuntas. Bila rumah sakit berhak menagih bahan yang terpakai, kasir menambahkannya lewat jalur tagihan bebas dari katalog tarif |
| `Cancelled` sebelum dimulai | **Tidak** | Tidak | Tidak ada tindakan yang dikerjakan |

**Mengapa tidak menagih dulu lalu membatalkan belakangan:** pembatalan tagihan normal hanya
diizinkan dari status sumber tertentu. Begitu tagihan mencapai status selesai, jalur pembatalan
normal tertutup. Jadi menagih sesi yang dihentikan lalu ingin membatalkannya bukan pilihan yang
tersedia.

### Kegagalan penyerahan

Ini aturan paling penting pada dokumen ini:

```text
Catatan klinis  = Finalized      ← tidak berubah
Penyerahan      = Gagal / Ulangi ← ditangani terpisah
```

Kegagalan Billing **tidak pernah** membuka kembali catatan klinis yang sudah disahkan.
Penyerahan yang gagal masuk daftar untuk diulang, dan koordinator dapat menjalankannya ulang
lewat endpoint tersendiri. Pengulangan membawa penanda yang sama sehingga tidak pernah
menghasilkan dua tagihan untuk satu sesi.

---

## 7. Rekam Medis — keutuhan dan koreksi

**Arah:** dua arah.

| Langkah | Pemilik |
|---|---|
| Mendaftarkan catatan sesi sebagai dokumen klinis | Rekam Medis, dipanggil Hemodialisa saat finalisasi |
| Menandatangani dan mengunci | Rekam Medis |
| Menolak perubahan setelah terkunci | Rekam Medis |
| Mencatat koreksi sebagai tambahan | Rekam Medis |
| Menyediakan isi catatan sesi | Hemodialisa |

**Prasyarat yang wajib dipenuhi lebih dulu:** jenis dokumen `HemodialysisSession` harus
didaftarkan di **dua** tempat — pada daftar jenis dokumen, dan pada himpunan jenis yang
ditegakkan aturannya. Bila hanya yang pertama dikerjakan, penguncian tidak pernah berlaku dan
catatan sesi tetap dapat disunting walau layar menampilkan sudah disahkan.

Perubahan itu menyentuh berkas milik Rekam Medis, sehingga **wajib** dikoordinasikan dengan
pemiliknya dan menjadi task tersendiri.

---

## 8. Human Resource — kewenangan klinis

**Arah:** Hemodialisa membaca. Tidak pernah menyalin.

| Yang disimpan Hemodialisa | Yang **tidak** disimpan |
|---|---|
| Rujukan ke profil petugas, hasil pemeriksaan kewenangan berupa salah satu dari tiga status, waktu pemeriksaan, dan rujukan sumbernya | Nama petugas, nomor STR, nomor SIP, sertifikat, atau daftar kewenangannya |

**Keadaan Phase 1:** pembacaan kewenangan belum tersedia (`HMD-DEP-002`). Selama itu, seluruh
penugasan tercatat berstatus **belum dapat diverifikasi** — bukan terverifikasi. Penegakan
dinyalakan lewat pengaturan begitu Human Resource membuka pembacaannya, tanpa perubahan tabel
dan tanpa penulisan ulang kode.

---

## 9. Rawat Inap — permintaan masuk dan konteks

**Arah:** Rawat Inap memanggil Hemodialisa. Hemodialisa membaca episode rawat inap.

| Arah | Isi |
|---|---|
| Rawat Inap → Hemodialisa | Dokter atau perawat bangsal membuat permintaan HD lewat endpoint permintaan |
| Hemodialisa → Rawat Inap | Tidak ada. Hemodialisa hanya **membaca** `InpEpisode` untuk melengkapi konteks |

**Dua layar Rawat Inap sudah menunggu ini**, keduanya sengaja dikosongkan sampai Hemodialisa
tersedia: layar layanan penunjang pada ruang kerja dokter, dan bagian penunjang pada ruang kerja
perawat.

**Aturan yang mengikat:** siklus hidup sesi HD **tidak** mengikuti siklus hidup episode rawat
inap. Pasien yang dipulangkan dari bangsal tidak membuat sesi HD-nya ikut batal; dan sesi HD yang
selesai tidak menutup episode rawat inapnya.

---

## 10. Yang tidak ada pada Phase 1

| Integrasi | Sebabnya | Kapan |
|---|---|---|
| SATUSEHAT Uronefrologi | Di luar scope Phase 1 (`HMD-DEC-002`). Infrastruktur integrasi keluar juga belum berstatus milik platform (`HMD-CONF-002`) | Phase 2 |
| BPJS dan VClaim | Di luar scope Phase 1. Bila kelak dibutuhkan, pemilik penghubungnya sebaiknya Registration, bukan Hemodialisa | Phase 3 |
| Pembacaan otomatis dari perangkat mesin HD | Di luar scope Phase 1. Seluruh parameter mesin dicatat manual dalam bentuk terstruktur | Di luar ketiga fase |
| Sistem insiden keselamatan pasien | Sistemnya belum ada di aplikasi ini. Komplikasi tetap dicatat lengkap sebagai fakta klinis | Phase 2 |
| Pelaporan regulator tahunan | Di luar scope Phase 1. Data dasarnya sudah tersimpan sehingga siap dipetakan kelak | Phase 2 |

Ketiadaan kelima integrasi itu **tidak** menghalangi satu pasien menjalani satu sesi cuci darah
lengkap dari awal sampai catatannya dikunci dan tagihannya terbit.
