# Roadmap Delivery Frontend — Modul Rawat Inap

## Metadata

```yaml
module_id: rawat-inap
repository: QuilvianSystemFrontendDev
roadmap_revision: 3
status: APPROVED
approval_gate: BLUEPRINT_APPROVED
owners:
  - "Product/Domain: Muhammad Hamzah (RWI-DEC-061)"
  - "Frontend authority: sesuai 03-frontend-architecture.md bagian 9"
approved_by:
  - "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061); revision 3 lewat RWI-DEC-075 s.d. RWI-DEC-079"
approved_at: "2026-08-27"
input_revisions:
  blueprint-manifest.md: 4
  03-frontend-architecture.md: 0.4
  04-prd-to-mvp.md: 0.4.0
  01-existing-capability-map.md: 1.2
contract_versions:
  - "API 0.4.0"
  - "Permission/Audit 0.4.0"
  - "Validation 0.4.0"
source_commits:
  backend: "5afb54bd75281648010e50ef14f43ca1f80d8efd"
  frontend: "dec4fdeff07c3c96ad9f07f41f184c54cf771371"
task_count: 35
task_count_done: 18
task_count_open: 17
supersedes: "roadmap_revision 2 — arsip di roadmap/archive/revision-2/frontend-roadmap.md"
```

---

## 0. Apa yang berubah dari revision 2, dan kenapa

Revision 2 memuat 19 task; **18 selesai**. Meski begitu, hasilnya **tidak dapat menjalankan
`FLOW-RI-MVP-001`** dari awal sampai akhir. Sebabnya bukan pelaksanaan, melainkan tiga cacat pada
`03-frontend-architecture.md` revision `0.3` yang diwarisi roadmap ini apa adanya.

| Yang hilang | Kenapa hilang | Ditutup oleh |
| --- | --- | --- |
| Memilih penjamin saat masuk (`RWI-CAP-002`, **Wajib**) | Disebut pada daftar layar, tidak pernah menjadi task, dan tidak ada kolomnya di `OpenAdmissionRequest` | `FE-RWI-024`, `FE-RWI-025` |
| Memesan tempat tidur (`RWI-CAP-006`, **Wajib**) | Tidak punya layar, tidak punya task | `FE-RWI-026` |
| Membatalkan admisi | Disebut pada matriks peran, tidak punya layar | `FE-RWI-031` |
| Menemukan episode `Draft` dan `Closed` | Tidak pernah ada daftar kerja; census hanya memuat yang sedang dirawat | `FE-RWI-020` |
| Beranda modul yang berguna | Tidak pernah dispesifikasikan | `FE-RWI-021` |

Akibatnya **sembilan operasi HTTP** yang sudah jadi di backend tidak pernah dipanggil satu pun
layar, dan satu layar yang sudah jadi — sesi koreksi `FE-RWI-018` — praktis tidak dapat dicapai.

Revision 3 juga menyerap bentuk baru admisi: dari satu formulir menjadi **alur berlangkah dua
jalur** sesuai `RWI-DEC-075`, dengan tulisan bertahap sesuai `RWI-DEC-076`.

### 0.1 Yang **tidak** berubah

- **Tidak ada perubahan backend.** Ke-49 endpoint kontrak `0.4.0` sudah tersedia dan terbukti
  berjalan 26 Agustus 2026. Endpoint milik Registrasi dan PatientManagement yang dipakai alur
  admisi juga sudah ada. `backend-roadmap.md` **tidak** dirancang ulang.
- Delapan belas task yang selesai **tetap dihitung selesai**. Yang hilang memang tidak pernah
  dispesifikasikan, bukan dikerjakan salah.
- Aturan privasi, penanganan 409/422, dan aturan tombol tetap berlaku apa adanya.

---

## 1. Batas kewenangan dokumen ini

`03-frontend-architecture.md` revision `0.4` menetapkan **kontrak fungsional**. Ia **tidak**
menetapkan warna, tata letak, pustaka komponen, nama menu, atau nama route.

Urutan wewenang pada setiap task di bawah:

```text
keamanan / privasi / invariant / keterjangkauan
  -> brief produk atau UI yang disetujui
  -> konvensi dan design system project
  -> DEV_DISCRETION
```

Enam hal yang **bukan** `DEV_DISCRETION`, dan karena itu ditulis sebagai acceptance criteria yang
mengikat: peta alur bagian 2A, aturan keterjangkauan `IA-INP-01` s.d. `IA-INP-05` bagian 2B, aturan
tombol bagian 3, kontrak alur berlangkah bagian 3A, penanganan 409 dan 422 bagian 5.4, dan privasi
bagian 6.

**Aturan baru yang perlu diperhatikan pelaksana:** `IA-INP-04` — layar yang tidak terjangkau dari
mana pun dihitung **belum selesai**, walaupun kodenya ada dan test-nya lulus. Aturan ini lahir dari
`FE-RWI-018`.

---

## 2. Keadaan awal revision 3

| Hal | Keadaannya |
| --- | --- |
| Endpoint backend | **Seluruhnya tersedia.** Tidak ada task yang menunggu backend |
| Route Rawat Inap | 13 ada |
| Menu Rawat Inap | Ada, delapan butir |
| Beranda modul | Ada tetapi hanya berisi kalimat penantian |
| Admisi | Satu formulir; **akan dibongkar** menjadi alur berlangkah |
| Berkas test frontend | Bertambah banyak sejak revision 2; e2e per task tersedia |

Karena tidak ada lagi task yang menunggu backend, **paralelisme revision 3 dibatasi hanya oleh
dependency antar task frontend**.

---

## 3. Slice dan milestone

