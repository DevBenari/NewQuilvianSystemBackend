# PLAN-REPAIR-003 — Tambahkan No. Kunjungan dan tanggal ke label dropdown Pasien/Kunjungan

```yaml
plan_id: PLAN-REPAIR-BKC-003
issue: ../issue/issue-003-kunjungan-pasien-tampil-identik.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-16"
diputuskan_oleh: "Pemilik modul Billing — menjawab K-01 langsung di sesi diagnosis: 'No. Kunjungan + tanggal+jam'"
tanggal_keputusan: "2026-10-16"
basis_source_backend: "e70f5abf (Yasmina)"
basis_source_frontend: "8ad45262b (yasmina)"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-BKC-003-01` | `ISS-BKC-003-01` | Tambahkan No. Kunjungan + tanggal/jam ke label opsi dropdown Pasien/Kunjungan | FE | 1 | `FE-BKC-FIX-014` | ✅ SELESAI | [FE-BKC-FIX-014.md](../../task/report/frontend/FE-BKC-FIX-014.md) |

**Ringkasan: 1 dari 1 perbaikan selesai.** Source selesai, lint 0 error, `npm run build` lulus;
uji manual live browser `NOT FEASIBLE` (backend tidak dijalankan otomatis per instruksi kerja —
lihat laporan task untuk alasan lengkap dan verifikasi logika sebagai gantinya).

Catatan: `ISS-BKC-003-T1` dan `ISS-BKC-003-T2` **tidak** mendapat `FIX` di sini karena akar
masalahnya berada di modul Registrasi/Rawat Inap, di luar kepemilikan modul Billing. Keduanya
menunggu pemilik modul tersebut, lihat `issue-003` bagian 6 dan 7.

## 2. Solusi terpilih per temuan

### ISS-BKC-003-01 — Label dropdown kunjungan tidak menyertakan field pembeda

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tambahkan `encounterNumber` (dan `encounterDate` yang diformat singkat) ke string label opsi di `create-manual-invoice-view.jsx` | Data sudah tersedia penuh di `ActiveEncounterOptionResponse`, tanpa ubah kontrak API; konsisten dengan prinsip `BKC-DEC-061` yang sudah disetujui pemilik untuk dropdown Tarif di form yang sama; radius dampak kecil (satu file, satu fungsi `map`) | Label jadi sedikit lebih panjang |
| B | Tambahkan badge/kolom terpisah di setiap baris opsi via `renderOption` kustom (pola yang sudah dipakai field `tariffId` lewat `TariffCoverageOption`) | Tampilan lebih rapi, bisa menonjolkan No. Kunjungan dengan gaya visual berbeda | Perubahan lebih besar (komponen baru) untuk manfaat yang bisa dicapai opsi A dengan jauh lebih sederhana; field ini belum butuh keterangan visual sekompleks preview coverage tarif |
| C | Biarkan label seperti sekarang, cukup andalkan urutan `OrderByDescending(EncounterDate)` dari backend supaya kunjungan terbaru selalu di atas | Tidak ada perubahan kode | Tidak menyelesaikan masalah — kasir tetap tidak bisa membedakan baris mana yang mana, hanya menebak dari posisi; berisiko salah pilih tetap ada |

**Solusi terpilih: Opsi A.**

1. Radius dampak paling kecil: satu fungsi `map` di satu file frontend, memakai field yang sudah
   dikirim backend (`EncounterNumber`, `EncounterDate` — `ActiveEncounterOptionResponse`,
   `BillingInvoiceDtos.cs:305-328`), tidak menyentuh backend maupun kontrak API sama sekali.
2. Konsisten dengan preseden yang sudah disetujui pemilik untuk masalah yang sama persis di form
   ini (`BKC-DEC-061`): opsi yang tampak sama wajib didisambiguasi dengan label eksplisit, bukan
   dibiarkan ambigu.
3. Tidak menduplikasi komponen (`TariffCoverageOption` pada opsi B dibangun khusus untuk preview
   kalkulasi coverage tarif — kebutuhan yang jauh lebih kompleks dari sekadar menampilkan nomor
   dan tanggal).

**Kenapa bukan opsi lain.** Opsi B berlebihan untuk kebutuhan sesederhana menampilkan dua field
teks. Opsi C tidak benar-benar menyelesaikan masalah yang dilaporkan.

## 3. Skema tampilan sebelum → sesudah

