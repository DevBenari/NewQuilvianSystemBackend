# Traceability Requirement — MCU (Medical Check-Up)

| Field | Nilai |
|---|---|
| Revision | `1`, status `approved` (Yoga Aji Pratama, 2026-10-21) |
| Roadmap | `MCU-RM-BE-001` r1, `MCU-RM-FE-001` r1 |
| Blueprint | `MCU-BP-001` r1 `approved` (2026-10-21) |
| SHA | BE `4d3e4f98`, FE `7fb1e1d3e` |
| Kontrak | `MCU-API-v1`, `MCU-STATE-v1`, `MCU-VAL-v1`, `MCU-INT-v1`, `MCU-PERM-v1` — `approved` |
| Input hash | Lihat [`blueprint-manifest.md`](../blueprint-manifest.md) bagian *Input revisions* dan *Artifact hashes* |

Kolom *Status* hanya berisi keadaan rencana: **Direncanakan**, **⛔ Tertahan**, **Selesai** (dengan
bukti), atau **Di luar roadmap**. Tidak ada task implementasi yang selesai pada revisi ini.

## 1. Requirement → Task → Bukti

| Requirement | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti yang diharapkan | Status |
|---|---|---|---|---|---|---|---|
| `FR-MCU-001` | `MCU-GOV-001` | BE §6 | Registry | `BE-MCU-000` ✅, `BE-MCU-002` ✅ | — | Baris registry + checker tanpa QBE-MOD-002 (`MCU-EVD-002` §1); checker `PASS` atas 21 model nyata + kontrol negatif ([`BE-MCU-002`](../task/report/backend/BE-MCU-002.md)) | Selesai — migration belum diterapkan |
| `FR-MCU-002` | `MCU-DEC-007`, `015`, `025` | BE §4.1 | `VAL-MCU-01`..`08` | `BE-MCU-005` ✅ | `FE-MCU-002` ✅, `003` ✅ | `AT-MCU-01`, `03`; `UAT-01`, `02`. `AT-MCU-01`, `03` lulus di harness ([`BE-MCU-005`](../task/report/backend/BE-MCU-005.md)); `UAT-01`, `02` lulus di E2E API-tiruan dan terhadap backend lokal dengan data uji devYoga ([`FE-MCU-003`](../task/report/frontend/FE-MCU-003.md)) | Selesai |
| `FR-MCU-003` | `MCU-DEC-020`, `025` | BE §4.1 | `VAL-MCU-05` | `BE-MCU-005` ✅ | `FE-MCU-002` ✅, `003` ✅ | `AT-MCU-02` lulus di harness ([`BE-MCU-005`](../task/report/backend/BE-MCU-005.md)); harga perusahaan bermasa berlaku lulus di E2E (`VAL-MCU-05` tampil di modal, Akhiri) dan terhadap backend lokal ([`FE-MCU-003`](../task/report/frontend/FE-MCU-003.md)) | Selesai |
| `FR-MCU-004` | `MCU-DEC-007`; `MCU-OQ-002`, `003` | BE §9 | `VAL-MCU-09` | `BE-MCU-004` ✅, `006` ✅ | `FE-MCU-002` ✅, `004` ✅ | Uji unit `BE-MCU-006` — `VAL-MCU-09`, urut nomor butir, nonaktif tanpa hapus terbukti ([`BE-MCU-006`](../task/report/backend/BE-MCU-006.md)); seeder idempotent — terbukti dua kali jalan tanpa ganda dan tanpa menimpa admin ([`BE-MCU-004`](../task/report/backend/BE-MCU-004.md)); layar: `VAL-MCU-09` tampil apa adanya dan urutan nomor butir terbukti di E2E API-tiruan, urutan `TANDA_VITAL` cocok API pada UAT baca ([`FE-MCU-004`](../task/report/frontend/FE-MCU-004.md)) | Selesai — isi master usulan menunggu ratifikasi dokter MCU (`MCU-OQ-002`, `003`) |
| `FR-MCU-005` | `MCU-DEC-012`, `042` | ERD jadwal | `VAL-MCU-10`, `14` | `BE-MCU-007` ✅ | `FE-MCU-002` ✅, `005` ✅ | Uji unit `BE-MCU-007` lulus — `VAL-MCU-10`, `14`, dokter jaga ([`BE-MCU-007`](../task/report/backend/BE-MCU-007.md)); layar: sisa kuota *26/30*, `409` `VAL-MCU-14` tampil apa adanya, dokter jaga dikirim/dikosongkan — E2E API-tiruan ([`FE-MCU-005`](../task/report/frontend/FE-MCU-005.md)) | Selesai — admin MCU butuh `Doctor : Read` untuk pilihan dokter jaga |
| `FR-MCU-006` | `MCU-DEC-029` | BE §5.5 | `VAL-MCU-13` | `BE-MCU-007` ✅ | `FE-MCU-005` ✅ | `AT-MCU-06` lulus di tingkat service ([`BE-MCU-007`](../task/report/backend/BE-MCU-007.md)); layar: tombol pelampauan hanya bagi `ApproveQuotaOverride`, alasan wajib, penolakan server tampil di modal — E2E API-tiruan ([`FE-MCU-005`](../task/report/frontend/FE-MCU-005.md)) | Selesai |
| `FR-MCU-010` | `MCU-DEC-003` | BE §2.1 | `MCU-STATE-v1` | `BE-MCU-011` ✅ | `FE-MCU-006` ✅, `008` ✅ | `AT-MCU-08` lulus ([`BE-MCU-011`](../task/report/backend/BE-MCU-011.md)); `UAT-03` lulus di E2E API-tiruan dan terhadap backend lokal ([`FE-MCU-008`](../task/report/frontend/FE-MCU-008.md)) | Selesai |
| `FR-MCU-011` | `MCU-DEC-012` | BE §2 | `VAL-MCU-11`, `12` | `BE-MCU-007` ✅, `011` ✅ | `FE-MCU-008` ✅ | `AT-MCU-04`, `05` lulus pada jalur booking ([`BE-MCU-011`](../task/report/backend/BE-MCU-011.md)) dan tingkat kuota ([`BE-MCU-007`](../task/report/backend/BE-MCU-007.md)); `UAT-04` lulus di E2E API-tiruan dan terhadap backend lokal — pesan `VAL-MCU-11` tampil apa adanya ([`FE-MCU-008`](../task/report/frontend/FE-MCU-008.md)) | Selesai |
| `FR-MCU-012` | `MCU-DEC-033` | ERD layanan | `VAL-MCU-16`, `17` | `BE-MCU-011` ✅ | `FE-MCU-008` ✅ | `AT-MCU-08`, `09` lulus ([`BE-MCU-011`](../task/report/backend/BE-MCU-011.md)); layar: identitas sementara (nama, NIK 16 digit, tanggal lahir, HP) dikirim tanpa `patientId`, kalimat `VAL-MCU-16`/`17` ([`FE-MCU-008`](../task/report/frontend/FE-MCU-008.md)) | Selesai |
| `FR-MCU-013` | `MCU-DEC-041` | — | `VAL-MCU-18` | `BE-MCU-011` ✅ | `FE-MCU-008` ✅ | `AT-MCU-10` lulus ([`BE-MCU-011`](../task/report/backend/BE-MCU-011.md)); layar: penolakan `VAL-MCU-18` melewati jalur pesan server yang sama dengan `VAL-MCU-11` ([`FE-MCU-008`](../task/report/frontend/FE-MCU-008.md)) | Selesai |
| `FR-MCU-014` | `MCU-DEC-014`, `038` | — | `VAL-MCU-23` | `BE-MCU-011` ✅ | `FE-MCU-008` ✅ | `AT-MCU-14` lulus ([`BE-MCU-011`](../task/report/backend/BE-MCU-011.md)); `AT-MCU-15` (check-in order *Tidak Hadir*) lulus ([`BE-MCU-013`](../task/report/backend/BE-MCU-013.md)); layar: jadwal ulang dan batal beralasan membawa `version`, lulus terhadap backend lokal ([`FE-MCU-008`](../task/report/frontend/FE-MCU-008.md)) | Selesai — check-in order *Tidak Hadir* tidak ditawarkan layar (`FE-MCU-001`) dan ditolak pratinjau server |
| `FR-MCU-015` | `MCU-DEC-014`, `030` | BE §13 | `MCU-STATE-v1` | `BE-MCU-012` ✅ | — | `AT-MCU-13` lulus dengan jam tiruan ([`BE-MCU-012`](../task/report/backend/BE-MCU-012.md)); `UAT-05` | Direncanakan — backend selesai, menunggu UAT |
| `FR-MCU-020` | `MCU-DEC-039` | — | `VAL-MCU-25` | `BE-MCU-013` ✅ | `FE-MCU-006` ✅, `009` ✅ | `AT-MCU-18`, `19` lulus ([`BE-MCU-013`](../task/report/backend/BE-MCU-013.md)); `UAT-07` lulus di E2E API-tiruan dan terhadap backend lokal (tanpa centang konfirmasi ditolak layar, tanpa kiriman) ([`FE-MCU-009`](../task/report/frontend/FE-MCU-009.md)) | Selesai |
| `FR-MCU-021` | `MCU-DEC-003`, `028`, `032`, `042` | BE §2.1 | `VAL-MCU-26`, `27`; `MCU-INT-v1` | `BE-MCU-001` ✅, `013` ✅ | `FE-MCU-009` ✅ | `AT-MCU-16`, `20`, `21`, `24` lulus ([`BE-MCU-013`](../task/report/backend/BE-MCU-013.md)); `AT-MCU-RUN-01`..`03` ✅ ([`BE-MCU-001`](../task/report/backend/BE-MCU-001.md)); `UAT-06` lulus terhadap backend lokal — pasien baru lewat formulir PM berisi isian awal, dokter jaga bawaan, `MCU-RSMMC-000002` *Hadir* ([`FE-MCU-009`](../task/report/frontend/FE-MCU-009.md)) | Selesai |
| `FR-MCU-022` | `MCU-DEC-020`, `025` | — | `VAL-MCU-29` | `BE-MCU-013` ✅ | `FE-MCU-009` ✅ | `AT-MCU-22` lulus ([`BE-MCU-013`](../task/report/backend/BE-MCU-013.md)); `UAT-08` lulus di E2E API-tiruan (harga perusahaan Rp 300.000 + masa berlaku) ([`FE-MCU-009`](../task/report/frontend/FE-MCU-009.md)) | Selesai — `UAT-08` sungguhan menunggu data kartu penjamin pasien + tarif perusahaan uji |
| `FR-MCU-023` | `MCU-DEC-013`, `029` | — | `VAL-MCU-27`, `28` | `BE-MCU-013` ✅, `022` ✅ | `FE-MCU-009` ✅ | `AT-MCU-20` lulus ([`BE-MCU-013`](../task/report/backend/BE-MCU-013.md)); `AT-MCU-07` lulus ([`BE-MCU-022`](../task/report/backend/BE-MCU-022.md)); layar: setiap persyaratan dipilih tanpa bawaan, catatan/tanggal susulan wajib sesuai hasil ([`FE-MCU-009`](../task/report/frontend/FE-MCU-009.md)) | Selesai |
| `FR-MCU-024` | `MCU-DEC-041` | — | `VAL-MCU-30` | `BE-MCU-013` ✅ | `FE-MCU-009` ✅ | `AT-MCU-17` lulus ([`BE-MCU-013`](../task/report/backend/BE-MCU-013.md)); layar: kartu penjamin perusahaan (bawaan kartu utama) dikirim, penolakan server tampil apa adanya — E2E ([`FE-MCU-009`](../task/report/frontend/FE-MCU-009.md)) | Selesai |
| `FR-MCU-025` | `MCU-DEC-017`, `037` | — | `VAL-MCU-10`, `11` | `BE-MCU-014` ✅ | `FE-MCU-009` ✅ | Uji integrasi `BE-MCU-014` lulus ([`BE-MCU-014`](../task/report/backend/BE-MCU-014.md)); layar: *walk-in* admisi lulus terhadap backend lokal (`MCU-RSMMC-000004` *Hadir*) ([`FE-MCU-009`](../task/report/frontend/FE-MCU-009.md)) | Selesai |
| `FR-MCU-030` | `MCU-DEC-004`, `024` | BE §2.1 | `VAL-MCU-34` | `BE-MCU-021` ✅ | `FE-MCU-011` ✅ | `AT-MCU-26`, `27`; `UAT-09`. `AT-MCU-26`, `27` lulus ([`BE-MCU-021`](../task/report/backend/BE-MCU-021.md)); `UAT-09` lulus di E2E dan terhadap backend lokal tanpa tulis — *"Tagihan paket belum lunas (sisa Rp 350.000)."*, nol pesanan ([`FE-MCU-011`](../task/report/frontend/FE-MCU-011.md)) | Selesai |
| `FR-MCU-031` | `MCU-DEC-024`, `032` | BE §5.4 | `MCU-INT-v1` `INT-MCU-05`, `06` | `BE-MCU-020` ✅, `021` ✅ | `FE-MCU-011` ✅ | `AT-MCU-26`. Jalur pesanan bertanda Lab/Rad terbukti ([`BE-MCU-020`](../task/report/backend/BE-MCU-020.md)); pesanan Lab per disiplin + Rad per komponen saat mulai ([`BE-MCU-021`](../task/report/backend/BE-MCU-021.md)); nomor pesanan per komponen tampil di tab Pemeriksaan (`UAT-10` E2E) ([`FE-MCU-011`](../task/report/frontend/FE-MCU-011.md)) | Selesai — `UAT-10` data sungguhan (sesudah lunas) menjadi batas verifikasi |
| `FR-MCU-032` | `MCU-DEC-005`, `022` | BE §5.4 | `MCU-INT-v1` | `BE-MCU-003` ✅, `BE-MCU-020` ✅ | `FE-MCU-015` ✅ | `AT-MCU-28`; `UAT-10`. Kolom penanda + FK `Restrict` terbukti ([`BE-MCU-003`](../task/report/backend/BE-MCU-003.md)); `AT-MCU-28` tingkat Lab/Rad lulus ([`BE-MCU-020`](../task/report/backend/BE-MCU-020.md)); ujung ke ujung lulus ([`BE-MCU-021`](../task/report/backend/BE-MCU-021.md)); label *Dicakup paket MCU* di detail pesanan Lab/Rad lulus unit + E2E ([`FE-MCU-015`](../task/report/frontend/FE-MCU-015.md)) | Selesai — label pada pesanan bertanda sungguhan menunggu `UAT-10` data sungguhan |
| `FR-MCU-033` | `MCU-DEC-036` | — | `MCU-STATE-v1` | `BE-MCU-022` ✅ | `FE-MCU-011` ✅ | `AT-MCU-29` lulus sisi MCU/Lab; nominal tagihan Billing tidak diuji ([`BE-MCU-022`](../task/report/backend/BE-MCU-022.md)); tambah komponen di luar paket (*Di luar paket*, *"Ditagih terpisah…"*) lulus di E2E ([`FE-MCU-011`](../task/report/frontend/FE-MCU-011.md)) | Selesai |
| `FR-MCU-034` | `MCU-DEC-015`, `043`; `ARC-TECH-01` ✅ | BE §5.5 | `INT-MCU-08`, `09`; `VAL-MCU-38`, `40` | `BE-MCU-024` ✅, `BE-MCU-026` ✅ | `FE-MCU-011` ✅ (aksi batal) | `AT-MCU-30`, `31`, `37` ✅ di harness integrasi ([`BE-MCU-024`](../task/report/backend/BE-MCU-024.md)); `UAT-11`; sisi Lab `MCU-EVD-002` §2.1 ✅; sisi Billing — ketiga uji `MCU-EVD-002` §2.2 berbalik ([`BE-MCU-026`](../task/report/backend/BE-MCU-026.md)); uji devYoga belum dijalankan; `UAT-11` lulus di E2E (dokter yang ditetapkan, alasan wajib, akun lain tanpa aksi) ([`FE-MCU-011`](../task/report/frontend/FE-MCU-011.md)) | Layar selesai — menunggu uji devYoga (butuh akun dokter yang ditetapkan) |
| `FR-MCU-035` | `MCU-DEC-021` | BE §2.1 | `VAL-MCU-41` | `BE-MCU-025` ✅ | `FE-MCU-011` ✅ | `AT-MCU-32` lulus dengan Lab, Billing, Registrasi sungguhan ([`BE-MCU-025`](../task/report/backend/BE-MCU-025.md)); `UAT-12` lulus di E2E (akibat batal penuh tampil, `VAL-MCU-41` apa adanya) ([`FE-MCU-011`](../task/report/frontend/FE-MCU-011.md)); refund utuh lewat jalur yang ada, kini ikut transaksi pemanggil | Selesai |
| `FR-MCU-036` | `MCU-DEC-018`, `035` | — | `VAL-MCU-36`, `42`, `43`, `44` | `BE-MCU-023` ✅ | `FE-MCU-001` ✅, `011` ✅ | `AT-MCU-33`, `34`, `35` lulus ([`BE-MCU-023`](../task/report/backend/BE-MCU-023.md)); parsing desimal `VAL-MCU-43` di layar ✅ ([`FE-MCU-001`](../task/report/frontend/FE-MCU-001.md)); `UAT-13` dan isian `6,4` → `"6.4"` lulus di E2E ([`FE-MCU-011`](../task/report/frontend/FE-MCU-011.md)) | Selesai |
| `FR-MCU-037` | `MCU-DEC-007`, `009` | — | `MCU-API-v1` | `BE-MCU-010` ✅, `022` ✅ | `FE-MCU-007` ✅, `011` ✅ | Uji integrasi `BE-MCU-010` (tampilan *3 dari 4*) lulus ([`BE-MCU-010`](../task/report/backend/BE-MCU-010.md)); sinkron status Lab/Rad lulus ([`BE-MCU-022`](../task/report/backend/BE-MCU-022.md)); kelengkapan + *Muat ulang status komponen* tanpa polling ([`FE-MCU-011`](../task/report/frontend/FE-MCU-011.md)) | Selesai |
| `FR-MCU-040` | `MCU-DEC-009`, `018` | — | `VAL-MCU-35`, `36` | `BE-MCU-030` ✅ | `FE-MCU-012` ✅ | `AT-MCU-38` ✅ ([`BE-MCU-030`](../task/report/backend/BE-MCU-030.md)); `UAT-14` lulus di E2E — isi minimum `VAL-MCU-37` diperiksa layar, penolakan *"Masih ada 1 komponen yang belum tuntas: Thorax PA."* tampil apa adanya ([`FE-MCU-012`](../task/report/frontend/FE-MCU-012.md)) | Selesai — `UAT-14` data sungguhan menjadi batas verifikasi |
| `FR-MCU-041` | `MCU-DEC-032` | BE §11 | `VAL-MCU-38` | `BE-MCU-030` ✅, `BE-MCU-032` ✅ | `FE-MCU-012` ✅ | `AT-MCU-39` dua bagian pertama ✅ ([`BE-MCU-030`](../task/report/backend/BE-MCU-030.md)), bagian ganti dokter ✅ ([`BE-MCU-032`](../task/report/backend/BE-MCU-032.md)); `UAT-16` lulus — dokter lain tanpa tombol dan resume baca-saja (E2E), server `403` `VAL-MCU-38` terhadap backend lokal tanpa tulis ([`FE-MCU-012`](../task/report/frontend/FE-MCU-012.md)) | Selesai — `UAT-16` dengan dua akun dokter sungguhan menjadi batas verifikasi |
| `FR-MCU-042` | `MCU-DEC-008` | BE §11 | `VAL-MCU-39` | `BE-MCU-030` ✅ | `FE-MCU-013` ✅ | `AT-MCU-40` ✅ ([`BE-MCU-030`](../task/report/backend/BE-MCU-030.md)); `UAT-15` lulus di E2E — verifikasi dari header, penolakan server apa adanya, *Verifikasi* tersembunyi bagi dokter pemberi approve ([`FE-MCU-013`](../task/report/frontend/FE-MCU-013.md)) | Selesai — `UAT-15` data sungguhan menjadi batas verifikasi |
| `FR-MCU-043` | `MCU-DEC-008` | — | `VAL-MCU-40` | `BE-MCU-030` ✅ | `FE-MCU-013` ✅ | Uji integrasi `BE-MCU-030` ✅ (kembalikan beralasan, approve ulang); *Kembalikan ke Dokter* beralasan lulus di E2E, alasan kosong `400` `VAL-MCU-40` terhadap backend lokal ([`FE-MCU-013`](../task/report/frontend/FE-MCU-013.md)) | Selesai |
| `FR-MCU-044` | `MCU-DEC-011` | — | `MCU-STATE-v1` | `BE-MCU-031` ✅, `034` ✅ | `FE-MCU-012` ✅ | `AT-MCU-43` penuh ✅ ([`BE-MCU-031`](../task/report/backend/BE-MCU-031.md), [`BE-MCU-034`](../task/report/backend/BE-MCU-034.md)); `UAT-17` lulus di E2E — buka ulang beralasan, v2 *Draf* berlaku, v1 *Dikoreksi* terbaca dengan alasannya ([`FE-MCU-012`](../task/report/frontend/FE-MCU-012.md)); penanda penyerahan lama tampil di tab Penyerahan (`FE-MCU-013`) | Selesai — `UAT-17` data sungguhan menjadi batas verifikasi |
| `FR-MCU-045` | `MCU-DEC-040` | — | `VAL-MCU-45` | `BE-MCU-032` ✅ | `FE-MCU-014` ✅ | `AT-MCU-39` bagian ketiga ✅ ([`BE-MCU-032`](../task/report/backend/BE-MCU-032.md)); tombol hanya bagi `McuOrder : ReassignDoctor` dan alasan kosong → *"Alasan wajib diisi."* lulus di E2E dan terhadap backend lokal tanpa tulis ([`FE-MCU-014`](../task/report/frontend/FE-MCU-014.md)) | Selesai |
| `FR-MCU-050` | `MCU-DEC-010` | BE §11 | `MCU-API-v1` Mcu Delivery | `BE-MCU-033` ✅ | `FE-MCU-010` ✅, `013` ✅ | `AT-MCU-42` refleksi DTO ✅ ([`BE-MCU-033`](../task/report/backend/BE-MCU-033.md)), bagian penyerahan `409` ✅ ([`BE-MCU-034`](../task/report/backend/BE-MCU-034.md)); ringkasan perusahaan diambil dari `variant=fitness-summary`, tidak disusun di browser — uji unit ✅ ([`FE-MCU-010`](../task/report/frontend/FE-MCU-010.md)); cetak hasil lengkap dan ringkasan (daftar putih 12 ruas, tanpa nilai/diagnosis) lulus unit + E2E ([`FE-MCU-013`](../task/report/frontend/FE-MCU-013.md)) | Selesai |
| `FR-MCU-051` | `MCU-DEC-010` | — | `VAL-MCU-31` | `BE-MCU-034` ✅ | `FE-MCU-013` ✅ | `AT-MCU-42` ✅ ([`BE-MCU-034`](../task/report/backend/BE-MCU-034.md)); `UAT-19` lulus di E2E — penolakan `VAL-MCU-31` apa adanya, satu klik beralih ke ringkasan ([`FE-MCU-013`](../task/report/frontend/FE-MCU-013.md)) | Selesai — bentuk bukti `MCU-OQ-003` menahan go-live; `UAT-19` data sungguhan menjadi batas verifikasi |
| `FR-MCU-052` | `MCU-DEC-019`, `027` | — | `VAL-MCU-32`, `33` | `BE-MCU-034` ✅ | `FE-MCU-013` ✅ | `AT-MCU-41`, `44` ✅ ([`BE-MCU-034`](../task/report/backend/BE-MCU-034.md)); `UAT-18` lulus di E2E (dua catatan penyerahan, body persis DTO); payload layar diterima backend lokal (`409` `VAL-MCU-32` pada order belum terverifikasi, tanpa tulis) ([`FE-MCU-013`](../task/report/frontend/FE-MCU-013.md)) | Selesai — `UAT-18` data sungguhan menjadi batas verifikasi |
| `FR-MCU-053` | `MCU-DEC-010` | — | `MCU-PERM-v1` | `BE-MCU-034` ✅ | `FE-MCU-013` ✅ | Uji integrasi `BE-MCU-034` ✅ (persetujuan, pencabutan); tombol per izin `McuDelivery` dan catat/cabut persetujuan lulus di E2E ([`FE-MCU-013`](../task/report/frontend/FE-MCU-013.md)) | Selesai |

