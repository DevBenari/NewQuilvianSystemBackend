# Kesiapan Modul IGD — audit ujung-ke-ujung sesudah langkah 5 — 27 Oktober 2026

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` · revision **`15`** · status `draft` (irisan-irisannya `approved` lewat `IGD-DEC-230`, `240`, `294`, `306`, `318`, `324`, `325`, `326`) |
| **Putusan** | **`READY_WITH_CONDITIONS`** — seluruh jalur yang **dimiliki IGD** terbukti lewat layar dan siap diserahkan ke UAT untuk alur normal. **Belum siap produksi**: delapan syarat di bagian 5, empat di antaranya di tangan pemilik modul lain, dua menyangkut keamanan/privasi dan konfigurasi lingkungan rilis |
| Skill | `verify-module-readiness` — hanya membaca source dan bukti; nol perbaikan dikerjakan |
| Wewenang tulis | Berkas ini, satu baris pada `MODULE-STATUS.md`, dan satu blok decision log — atas perintah Rizki Gunawan 27 Oktober 2026 (*"yaa lanjutkan"* sesudah pilihan *Audit kesiapan modul* ditawarkan) |
| Backend diperiksa | `NewQuilvianSystemBackend` `rizkiG` **`2c6bd535`** + working tree dokumen `IGD-REQ-009`; DLL build Rizki 27 Oktober 2026 08.49; backend berjalan di `https://localhost:7184` |
| Frontend diperiksa | `QuilvianSystemFrontendDev` `RizkiV2` **`8ed1e101e`**; build produksi BUILD_ID `HXGe2JMsrS4xsJSmiOFKW` (27 Oktober 2026, 517/517); `npm run dev` pemilik berjalan |
| Kontrak | API **`0.20.0`** (`1335fe1c…`), validation **`0.20.0`** (`ba54bce5…`), state **`0.15.0`** (`4e49bcec…`), permission/audit **`0.12.0`** (`c8dadf9a…`), integration **`0.11.0`** (`3600c836…`) — sha256 isi dinormalkan LF, 12 karakter pertama |
| Masukan lain | `00-interview-decisions.md` `96b6218c…` (working tree, sesudah blok `IGD-REQ-009`); `backend-roadmap.md` `33a749c1…`; `frontend-roadmap.md` `71fbf293…`; `requirement-traceability.md` `1f535089…` |
| Keputusan relevan | `IGD-DEC-332` (status hanya dari bukti mentah), `IGD-DEC-353`…`358`; fakta `IGD-FACT-236`…`252`; permintaan `IGD-REQ-003`, `007`, `008`, `009` |
| Aturan pemilik yang mengikat audit | Tanpa unit test; dilarang kueri basis data dev; UAT dikerjakan tim terpisah dan **tidak** diklaim di sini |

---

## 1. Putusan dalam bahasa sederhana

Modul IGD sekarang dapat dipakai dari ujung ke ujung untuk pasien yang datang, didaftarkan, ditriase,
diperiksa perawat dan dokter, ditagih, lalu dipulangkan atau dipindahkan — dan setiap langkah itu punya
bukti layar atau jaringan yang diukur, bukan hanya kode yang ada. Pada 27 Oktober 2026 saja, satu alur
utuh dijalankan ulang dengan akun nyata: petugas loket mendaftarkan pasien tanpa identitas, perawat
memulai triase dan menandainya, triase HIJAU tersimpan, pengkajian tersimpan dan terbaca kembali, dan
tab Tagihan menyamarkan nominal bagi dokter tanpa izin finansial.

Yang **belum** membuatnya siap produksi bukan pekerjaan IGD yang tertinggal. Enam task aktif yang
tersisa seluruhnya menunggu orang lain — Rawat Inap, Laboratorium/Radiologi, Gizi, pemilik data — dan
dua urusan menyangkut keamanan serta konfigurasi lingkungan yang belum punya pemilik.

*Contoh.* Pasien kecelakaan tanpa identitas dapat diterima loket, ditriase, diperiksa, ditagih, dan
dipindah ke rawat inap **bila admisi Rawat Inap diproses lebih dulu, baru pasien diantar**. Bila pasien
diantar sebelum admisi diproses, kunjungan IGD-nya tidak tertutup sendiri sampai Rawat Inap memasang
satu pemanggilan (`BE-IGD-077`, `IGD-REQ-003`).

## 2. Cakupan dan denominator