```text
[ Tombol ]      tombol             ( ) pilihan tunggal      [ o] sakelar
[ isian      ]  isian teks         (•) terpilih             ▾ daftar pilihan
(×) tidak dapat dipilih            ⚠ peringatan             * wajib diisi
[✓] kotak centang tercentang       [ ] kotak centang kosong
```

**SEBELUM** (sesuai screenshot pelapor — tiga baris identik):

```text
Pasien / Kunjungan *                                              ▾
┌──────────────────────────────────────────────────────────────┐
│ [ Cari data...                                              ] │
├──────────────────────────────────────────────────────────────┤
│ Jesse Pinkman · 00-00-00-18 · RANAP · Tunai                   │  ← terpilih
│ Jesse Pinkman · 00-00-00-18 · RANAP · Tunai                   │
│ Jesse Pinkman · 00-00-00-18 · RANAP · Tunai · sudah ada invoice│
└──────────────────────────────────────────────────────────────┘
```

**SESUDAH** (No. Kunjungan + tanggal/jam disertakan di tiap baris):

```text
Pasien / Kunjungan *                                              ▾
┌──────────────────────────────────────────────────────────────────────┐
│ [ Cari data...                                                      ] │
├──────────────────────────────────────────────────────────────────────┤
│ Jesse Pinkman · 00-00-00-18 · ENC-RSMMC-00042 · 16 Okt 2026 08.10 ·   │  ← terpilih
│   RANAP · Tunai                                                      │
│ Jesse Pinkman · 00-00-00-18 · ENC-RSMMC-00039 · 15 Okt 2026 14.05 ·   │
│   RANAP · Tunai                                                      │
│ Jesse Pinkman · 00-00-00-18 · ENC-RSMMC-00031 · 12 Okt 2026 09.22 ·   │
│   RANAP · Tunai · sudah ada invoice                                  │
└──────────────────────────────────────────────────────────────────────┘
```

(`ENC-RSMMC-00042` dan tanggal di atas adalah contoh pseudonim, bukan data pasien nyata.)

**Wilayah:**

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Baris opsi dropdown | Nama pasien · No. RM · **No. Kunjungan (baru)** · **Tanggal+jam kunjungan (baru)** · Jenis layanan · Cara bayar (+ penjamin bila ada) · tanda "sudah ada invoice" bila berlaku | `encounterOptions` dari `getActiveEncounterOptions` (field `encounterNumber`, `encounterDate` yang sudah ada di response, sebelumnya tidak dibaca) | `create-manual-invoice-view.jsx` — fungsi `map` pembentuk `options` field `encounterId` |

**Tombol:** Tidak ada tombol baru atau berubah pada perbaikan ini.

**Keadaan:** Tidak ada keadaan baru (memuat, kosong, gagal) — perilaku loading/error/empty
dropdown yang sudah ada tidak disentuh, hanya teks label tiap opsi yang bertambah.

## 4. Rincian perbaikan

### FIX-BKC-003-01 — Tambahkan No. Kunjungan + tanggal/jam ke label opsi dropdown Pasien/Kunjungan

| | |
| --- | --- |
| **Menutup** | `ISS-BKC-003-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/components/view/health-services/billing-management/billing-invoices/create-manual/create-manual-invoice-view.jsx` — fungsi `map` pembentuk opsi field `encounterId` (baris 52-86) |
| **Radius dampak** | Hanya layar **Tambah Invoice Manual** (testing tool) — field `options` ini dibangun lokal di komponen ini, tidak direuse komponen/modul lain |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Di dalam fungsi `map` yang membentuk `options` untuk field `encounterId`
   (`create-manual-invoice-view.jsx:52-86`), baca field `encounterNumber`/`EncounterNumber` dan
   `encounterDate`/`EncounterDate` dari tiap `encounter`, dengan pola fallback camelCase/PascalCase
   yang sama seperti field lain di fungsi ini (lihat `patientName`, `medicalRecordNumber`, dsb).
2. Format `encounterDate` memakai utilitas format tanggal yang sudah dipakai repo frontend untuk
   label ringkas (cek util format tanggal existing di `src/utils/` sebelum menulis formatter baru
   — pertahankan pola reuse, jangan duplikasi fungsi format tanggal).
3. Sisipkan `encounterNumber` dan tanggal terformat ke template string `label` 
   (baris 82-84), di antara `medicalRecordNumber` dan `serviceType`, sehingga urutannya menjadi:
   nama pasien · No. RM · No. Kunjungan · tanggal+jam · jenis layanan · cara bayar · (tanda sudah
   ada invoice bila berlaku). Pertahankan seluruh logika `paymentDisplay` dan tanda `hasInvoice`
   yang sudah ada, tidak diubah.