## 2. Kebutuhan non-fungsional → Task

| NFR | Isi singkat | Task | Bukti |
|---|---|---|---|
| `NFR-001` | Atomicity check-in, mulai pemeriksaan, batal penuh | `BE-MCU-013`, `021`, `025` | `AT-MCU-24`; uji interceptor gagal-simpan |
| `NFR-002` | Konkurensi kuota dan `Version` order | `BE-MCU-007`, `011`, seluruh task ubah | `AT-MCU-05`; `VAL-MCU-46` per task |
| `NFR-003` | Idempotensi check-in | `BE-MCU-013` | `AT-MCU-17` |
| `NFR-004` | Audit tahan lama | Seluruh task ubah (riwayat status/dokter, versi resume, penyerahan) | Baris riwayat pada setiap AT — lihat celah `GAP-MCU-05` |
| `NFR-005` | Izin **dan** kecocokan dokter yang ditetapkan | `BE-MCU-024`, `030`, `031`, `032` | `AT-MCU-31`, `39` |
| `NFR-006` | Log tanpa data sensitif | `BE-MCU-035` ✅ (dan DoD setiap task) | `AT-MCU-45` ✅ ([`BE-MCU-035`](../task/report/backend/BE-MCU-035.md)) |
| `NFR-007` | Tanggal kalender WIB, jam tutup dapat dikonfigurasi | `BE-MCU-012`, `013` | `AT-MCU-13`; `VAL-MCU-24` |
| `NFR-008` | Isian terkunci sesudah approve; revisi tercatat | `BE-MCU-023`, `031` | `AT-MCU-43`; `VAL-MCU-44` |