| Lapisan | Denominator | Selesai | Bukti |
| --- | --- | --- | --- |
| Task backend | 64 aktif (67 baris − 3 dibatalkan) | **61 ✅ (95,3%)** | Register `backend-roadmap.md`; **61 dari 61** baris ✅ bertautan ke laporan yang berkasnya ada |
| Task frontend | 57 aktif (59 baris − 2 dibatalkan) | **54 ✅ (94,7%)** | Register `frontend-roadmap.md`; 52 dari 54 baris ✅ bertautan ke laporan; `FE-IGD-056` dan `FE-IGD-065` **punya** laporan tetapi barisnya tanpa tautan |
| Gabungan | 121 aktif | **115 (95,0%)** | Dihitung ulang dari kedua register; atas 126 baris: 115 (91,3%) |
| Requirement `FR-IGD-*` | 82 baris traceability | 13 eksplisit *Terbukti*; 17 tidak hijau dirinci di bagian 3; 52 berstatus pada tabel lama yang kolomnya berbeda | `requirement-traceability.md`. **Keterbatasan:** ke-52 baris itu tidak dibaca satu per satu pada audit ini |

Enam task aktif yang belum ✅: `BE-IGD-042` (⛔ pemilik data), `BE-IGD-071` (⛔ `IGD-OQ-117`),
`BE-IGD-077` (🟡 5/7, Rawat Inap), `FE-IGD-053` (⛔ `IGD-OQ-119`), `FE-IGD-058` (⛔ menunggu
`BE-IGD-071`), dan `FE-IGD-057` (🟢, tidak dihitung — `IGD-DEC-353`).

## 3. Skor per dimensi

Skor ditulis sebagai rasio yang dapat dihitung ulang, bukan persentase taksiran.

| Dimensi | Bobot | Skor | Bukti | Gap/blocker |
| --- | ---: | --- | --- | --- |
| Fondasi — keputusan, desain, kontrak | 15 | Kontrak lima berkas berversi; irisan yang dipakai `approved`. **Keputusan terbuka: 9** (`IGD-OQ-117`, `119`, `127`, `129`, `132`, `137`, `140`, `142`, `143`) ditambah `IGD-DEC-100`…`102` masih `draft` | `00-interview-decisions.md@96b6218c`; tabel *Blockers and owners* `MODULE-STATUS.md` | Security/Privacy owner dan Clinical Governance **`OPEN`** — belum ditunjuk |
| Backend | 20 | **61/64** task ✅ | `backend-roadmap.md@33a749c1` register; laporan `task/report/backend/` | `BE-IGD-042`, `071`, `077` — ketiganya menunggu pihak luar |
| Frontend | 20 | **54/57** task ✅ | `frontend-roadmap.md@71fbf293` register; laporan `task/report/frontend/`; build `HXGe2JMsrS4xsJSmiOFKW` | `FE-IGD-053`, `058` menunggu pihak luar; `FE-IGD-057` 🟢 |
| Integrasi dan runtime | 20 | Alur IGD utuh terbukti 27 Oktober 2026 pada backend dan `next dev` yang berjalan; **4** ketergantungan lintas modul terbuka | `uji-fe-igd-070-20261027/hasil.json`, `uji-fe-igd-071-20261027/hasil.json`, `uji-fe-igd-022-20261027/hasil-T4b.json`, `uji-fe-igd-012-l1-03r-20261027/hasil.json` | Rawat Inap (`IGD-REQ-003`), Lab/Rad (`IGD-REQ-007`), Gizi (`IGD-REQ-008`), Billing (`IGD-OQ-129`) |
| Keamanan dan privasi | 10 | **Terbatas** — kewenangan unit bekerja (`BE-IGD-039` ✅ `IGD-DEC-201`); penyamaran nominal terbukti dua akun (`FE-IGD-071`) | `IGD-DEC-201`, `IGD-DEC-358`, `T3b-*` | Jalan keluar beralasan `IGD-DEC-092` tidak ada di kode; empat endpoint `ClinicalManagement` tanpa penyaring pasien (`IGD-OQ-143`); pemilik `OPEN` |
| Data dan konfigurasi | 5 | **Belum siap untuk lingkungan baru** | `IGD-FACT-236`, `238`, `127`, `136`, `IGD-UNK-18` | Lihat syarat C5, C6 |
| Verifikasi | 10 | Bukti layar mentah untuk 115 task; **0** test otomatis; UAT tim terpisah **belum ada hasil** | `IGD-DEC-332`; proyek test dihapus 11 September 2026; aturan pemilik tanpa unit test | Regresi bergantung pada uji layar manual-terskrip |

### Requirement yang tidak hijau