| Slice | Hasil yang dapat diperiksa | Task | Keadaan |
| --- | --- | --- | --- |
| **F0–F7** | Pekerjaan revision 2 | `FE-RWI-001` s.d. `FE-RWI-018` | ✅ selesai |
| **F8 — Keterjangkauan** | Setiap episode dapat ditemukan; beranda berguna | `FE-RWI-020`, `FE-RWI-021` | terbuka |
| **F9 — Alur admisi** | Petugas dapat mendaftarkan pasien, memilih penjamin, membuka episode, dan memesan tempat tidur dalam satu alur | `FE-RWI-022` s.d. `FE-RWI-027` | terbuka |
| **F10 — Cetak** | Persetujuan rawat inap dan kartu pasien tercetak dari alur | `FE-RWI-028`, `FE-RWI-029` | terbuka |
| **F11 — Aksi yang hilang** | Pasien dikonfirmasi masuk; admisi dapat dibatalkan; admisi tertinggal dapat dilanjutkan | `FE-RWI-030` s.d. `FE-RWI-032` | terbuka |
| **F12 — Perapian dan kesiapan** | Navigasi rapi, jalur ganda hilang, seluruhnya terbukti | `FE-RWI-033` s.d. `FE-RWI-035` | terbuka |

### Urutan dependency

```text
FE-RWI-020 (daftar kerja episode)
   ├── FE-RWI-021 (beranda)
   └── FE-RWI-032 (melanjutkan admisi tertinggal)   ← juga butuh FE-RWI-026

FE-RWI-022 (kerangka alur dua jalur)
   └── FE-RWI-023 (langkah Pendaftaran + Pasien Lama)
          └── FE-RWI-024 (langkah Pembayaran: penjamin + kelas)
                 └── FE-RWI-025 (langkah Dokter — TITIK TULIS 1)
                        └── FE-RWI-026 (Pilih Bed + Booking Bed — TITIK TULIS 2)
                               └── FE-RWI-027 (Konfirmasi — TITIK TULIS 3)
                                      ├── FE-RWI-028 (cetak persetujuan)
                                      └── FE-RWI-029 (cetak kartu pasien)

FE-RWI-030 (konfirmasi pasien masuk)   ← butuh FE-RWI-026
FE-RWI-031 (pembatalan admisi)         ← butuh FE-RWI-020

FE-RWI-033 (keterjangkauan + menu)     ← butuh F8 s.d. F11
FE-RWI-034 (bongkar layar admisi lama) ← butuh FE-RWI-027
FE-RWI-035 (kesiapan diuji ujung ke ujung) — paling akhir
```

---

## 4. Task revision 2 — register status

Kartu lengkap kesembilan belas task ini ada pada arsip `roadmap/archive/revision-2/frontend-roadmap.md`.
Yang di bawah adalah registernya.

| ID | Hasil | Status | Laporan |
| --- | --- | :---: | --- |
| `FE-RWI-001` | Admin dapat menutup tempat tidur yang rusak | ✅ | [FE-RWI-001](../task/report/frontend/FE-RWI-001.md) |
| `FE-RWI-002` | Kerangka pemanggilan Rawat Inap berdiri | ✅ | [FE-RWI-002](../task/report/frontend/FE-RWI-002.md) |
| `FE-RWI-003` | Admin dapat mengubah pengaturan Rawat Inap | 🟡 3 dari 4 kriteria | [FE-RWI-003](../task/report/frontend/FE-RWI-003.md) |
| `FE-RWI-004` | Admin dapat mengelola butir daftar periksa | 🟡 3 dari 4 kriteria | [FE-RWI-004](../task/report/frontend/FE-RWI-004.md) |
| `FE-RWI-005` | Papan tempat tidur yang benar-benar dapat dipakai | ✅ | [FE-RWI-005](../task/report/frontend/FE-RWI-005.md) |
| `FE-RWI-006` | Membuka admisi beserta catatan awal isolasi | ✅ | [FE-RWI-006](../task/report/frontend/FE-RWI-006.md) |
| `FE-RWI-007` | Penolakan penempatan terbaca alasannya | ✅ | [FE-RWI-007](../task/report/frontend/FE-RWI-007.md) |
| `FE-RWI-008` | Census — siapa dirawat, di mana, berapa hari | ✅ | [FE-RWI-008](../task/report/frontend/FE-RWI-008.md) |
| `FE-RWI-009` | Detail episode utuh beserta riwayatnya | ✅ | [FE-RWI-009](../task/report/frontend/FE-RWI-009.md) |
| `FE-RWI-010` | Perpindahan pasien beserta penjaga DPJP | ✅ | [FE-RWI-010](../task/report/frontend/FE-RWI-010.md) |
| `FE-RWI-011` | DPJP dan perawat penanggung jawab dialihkan | ✅ | [FE-RWI-011](../task/report/frontend/FE-RWI-011.md) |
| `FE-RWI-012` | Keputusan pulang dan resume bertanda tangan | 🟡 4 dari 5 kriteria | [FE-RWI-012](../task/report/frontend/FE-RWI-012.md) |
| `FE-RWI-013` | Kasir menandai kelayakan keuangan | 🟡 3 dari 4 kriteria | [FE-RWI-013](../task/report/frontend/FE-RWI-013.md) |
| `FE-RWI-014` | Kelima syarat penutupan dan jalan keluar supervisor | ✅ | [FE-RWI-014](../task/report/frontend/FE-RWI-014.md) |
| `FE-RWI-015` | Pencatatan kepergian pasien | 🟡 kriteria 4 siap dinaikkan | [FE-RWI-015](../task/report/frontend/FE-RWI-015.md) |
| `FE-RWI-016` | Empat daftar pantau | ✅ | [FE-RWI-016](../task/report/frontend/FE-RWI-016.md) |
| `FE-RWI-017` | Laporan selisih tempat tidur | ✅ | [FE-RWI-017](../task/report/frontend/FE-RWI-017.md) |
| `FE-RWI-018` | Sesi koreksi episode | ✅ layar jadi, **belum terjangkau** — ditutup `FE-RWI-033` | [FE-RWI-018](../task/report/frontend/FE-RWI-018.md) |
| `FE-RWI-019` | Kesiapan diuji per peran | ⛔ **dibuka ulang** — cakupannya digantikan `FE-RWI-035` karena jumlah layar bertambah | — |

