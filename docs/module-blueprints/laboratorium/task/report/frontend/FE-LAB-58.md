# Laporan Perubahan Frontend — `FE-LAB-58`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-58` |
| Judul | Detail pesanan: tanggal rencana, tombol Jadwal Ulang, dan riwayat |
| Slice | `S20-1` penjadwalan ulang pemeriksaan — gelombang `MVP-16d` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) *Gelombang `MVP-16`* — `FE-LAB-58` |
| Trace | `FR-21.9`, `FR-21.12`, `FR-21.13`; `LAB-DEC-264`, `267`, `268`, `273`; `03-frontend-architecture.md` amandemen 2026-11-25 *Kebutuhan layar — detail pesanan* |
| Contract version | `LAB-API-v1` **`r50`** 45.2, 45.3 — **`approved` 2026-11-25** (Yoga Aji Pratama) |
| Wewenang UI | `DEV_DISCRETION`: letak tombol, bentuk tabel riwayat, bentuk penanda *Terlewat*, label. **Bukan** `DEV_DISCRETION` (dipatuhi): tombol lewat dialog tanggal + alasan yang sama dengan daftar; tersembunyi tanpa izin, **nonaktif beralasan** bila `canReschedule = false`; riwayat bagi pemegang `LabOrder : Read`; `version` selalu dikirim |
| Dependency | `FE-LAB-57` ✅ (dialog, service, aturan); backend `BE-LAB-101`..`103` ✅ selesai pada kode, belum di API dev bersama |
| Klasifikasi | `MEDIUM` — repository 0, berkas diperiksa 1 (±12), berkas diubah 1 (5), logika 1, kontrak API 1 (endpoint yang sudah disiapkan `FE-LAB-57`), database 0, keamanan 1 (izin), UI 1 (satu layar) = **6** |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — detail pesanan Lab, aturan jadwal ulang, uji; laporan, roadmap, dan traceability modul. Tanpa commit |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `11b3aaece` (branch `YogaV2`) + working tree `FE-LAB-57` |
| Commit backend yang dijadikan rujukan | `cce1dbf5` + working tree `BE-LAB-101`..`103` |
| Tanggal | 2026-11-26 |
| Status | ✅ **Selesai pada kode** — lint bersih (satu peringatan lama di luar perubahan), uji unit 40/40, **e2e 39/39** (7 baru + 32 terkait) terhadap API tiruan `r50`. Verifikasi peramban dengan akun Kepala Instalasi terhadap backend nyata **belum** — menunggu langkah rilis `MVP-16e` no. 1–3 |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti |
| --- | --- |
| Detail pesanan menampilkan delapan ruas ringkasan; tanpa tanggal rencana, penanda *Terlewat*, maupun hitungan jadwal ulang | `lab-order-detail-view.jsx` `summaryRows` |
| Tidak ada tombol Jadwal Ulang dan bagian riwayat | View dan `use-lab-order-detail.jsx` |
| Detail sudah membawa `version`, `plannedDate`, dan (sejak `BE-LAB-103`) `rescheduleCount`, `isOverdue`, `canReschedule`, alasan | Kontrak `r50` 45.3 |
| Dialog bersama, `rescheduleLabOrder`, dan `getLabOrderReschedules` tersedia dari `FE-LAB-57` | `lab-orders/lab-reschedule-dialog.jsx`, `lab-order.service.js` |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** Kepala Instalasi Laboratorium (pemegang `LabOrder : Reschedule` sesudah langkah rilis 3; **tidak** membuka
Daftar Pasien Lab), Analis, dan pemegang `LabOrder : Read` saja (mis. dokter).

1. dr. Bima membuka detail Gula Darah Puasa. Panel *Informasi Pesanan* kini memuat **Tanggal Rencana** `23-11-2026`,
   **Jadwal Ulang** *Dijadwalkan ulang 2 kali*, dan lencana **Terlewat 3 hari** di samping status.
2. Ia menekan **Tampilkan** pada *Riwayat Jadwal Ulang*. Riwayat dimuat saat itu juga (bukan bersama detail): tanggal
   lama → baru, alasan, pelaku, dan waktu **WIB** — terbaru di atas.
3. Ia menekan **Jadwal Ulang** di kepala halaman. Dialog yang sama dengan Daftar Pasien Lab terbuka; ia memilih tanggal
   baru dan menulis alasan, lalu **Simpan Jadwal**.
4. Dialog tertutup, notifikasi *Pesanan LAB-… dijadwalkan ulang ke …*, detail dimuat ulang (*Dijadwalkan ulang 3 kali*,
   tanggal baru), dan riwayat yang sedang terbuka ikut dimuat ulang dengan baris baru di atas.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Pemegang `LabOrder : Read` saja | Tombol tidak tampil; riwayat tetap dapat dibuka |