4. Tidak ada perubahan pada `encounterOptions` (hook), backend, Redux slice, atau `FilterSelect` —
   perbaikan ini murni di titik pembentukan `label`.

**Acceptance criteria.**

1. Untuk pasien pseudonim dengan ≥2 kunjungan aktif yang kombinasi nama+RM+jenis layanan+cara
   bayarnya sama, setiap baris opsi di dropdown menampilkan No. Kunjungan yang berbeda satu sama
   lain, sehingga tidak ada dua baris dengan teks identik selama No. Kunjungan-nya memang berbeda.
2. Label tetap menampilkan seluruh informasi yang sudah ada sebelumnya (nama, No. RM, jenis
   layanan, cara bayar, penjamin bila ada, tanda "sudah ada invoice") — tidak ada informasi lama
   yang hilang.
3. Pencarian di kolom "Cari data..." tetap berfungsi mencari berdasarkan nama pasien, No. RM, atau
   No. Kunjungan seperti sebelumnya (perilaku pencarian sisi server pada `handleEncounterSearch`
   tidak diubah oleh perbaikan ini).
4. Kartu konteks kunjungan (`EncounterContextCard`) yang muncul setelah satu opsi dipilih tetap
   menampilkan data yang sama seperti sebelumnya — tidak ada regresi di sana.

**Verifikasi.** Jalankan lint/build frontend sesuai konvensi repo (mis. `npm run lint` dan
`npm run build` pada `QuilvianSystemFrontendDev`, sesuai yang diwajibkan task rules repo
tersebut) tanpa galat baru pada berkas yang diubah. Verifikasi manual: buka layar Tambah Invoice
Manual, buka dropdown Pasien/Kunjungan, pastikan setiap baris kini menampilkan No. Kunjungan dan
tanggal; untuk pasien yang punya >1 kunjungan aktif, pastikan baris-barisnya kini bisa dibedakan.

**Risiko.** Label jadi lebih panjang — berpotensi terpotong di layar sempit bila `FilterSelect`
tidak membungkus teks panjang; perlu dicek visual pada lebar form yang biasa dipakai kasir
(desktop). Tidak ada risiko pada data tersimpan karena perbaikan ini murni kosmetik, tidak
mengubah `value` opsi (`encounter.id`) yang dikirim ke server saat submit.

## 5. Urutan pengerjaan

```text
FIX-BKC-003-01   tanpa dependency — boleh dikerjakan sekarang
```

## 6. Keputusan pemilik yang dibutuhkan

Disalin dari `issue-003` bagian 7, diperbarui dengan keputusan 16-10-2026:

| No | Keputusan | Pilihan | Keputusan diambil | Status |
| ---: | --- | --- | --- | --- |
| K-01 | Format tambahan pada label dropdown kunjungan: cukup No. Kunjungan saja, atau No. Kunjungan + tanggal/jam? | No. Kunjungan saja / No. Kunjungan + tanggal+jam | **No. Kunjungan + tanggal+jam** | **Diputuskan** — `FIX-BKC-003-01` tidak lagi tertahan |
| K-02 | (Lintas modul, pemilik Registrasi/Rawat Inap) Apakah satu pasien boleh punya >1 kunjungan RANAP aktif/Draft bersamaan? | Boleh (disengaja) / Tidak boleh (perlu guard baru) | **Tidak boleh — perlu guard baru** | **Diputuskan**, tapi implementasinya di luar blueprint ini (milik Registrasi/Rawat Inap) — tidak menahan `FIX-BKC-003-01`. Diteruskan sebagai [`ISSUE-EPS-022`](../../../rawat-inap/episode-rawat-inap/docs/issue/issue-022-admisi-ranap-ganda-tanpa-guard.md) |
| K-03 | (Lintas modul, pemilik Registrasi) Apakah `CompletedAt` perlu diisi di endpoint `UpdateEncounterStatus` (`PatientEncounterController.cs:2003`) setiap kali status diubah jadi `Completed`? | Ya, perbaiki endpoint tersebut / Tidak | **Ya — perbaiki endpoint agar mengisi `CompletedAt`** | **Diputuskan**, tapi implementasinya di luar blueprint ini (milik Registrasi) — tidak menahan `FIX-BKC-003-01`. Diteruskan sebagai [`ISSUE-RJ-DOC-001`](../../../rawat-jalan/docs/issue/issue-001-encounter-completed-tanpa-completed-at.md) |