**Empat task bertanda 🟡 tidak dibuka ulang.** Kriteria yang tertahan menyangkut bentuk pesan server
dan kontrak pembacaan, bukan kemampuan yang hilang. Penyelesaiannya menjadi bagian `FE-RWI-035`.

---

## 5. Task revision 3

### `FE-RWI-020` — Setiap episode dapat ditemukan, termasuk yang tertinggal

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas dapat menemukan episode apa pun menurut status, unit layanan, kelas, rentang tanggal, dan kata kunci — termasuk `Draft` yang ditinggal di tengah admisi dan `Closed` yang perlu dikoreksi. Tanpa ini, layar sesi koreksi yang sudah jadi tidak dapat dicapai siapa pun |
| **Trace** | `03-frontend-architecture.md` `FE-INP-16`, `IA-INP-02`, `IA-INP-03`, `IA-INP-04`; bagian 11A |
| **Reuse** | `DataTable`, `DataFilter`, `FilterSelect`, `ResourceFilterSelect`, `RegionPagination` yang dipakai census; `inpatient-api.service.js` |
| **Scope** | Route daftar episode, view, hook, constants, utils. `GET /episodes`, `GET /episodes/filters/metadata` |
| **Dependency** | — |
| **Wewenang UI** | Nama menu, urutan kolom, dan bentuk penyaring `DEV_DISCRETION`. **Batasnya:** kelima nilai status wajib dapat dipilih |
| **Acceptance criteria** | 1. Kelima nilai status episode dapat disaring, termasuk `Draft`, `Cancelled`, dan `Closed`. 2. Baris `Draft` yang masih memegang pemesanan tempat tidur **terbeda** dari yang pemesanannya sudah gugur, dan sisa waktunya terbaca. 3. Setiap baris membuka detail episode. 4. Kolom sensitif — diagnosis, catatan episode, keterangan isolasi — **tidak** muncul. 5. Keempat keadaan daftar bagian 5.1 terpenuhi |
| **Verification** | E2E: menyaring `Draft` menampilkan episode yang belum punya tempat tidur; menyaring `Closed` menampilkan episode tertutup dan membukanya sampai layar sesi koreksi |
| **Risk/blocker** | Godaan terbesar adalah menjadikan ini census kedua. Census berarti "sedang dirawat"; mencampurnya melanggar `IA-INP-03`. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus; laporan menyebut endpoint mana yang berhenti menganggur |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-021` — Beranda Rawat Inap menjadi pintu masuk, bukan halaman penantian

| Field | Isi |
| --- | --- |
| **Outcome** | Orang yang membuka menu Rawat Inap langsung melihat keadaan hari ini dan tahu ke mana harus pergi. Hari ini yang terbaca hanya "kemampuan operasional akan tersedia bertahap" |
| **Trace** | `03-frontend-architecture.md` `FE-INP-19`, bagian 2B "Isi Beranda", `IA-INP-01` |
| **Reuse** | `Hero`, kartu ringkasan yang sudah ada di modul lain; `inpatient-api.service.js` |
| **Scope** | `src/app/health-services/inpatient-management/page.jsx` beserta view dan hooknya. `GET /episodes/summary`, `GET /census/summary`, keempat endpoint daftar pantau |
| **Dependency** | `FE-RWI-020` |
| **Wewenang UI** | Tata letak `RWI-FE-005`, `DEV_DISCRETION`. **Batasnya:** ketiga isi wajib tercapai dan setiap angka dapat diklik |
| **Acceptance criteria** | 1. Jumlah pasien dirawat per unit layanan dan per kelas terbaca. 2. Jumlah episode per status terbaca; angka `Draft` dapat diklik menuju daftar kerja yang **sudah tersaring** `Draft`. 3. Jumlah baris keempat daftar pantau terbaca dan dapat diklik. 4. Setiap layar tingkat dua Rawat Inap dapat dicapai dari sini dalam paling banyak tiga klik — `IA-INP-01`. 5. Tidak ada lagi kalimat penantian |
| **Verification** | E2E: dari beranda, klik angka `Draft` mendarat pada daftar kerja tersaring; ketiga blok ringkasan terbaca angkanya |
| **Risk/blocker** | Angka yang tidak dapat diklik membuat beranda jadi hiasan. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-022` — Kerangka alur admisi dua jalur berdiri