| Kelompok | Requirement | Keadaan |
| --- | --- | --- |
| Coverage gap | `FR-IGD-049` | Terima/tolak pesanan oleh unit penerima tanpa layar (`IGD-EV-109`) |
| Coverage gap | `FR-IGD-117`, `FR-IGD-118` | `OPEN DECISION` `IGD-OQ-127`, `IGD-OQ-129` (penautan invoice IGD ke rawat inap) |
| Coverage gap | `FR-IGD-124`, `FR-IGD-125` | Prioritas antrean transfer berbasis tier, diskon membership — menunggu Rawat Inap, Billing, CRM |
| Terblokir | `FR-IGD-101` (gizi), `FR-IGD-102` | `IGD-OQ-119`, `IGD-OQ-117` |
| Sebagian — pihak luar | `FR-IGD-119`, `FR-IGD-120` | *Tolak* dan penutupan susulan menunggu pemanggil Rawat Inap (`IGD-OQ-132`) |
| **`STALE`** | `FR-IGD-109`, `110`, `111` | Masih menulis *"TTD dan kirim menunggu `BE-IGD-074`"*, padahal `BE-IGD-074` ✅ sejak 18 Oktober 2026 |
| **`STALE`** | `FR-IGD-126` | Masih menulis *"layar kasir belum (K1-09)"*, padahal K1-09 lulus 19 Oktober 2026 (`BE-IGD-084` ✅) |
| Lengkap dengan catatan | `FR-IGD-051`, `FR-IGD-085` | Bergantung coverage gap `FR-IGD-049`; praktik lapangan `IGD-UNK-10` |

## 4. Kemajuan scaffold vs kesiapan sesungguhnya

| Hal | Scaffold ada | Terbukti berjalan |
| --- | --- | --- |
| Pendaftaran pasien baru oleh loket | Sejak MVP-7 | **Baru 27 Oktober 2026** — sebelumnya loket tidak pernah dapat menyimpan pasien baru karena rute kiosk (`IGD-FACT-249`); diperbaiki `IGD-DEC-355` |
| Penanda pasien tanpa identitas | Sejak `FE-IGD-070` | **Baru 27 Oktober 2026** — selama 15–27 Oktober tidak ada layar yang dapat mengisinya (`IGD-FACT-250`); diperbaiki `IGD-DEC-356` |
| Penyamaran nominal tagihan | Sejak 23 Oktober | **Baru 27 Oktober 2026** — terbukti dengan dua akun non-SuperAdmin (`FE-IGD-071`) |
| Master data IGD di lingkungan baru | Seeder ada | **Tidak berjalan** — `EmergencyMasterDataSeeder` nol pemanggil (`IGD-FACT-236`) |
| Validasi Emergency Visit pendaftaran | Aturan ada | **Dimatikan sementara sejak 7 Agustus 2026** (`IGD-FACT-238`) |

Dua penghalang terbesar yang ditemukan hari ini (baris 1 dan 2) tidak terlihat dari jumlah berkas maupun
build yang sukses — keduanya baru tampak ketika alur dijalankan dengan akun peran nyata.

## 5. Syarat (`READY_WITH_CONDITIONS`)

| # | Syarat | Pemilik | Mitigasi sementara | Dasar |
| --- | --- | --- | --- | --- |
| C1 | Rawat Inap memanggil `EvaluasiPenutupanKunjunganSusulanAsync` sesudah *admit* dan menyediakan endpoint *Tolak* | Muhammad Hamzah (Rawat Inap) | Prosedur: admisi diproses dulu, baru pasien diantar | `BE-IGD-077` 🟡, `IGD-REQ-003`, `FR-IGD-119`, `120` |
| C2 | Aturan *dokter pemberi instruksi* Lab/Rad diperluas ke encounter IGD — atau dinyatakan tidak | Yoga Aji Pratama (Radiologi), pemilik Laboratorium | Dokter memesan langsung dari Ruang Kerja Dokter (`FE-IGD-052` ✅); pesanan perawat tanpa verifikasi | `BE-IGD-071`, `FE-IGD-058`, `FE-IGD-057`, `IGD-REQ-007` |
| C3 | Gizi menyatakan melayani atau tidak pesanan konsultasi pasien IGD | Pemilik Gizi | Empat jenis penunjang lain berjalan | `FE-IGD-053`, `IGD-REQ-008` |
| C4 | Security/Privacy owner ditunjuk dan memutuskan `IGD-DEC-092` (jalan keluar beralasan) serta `IGD-OQ-143` (penyaring pasien di empat endpoint `ClinicalManagement`) | Rizki → manajemen | Layar IGD menyaring per kunjungan; risikonya pada pemanggil lain | `BE-IGD-039`, `IGD-OQ-143` |
| C5 | Lingkungan rilis siap: master data IGD diisi (seeder nol pemanggil), `IGD-UNK-18` dicatat, `max_locks_per_transaction` > 64 untuk basis data baru, akses bed petugas admisi tidak lagi per `UserId` di `appsettings.json` | Rizki / Platform | Isi master lewat layar Master Data; konfigurasi manual per lingkungan | `IGD-FACT-236`, `136`, `127`, `IGD-UNK-18` |
| C6 | Keputusan atas validasi Emergency Visit yang dimatikan sementara sejak 7 Agustus 2026 | Rizki | Backend menerima pendaftaran tanpa cara datang/jenis kasus — sah menurut kontrak | `IGD-FACT-238` |
| C7 | Izin uji `IGD-DEC-358` tidak terbawa ke lingkungan lain; role-access ditata ulang per lingkungan — termasuk izin pendaftaran loket dan tagihan | Rizki | Cadangan policy tersimpan; berlaku di dev saja | `IGD-DEC-358`, `IGD-DEC-352` |
| C8 | UAT oleh tim terpisah dijalankan dan hasilnya dicatat terpisah | Tim UAT | Bukti layar agen per task tersedia sebagai bahan UAT | Aturan pemilik; tidak diklaim di sini |