| Pesanan tidak layak (sampel diambil, bagian MCU, CITO, …) | Tombol **nonaktif** dengan alasan backend sebagai keterangan, **dan** kotak *Tidak dapat dijadwalkan ulang* berisi alasan yang sama di panel |
| `409` versi (petugas lain baru menyimpan) | Pesan server apa adanya di dialog, dialog tetap terbuka, detail dimuat ulang — percobaan berikutnya membawa versi baru |
| `422` tanggal | Pesan di bawah isian tanggal |
| Riwayat kosong | *Belum pernah dijadwalkan ulang* |
| Riwayat gagal dimuat | Pesan galat dan tombol **Coba lagi** |
| Tautan detail tidak sah / detail belum dimuat | Tombol tidak dirender (tidak ada pesanan untuk dinilai) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Roadmap `FE-LAB-58`; amandemen frontend 2026-11-25; `LAB-API-v1` 45.2/45.3; `lab-order-detail-view.jsx`,
`use-lab-order-detail.jsx`, `lab-order-slice.jsx` (`fetchLabOrderDetail`, `fetchLabExaminations`),
`lab-examination.service.js`, `private-route-token-utils.js`, `BaseButton`, `ToastStack`; berkas `FE-LAB-57`
(dialog, aturan, service); `tests/e2e/mcu-coverage-label.spec.mjs` (pola membuka detail).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/laboratory-management/lab-reschedule-rules.js` | `RESCHEDULE_HISTORY_EMPTY`, `readRescheduleCount`, `formatRescheduleCount`, `formatRescheduleTime` (zona `Asia/Jakarta` eksplisit; nilai tanpa zona dibaca UTC), `normalizeRescheduleHistory` |
| `src/lib/hooks/health-services/laboratory-management/use-lab-order-detail.jsx` | Izin `LabOrder : Reschedule`; `rescheduleAction` dari `canReschedule` backend (tidak dirender tanpa detail); dialog (buka, tutup, simpan dengan `version` detail, sukses/`409`/`422`); riwayat sebagai satu objek bertanda id pesanan — dimuat saat dibuka, dimuat ulang sesudah simpan atau `409`, tidak terbawa ke pesanan lain |
| `src/components/view/health-services/laboratory-management/lab-orders/detail/lab-order-detail-view.jsx` | Ruas ringkasan *Tanggal Rencana* dan *Jadwal Ulang*; lencana *Terlewat*; tombol Jadwal Ulang; kotak alasan tidak layak; bagian *Riwayat Jadwal Ulang* (tampilkan/sembunyikan, memuat, kosong, gagal + coba lagi, tabel); dialog bersama |
| `tests/unit/lab-reschedule-rules.test.mjs` | Tiga uji baru (hitungan, waktu WIB lintas tengah malam, normalisasi riwayat) — total 40 |
| `tests/e2e/lab-reschedule-order-detail.spec.mjs` | **Baru** — 7 skenario API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Alur `view → hook → service → InstanceAxios`: hook memanggil `rescheduleLabOrder` dan `getLabOrderReschedules`
langsung (keadaan lokal satu layar — sah menurut `frontend-architecture.md`; tidak perlu dibagi lintas halaman). Format
dan normalisasi di berkas aturan murni. Dialog dipakai ulang dari `FE-LAB-57` tanpa perubahan; komponen dasar
`BaseButton`, `DataTable`, `InformationAlert`, `StatusBadge`, `ToastStack`. Nol pola baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Detail: pola yang ada; riwayat: *Memuat riwayat jadwal ulang...*; dialog: Simpan nonaktif selama detail dimuat ulang |
| Kosong | *Belum pernah dijadwalkan ulang* (ringkasan dan tabel riwayat) |
| Gagal | Riwayat: pesan server + **Coba lagi**; simpan: pesan server di dialog (`409` apa adanya, `422` di bawah isian tanggal); notifikasi galat lain dari pola detail yang ada |
| Tanpa hak akses | Tombol tidak tampil; `AccessDeniedGate` yang ada untuk detail; `403` saat menyimpan tampil sebagai pesan server |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/{id}` | Ringkasan, versi, kelayakan (sudah dipakai layar) | `LabOrder : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/{id}/reschedules` | Riwayat saat bagiannya dibuka | `LabOrder : Read` |
| `PUT` | `/v1/health-services/laboratory-management/lab-orders/{id}/reschedule` | Menyimpan jadwal ulang | `LabOrder : Reschedule` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas berkas yang diubah | 0 error; satu peringatan `set-state-in-effect` baris 126 (`setResolvedId`, kode lama). Peringatan serupa pada kode baru dihilangkan dengan riwayat bertanda id pesanan | `PASS` / `EXISTING / ENVIRONMENT ISSUE` | Keluaran eslint |
| Uji unit `lab-reschedule-rules` | **40/40** — termasuk `18:30 UTC` → `03-12-2026 01.30 WIB` | `PASS` | Keluaran uji |
| Uji unit Lab terkait (213) | 212 lulus; 1 gagal `FE-LAB-49 AC-282` — membaca `menu-items.jsx` yang tidak berubah, sudah gagal sebelum `FE-LAB-57` | `PASS` / `EXISTING / ENVIRONMENT ISSUE` | Keluaran uji |
| `npm run build` (dua kali; server lokal dimatikan lebih dulu) | `Compiled successfully` | `PASS` | Keluaran build |
| E2E baru `lab-reschedule-order-detail.spec.mjs` | **7/7**: ringkasan *Tanggal Rencana*, *Terlewat 3 hari*, *Dijadwalkan ulang 2 kali*; pemegang `Read` tanpa tombol, riwayat dimuat **hanya saat dibuka** (0 → 1 permintaan, tidak dimuat ulang saat ditutup-buka), urutan terbaru di atas, waktu WIB; riwayat kosong; riwayat gagal + Coba lagi; tidak layak → nonaktif beralasan + kotak alasan; simpan (muatan `{plannedDate, reason, version: 4}`, notifikasi, detail dimuat ulang, hitungan 3, tanggal baru, riwayat dimuat ulang); `409` versi (pesan apa adanya, dialog terbuka, detail dimuat ulang, percobaan kedua `version: 5`) | `PASS` | `pw-fe58.log` |
| E2E akhir pada build terakhir — 9 spec (detail, daftar `FE-LAB-57`, MCU, enam spec daftar pantau) | **39/39** | `PASS` | `pw-fe58-final.log` |
| Pemeriksaan visual (tangkapan layar 1500 px) | Ringkasan, lencana, tombol, tabel riwayat ber-WIB, kotak alasan tidak layak rapi | `PASS` | `fe58-shots/*.png` |
| `test-results/`, `playwright-report/` ter-track | Tidak berubah | `PASS` | `git status` |