| Field | Isi |
| --- | --- |
| **Outcome** | Admisi berhenti menjadi satu formulir. Berdiri kerangka berlangkah dengan dua jalur masuk — pasien baru dan pasien lama — yang langkah-langkah berikutnya tinggal diisi |
| **Trace** | `RWI-DEC-075`; `03-frontend-architecture.md` 3A.1, 3A.2 langkah 1, 3A.3 langkah 1–3, 5.5 |
| **Reuse** | **Wajib** memakai pola `emergency-registration/`: `patient-entry-choice-step`, `emergency-registration-stepper`. Mengarang kerangka langkah keempat untuk pekerjaan yang sama **tidak diizinkan** |
| **Scope** | Route admisi, kerangka langkah, penanda langkah, langkah **Tipe Pasien**, pemulihan langkah dari URL |
| **Dependency** | — |
| **Wewenang UI** | Nama dan label langkah `RWI-FE-003`; bentuk penanda langkah `RWI-FE-004`. **Batasnya:** urutan dan isi langkah mengikat |
| **Acceptance criteria** | 1. Dua jalur masuk tersedia dan terpisah. 2. Kesembilan langkah jalur pasien baru dan kedelapan langkah jalur pasien lama tampil pada penanda langkah dengan urutan sesuai 3A.2 dan 3A.3. 3. Langkah yang sedang berjalan dan yang sudah lewat **terbeda**. 4. Memuat ulang halaman di tengah alur **memulihkan** langkah yang sedang dikerjakan dari URL, bukan mengembalikannya ke langkah 1. 5. Jenis pasien **bayi baru lahir** menampilkan pilihan episode ibu; jenis lain tidak |
| **Verification** | E2E: memilih jalur pasien baru, maju satu langkah, memuat ulang halaman, dan langkahnya tetap |
| **Risk/blocker** | Menyimpan langkah hanya di state React membuat kriteria 4 gagal dan membuat alur bertahap `RWI-DEC-076` berbahaya. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; laporan menyebut berkas mana dari `emergency-registration/` yang dipakai ulang |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-023` — Pasien dapat didaftarkan atau ditemukan dari dalam alur admisi

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas admisi tidak perlu keluar ke modul lain untuk mendaftarkan pasien. Jalur pasien lama menemukan pasien dan menampilkan datanya untuk ditinjau |
| **Trace** | `FLOW-RI-MVP-001` langkah 1; 3A.2 langkah 2; 3A.3 langkah 1–2 |
| **Reuse** | `new-patient-form`, `patient-selection-step`, `plustek-scan-panel` dari `emergency-registration/`; scan KTP kiosk |
| **Scope** | Langkah **Pendaftaran** jalur baru; langkah **Pasien Lama** dan **Informasi Pasien Lama** jalur lama. `POST /patients`, `POST /patient-identity-documents`, `POST /patient-emergency-contacts`, `GET /patients/options` |
| **Dependency** | `FE-RWI-022` |
| **Wewenang UI** | Susunan isian dan pemakaian scanner `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Pasien baru tersimpan beserta dokumen identitas dan kontak darurat. 2. Pencarian pasien lama menerima nomor rekam medis dan NIK. 3. Data pasien lama ditinjau sebelum alur dilanjutkan. 4. Penolakan server ditampilkan apa adanya dan isian **tidak hilang**. 5. Menekan simpan dua kali hanya menghasilkan satu pasien |
| **Verification** | E2E kedua jalur; pemeriksaan jaringan bahwa tidak ada pasien kembar saat tombol ditekan dua kali |
| **Risk/blocker** | Data pasien adalah data pribadi. Contoh dan data uji **tidak boleh** memakai data asli. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; e2e kedua jalur ada dan lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-024` — Penjamin dan kelas perawatan dipilih, bukan diasumsikan

| Field | Isi |
| --- | --- |
| **Outcome** | Cara bayar pasien rawat inap ditentukan sadar oleh petugas. **Inilah kemampuan yang hilang pada revision 2** dan yang membuat setiap admisi tercatat tunai |
| **Trace** | `RWI-CAP-002` **Wajib**; `FLOW-RI-MVP-001` langkah 3; 3A.2 langkah 3; `04-prd-to-mvp.md` bagian 7 |
| **Reuse** | `payment-method-step`, `emergency-patient-payer-modal`, `patient-payer-drawer`, `patient-payer-table` dari `emergency-registration/` |
| **Scope** | Langkah **Pembayaran**. Tunai, asuransi, penjamin perusahaan. Pemilihan atau pendaftaran kartu. **Pemilihan kelas perawatan.** `POST /patient-insurances`, `POST /patient-company-guarantors` |
| **Dependency** | `FE-RWI-023` |
| **Wewenang UI** | Bentuk pemilihan penjamin `DEV_DISCRETION`. **Batasnya:** kelas perawatan wajib dipilih di langkah ini |
| **Acceptance criteria** | 1. Ketiga cara bayar tersedia dan dipilih sadar — **tidak ada** nilai bawaan yang tersimpan diam-diam. 2. Asuransi dan penjamin perusahaan menuntut kartunya dipilih atau didaftarkan; tanpa itu langkah tidak dapat dilanjutkan. 3. Kelas perawatan dipilih di langkah ini. 4. Nomor kartu asuransi dan nomor peserta **tidak** muncul di luar langkah ini dan formulir cetak — bagian 6. 5. Isian tidak hilang ketika server menolak |
| **Verification** | E2E ketiga cara bayar; pemeriksaan bahwa melanjutkan tanpa kartu ditolak di layar dengan nol permintaan terkirim |
| **Risk/blocker** | Kriteria 1 adalah inti perbaikan revision ini. Menyediakan "tunai" sebagai pilihan terpilih otomatis mengulang cacat yang sama dalam bentuk lain. Owner: Frontend bersama Product/Domain |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-025` — Kunjungan dan episode terbentuk beserta penjaminnya — titik tulis 1