## 3. Uji penerimaan → Task

| Uji | Task | | Uji | Task |
|---|---|---|---|---|
| `AT-MCU-01`..`03` | `BE-MCU-005` | | `AT-MCU-26`, `27` | `BE-MCU-021` |
| `AT-MCU-04`, `05` | `BE-MCU-007`, `011` | | `AT-MCU-28` | `BE-MCU-020`, `021` |
| `AT-MCU-06` | `BE-MCU-007` | | `AT-MCU-29`, `07` | `BE-MCU-022` |
| `AT-MCU-08`..`10`, `14`, `15` | `BE-MCU-011` | | `AT-MCU-30`, `31`, `37` | `BE-MCU-024` |
| `AT-MCU-11` | — (`EPIC MCU-09`) | | `AT-MCU-32` | `BE-MCU-025` |
| `AT-MCU-12` | — (`EPIC MCU-07`) | | `AT-MCU-33`..`35` | `BE-MCU-023` |
| `AT-MCU-13` | `BE-MCU-012` | | `AT-MCU-36` | — (`EPIC MCU-10`) |
| `AT-MCU-16`..`24` | `BE-MCU-013` | | `AT-MCU-38`..`40` | `BE-MCU-030`, `032` |
| `AT-MCU-25` | — (`EPIC MCU-08`) | | `AT-MCU-41`..`44` | `BE-MCU-031`, `033`, `034` |
| `AT-MCU-RUN-01`..`03` | `BE-MCU-001` ✅ `PASS` 2026-10-21 | | `AT-MCU-45` | `BE-MCU-035` ✅ |