Uji manual: **`NOT FEASIBLE`** terhadap backend nyata — build menunjuk API dev bersama yang belum memuat kode
`BE-LAB-101`..`103`, devYoga belum menerapkan migration `BE-LAB-101`, dan Kepala Instalasi belum diberi izin (langkah
rilis 3). Setiap kontrol yang diubah — tombol Jadwal Ulang, Tampilkan/Sembunyikan, Coba lagi, dialog — diperiksa di
Chromium sungguhan terhadap API tiruan `r50`.

**Tidak dijalankan:** peramban dengan akun Kepala Instalasi (dr. Bima) terhadap backend nyata — menunggu `MVP-16e`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-349` sisi layar — hitungan jadwal ulang, tanpa batas | Terpenuhi (API tiruan) | E2E ringkasan dan simpan (2 → 3) |
| `AC-352` / `T-S20-15` sisi layar — riwayat terbaca pemegang `Read`, dimuat saat dibuka | Terpenuhi (API tiruan) | E2E riwayat |
| `AC-353` / `T-S20-18` sisi layar — tombol tampil / nonaktif beralasan / tersembunyi | Terpenuhi (API tiruan) | E2E izin dan tidak layak |
| Riwayat kosong *Belum pernah dijadwalkan ulang* | Terpenuhi | E2E riwayat kosong |
| Muat ulang detail sesudah `409` versi | Terpenuhi | E2E `409` |
| Jebakan: waktu riwayat tidak tampil UTC | Terpenuhi | Uji unit + e2e `01.30 WIB` |
| Peramban akun Kepala Instalasi terhadap backend nyata | **Belum** | Menunggu `MVP-16e` no. 1–3 |
| DoD — laporan `FE-LAB-58.md` | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Terhadap API dev bersama yang belum memuat `r50`, ringkasan *Tanggal Rencana* berisi `-`, hitungan *Belum pernah dijadwalkan ulang*, dan tombol nonaktif dengan keterangan *Kelayakan jadwal ulang belum tersedia* — tidak menebak |
| Masalah yang diketahui | Peringatan lint lama `setResolvedId` di efek (baris 126) — di luar scope. `FE-LAB-49 AC-282` gagal sebelum task ini |
| Dependency backend | `BE-LAB-101`..`103` selesai pada kode; langkah rilis `MVP-16e` no. 1–3 sebelum verifikasi akun asli |
| Perubahan sampingan | `NONE`. Spec visual sementara `zz-fe58-visual.tmp.spec.mjs` dibuat dan dihapus. Perubahan SDM di working tree (`src/lib/services/hr/*`, `src/utils/hr/hr-api-utils.js`, `tests/unit/hr-a52b-api-client.test.mjs`) milik sesi lain — tidak disentuh |
| Interupsi | `NONE`. Spec pertama gagal karena ID tiruan bukan UUID v4 sah (rute detail menolaknya) — diperbaiki di spec; temuan ikutan *tombol tampil saat detail belum ada* diperbaiki di hook |
| Status Git | Berkas task: ` M` `lab-order-detail-view.jsx`, `use-lab-order-detail.jsx`; `lab-reschedule-rules.js`, `tests/unit/lab-reschedule-rules.test.mjs` (baru sejak `FE-LAB-57`) diperluas; `??` `tests/e2e/lab-reschedule-order-detail.spec.mjs`. Berkas `FE-LAB-57` tetap di working tree. Tanpa stage/commit |
| Langkah berikutnya | `FE-LAB-59` (kiosk *Konfirmasi Kehadiran*). Sesudah langkah rilis `MVP-16e`: verifikasi peramban dengan akun Gilang dan dr. Bima |