| Field | Isi |
| --- | --- |
| **Outcome** | Unit layanan, DPJP, dan kebutuhan isolasi ditetapkan, lalu kunjungan rawat inap dan episode `Draft` terbentuk. Kunjungan yang terbentuk **membawa penjamin yang dipilih**, bukan tunai bawaan |
| **Trace** | `FLOW-RI-MVP-001` langkah 2, 3, 4; 3A.2 langkah 4; 3A.4 titik tulis 1; bagian 11A catatan `POST /patient-encounters` |
| **Reuse** | Isian pilihan sumber daya yang sudah ada; `use-inpatient-admission` bagian isolasi |
| **Scope** | Langkah **Dokter**. Berurutan: `POST /patient-encounters` dengan `EncounterType=Inpatient` dan `RegistrationSource=InpatientAdmission` → `POST /episodes` dengan `EncounterId` terisi → `PATCH /episodes/{id}/isolation-requirement` bila isolasi menyala |
| **Dependency** | `FE-RWI-024` |
| **Wewenang UI** | Susunan isian `DEV_DISCRETION`. **Batasnya:** peringatan tentang langkah yang tidak dapat dimundurkan wajib tampil **sebelum** disimpan |
| **Acceptance criteria** | 1. Kunjungan yang terbentuk bertipe `Inpatient` dan **membawa penjamin yang dipilih pada langkah Pembayaran** — dibuktikan dari permintaan dan jawaban, bukan dari kalimat di layar. 2. `POST /episodes` dikirim dengan `EncounterId` **terisi**; episode terbentuk berstatus `Draft`. 3. Admisi tanpa DPJP ditolak dan pesannya menyebut DPJP wajib. 4. Kebutuhan isolasi yang menyala **wajib** disertai keterangan. 5. Unit layanan yang dapat dipilih hanya yang bertipe rawat inap. 6. Menekan simpan dua kali hanya menghasilkan satu kunjungan dan satu episode. 7. Sebelum disimpan, layar menyatakan bahwa penjamin **tidak dapat diubah** setelah langkah ini |
| **Verification** | E2E jalur penuh sampai episode `Draft` terbentuk; pemeriksaan jaringan atas ketiga permintaan berurutan; pemeriksaan bahwa kunjungan tidak bercara bayar tunai ketika yang dipilih asuransi |
| **Risk/blocker** | Bila `POST /patient-encounters` gagal sementara pasien sudah tersimpan, alur wajib berhenti di langkah ini dengan isian utuh — bagian 5.5. Jangan meneruskan ke `POST /episodes`. Owner: Frontend |
| **DoD** | Ketujuh kriteria lulus; e2e ada dan lulus; laporan melampirkan bukti cara bayar yang tercatat |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-026` — Tempat tidur dicari lalu dipesan — titik tulis 2

| Field | Isi |
| --- | --- |
| **Outcome** | Tempat tidur ditahan atas nama pasien selama masa berlaku pemesanan, sehingga dua petugas tidak merebut tempat tidur yang sama. **Kemampuan ini `RWI-CAP-006` tandai Wajib dan tidak pernah dibangun** |
| **Trace** | `RWI-CAP-006` **Wajib**; `FLOW-RI-MVP-001` langkah 5; 3A.2 langkah 5–6; 3A.4 titik tulis 2; 4.3A |
| **Reuse** | `inpatient-bed-board.jsx`, `placement-failure-list.jsx`, `use-inpatient-bed-board.jsx` yang **sudah ada** dari `FE-RWI-005` dan `FE-RWI-007` |
| **Scope** | Langkah **Pilih Bed** dan **Booking Bed**. `GET /bed-occupancies/available-beds`, `POST /bed-occupancies/reservations`, `PATCH /bed-occupancies/reservations/{id}/cancel` |
| **Dependency** | `FE-RWI-025` |
| **Wewenang UI** | Bentuk penandaan tempat tidur `DEV_DISCRETION`. **Batasnya:** sisa waktu pemesanan wajib terbaca |
| **Acceptance criteria** | 1. Daftar tempat tidur berasal **hanya** dari `available-beds`; layar tidak menyaring ulang sendiri. 2. Tempat tidur yang tidak layak tampil sebagai baris nonaktif beserta alasannya dan **tidak dapat dipilih**. 3. Pemesanan berhasil membuat tempat tidur terbaca `Reserved`, dan **sisa waktunya terbaca**. 4. Tempat tidur ber-`IsReservable` salah ditolak dengan pesan server apa adanya. 5. Membatalkan pemesanan lalu memilih tempat tidur lain berhasil, dan **tidak** meninggalkan dua pemesanan aktif. 6. 409 karena tempat tidur direbut memicu muat ulang daftar, dan isian tidak hilang |
| **Verification** | E2E: memesan, membatalkan, memesan ulang; perebutan tempat tidur oleh sesi kedua; pemeriksaan bahwa hanya ada satu pemesanan aktif per episode |
| **Risk/blocker** | Kriteria 5 adalah yang paling mudah dilanggar saat pengguna menekan tombol mundur. Aturan 3A.5 menuntut pembatalan lebih dulu. Owner: Frontend |
| **DoD** | Keenam kriteria lulus; e2e ada dan lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-027` — Alur ditutup tanpa menempatkan pasien — titik tulis 3

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas meninjau seluruh isian lalu mengunci admisi. Tempat tidur tetap `Reserved` dan episode tetap `Draft`; pasien menjadi `Admitted` hanya ketika kedatangannya dikonfirmasi |
| **Trace** | `RWI-DEC-076`; 3A.2 langkah 7; 3A.4 titik tulis 3; 3A.7 |
| **Reuse** | `verification-step` dari `emergency-registration/` |
| **Scope** | Langkah **Konfirmasi**. `PUT /episodes/{id}` bila ada isian yang berubah |
| **Dependency** | `FE-RWI-026` |
| **Wewenang UI** | Susunan ringkasan `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Ringkasan memuat pasien, penjamin, kelas, unit, DPJP, kebutuhan isolasi, dan tempat tidur yang dipesan. 2. Perubahan isian admisi tersimpan lewat `PUT /episodes/{id}`. 3. Layar **tidak** memanggil `POST /placements`, dan **tidak** menyatakan pasien sudah dirawat. 4. Layar menyatakan langkah berikutnya adalah konfirmasi kedatangan pada papan tempat tidur. 5. Menutup alur setelah titik tulis 1 memunculkan peringatan yang menyebut episode `Draft` sudah terbentuk dan dapat dilanjutkan dari daftar kerja |
| **Verification** | E2E jalur penuh; pemeriksaan jaringan bahwa nol permintaan penempatan terkirim; pemeriksaan status episode tetap `Draft` |
| **Risk/blocker** | Godaan terbesar adalah "sekalian saja ditempatkan". Itu meniadakan pemeriksaan ulang Kelayakan Penempatan saat pasien tiba — alasan `RWI-DEC-076` ada. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-028` — Persetujuan rawat inap dapat dicetak