Syarat yang **tidak** menahan IGD tetapi perlu diketahui: Pendaftaran Rawat Jalan memanggil rute kiosk yang
sama (`IGD-REQ-009`).

## 6. Ketidaksesuaian dokumen dan bukti `STALE`

| # | Temuan | Arah |
| ---: | --- | --- |
| 1 | `FR-IGD-109`, `110`, `111`, `126` di traceability menulis keadaan sebelum 18–19 Oktober | Satu pass dokumentasi — baris status saja |
| 2 | Tabel fase `MODULE-STATUS.md`: `MVP-5` masih `IN_PROGRESS` padahal kolomnya sendiri menyatakan *"butir terakhir gelombang ini lunas"*; `MVP-9` masih `DESIGN` padahal kartunya sudah dikerjakan dan sebagian besar ✅ | Satu pass dokumentasi |
| 3 | Tanda node grafik dependency berbeda dari register untuk 9 node backend (`BE-IGD-017` 🟡 padahal dibatalkan; `035`, `065`…`068` 🟡 padahal ✅; `043`, `070` tanpa tanda) dan 16 node frontend (`FE-IGD-013`, `029`, `031`, `032`, `045`…`051` 🟡 padahal ✅; `025`, `026`, `054`, `055`, `056` tanpa tanda; `057` ⛔ padahal 🟢) | `plan-module-delivery` — sinkronkan tanda node |
| 4 | Judul kartu berbeda dari register: `BE-IGD-047`, `069`; `FE-IGD-020`, `021`, `029`, `031`, `032`, `033`, `045`, `046`, `048`, `049`, `050`, `051`, `056` | Sama dengan butir 3 |
| 5 | Baris register `FE-IGD-056` dan `FE-IGD-065` tanpa tautan laporan, padahal `FE-IGD-056.md` dan `FE-IGD-065.md` ada | Satu pass dokumentasi |
| 6 | Snapshot EF: memori 7 September mencatat 39 blok entitas modul lain hilang; `IGD-FACT-230` (26 Oktober) menunjukkan migration terakhir mengubah snapshot tepat satu baris sesudah merge integration tanpa konflik | **Tidak diukur ulang** pada audit ini; diperiksa sebelum merge balik ke integration |

## 7. Risiko yang dicatat (bukan syarat)

| Risiko | Bukti | Catatan |
| --- | --- | --- |
| Sapaan *Mr./Mrs.* tidak dapat tersimpan | `IGD-FACT-252` | Kosmetik-klinis; pasien tanpa jenis kelamin teramati tidak dapat didaftarkan lewat jalur tanpa identitas |
| Isian *Metode Pembayaran Sistem* tidak pernah tampil di Pendaftaran IGD | `IGD-FACT-251` | Encounter terbentuk tanpanya; dampak ke kasir belum dinilai |
| Penulis catatan dokter disajikan sebagai DPJP | `IGD-OQ-140` | Jejak dokumentasi klinis |
| Tarif kunjungan tanpa kelas dicocokkan lewat string `"RAWAT JALAN"` di berkas Rawat Inap | `IGD-OQ-142` | Bergantung pada nama master data |
| Regresi tanpa test otomatis | Proyek test dihapus; unit test dilarang | Setiap perubahan bergantung pada uji layar terskrip |
| Status kartu Riwayat tampil angka mentah | `fe-igd-022` laporan bagian 18.3 | Kosmetik |

## 8. Langkah berikutnya

1. Kirim `IGD-REQ-003` (tindak lanjut), `IGD-REQ-007`, `IGD-REQ-008`, dan `IGD-REQ-009` — empat dokumen
   sudah siap; tiga yang pertama menahan C1–C3.
2. Satu pass dokumentasi terbatas untuk bagian 6 butir 1, 2, dan 5; butir 3–4 lewat `plan-module-delivery`.
3. Tetapkan pemilik C4 dan C5 sebelum menjadwalkan rilis ke lingkungan bersama.
4. Serahkan ke tim UAT dengan daftar bukti per task (C8). Putusan ini dapat naik ke `READY` untuk UAT
   penuh bila C1–C3 lunas, dan ke kesiapan produksi bila C4–C7 lunas.
