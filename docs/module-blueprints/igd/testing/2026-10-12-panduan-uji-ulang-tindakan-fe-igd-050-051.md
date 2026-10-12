# Panduan Uji Ulang Layar Tindakan IGD — `FE-IGD-050` dan `FE-IGD-051`

| Field | Nilai |
| --- | --- |
| Tanggal disusun | 12 Oktober 2026 |
| Untuk | Agen penguji Antigravity yang dijalankan pemilik modul (Rizki) |
| Dasar | Pemeriksaan bukti putaran 3 (`IGD-FACT-118`); keputusan `IGD-DEC-296` — bukti layar putaran 3 tidak sah sebagai uji hasil build |
| Cakupan | Acceptance kartu `FE-IGD-050` (1, 3, 4, 5) dan `FE-IGD-051` (1–4). Acceptance diff/`eslint`/build diperiksa agent, bukan penguji. `FE-IGD-050` acceptance 2 **tidak diuji** (bagian 4) |
| Jumlah skenario | 9 — U-01…U-09 |
| Backend | `rizkiG` HEAD, dijalankan pemilik dari build pemilik. Agen tidak menjalankan `dotnet build` |
| Frontend | `RizkiV2` `03b057c03` atau lebih baru. Wajib hasil build produksi: `.next/BUILD_ID` lebih baru dari commit itu; server port 3000 = `node .next/standalone/server.js` |
| Folder bukti | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-tindakan-<YYYY-MM-DD>/` |
| Laporan | `NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-ulang-tindakan.md` |

---

## 1. Aturan yang mengikat

Putaran 3 ditolak sebagai uji layar karena dilayani `next dev`, tangkapan layar dipakai ulang, dan tiga putaran tidak
dilaporkan. Aturan berikut diperiksa agent terhadap log backend.

| No | Aturan |
| ---: | --- |
| 1 | Layar dilayani hasil build produksi, bukan `next dev`. Setiap sesi layar mencatat `document.querySelector('nextjs-portal') === null`; tangkapan layar **tanpa** lencana "N" di pojok kiri bawah |
| 2 | Viewport 1440 × 900. **Satu tangkapan layar per skenario**, diambil sesudah aksi skenario itu dan minimal 1000 ms sesudah respons terakhir. Berkas tangkapan layar tidak boleh identik antar skenario |
| 3 | Akun perawat dari `QUILVIAN_PERAWAT_EMAIL`/`QUILVIAN_PERAWAT_PASSWORD`; akun dokter dari `QUILVIAN_DOKTER_EMAIL`/`QUILVIAN_DOKTER_PASSWORD`. Tanpa SuperAdmin, tanpa *Simpan Akses*, tanpa `/api/v1/administrator/**` |
| 4 | Sandi dan token tidak pernah ditulis di skrip, JSON, atau laporan. Header `Authorization` dan `Cookie` disamarkan |
| 5 | **Tidak ada kueri basis data sama sekali, termasuk `SELECT`.** Tidak mengubah data master, Akses Role, atau penugasan |
| 6 | Penulisan yang boleh **hanya**: U-02 (satu draf catatan dokter, satu tindakan dokter), U-06 (satu tindakan keperawatan), U-07 (satu tindakan keperawatan lewat klik ganda), U-08 dan U-09 (penyimpanan yang harus ditolak). Penulisan lain adalah pelanggaran. Tidak ada penulisan lewat API langsung — semua lewat layar |
| 7 | **Setiap** percobaan dicatat di `percobaan.json` (jam mulai dan selesai WIB, skenario, jumlah permintaan), termasuk percobaan gagal dan pengulangan. Setiap skrip disimpan dan tidak ditimpa (`run-u01-p1.mjs`, `run-u01-p2.mjs`, …). Catatan jaringan merekam **semua** URL `/api/v1/` beserta kode statusnya |
| 8 | `PASS` hanya bila **semua** syarat pada kolom *Lulus bila* terpenuhi — bukan salah satu. Hasil ditulis `PASS`, `FAIL`, atau `NOT RUN` beserta alasannya. Kegagalan skrip (selector, timeout) dibedakan dari kegagalan perilaku. Jangan menulis "UAT lulus" |
| 9 | Penguji **tidak** menyunting roadmap, laporan task, `MODULE-STATUS.md`, atau decision log, dan tidak menulis ✅ di mana pun. Penilaian dilakukan agent dari bukti mentah |

---

## 2. Data uji

| Kode | Isi | Keadaan awal yang diharapkan |
| --- | --- | --- |
| T1 | Kunjungan `IGD-261009030511-26F549`, encounter `cc382d65-9a5e-46d6-9f6d-3b53994f5761`, pasien RM `00-00-00-19` | Berjalan; DPJP IGD dr. Ranger Biru (akun dokter uji); tiga tindakan keperawatan dari putaran 3; tanpa catatan dokter terbuka |
| T2 | Kunjungan IGD berjalan **tanpa DPJP** — disiapkan pemilik lewat layar pendaftaran dan triage | Belum ada penugasan DPJP |
| T3 | Kunjungan IGD yang **sudah berakhir** (`Completed`) — dipilih pemilik | Status kunjungan `Completed` |

**P0 — Pemeriksaan awal (hanya `GET`).** Untuk T1, T2, T3: `GET …/emergency-installation-management/emergency-visits/{id}`
dan `GET …/clinical-management/patient-procedures?encounterId={encounterId}&pageSize=100`. Catat jumlah tindakan awal tiap
kunjungan. Bila keadaan tidak cocok dengan kolom terakhir, hentikan putaran dan laporkan.

URL layar:

- Dokter: `http://localhost:3000/health-services/emergency-installation-management/doctor-emergency`
- Perawat: `http://localhost:3000/health-services/emergency-installation-management/emergency-assessment/{emergencyVisitId}`

---

## 3. Skenario

Jalankan berurutan. U-02 wajib sebelum U-03 dan U-06.

### 3.1 Layar dokter — `FE-IGD-050`

| ID | Langkah | Lulus bila | Acceptance |
| --- | --- | --- | --- |
| U-01 | Akun dokter → Ruang Kerja Dokter IGD → pilih T1 → tab *Tindakan* | Tab *Tindakan* aktif; kotak *"Belum Ada Catatan Dokter (SOAP) Aktif"* dan tombol *Buka Catatan Dokter* tampil; tidak ada isian katalog yang dapat dipakai | 050-4 |
| U-02 | Tab *Catatan Dokter* → buka draf catatan dokter → kembali ke tab *Tindakan* → pilih satu tindakan dari katalog → simpan | Jaringan: satu `POST …/patient-procedures` berhasil (`200`/`201`) yang merujuk catatan terbuka; daftar tindakan memuat baris baru beserta nama dokter. Tangkapan layar sesudah simpan | 050-1 |
| U-03 | Segmen riwayat/daftar tindakan pada tab *Tindakan* dokter | Baris tindakan dokter dari U-02 **dan** sedikitnya satu baris berlencana *Keperawatan* terlihat bersama di tangkapan layar, masing-masing dengan asalnya | 050-3 |
| U-04 | Regresi: Ruang Kerja Dokter Rawat Inap → satu pasien rawat inap → tab *Tindakan*, hanya membaca | Tab terbuka tanpa galat dan memuat daftar tindakan pasien itu. Bila tidak ada pasien rawat inap yang dapat dibuka akun dokter uji: `NOT RUN` dengan alasan | 050-5 |

### 3.2 Layar perawat — `FE-IGD-051`

| ID | Langkah | Lulus bila | Acceptance |
| --- | --- | --- | --- |
| U-05 | Akun perawat → Pengkajian Pasien IGD T1 → menu *Tindakan* → segmen *Daftar Tindakan* | Daftar memuat tindakan klinis umum: baris tindakan dokter dari U-02 **dan** baris berlencana *Keperawatan*. Jaringan: `GET …/patient-procedures?encounterId=` (bukan tabel tindakan IGD lama). Bagian riwayat tindakan lama hanya tampil bila tabel lama berisi | 051-4 |
| U-06 | Segmen *Catat Tindakan* → isi satu tindakan keperawatan → simpan → segmen *Daftar Tindakan* | Jaringan: satu `POST …/emergency-nursing-actions` berhasil; baris baru berlencana *Keperawatan* menampilkan pelaksana **Dimas Kurniawan** dan DPJP **dr. Ranger Biru** dalam satu baris. Tangkapan layar sesudah simpan | 051-1 |
| U-07 | Isi satu tindakan keperawatan lagi → klik *Simpan* dua kali secepat mungkin | Semua `POST` membawa `idempotencyKey` yang sama; jumlah tindakan T1 bertambah **tepat satu** (cek `GET` sesudahnya). Tombol nonaktif selama permintaan berjalan | 051-2 |
| U-08 | Pengkajian Pasien IGD T2 (tanpa DPJP) → catat satu tindakan keperawatan → simpan | Ditolak; pesan persis *"Pasien belum punya dokter penanggung jawab. Tetapkan DPJP di layar triage lebih dulu, lalu catat tindakan ini."* tampil di formulir; isian tidak dikosongkan; jumlah tindakan T2 tidak bertambah | 051-3 |
| U-09 | Pengkajian Pasien IGD T3 (kunjungan berakhir) → coba catat satu tindakan keperawatan | Bila layar mengizinkan simpan: ditolak dan pesan server tampil apa adanya; jumlah tindakan T3 tidak bertambah. Bila layar tidak menyediakan simpan: catat keadaannya dengan tangkapan layar | 051-3 |

**Pengamatan, bukan putusan.** Bila daftar tindakan perawat menampilkan teks mentah *"NotApplicable"* di bawah biaya,
catat di laporan (`IGD-FACT-119`).

---

## 4. Tidak diuji pada putaran ini

| Acceptance | Alasan |
| --- | --- |
| `FE-IGD-050` acceptance 2 — tindakan terbaca ringkasan tagihan pasien | Ringkasan tagihan pasien hanya tersedia per episode rawat inap (`GET …/billing-management/patient-billing-summaries/episodes/{episodeId}/…`); tidak ada ringkasan untuk kunjungan IGD, dan tindakan dokter IGD hari ini tidak menerbitkan fakta tagih (`IGD-FACT-093`, `095`). Keputusannya milik pemilik |
| `FE-IGD-050` acceptance 6, `FE-IGD-051` acceptance 5 — diff, komentar, akhiran baris, `eslint`, build | Diperiksa agent |

---

## 5. Laporan

Laporan memuat: metadata lingkungan (SHA, `BUILD_ID`, proses port 3000), daftar percobaan dari `percobaan.json`, tabel
U-01…U-09 dengan putusan dan berkas bukti, nama pasien yang diambil dari layar, dan seluruh penyimpangan dari bagian 1.
Agent akan mencocokkan setiap putusan dengan bukti mentah dan `Logs/quilvian-backend-<YYYYMMDD>.json` sebelum status task
diubah.