| Field | Isi |
| --- | --- |
| **Outcome** | Formulir persetujuan umum tercetak berisi data yang sudah ada di sistem, sehingga petugas tidak menulis ulang dengan tangan. Tanda tangan tetap di atas kertas |
| **Trace** | `RWI-DEC-077`; `03-frontend-architecture.md` `FE-INP-18` dan 3A.8; `RWI-DEC-035` isi minimal |
| **Reuse** | Pola halaman cetak kiosk |
| **Scope** | Halaman cetak per episode. Tidak ada endpoint baru; data dibaca dari detail episode dan kunjungan |
| **Dependency** | `FE-RWI-027` |
| **Wewenang UI** | Tata letak formulir `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Formulir memuat identitas pasien, penjamin, unit layanan, kelas, DPJP, nomor episode, dan tanggal. 2. Ketiga isi minimal `RWI-DEC-035` tercetak. 3. Layar **tidak** menyatakan persetujuan tersimpan atau tertanda tangan — sistem tidak menyimpan apa pun. 4. Halaman cetak tidak dapat dibuka tanpa hak akses. 5. Dapat dicapai dari alur admisi **dan** dari detail episode |
| **Verification** | E2E membuka halaman cetak dari kedua jalur; pemeriksaan bahwa peran tanpa hak ditolak |
| **Risk/blocker** | Menyatakan "tersimpan" akan membuat petugas mengira kertasnya tidak perlu disimpan. `RWI-CAP-031` dan `DEC-INP-003` **tetap terbuka**. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; laporan menegaskan nol penyimpanan |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-029` — Kartu pasien tercetak pada jalur pasien baru

| Field | Isi |
| --- | --- |
| **Outcome** | Pasien baru pulang dari meja admisi membawa kartunya, tanpa petugas berpindah ke aplikasi kiosk |
| **Trace** | 3A.2 langkah 9; 3A.3 catatan "Kartu Pasien tidak ada pada jalur pasien lama" |
| **Reuse** | `src/components/view/kiosk/registration/patient-card/print/` yang **sudah ada** |
| **Scope** | Langkah **Kartu Pasien** jalur pasien baru |
| **Dependency** | `FE-RWI-027` |
| **Wewenang UI** | `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Kartu tercetak berisi data pasien yang baru didaftarkan. 2. Langkah ini **tidak** ada pada jalur pasien lama. 3. Melewatinya tidak membatalkan admisi yang sudah terbentuk |
| **Verification** | E2E jalur pasien baru sampai langkah terakhir; e2e jalur pasien lama membuktikan langkah ini tidak muncul |
| **Risk/blocker** | Menyalin komponen cetak alih-alih memakainya ulang akan melahirkan dua bentuk kartu. Owner: Frontend |
| **DoD** | Ketiga kriteria lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-030` — Pasien dikonfirmasi masuk saat benar-benar tiba

