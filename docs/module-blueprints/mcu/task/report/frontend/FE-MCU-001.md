# Laporan Perubahan Frontend — `FE-MCU-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-001` |
| Judul | Aturan bersama tanpa tampilan |
| Slice | Gelombang *Kapan saja* (tanpa pasangan backend) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-001`* |
| Trace | `MCU-DEC-031`; `MCU-FE-003`, `004`, `005`; `VAL-MCU-24`, `38`, `39`, `41`, `43`; `03-frontend-architecture.md` §3; `05-screen-brief.md` tabel header detail order |
| Contract version | `MCU-STATE-v1`, `MCU-PERM-v1` — `approved` 2026-10-21; nilai enum dan label diselaraskan dengan source backend `McuOrderStatus`/`McuLabels` |
| Wewenang UI | `NOT APPLICABLE` — tanpa tampilan. Label aksi mengikuti brief (`MCU-FE-002`); warna lencana `DEV_DISCRETION` dari varian `StatusBadge` yang ada (`MCU-FE-005`) |
| Dependency | — (mandiri) |
| Klasifikasi | `LIGHT` — skor 3: satu repository (0), berkas diperiksa 9–20 (1), berkas diubah ≤ 3 (0), logika sedang (1), tanpa kontrak API (0), tanpa database (0), keamanan berkaitan — penyembunyian tombol berizin (1), UI kecil (0) |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: `src/utils/health-services/medical-checkup-management/` (baru), `tests/unit/mcu-shared-rules.test.mjs` (baru). Backend: laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `d0888bf6f` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `a3b63b50` (branch `yoga`) |
| Tanggal | 2026-10-23 |
| Status | Selesai — kriteria terbukti uji unit; lint dan build lulus |

---

## 1. Keadaan yang ditemukan di awal

- Frontend belum punya satu baris pun kode MCU (`src/**/medical-checkup-management` tidak ada).
- Backend sudah lengkap (`BE-MCU-000`..`035` ✅) dan mengirim `orderStatus` sebagai **angka** enum (1–9; tidak ada
  `JsonStringEnumConverter` di `Program.cs`) beserta `orderStatusLabel`.
- Pola desimal yang aman sudah ada di Lab: `src/lib/hooks/health-services/laboratory-management/lab-decimal-input-rules.js`
  (`DECIMAL_FIELD_PROPS`, `parseDecimal`) — dibuat karena `BaseTextField` `type: "number"` membuang pemisah desimal.
- Keputusan izin di frontend lewat `selectHasPermission(resource, action)` (`permission-slice.jsx`).
- Tabel aksi `03-frontend-architecture.md` §3 dan brief layar berbeda tipis: brief (lebih rinci dan mengatur
  penempatan) menambah **Buka Ulang** pada *Terverifikasi* dan **Ganti Dokter MCU** — sesuai state machine backend.

---

## 2. Proses bisnis dari sisi pengguna

Tanpa layar. Aturan ini dipakai layar MCU berikutnya agar setiap pengguna hanya melihat tombol yang memang boleh ia
tekan, dengan bahasa dan warna status yang sama di seluruh MCU:

| Status | Aksi utama | Aksi lain | Syarat tambahan |
| --- | --- | --- | --- |
| Terjadwal | Check-in | Jadwal Ulang, Batal | Check-in hanya pada tanggal pemeriksaan |
| Tidak Hadir | Jadwal Ulang | — | |
| Hadir | Mulai Pemeriksaan | Batal Penuh, Ganti Dokter MCU | |
| Dalam Pemeriksaan | Buka Pemeriksaan | Batal Penuh, Ganti Dokter MCU | Batal Penuh disembunyikan bila diketahui sudah ada komponen dimulai |
| Menunggu Resume | Buka Resume | Ganti Dokter MCU | |
| Disetujui Dokter | Verifikasi | Kembalikan ke Dokter, Buka Ulang, Ganti Dokter MCU | Verifikasi disembunyikan bagi dokter pemberi approve; Buka Ulang hanya dokter yang ditetapkan |
| Terverifikasi, Diserahkan | Catat Penyerahan | Buka Ulang | Buka Ulang hanya dokter yang ditetapkan |
| Dibatalkan | — | — | |

Setiap aksi juga menuntut izin `MCU-PERM-v1`-nya. Izin dokter (tambah/batal komponen, simpan/approve/buka ulang resume)
menuntut **izin dan** akun = dokter MCU yang ditetapkan; izin saja tidak cukup.

**Contoh.** Dewi (admisi, izin `McuOrder : CheckIn/Update/Cancel`) membuka order Budi *Terjadwal* hari ini → Check-in,
Jadwal Ulang, Batal. Order Budi kemudian *Disetujui Dokter*: dr. Rina (dokter yang ditetapkan dan pemberi approve) hanya
melihat Buka Ulang; dr. Bima (berizin `Reopen` tetapi bukan dokter yang ditetapkan) tidak melihat apa pun; Dewi melihat
Verifikasi dan Kembalikan ke Dokter.

**Server tetap penentu akhir.** Aturan ini hanya menyembunyikan tombol yang pasti ditolak; pesan `400`/`403`/`409`
server tetap ditampilkan apa adanya oleh layar.

Format: NIK tersamar `3201230101900001` → `3201********0001` (sama dengan backend); isian angka `6,4` dan `6.4` → `6.4`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/frontend-roadmap.md` (`FE-MCU-001`, batas dokumen), `03-frontend-architecture.md` (§2, §3), `05-screen-brief.md`
(header detail order, modal), `00-interview-decisions.md` (`MCU-DEC-031`, `MCU-FE-003`..`005`), `contracts/permission-audit-matrix.md`,
backend `MedicalCheckupEnums.cs`, `McuLabels.cs` (`OrderStatus`, `MaskNik`), `McuExaminationService.TryNormalizeNumber`,
`Program.cs` (serialisasi JSON); frontend `AGENTS.md`, `rules/frontend/frontend-architecture.md`, `test-policy.md`,
`lab-decimal-input-rules.js` + ujinya, `lab-order-constants.jsx` (pola label/lencana), `status-badge.jsx` (varian),
`permission-slice.jsx` (`selectHasPermission`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/medical-checkup-management/mcu-order-rules.js` (baru) | Sembilan status (angka enum, urutan, label, varian lencana), `normalizeMcuOrderStatus`, kunci izin `MCU_PERMISSION`, izin dokter `MCU_ASSIGNED_DOCTOR_PERMISSIONS` + `canShowMcuPermission`, `MCU_ORDER_ACTION`, `resolveMcuOrderActions(order, context)` |
| `src/utils/health-services/medical-checkup-management/mcu-format-rules.js` (baru) | `maskMcuNik`; `MCU_DECIMAL_FIELD_PROPS`, `normalizeMcuDecimalInput`, `parseMcuDecimal` (memakai ulang pola Lab), `toMcuDecimalValue` (teks yang dikirim, aturan sama dengan backend), `MCU_DECIMAL_ERROR_MESSAGE` |
| `tests/unit/mcu-shared-rules.test.mjs` (baru) | 12 uji `node:test` |

### 3.3 Kepatuhan arsitektur frontend

- Fungsi murni di `src/utils/<domain>` sesuai `frontend-architecture.md`; tidak mengimpor view, Redux, maupun Axios.
  Keputusan izin **diterima** sebagai fungsi `hasPermission(resource, action)` dari pemanggil (layar akan meneruskan
  `selectHasPermission`), sehingga aturan tetap murni dan teruji tanpa store.
- Memakai ulang `lab-decimal-input-rules.js` — tidak membuat normalizer desimal kedua. Impor `utils` → `@/lib/hooks` punya
  preseden (`receivable-utils.jsx`, `billing-invoice-review-utils.js`, `petty-cash-category-utils.jsx`); berkas Lab itu
  sendiri fungsi murni.
- Label dan lencana status ditempatkan di modul `utils` sesuai *Outcome* roadmap (bukan `src/lib/constants`), sebagai
  **satu** peta warna status MCU (`MCU-FE-005`). Varian hanya dari `StatusBadge` yang ada.
- Tidak ada komponen, hook, slice, atau dependency baru.

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` — tanpa tampilan. Untuk status tidak dikenal: label cadangan dari respons, lencana `info`, tanpa aksi.

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak memanggil API.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/mcu-shared-rules.test.mjs` | 12 lulus | `PASS` | `# pass 12`, `# fail 0` |
| Kriteria: *Terjadwal* hari ini → Check-in, Jadwal Ulang, Batal | Sesuai, berikut labelnya | `PASS` | Uji `kriteria: order Terjadwal…` |
| Kriteria: *Disetujui Dokter* → Buka Ulang hanya dokter yang ditetapkan | Dokter ditetapkan: Buka Ulang; dokter lain berizin: kosong; petugas: Verifikasi + Kembalikan | `PASS` | Uji `kriteria: Disetujui Dokter…` |
| Kriteria: masking dan parsing | `3201********0001`; `6,4`/`6.4` → `6.4`; `1.234,5` dan `enam` ditolak; `type` tidak dipakai | `PASS` | Dua uji `kriteria: …` |
| Penjaga lain | Bukan hari H tanpa Check-in (termasuk `Date` lokal); Verifikasi tersembunyi bagi pemberi approve; Batal Penuh tersembunyi bila komponen dimulai; tanpa izin/status asing → kosong; seluruh 9 status sesuai brief | `PASS` | Uji lainnya |
| Uji mutasi | Penjaga dokter dan penjaga hari H dilepas → 3 uji **gagal**; dipulihkan identik (`cmp`), kembali 12/12 | `PASS` | Keluaran `node --test` |
| `npx eslint` (berkas baru) | Tanpa temuan | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Exit 0, tanpa error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.601 uji: 3.555 lulus, **46 gagal** — seluruhnya di berkas uji modul lain (Billing, CRM, IGD, Rawat Inap, Hemodialisa, Bank Darah, dll.) | `EXISTING / ENVIRONMENT ISSUE` | Task ini hanya menambah berkas baru (`git status`: `??` saja) dan tak satu pun uji lain mengimpor modul MCU; ke-12 uji MCU lulus. Baseline tercatat 2026-10-13: 29 gagal — bertambah oleh perubahan repo sejak itu, bukan oleh task ini |
| `npm run build` | Exit 0: *Compiled successfully in 67s*, 517/517 halaman statis, `prepare-standalone` siap; tidak ada berkas tersentuh di luar dua berkas baru | `PASS` | Keluaran build |

`MANUAL TEST: NOT APPLICABLE` — tanpa kontrol interaktif; dipakai layar `FE-MCU-007`..`014`.
`AUTOMATED TEST: npm run test:unit — FAIL (46 kegagalan existing di luar MCU; 12/12 uji MCU PASS)`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Order *Terjadwal* hari ini menampilkan Check-in, Jadwal ulang, Batal | Terpenuhi | §6 |
| Order *Disetujui Dokter* menampilkan Buka ulang **hanya** bagi dokter yang ditetapkan | Terpenuhi | §6 |
| Masking (`3201230101900001` → `3201********0001`) dan parsing (`6,4`/`6.4` → `6.4`) sesuai contoh | Terpenuhi | §6 |
| DoD: uji unit lulus; lint lulus | Terpenuhi untuk uji MCU dan lint; suite penuh membawa 46 kegagalan existing | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tabel aksi `03-frontend-architecture.md` §3 lebih sempit dari brief; yang dipakai **brief** (mengatur penempatan, sesuai state machine): Buka Ulang pada *Terverifikasi* dan Ganti Dokter MCU ikut tampil |
| Masalah yang diketahui | Batal Penuh hanya disembunyikan bila pemanggil tahu sudah ada komponen dimulai (`anyComponentStarted`); respons backend belum membawa penanda itu, jadi layar biasanya tetap menampilkannya dan server menolak `409` `VAL-MCU-41`. *Hari ini* memakai tanggal lokal peramban |
| Dependency backend | Tidak ada; backend `BE-MCU-000`..`035` sudah selesai |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git (frontend) | `?? src/utils/health-services/medical-checkup-management/`, `?? tests/unit/mcu-shared-rules.test.mjs` |
| Langkah berikutnya | `FE-MCU-002` (lapisan data master), `FE-MCU-006`, atau `FE-MCU-010` — ketiganya siap |
