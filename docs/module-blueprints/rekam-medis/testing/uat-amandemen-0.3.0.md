# Laporan Kesiapan UAT — Amandemen `0.3.0` (Optik dan Rehab Medik)

| Field | Isi |
|---|---|
| Task | `FE-18` — **`SELESAI`** 10 November 2026 (`RM-DEC-096`, pola `FE-15`): satu UAT lulus nyata, tiga terbuka dan terlacak |
| Tanggal | 10 November 2026 |
| Kontrak | API `0.3.0` (`RM-DEC-088`) |
| Lingkungan | Backend lokal (kode `BE-42`–`BE-46`, belum di-commit) `https://localhost:7184` terhadap devYoga; frontend `next dev` port 3710 (kode `FE-16`/`FE-17`, belum di-commit) |
| Akun | Superadmin (`SeedSuperAdmin`) |

## 1. Keadaan devYoga yang menjadi dasar

| Hal | Keadaan | Bukti |
|---|---|---|
| Migration `20261110043756_AddClinicTypeToMedicalRecordCompletenessProfile` | **Diterapkan** 10 November 2026 atas instruksi Yoga Aji Pratama (pilihan "Hanya BE-42"), lewat skrip `--idempotent` satu migration. Riwayat 386 → 387 | Preflight dan post baca-saja: kolom, CHECK, index empat kolom terpasang |
| Migration `20261105045613_AddRehabilitationManagement` | **Diterapkan** 10 November 2026 sore atas instruksi Yoga Aji Pratama selaku pemilik devYoga (`RM-DEC-092`): preflight, uji kering di dalam transaksi lalu `ROLLBACK`, lalu penerapan. 20 tabel `Rhb*`; riwayat 387 → 388; **nol tertunda** | `migrations list`; program penerap |
| Profil `RJ-OPT`, `RJ-REHAB` | **Terpasang** lewat seeder saat startup ("Ditambahkan 2, dilewati 11") | Log startup; total profil 13 |
| Poli Rehabilitasi Medik | Ada: `CLN-REHAB-001`, `CLN-FISIO-001` (jenis 6) | Kueri baca-saja |
| Data Optik (refraksi, resep) | **Nol** | Kueri baca-saja |

**Temuan lingkungan (sudah tertutup).** Sesudah migration Rehab, backend start normal tanpa sakelar (seeder Rehab 227 ms). Catatan aslinya dipertahankan di bawah.

**Temuan lingkungan (sebelum migration Rehab).** Selama tabel Rehab belum ada, backend dengan kode sejak 5 November **tidak dapat
dinyalakan** terhadap devYoga: `RehabilitationMasterDataSeeder` gagal `42P01` (`RhbDiscipline`) dan
aplikasi berhenti. UAT ini dijalankan dengan sakelar pengembangan yang sudah ada,
`Seeders__ContinueOnFailure=true` (tidak berlaku di Production). Akibat yang perlu diketahui: kunjungan poli
Rehabilitasi Medik di devYoga akan dinilai `Tidak dapat dipastikan` (probe Rehab membaca tabel yang belum
ada) sampai migration Rehab diterapkan — perilaku aman yang dirancang (`RM-DEC-044`), bukan kesalahan.

## 2. Hasil per UAT

| UAT | Skenario | Hasil | Bukti |
|---|---|---|---|
| `UAT-20` | Berkas pasien berisi dokumen Optik dan Rehab final | **Terbuka dan terlacak** (`RM-DEC-096`) | Nol data Optik (`RM-DEC-093`); master Rehab belum siap (`RM-FACT-037`). Bukti pengganti: `AT-RM-75`–`80`, `83`–`88`, E2E API tiruan `FE-16` |
| `UAT-21` | Dokumen Optik bertanda tangan tanpa tombol koreksi | **Terbuka dan terlacak** (`RM-DEC-096`; layar terbukti E2E API tiruan `FE-16`) | Nol dokumen Optik di devYoga (`RM-DEC-093`). Bukti pengganti: `AT-RM-91`, `92` |
| `UAT-22` | Profil `RJ-REHAB` lewat layar; kondisi Operasi mengunci jenis poli | **Lulus** | API: metadata `clinicTypeOptions`, kriteria 17, saringan `clinicType=6` → `RJ-REHAB`, unit Optik → `RJ-OPT` tanpa butir, `clinicType=999` → `400`, POST berkondisi + jenis poli → `400` "Jenis poli hanya dapat dipakai pada profil tanpa kondisi." tanpa baris tertulis. Layar (Chromium, `/v1/**` diteruskan ke backend): baris `RJ-REHAB` "Rehabilitasi Medik" "1 dokumen", `RJ-OPT` "Tanpa dokumen wajib", kolom Jenis Poli, saringan, isian terkunci saat Operasi; nol permintaan tulis. Tangkapan layar di scratchpad sesi |
| `UAT-23` | Kunjungan poli rehab dengan sesi final lengkap di daftar kerja | **Terbuka dan terlacak** (`RM-DEC-096`) | Tabel Rehab kini ada, tetapi master Rehab belum siap: disiplin nonaktif, poli rehab mewajibkan dokter (`RM-FACT-037`). Bukti pengganti: `AT-RM-83`–`88` |

## 3. Temuan

| No | Temuan | Tindak lanjut |
|---:|---|---|
| 1 | Opsi jenis poli ikut menampilkan "Tidak diketahui" (`0`). Profil berjenis itu tidak pernah terpilih | **Tertutup** `RM-DEC-095`: `Unknown` dikeluarkan dari opsi dan ditolak `400` saat menyimpan; 269 uji lulus. Penyegaran kontrak `0.3.1` dicatat untuk pass blueprint |
| 2 | Pesan galat saringan `clinicType=999` tidak terbaca di `message` (status `400` benar) | Bentuk badan galat binding enum; tidak menghalangi layar |

## 4. Yang dibutuhkan untuk menutup UAT terbuka

| UAT | Prasyarat | Pemilik |
|---|---|---|
| ~~`UAT-20`, `UAT-23`~~ | ~~Migration `AddRehabilitationManagement` diterapkan ke devYoga~~ | **Selesai** (`RM-DEC-092`) |
| `UAT-20`, `UAT-23` | Minimal satu disiplin Rehab **aktif dan berprofesi** (keempatnya nonaktif hasil seeder), dokter terpasang di `CLN-REHAB-001`, pasien berlabel uji (`RM-FACT-037`) | Pemilik Rehab Medik (Sukma Giri Pratama), admin data |
| `UAT-20`, `UAT-21` | Minimal satu pemeriksaan refraksi final dan satu resep Optik di devYoga; butuh akun berprofesi RO/Sp.M | Pemilik Optik (Yoga Aji Pratama) |
| `UAT-23` | Satu kunjungan poli rehab dengan sesi final yang ditutup klinis | Sesudah dua baris di atas |