| Field | Isi |
| --- | --- |
| **Outcome** | Episode menjadi `Admitted` dan tempat tidur menjadi `Occupied` pada saat pasien benar-benar sampai di kamar, dengan Kelayakan Penempatan diperiksa **ulang** di detik itu |
| **Trace** | `RWI-DEC-076`; `FLOW-RI-MVP-001` langkah 6; `FE-INP-02`; 3.2; 4.3A |
| **Reuse** | Papan tempat tidur `FE-RWI-005`; penanganan penolakan `FE-RWI-007` |
| **Scope** | Aksi konfirmasi masuk pada papan tempat tidur. `POST /bed-occupancies/placements` |
| **Dependency** | `FE-RWI-026` |
| **Wewenang UI** | Penempatan tombol `DEV_DISCRETION`. **Batasnya:** konfirmasi wajib menyebut nama pasien dan tempat tidur |
| **Acceptance criteria** | 1. Aksi hanya dirender bagi **petugas admisi** dan **supervisor** — bagian 3.2. Peran lain tidak melihatnya. 2. Tempat tidur `Reserved` menampilkan episode yang memegangnya beserta sisa waktunya pada layar yang berhak. 3. Penolakan 422 karena Kelayakan Penempatan berubah ditampilkan apa adanya dan terbaca sebagai **keadaan yang berubah**, bukan kesalahan petugas. 4. Papan dimuat ulang tepat sebelum dialog konfirmasi tampil. 5. Setelah berhasil, episode terbaca `Admitted` dan pasien muncul pada census |
| **Verification** | E2E per peran; e2e penolakan dengan tempat tidur yang sengaja dibuat tidak layak setelah dipesan |
| **Risk/blocker** | Kontrak hak akses **tidak** memberi `InpatientBedOccupancy : Create` kepada perawat maupun kepala ruangan. Merender tombol bagi mereka menghasilkan tombol yang pasti ditolak server. Butir terbuka `RWI-OQ-045`. Owner: Frontend bersama Product/Domain |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-031` — Admisi yang keliru dapat dibatalkan

| Field | Isi |
| --- | --- |
| **Outcome** | Admisi yang salah — penjamin keliru, DPJP keliru, atau pasien batal dirawat — dapat dibatalkan beserta pemesanan dan penempatannya dalam satu tindakan. Tanpa ini, satu-satunya jalan keluar dari kesalahan adalah membiarkannya |
| **Trace** | `FE-INP-17`; matriks peran bagian 3; 3A.5 |
| **Reuse** | `confirm-modal.jsx`; detail episode |
| **Scope** | Aksi pembatalan pada detail episode dan daftar kerja. `PATCH /episodes/{id}/cancel` |
| **Dependency** | `FE-RWI-020` |
| **Wewenang UI** | Penempatan `DEV_DISCRETION`. **Batasnya:** konfirmasi wajib menyebut bahwa pemesanan dan penempatan ikut dilepas |
| **Acceptance criteria** | 1. Pembatalan `Draft` tersedia bagi petugas admisi dan supervisor. 2. Pembatalan `Admitted` tersedia bagi kepala ruangan dan supervisor, **tidak** bagi petugas admisi. 3. Pembatalan wajib beralasan. 4. Konfirmasi menyebut bahwa tempat tidur akan dilepas. 5. Setelah dibatalkan, episode terbaca `Cancelled` dan tempat tidurnya terbaca bebas pada papan |
| **Verification** | E2E per peran untuk kedua status; pemeriksaan papan sesudah pembatalan |
| **Risk/blocker** | Kewenangannya **berbeda** menurut status episode — pola yang sama dengan tombol isolasi dan sama mudahnya salah. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-032` — Admisi yang ditinggal dapat dilanjutkan

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas yang terputus di tengah alur — browser tertutup, giliran kerja berganti, pasien pergi sebentar — dapat melanjutkan admisi yang sama, bukan memulai dari nol dan meninggalkan episode yatim |
| **Trace** | `RWI-DEC-076`; 3A.6; `IA-INP-02` |
| **Reuse** | Daftar kerja `FE-RWI-020`; kerangka alur `FE-RWI-022` |
| **Scope** | Jalur dari daftar kerja menuju alur admisi pada langkah yang tepat. `GET /episodes/{id}` |
| **Dependency** | `FE-RWI-020`, `FE-RWI-026` |
| **Wewenang UI** | Bentuk tautan `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Episode `Draft` tanpa pemesanan dilanjutkan ke langkah **Pilih Bed**. 2. Episode `Draft` dengan pemesanan aktif dilanjutkan ke langkah **Konfirmasi**, dan sisa waktu pemesanannya terbaca. 3. Episode `Draft` yang pemesanannya sudah gugur dilanjutkan ke langkah **Pilih Bed** disertai keterangan bahwa pemesanan sebelumnya gugur. 4. Langkah yang sudah lewat **tidak** meminta pengguna mengetik ulang data yang sudah tersimpan. 5. Episode selain `Draft` **tidak** menawarkan pelanjutan |
| **Verification** | E2E ketiga keadaan `Draft`; pemeriksaan bahwa data pasien, penjamin, dan DPJP terbaca dari server, bukan kosong |
| **Risk/blocker** | Kriteria 3 menuntut layar membedakan pemesanan gugur dari tidak pernah ada. Sumbernya jawaban server, bukan hitungan waktu di sisi layar. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-033` — Tidak ada lagi layar yang tidak dapat dicapai

| Field | Isi |
| --- | --- |
| **Outcome** | Setiap layar Rawat Inap punya jalan masuk yang jelas. Hari ini layar sesi koreksi yang sudah jadi tidak dapat dicapai siapa pun, dan itu terjadi tanpa disadari sampai revision ini |
| **Trace** | `IA-INP-01` s.d. `IA-INP-05`; bagian 11A |
| **Reuse** | Menu yang sudah ada; daftar kerja `FE-RWI-020` |
| **Scope** | Menu sidebar, tautan antar layar, `GET /census/filters/metadata` yang masih menganggur |
| **Dependency** | `FE-RWI-020` s.d. `FE-RWI-032` |
| **Wewenang UI** | Nama dan urutan menu `DEV_DISCRETION`. **Batasnya:** kelima aturan `IA-INP` |
| **Acceptance criteria** | 1. Setiap layar bagian 2 dapat dicapai dari beranda dalam paling banyak tiga klik. 2. Layar sesi koreksi dapat dicapai dari daftar kerja tersaring `Closed`. 3. Menu tingkat dua paling banyak sembilan butir; layar per-episode tidak mendapat butir menu. 4. Tidak ada operasi pada api contract yang tidak dimiliki satu layar, kecuali yang dinyatakan sengaja tidak dipakai pada bagian 11A. 5. Penyaring census memakai `filters/metadata`, bukan daftar yang ditanam di kode |
| **Verification** | Penelusuran manual seluruh 19 layar dari beranda, dilampirkan sebagai daftar jalur; e2e menuju sesi koreksi lewat daftar kerja |
| **Risk/blocker** | Kriteria 4 menuntut pemeriksaan endpoint satu per satu terhadap bagian 11A, bukan perasaan sudah lengkap. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; laporan memuat tabel jalur untuk seluruh 19 layar |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-034` — Layar admisi lama dibongkar, jalur gandanya hilang

| Field | Isi |
| --- | --- |
| **Outcome** | Hanya ada **satu** jalan menuju admisi. Membiarkan formulir lama berdampingan dengan alur baru menghasilkan dua jalur menuju hal yang sama, dan salah satunya pasti lupa diperbarui |
| **Trace** | `RWI-DEC-079`; 2C "satu kemampuan, satu tempat" |
| **Reuse** | Bagian yang masih terpakai dari `use-inpatient-admission` dipindahkan, bukan disalin |
| **Scope** | `inpatient-admission-view.jsx`, `use-inpatient-admission.jsx`, `inpatient-admission-utils.jsx`, `inpatient-admission-constants.jsx`, dan test yang menyertainya |
| **Dependency** | `FE-RWI-027` |
| **Wewenang UI** | Tidak ada |
| **Acceptance criteria** | 1. Tidak ada lagi route, komponen, atau menu yang membuka formulir admisi tunggal. 2. Tidak ada berkas yatim yang tidak diacu siapa pun. 3. Test lama yang menguji formulir tunggal dihapus atau diarahkan ke alur baru — **tidak** dibiarkan dilewati. 4. `lint`, `test:unit`, dan `build` lulus |
| **Verification** | Pencarian menyeluruh atas nama berkas lama; keluaran ketiga perintah dilampirkan apa adanya |
| **Risk/blocker** | Menandai test lama sebagai dilewati alih-alih menghapusnya menyembunyikan penurunan cakupan. Owner: Frontend |
| **DoD** | Keempat kriteria lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-035` — Alur bisnis utama terbukti berjalan ujung ke ujung