## 4. UAT → Task

| UAT | Task FE | Task BE |
|---|---|---|
| `UAT-01`, `02` | `FE-MCU-003` ✅ `PASS` 2026-10-27 | `BE-MCU-005` ✅ |
| `UAT-03`, `04` | `FE-MCU-008` ✅ `PASS` 2026-10-27 | `BE-MCU-011` ✅ |
| `UAT-05` | — (tanpa layar; diamati di `FE-MCU-007`) | `BE-MCU-012` |
| `UAT-06`..`08` | `FE-MCU-009` ✅ `PASS` 2026-10-27 (`UAT-08` di E2E) | `BE-MCU-013` ✅ |
| `UAT-09`, `10`, `12`, `13` | `FE-MCU-011` ✅ `PASS` 2026-10-27 (`UAT-09` backend lokal tanpa tulis; `10`, `12`, `13` di E2E) | `BE-MCU-020`..`023` ✅, `025` ✅ |
| `UAT-11` | `FE-MCU-011` ✅ `PASS` 2026-10-27 (E2E) | `BE-MCU-024` ✅, `026` ✅ |
| `UAT-14`, `16`, `17` | `FE-MCU-012` ✅ `PASS` 2026-10-28 (E2E; `UAT-16` sisi server `403` terhadap backend lokal tanpa tulis) | `BE-MCU-030` ✅, `031` ✅ |
| `UAT-15`, `18`, `19` | `FE-MCU-013` ✅ `PASS` 2026-10-28 (E2E; payload dan penolakan server terhadap backend lokal tanpa tulis) | `BE-MCU-030` ✅, `033` ✅, `034` ✅ |