`FIX-BKC-003-01` sudah tidak tertahan keputusan apa pun dan siap didaftarkan ke roadmap untuk
dikerjakan. K-02 dan K-03 adalah keputusan yang sah dicatat di sini (ditanyakan dan dijawab dalam
sesi diagnosis issue ini), tetapi eksekusinya menyentuh source milik modul Registrasi/Rawat Inap —
di luar wewenang tulis `MODULE BLUEPRINT MODE` untuk blueprint `billing-kasir` ini. Sudah
diteruskan 16-10-2026 sebagai issue resmi di blueprint masing-masing — `ISSUE-EPS-022` (K-02,
`rawat-inap/episode-rawat-inap`) dan `ISSUE-RJ-DOC-001` (K-03, `rawat-jalan`) — keduanya
`MENUNGGU_PERSETUJUAN` pemilik modul tersebut sebelum didaftarkan sebagai task.

## 7. Dokumen hulu yang ikut direvisi

Tidak ada. `FIX-BKC-003-01` berjenis `BUG`, bukan `DESIGN_CHANGE` — tidak ada dokumen desain yang
secara eksplisit menetapkan label dropdown ini sehingga tidak ada dokumen hulu yang perlu direvisi
agar tidak saling membantah dengan source.

## 8. Verifikasi menyeluruh

Setelah `FIX-BKC-003-01` selesai:

1. Buka layar Tambah Invoice Manual, cari pasien yang pada data uji memiliki ≥2 kunjungan RANAP
   aktif dengan cara bayar sama (skenario persis seperti laporan pelapor).
2. Pastikan dropdown menampilkan baris-baris dengan No. Kunjungan dan tanggal yang berbeda satu
   sama lain, dan kasir dapat secara visual memastikan kunjungan mana yang akan dipilih sebelum
   menekannya.
3. Pilih salah satu baris, pastikan kartu konteks kunjungan di bawah dropdown menampilkan data
   yang konsisten dengan baris yang dipilih (No. Kunjungan yang sama).
4. Pastikan submit invoice manual tetap mengirim `encounterId` (GUID) yang benar sesuai baris yang
   dipilih — tidak berubah oleh perbaikan ini.

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-16 | Rencana dibuat, 1 perbaikan, menunggu persetujuan | `diagnose-module-issue` |
| 2026-10-16 | Dasar `FIX-BKC-003-01` dikonfirmasi dengan data nyata (jawaban P-01 pada `issue-003`): tiga `Id`/`EncounterNumber` berbeda terverifikasi di database, bukan dugaan. Tidak ada perubahan pada solusi atau langkah perbaikan | `diagnose-module-issue` |
| 2026-10-16 | Pemilik modul Billing memutuskan K-01, K-02, K-03 (lihat bagian 6). `status_rencana` berubah `MENUNGGU_PERSETUJUAN` → `DISETUJUI`. `FIX-BKC-003-01` siap didaftarkan ke roadmap untuk dikerjakan | `diagnose-module-issue` atas perintah pemilik |
| 2026-10-16 | `FIX-BKC-003-01` didaftarkan sebagai `FE-BKC-FIX-014` pada `roadmap/frontend-roadmap.md` (gelombang `PLAN-REPAIR-BKC-003`) dan dikerjakan lewat `build-module-frontend`: label dropdown kunjungan kini menyertakan No. Kunjungan + tanggal/jam. Lint 0 error, `npm run build` lulus; uji manual live browser `NOT FEASIBLE` (backend tidak dijalankan otomatis). Register ditandai `✅ SELESAI`; `status_rencana` berubah `DISETUJUI` → `SELESAI`. Laporan: [FE-BKC-FIX-014.md](../../task/report/frontend/FE-BKC-FIX-014.md) | `build-module-frontend` atas perintah pemilik |
| 2026-10-16 | K-02 diteruskan sebagai `ISSUE-EPS-022`/`PLAN-REPAIR-EPS-022` pada blueprint `rawat-inap/episode-rawat-inap`; K-03 diteruskan sebagai `ISSUE-RJ-DOC-001`/`PLAN-REPAIR-RJDOC-001` pada blueprint `rawat-jalan`. Keduanya `MENUNGGU_PERSETUJUAN` pemilik modul masing-masing | `diagnose-module-issue` atas perintah pemilik |