| Field | Isi |
| --- | --- |
| **Outcome** | `FLOW-RI-MVP-001` terbukti dapat dijalankan dari pasien datang sampai episode ditutup, dan setiap layar terbukti hanya dijangkau peran yang berhak. Menggantikan cakupan `FE-RWI-019` yang disusun ketika layarnya masih lima belas |
| **Trace** | `03-frontend-architecture.md` bagian 10; `RWI-DEC-051`; `GUARD-INP-01` s.d. `GUARD-INP-04` |
| **Reuse** | `tests/e2e/route-smoke.spec.mjs` dan seluruh e2e yang sudah ada — menambah kasus, bukan membuat kerangka baru |
| **Scope** | Rangkaian e2e; penyelesaian empat kriteria yang tertahan pada `FE-RWI-003`, `004`, `012`, dan `013` |
| **Dependency** | Seluruh task `FE-RWI-020` s.d. `FE-RWI-034` |
| **Wewenang UI** | Tidak ada |
| **Acceptance criteria** | 1. Satu e2e menjalankan `FLOW-RI-MVP-001` jalur pasien baru dari langkah 1 sampai episode `Closed`. 2. Satu e2e menjalankan jalur pasien lama sampai tempat tidur `Reserved`. 3. Kunjungan yang terbentuk terbukti membawa penjamin yang dipilih, bukan tunai bawaan. 4. Alur yang ditinggal setelah titik tulis 1 terbukti dapat ditemukan kembali dan dilanjutkan. 5. Setiap layar dari kesembilan belas terbukti tertutup bagi peran yang tidak berhak. 6. Keempat aturan penjaga `GUARD-INP-01` s.d. `GUARD-INP-04` terbukti terlihat di layar. 7. Empat kriteria yang tertahan sejak revision 2 diselesaikan atau dinyatakan tertahan beserta alasannya yang masih berlaku |
| **Verification** | Jalankan rangkaian e2e penuh; lampirkan keluarannya apa adanya; tidak ada kasus yang ditandai dilewati |
| **Risk/blocker** | Kriteria 1 adalah e2e terpanjang pada modul ini dan menyentuh tiga bounded context. Menyiapkan data masternya lebih dulu — unit layanan bertipe rawat inap, kamar, tempat tidur, kelas, penjamin — adalah prasyarat, bukan bagian dari test. Owner: Frontend bersama penanggung jawab data master |
| **DoD** | Ketujuh kriteria lulus; keluaran rangkaian e2e terlampir |
| **Status** | ⬜ belum dikerjakan |

---

## 6. Gerbang yang masih terbuka

| Gerbang | Keadaannya | Menahan |
| --- | --- | --- |
| Approval blueprint | ✅ **Tertutup.** `RWI-DEC-075` s.d. `RWI-DEC-079` disetujui Muhammad Hamzah pada 27 Agustus 2026 dan metadata roadmap revision `3` sudah `APPROVED`. Label artefak `draft` tetap dipertahankan mengikuti konvensi blueprint | — |
| Endpoint backend tersedia | ✅ **Tertutup.** Ke-49 endpoint terbukti berjalan 26 Agustus 2026 | — |
| Kesiapan data master | `RWI-DEC-063`. Unit layanan bertipe rawat inap, kamar, tempat tidur, kelas, dan penjamin | `FE-RWI-026` ke atas tidak dapat diuji dengan data nyata |
| `IsQueueRequired` unit rawat inap | Harus bernilai salah agar admisi tidak membuat antrean semu — 3A.7 | `FE-RWI-025` |
| `RWI-OQ-045` hak akses konfirmasi masuk | Kepala ruangan belum punya `InpatientBedOccupancy : Create` | Tidak menahan; `FE-RWI-030` berjalan dengan peran yang kontraknya izinkan |
| `RWI-OQ-046` jalur admisi tanpa `EncounterId` | Masih terbuka di backend | Tidak menahan; tidak ada layar yang menempuhnya |
| Security/privacy owner | Belum ditunjuk | Tidak menahan; aturan privasi tetap berlaku dan tetap diuji |

---

## 7. Yang sengaja tidak ada di roadmap ini

| Yang tidak dikerjakan | Alasan |
| --- | --- |
| Layar pengkajian, catatan dokter, CPPT, dan resep | Slice di luar scope MVP — `DEC-INP-001` |
| Layar serah terima IGD | Di luar scope — `DEC-INP-002` |
| **Penyimpanan** persetujuan umum rawat inap | `RWI-DEC-077` memilih cetak tanpa menyimpan. `RWI-CAP-031` dan `DEC-INP-003` tetap terbuka |
| Daftar pantau kepatuhan pengkajian dan CPPT | Bergantung pada slice yang di luar scope — `DEC-INP-001` |
| Perubahan hak akses agar perawat dapat mengonfirmasi masuk | Wewenang kontrak, bukan wewenang roadmap frontend — `RWI-OQ-045` |
| Penutupan jalur admisi tanpa `EncounterId` di backend | Wewenang Backend/API — `RWI-OQ-046` |
| Menyalin ruang kerja antrean dokter | Pasien rawat inap tidak punya antrean |
| Menyaring ulang tempat tidur di sisi layar | Aturan Kelayakan Penempatan hanya boleh ada **satu**, dan tempatnya di server |