## 5. Celah cakupan (coverage gap)

| ID | Celah | Akibat | Tindak lanjut |
|---|---|---|---|
| `GAP-MCU-01` | `AT-MCU-11`, `12`, `25`, `36` tidak punya task | Rekomendasi, impor, kiosk, dan sumber jasa medis tidak teruji di MVP | Sengaja — epic `POST-MVP`/menunggu koordinasi; dibawa ke roadmap revisi berikutnya |
| `GAP-MCU-02` | ~~`FR-MCU-034` tertahan Billing~~ — **ditutup 2026-10-21** | — | `MCU-COORD-003` disetujui Yasmina, pemilik `billing-kasir` |
| `GAP-MCU-03` | ~~Seluruh layar MCU tertahan `MCU-FE-002`~~ — **ditutup 2026-10-21** | — | `FE-MCU-000` ✅ — `05-screen-brief.md` |
| `GAP-MCU-04` | Master klinis usulan belum diratifikasi (`MCU-OQ-002`, `003`) | Development dan UAT boleh; **go-live tidak** | Jawaban dokter MCU dan pemilik privasi/legal sebelum go-live |
| `GAP-MCU-05` | `NFR-004` (audit) tidak punya uji penerimaan khusus | Riwayat bisa saja tidak lengkap pada satu tindakan tanpa ketahuan | Setiap task ubah mencantumkan baris riwayat yang terbentuk di laporan; `BE-MCU-035` memeriksa ulang daftar tindakan ubah |
| `GAP-MCU-06` | Aksesibilitas dan layar 1366 px (`03-frontend-architecture.md` §4) tidak punya uji | Kekurangan baru terlihat saat UAT | Diperiksa di DoD setiap task layar sesudah `FE-MCU-000` |
| `GAP-MCU-07` | Cacat refund ganda Billing (`MCU-EVD-002` §2.2) berlaku untuk **semua** refund, bukan hanya MCU | Satu baris tagihan dapat direfund melebihi nilainya hari ini | Penutupannya termasuk dalam persetujuan Yasmina 2026-10-21; **ditutup** oleh `BE-MCU-026` ✅ 2026-10-22 ([laporan](../task/report/backend/BE-MCU-026.md)) — migration `AddBillingRefundCaseItem` belum diterapkan |
