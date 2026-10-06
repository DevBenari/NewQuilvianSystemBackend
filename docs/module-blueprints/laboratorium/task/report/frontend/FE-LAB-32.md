# Laporan Perubahan Frontend — `FE-LAB-31`, `FE-LAB-32`, `FE-LAB-33`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-31`, `FE-LAB-32`, `FE-LAB-33` — **dikerjakan bersama atas permintaan pemilik modul** |
| Judul | Form isolat dan antibiogram; Informasi Specimen yang dapat disunting; Kelengkapan, konsultasi, dan dokter konfirmator |
| Slice | `S4b` — pengisian hasil Mikrobiologi |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) |
| Trace | `LAB-DEC-095`..`LAB-DEC-128`; `AC-157`..`AC-170` |
| Contract version | `LAB-API-v1` `r24` 19.2, `r26` 21.2/21.3/21.4/21.5/21.7, `r27` 22.3/22.8 — seluruhnya `approved` |
| Wewenang UI | Mengisi tempat hasil pada halaman `FE-LAB-30`; satu slice Redux baru; satu service baru. **Nol wewenang** mengubah halaman lain |
| Dependency | `FE-LAB-30` ✅; backend `BE-LAB-53`..`BE-LAB-63` ✅ |
| Klasifikasi | `HEAVY` — tiga task, 13 acceptance criteria, satu slice, satu service, empat komponen |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `3339ecdf1` |
| Commit backend yang dijadikan rujukan | `458f38aa` |
| Tanggal | 2026-09-22 |
| Status | **Per task, sesudah verifikasi di peramban 2026-10-02 dan 2026-10-06 (bagian 9):** `FE-LAB-31` ✅ **`SELESAI`** · `FE-LAB-32` ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — ketiga AC-nya terbukti, tetapi formulir koreksi tidak terisi nilai tersimpan dan respons specimen backend nol memuat id Spesifik Specimen (9.4) · `FE-LAB-33` ✅ **`SELESAI`**. *(Semula 2026-09-22: ⚠ ketiganya — permukaan terbangun, 22 uji baru lulus, nol AC terbukti di layar.)* |

---

## 1. Kenapa ketiganya dikerjakan sebagai satu slice

Ketiganya **satu layar**, dan saling mengunci:

- `VAL-109` membuat Informasi Specimen (`FE-LAB-32`) **baca-saja** begitu hasil (`FE-LAB-31`)
  dinyatakan selesai lewat kendali `FE-LAB-33`.
- `finalize` (`FE-LAB-33`) hanya sah ketika hasilnya (`FE-LAB-31`) sudah terisi.

Memecahnya menjadi tiga hook berarti tiga tempat membaca keadaan kunci yang sama, dan ketiganya
pasti bercabang.

---

## 2. Perubahan yang dikerjakan

| Path | Isi |
|---|---|
| `constants/.../lab-microbiology-result-constants.jsx` | Enum, alamat endpoint, dan salinan teks |
| `services/.../lab-microbiology-result.service.js` | 11 fungsi; **nol endpoint rilis/validasi/kirim** |
| `state/slice/.../lab-microbiology-result-slice.jsx` | 11 thunk, terdaftar di store sebagai `labMicrobiologyResult` |
| `hooks/.../lab-microbiology-result-rules.js` | Validasi dan penyusun payload — **fungsi murni** |
| `hooks/.../use-lab-microbiology-result-editor.jsx` | Controller satu layar |
| `view/.../lab-microbiology-result-form.jsx` | `FE-LAB-31` |
| `view/.../lab-microbiology-specimen-section.jsx` | `FE-LAB-32` |
| `view/.../lab-microbiology-completion-bar.jsx` | `FE-LAB-33` |
| `view/.../lab-microbiology-result-panel.jsx` | Penyusun ketiganya |
| `view/.../lab-microbiology-workspace-view.jsx` | Tempat hasil diisi panel |
| `constants/.../laboratory-constants.jsx` | Tiga alamat data induk |
| `style/.../lab-microbiology-workspace.module.css` | Style ketiga bagian |
| `tests/unit/lab-microbiology-result-rules.test.mjs` | 22 uji |

### Satu koreksi yang ditemukan sebelum kode dikirim

Payload konsultasi semula saya susun sebagai `{ consultedByUserId, note }`. Pembacaan
`LabConsultationRequest` menunjukkan bentuk sebenarnya **`{ consultedToName, consultedAt }`** —
**nama teks, bukan penunjuk pengguna**, sebab konsultannya sering berada di luar daftar
pengguna sistem (`LAB-EVD-005`), dan keduanya wajib.

Pemilih dokter karena itu **mengisi** ruas nama, bukan menggantikannya: ruasnya tetap dapat
diketik bebas. Inilah alasan aturan "jangan menebak payload backend" ada.

---

## 3. Keputusan yang layak dibaca

**`key` pada panel hasil adalah bentuk teknis `AC-156`.** Panel dipasang ulang setiap kali
baris yang dipilih berganti, sehingga keadaan formulir pemeriksaan sebelumnya **nol terbawa**.
Tanpa itu, berpindah baris akan meninggalkan zona hambat milik kuman lain di dalam formulir.

**`Simpan Final` selalu menyimpan lebih dulu, baru menyatakan selesai.** Analis yang mengetik
lalu langsung menekan Final tanpa itu akan ditolak backend dengan pesan yang membingungkan —
*"hasil belum diisi"* — padahal layarnya penuh.

**Data induk ditarik sekali per halaman, bukan per baris antibiogram.** Satu antibiogram lazim
memuat dua puluh baris; menariknya per baris berarti dua puluh permintaan untuk daftar yang
sama persis.

**Riwayat perubahan specimen dimuat atas permintaan.** Ia jarang dibuka; memuatnya otomatis
hanya menambah satu permintaan yang nol dibaca siapa pun.

---

## 4. Larangan DoD yang ditegakkan secara struktural

| Larangan | Cara ditegakkan |
|---|---|
| Nol pengetikan bebas organisme dan antibiotik | Keduanya `BaseSelectField` atas data induk; nol ruas teks |
| Nol pilihan `NeedsAttention`/`Critical` | `LAB_MICROBIOLOGY_FINDING_OPTIONS` hanya memuat tiga nilai |
| Nol tombol menambah Spesifik Specimen | Bagian specimen nol punya tombol tambah |
| Nol tombol kirim ke pasien | Nol di service, nol di komponen |
| Nol pilihan `HL7` | Nol kemunculan |
| Nol tombol validasi/rilis | Nol di service, nol di komponen |

---

## 5. Verifikasi

| Perintah | Hasil | Klasifikasi |
|---|---|---|
| `npx eslint` atas 9 berkas baru | 0 error, 1 warning | `PASS` — warning `set-state-in-effect`, pola yang sama dengan hook acuan repo |
| `npm run build` | `✓ Compiled successfully` | `PASS` |
| Uji unit baru | **22/22 lulus** | `PASS` |
| Seluruh suite | 1536/1542 | `EXISTING / ENVIRONMENT ISSUE` — **6 kegagalan sama persis** dengan sebelum perubahan |
| Route `[slug]` terender | `200` | `PASS` |

**`AUTOMATED TEST: node --test tests/unit/ — PASS`** (1536 lulus; 6 kegagalan lama, nol
bertambah).

**`MANUAL TEST: NOT FEASIBLE`.** Sesi ini nol punya alat kendali peramban. Seluruh kontrol
interaktif yang dibangun — pemilih status temuan, tambah/hapus isolat, tabel antibiogram,
kotak centang Spesifik Specimen, tombol Simpan/Final/Reopen, pencatatan konsultasi, pemilih
dokter konfirmator — **belum satu pun diklik**.

---

## 6. Acceptance criteria — dibaca apa adanya

| AC | Status | Bukti |
|---|---|---|
| `AC-163` baris tanpa MIC dan tanpa zona tersimpan | ✅ **Terbukti di layar** 2026-10-02 | Uji unit; M6 di 9.1 |
| `AC-164` kultur tanpa isolat tersimpan tanpa peringatan | ✅ **Terbukti di layar** | Uji unit; M4 |
| `AC-165` baris tanpa interpretasi ditolak beserta sebabnya | ✅ **Terbukti di layar** | Uji unit; M5. Sejak 2026-10-06 berlaku bagi baris **tanpa zona**; baris berzona ditolak server beserta sebab bila breakpoint belum disetel (9.3 (d)) |
| `AC-160` lebih dari satu Spesifik Specimen | ✅ **Terbukti di layar** | Uji unit; K2 |
| `AC-162` volume sebagai angka + satuan | ✅ **Terbukti di layar** | Uji unit; K2 (`2,5` L/jam) |
| `AC-157` kedua waktu baca-saja | ✅ **Terbukti di layar** | Payload nol memuatnya; M2 |
| `AC-168` Analis baca-saja | ✅ **Terbukti di layar** | M1, M2 |
| `AC-158` sesudah Final layar menyatakan belum dirilis | ✅ **Terbukti di layar** | M12 |
| `AC-166` ketiadaan aturan kritis dinyatakan | ✅ **Terbukti di layar** | M3, K8 |
| `AC-169` konsultasi nol membuat Definitif | ✅ **Terbukti di layar** | M9 |
| `AC-170` sesudah Final specimen baca-saja | ✅ **Terbukti di layar** | M13; riwayat koreksi K3 |
| `AC-173`, `AC-174` (`FE-LAB-33`) | ✅ **Terbukti di layar** | T15–T17', D1–D2 |
| `AC-176`, `AC-178`, `AC-186`..`AC-191` (`FE-LAB-31`, cakupan `MVP-7b`) | ✅ **Terbukti di layar** 2026-10-06 | T1–T14 di 9.2 |

> **Perbedaan "terbukti pada aturannya" dan "terbukti" ditulis di sini dengan sengaja.**
> Aturan murninya diuji dan lulus; yang **belum** diverifikasi adalah bahwa komponennya
> memanggil aturan itu pada keadaan yang benar, dan bahwa hasilnya terlihat sebagaimana
> mestinya di layar. Menyatakan ketigabelasnya "terpenuhi" akan mengklaim lebih daripada yang
> saya kerjakan.

---

## 7. Yang perlu dikerjakan sebelum ketiganya boleh ditandai selesai

1. ~~**Verifikasi klik menyeluruh** pada peramban, memakai akun bukan superadmin.~~ **Selesai
   2026-10-02 dan 2026-10-06** (bagian 9) dengan akun analis asli; superadmin hanya menggantikan
   petugas koreksi specimen yang belum punya akun.
2. ~~Data ujinya sudah siap …~~ Dipakai: pesanan uji `LAB-RSMMC-000014`.
3. **Data induk masih hampir kosong** — satu organisme, satu antibiotik, satu breakpoint. Bukan
   penahan status: pekerjaan data kepala instalasi (`B3`), bukan kode.
4. **Sisa `FE-LAB-32`:** ruas id Spesifik Specimen pada respons specimen backend, lalu formulir
   koreksi diisi nilai tersimpan (9.4).

**Nol operasi git dijalankan.** Satu dev server sisa dari sesi ini ditemukan masih hidup
(`PID 8880`) dan sudah dihentikan.

---

## 8. Perbaikan susulan 2026-10-01 — volume specimen kehilangan desimal

**Masalah.** Volume specimen diketik lewat `BaseTextField` dengan `type: "number"`, yang membuang pemisah desimal
(`normalizeNumberInputValue`: hanya digit, lalu `Math.trunc`). Volume `1,5` mL tersimpan `15`. Ditemukan saat `FE-LAB-36`; diperbaiki
atas instruksi pemilik modul.

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/.../microbiology/lab-microbiology-specimen-section.jsx (volume)` | Ruas bertipe teks dengan `DECIMAL_FIELD_PROPS` (`inputMode: decimal` + `normalizeDecimalInput`) |
| `src/lib/hooks/.../lab-microbiology-result-rules.js` | `hasNumber`, `optionalNumber`, dan batas negatif memakai `parseDecimal` — koma diterima. Tanpa ini `"0,5"` dianggap bukan angka dan **dikirim kosong** |
| `src/lib/hooks/.../lab-decimal-input-rules.js` | **Baru** — normalizer desimal bersama Laboratorium |

| Verifikasi | Hasil | Klasifikasi |
| --- | --- | --- |
| `tests/unit/lab-decimal-input-rules.test.mjs` | 6/6 — kadar `0,5` → `0.5`, volume `1,5` → `1.5`, `VAL-112` tetap berlaku, kedua ruas tidak lagi `type: "number"` | `PASS` |
| Uji unit Mikrobiologi lama | Lolos | `PASS` |
| Layar (akun analis, permintaan simpan **dicegat** — nol data berubah) | Isian menampilkan `0,5` / `1,5`; badan permintaan `concentration: 0.5`, `volumeAmount: 1.5` | `PASS` |

Rincian di [`FE-LAB-36.md`](FE-LAB-36.md) bagian 10.4-10.5.

---

## 9. Verifikasi susulan di peramban — 2026-10-02 dan 2026-10-06

**Status sesudah verifikasi ini:** `FE-LAB-31` ✅ `SELESAI` · `FE-LAB-32` ⚠ **tetap** `SELESAI DENGAN BATAS VERIFIKASI` (batas baru, 9.4) · `FE-LAB-33` ✅ `SELESAI`.

**Lingkungan.** Backend lokal (`dotnet run`, `Development`) terhadap PostgreSQL dev bersama; `next dev`
port 3000 dari working tree `YogaV2`; Chromium lewat Playwright; login **lewat formulir**.
**Akun:** **Vina (analis asli**, pemegang `LabExamination : Update`, tanpa `LabSpecimen : Update`);
**superadmin** sebagai pengganti pemegang `LabSpecimen : Update`, sebab akun petugas koreksi specimen
belum ada di dev. **Wewenang tulis (izin pemilik modul):** penjaga tulis hanya meneruskan `PUT` hasil,
`finalize`, `reopen`, `consultation` milik pemeriksaan BTA kedua pesanan uji **`LAB-RSMMC-000014`**
(`1f3670d7…`) dan `PATCH …/correction` specimennya (`14e5794d…`); selebihnya digagalkan.

### 9.1 Sesi 2026-10-02 — `AC-157`..`AC-170`

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| M1 | BTA kedua: keadaan *Menunggu Hasil*; tombol *Pemeriksaan Selesai*; Analis baca-saja | `PASS` | — |
| M2 | `AC-157`/`AC-168` Waktu Efektif, Waktu Issued, Analis tampil sebagai teks — nol isian | `PASS` | 0 isian |
| M3, K8 | `AC-166` aturan kritis kosong → layar **menyatakannya** | `PASS` | Dev punya aturan kritis; jawaban backend diubah di peramban menjadi `criticalRuleAvailable: false` → *"Hasil tanpa penanda BUKAN berarti hasilnya aman"* |
| M4 | `AC-164` kultur tanpa isolat (Negatif) tersimpan tanpa penolakan dan tanpa peringatan | `PASS` | `PUT` 200, `peringatan: []` |
| M5 | `AC-165` baris kepekaan tanpa interpretasi ditolak beserta sebabnya; nol permintaan | `PASS` | Baris tanpa zona |
| M6 | `AC-163` baris tanpa kadar dan tanpa zona tersimpan | `PASS` | `PUT` 200, `concentration`/`zoneDiameterMm` `null` |
| M7 | Muat ulang: isolat dan antibiogram terbaca dari backend | `PASS` | *Branhamella catarrhalis* × Ampicillin |
| M8 → K6 | Kadar desimal: metode Dilusi, `0,5` bersatuan → terkirim `0.5`, tersimpan, terbaca `0,5` | **`FAIL` → `PASS`** | Cacat (b) dan (c) di 9.3 |
| M9 | `AC-169` Definitif tersimpan; konsultasi tercatat; nol tombol Kirim/Rilis/Validasi bagi analis | `PASS` | Keduanya `200`; konsultan *"dr. Konsultan Uji FE-LAB-33"* |
| M10 → K2 | `AC-160`/`AC-162` koreksi specimen: dua Spesifik Specimen (centang) + volume `2,5` bersatuan → `PATCH` **200** | **`FAIL` → `PASS`** | Vina mendapat `403` karena layar menawarkan koreksi tanpa hak — cacat (a); lalu `422` karena satuan non-laboratorium — cacat (b). Sesudah keduanya: superadmin, `PATCH` 200 "Informasi Specimen berhasil dikoreksi; 3 ruas tercatat." |
| M11 → K3 | `AC-170` koreksi berjejak — riwayat memuat ruas, nilai lama, dan nilai baru | **`PASS`** sesudah K2 | "Satuan Volume: - → L/jam; Volume: - → 2.5; Spesifik Specimen: - → Arterial cord blood specimen, Darah arteri" |
| M12 | `AC-158` *Pemeriksaan Selesai* → `finalize` 200; layar menyatakan **belum dirilis**; nol tombol Validasi bagi analis | `PASS` | "Penulisan hasil dinyatakan selesai. Hasil ini belum dirilis." |
| M13 | `AC-170` sesudah Final: Informasi Specimen baca-saja beserta keterangannya | `PASS` | — |
| M14 | `AC-159` Buka Kembali beralasan → 200; kembali Draft; hitungan dibuka kembali bertambah | `PASS` | "Dibuka kembali 1 kali" |
| M15 | 390 px tanpa gulir horizontal halaman | `PASS` | `375 ≤ 390` |
| K1 | `AC-161` jenis *Lainnya* wajib berketerangan | `PASS` (aturan) | Opsi *Lainnya* tidak ada pada data induk dev — dibuktikan uji unit |
| K4 | Muat ulang: volume terbaca `2,5` pada **formulir koreksi** | **`FAIL`** | Formulir koreksi tidak diisi nilai tersimpan — **batas `FE-LAB-32`**, 9.4 |
| K5 | Vina (tanpa `LabSpecimen : Update`): Informasi Specimen baca-saja **dengan sebabnya**; nol *Simpan Koreksi* | `PASS` | Sesudah cacat (a) |
| D1, D2 | `AC-174` jadwal jaga kosong: pilihan dokter dimuat backend, jalur jatuhnya dinyatakan; memilih mengisi nama konsultan | `PASS` | `onDutyScheduleAvailable: false` |
| M16, K7 | Nol tulis di luar pemeriksaan uji | `PASS` | — |

### 9.2 Sesi 2026-10-06 — cakupan `MVP-7b` dan `AC-173`

Data induk dev: satu breakpoint *Branhamella catarrhalis* × Ampicillin, rentang **13–17 mm**.

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| T1 | `AC-176` status temuan menawarkan **tepat** Normal, Positif, Negatif | `PASS` | Nol *Perlu Perhatian*/*Kritis* |
| T2 | `AC-188` Difusi Cakram: `UG`, `R-S`, `Zona (mm)`, nol `Kadar`; Dilusi: `Kadar` + `Satuan`, nol `UG`/zona | `PASS` | Kepala tabel |
| T3 | `AC-190` isolat bertanda *Kepekaan tidak diuji* tanpa satu pun baris → `PUT` 200; muat ulang: penanda tetap | `PASS` | `isSusceptibilityTested: false` |
| T4 | `AC-186` zona 15, S/I/R **tidak dipilih**: layar memberi keterangan; `result: null` terkirim; server menghitung **I** | `PASS` | Sesudah perubahan (d) |
| T5 | `AC-185` kolom `R-S` = snapshot baris `13-17`, nol isian | `PASS` | — |
| T6, T7 | `AC-186` zona 10 → **R**; zona 30 → **S**; interpretasi lama dikosongkan sendiri, nol penolakan penimpaan | `PASS` | Sesudah perubahan (e) |
| T8 | `AC-191` zona **0** → **R** | `PASS` | `zoneDiameterMm: 0` tersimpan sebagai angka |
| T9 | `AC-187` menimpa R menjadi S: kotak *Alasan menimpa* muncul; tanpa alasan ditolak layar, nol permintaan | `PASS` | — |
| T10 | `AC-187` dengan alasan → 200; muat ulang: S tersimpan **beserta hitungan aslinya**, keterangan *"sistem menghitung R"* | `PASS` | `isResultOverridden: true`, `computedResult: R` |
| T11 | `AC-191` zona dikosongkan → `null`; tersimpan belum diukur, nol interpretasi hitungan | `PASS` | `computedResult: null` |
| T12 | `AC-178` kadar `1,25` tanpa satuan ditolak layar beserta sebabnya; nol permintaan | `PASS` | "Pilih satuan untuk nilai kadar." |
| T13 | `AC-178` dengan satuan → 200; terbaca `1,25` | `PASS` | Satuan MILIGRAM. Bagian *"mg/L dan ug/mL berdampingan"* tidak dapat diulang di layar: dev hanya punya satu antibiotik dan satuan laboratorium tanpa `mg/L`/`ug/mL` — sisi penyimpanannya milik `BE-LAB-61` ✅ |
| T14 | `AC-189` profil tanpa set bakteri: bagian isolat dan antibiogram **tidak tampil**, sebabnya dinyatakan | `PASS` | Nol pemeriksaan dev berprofil demikian; jawaban backend diubah di peramban (`usesSusceptibilitySet: false`) |
| T15 | `AC-173` jadwal jaga terisi: hanya dokter jaga (dan DPJP) ditawarkan, **beserta nomor WhatsApp**; nomor kosong dinyatakan; nol keterangan jalur jatuh | `PASS` | Jawaban backend diubah di peramban (`TrxOnCallAssignment` belum punya pengisi, `LAB-COORD-014`). Sesudah perubahan (f) |
| T16 | Memilih dokter jaga mengisi nama konsultan **tanpa** peran maupun nomor | `PASS` | — |
| T17' | `AC-174` ulang dengan jawaban asli: jalur jatuh dinyatakan; nama terisi bersih | `PASS` | Percobaan pertama T17 tidak sah — `page.unroute` skrip uji tidak melepas tiruan T15; diulang dalam skrip terpisah tanpa tiruan |
| T18 | 390 px tanpa gulir horizontal halaman | `PASS` | — |
| T19 | Nol tulis di luar pemeriksaan uji | `PASS` | — |

**AC yang tidak diklaim di sini, dan sebabnya.** `AC-177`, `AC-179`, dan `AC-183` berbunyi tentang
**cetakan** — tata letak cetak dikecualikan dari gelombang ini (`LAB-OPEN-039`). Sisi layarnya berdiri:
pemilih kualifikasi `Definitif`/`Sementara` tersimpan sebagai nilai (M9; `FE-LAB-41` M4), penanda jenis
biakan tersimpan, dan *Petugas Otorisasi* baru tampil sesudah rilis (`FE-LAB-41` M12).

### 9.3 Cacat yang ditemukan uji, dan perbaikannya

| # | Temuan | Perbaikan | Berkas | Commit |
| --- | --- | --- | --- | --- |
| (a) | Bagian Informasi Specimen **dapat disunting** oleh akun tanpa `LabSpecimen : Update`, lalu berakhir `403` | Dikunci beserta sebabnya bila izin tidak ada | `use-lab-microbiology-result-editor.jsx` (`canCorrectSpecimen`), `lab-microbiology-result-panel.jsx`, `lab-microbiology-specimen-section.jsx` (`readOnlyMessage`), `lab-microbiology-result-constants.jsx` (`specimenCorrectionForbidden`) | `d05fb95e0` |
| (b) | Pemilih satuan volume dan kadar menawarkan **seluruh** satuan (GALON, ROL BESAR, …); backend menolaknya `422` (`VAL-110`/`VAL-112`) | Pilihan disaring `isForLaboratory: true` | `lab-microbiology-result-panel.jsx`, `health-service-select-resources.js` (`filterKeys`) | `d05fb95e0` |
| (c) | Kadar tersimpan `0.5` terbaca kembali sebagai `0.5` — titik, bukan koma | Dibaca kembali dengan koma desimal | `use-lab-microbiology-result-editor.jsx` (`readFormFromResult`) | `d05fb95e0` |
| (d) | **Selisih kontrak `r27` 22.2** yang terbuka sejak 2026-09-23: layar mewajibkan S/I/R pada setiap baris, sehingga `AC-186` ("tanpa satu pun ketikan analis") mustahil | **Keputusan pemilik modul 2026-10-06: ikuti `r27`.** Baris **berzona** boleh dikirim tanpa S/I/R — server menghitungnya, atau menolak `422` beserta sebab bila breakpoint belum disetel (`VAL-114`). Baris tanpa zona tetap wajib S/I/R (`AC-165` utuh). Isian *Hasil* tidak lagi bertanda wajib pada baris berzona, dan layar menulis *"dihitung sistem dari breakpoint saat disimpan"* | `lab-microbiology-result-rules.js` (`validateSusceptibilityRow`), `lab-microbiology-result-form.jsx` | belum ter-commit |
| (e) | Sesudah disimpan, S/I/R hasil hitungan menetap di isian; mengubah zona mengirim nilai lama dan server menolaknya sebagai **penimpaan tanpa alasan** (`VAL-113`) | `applySusceptibilityChange`: mengubah zona mengosongkan S/I/R yang **sama dengan hitungan server**; timpaan analis dibiarkan | `lab-microbiology-result-rules.js`, `use-lab-microbiology-result-editor.jsx` | belum ter-commit |
| (f) | `AC-173` menuntut nomor WhatsApp dokter jaga; layar **tidak pernah** menampilkan `whatsAppNumber` walau backend mengirimnya | Label pilihan memuat `WA <nomor>`; dokter jaga tanpa nomor bertanda *"WA belum tercatat"*; nama konsultan diisi dari `fullName`, bukan dari potongan label | `lab-microbiology-completion-bar.jsx` | belum ter-commit |

Uji unit baru untuk (d) dan (e): **5** di `tests/unit/lab-microbiology-result-rules.test.mjs`
(`AC-186` baris berzona lolos dan terkirim `result: null`; zona 0 diserahkan ke server; zona diubah
mengosongkan hitungan; timpaan dibiarkan; ruas lain nol menyentuh interpretasi).

### 9.4 Batas `FE-LAB-32` yang tersisa — kenapa ia TIDAK naik

Ketiga AC-nya (`AC-160`, `AC-162`, `AC-170`) dan DoD-nya (nol tombol menambah Spesifik Specimen) kini
terbukti di layar. Yang menahannya adalah **outcome** task — *"mengoreksi specimen … dan dapat melihat
nilai lamanya"*:

1. **Formulir koreksi mulai kosong** (`EMPTY_SPECIMEN_FORM`), tidak diisi nilai yang tersimpan (K4).
   Petugas tidak melihat nilai sekarang di formulir — hanya di riwayat.
2. **`LabSpecimenResponse` tidak memuat id Spesifik Specimen** (diperiksa pada source backend
   2026-10-06: ada `SpecimenTypeId`, `VolumeAmount`, `VolumeUnitId`, `SpecimenDescription`,
   `PhysicallyReceivedAt`, tetapi nol `DetailTypeIds`). Karena itu layar **tidak dapat** mencentang yang
   sudah tersimpan, dan mencentang satu kotak **mengganti seluruh set** tanpa peringatan.

Butir 2 menuntut **task backend** (ruas `detailTypeIds` pada respons specimen, kontrak `r26` 21.4);
sesudah itu butir 1 dapat ditutup di frontend sekaligus. Risiko sementara dikurangi oleh payload yang
hanya membawa ruas yang disentuh (`buildSpecimenCorrectionPayload`) — ruas yang tidak disentuh tidak
pernah ditimpa — dan oleh riwayat perubahan yang mencatat nilai lama setiap koreksi.

### 9.5 Jejak di database dev dan validasi akhir

**Pemeriksaan BTA `1f3670d7…` (`LAB-RSMMC-000014`)**, keadaan akhir *Draft*: temuan Positif, Definitif,
metode Dilusi; satu isolat *Branhamella catarrhalis* (diuji) dengan baris Ampicillin: zona kosong, S,
kadar `1,25` MILIGRAM. Dibuka kembali satu kali (2026-10-02); konsultasi kepada *"dr. Konsultan Uji
FE-LAB-33"*. **Specimen `14e5794d…`:** dua Spesifik Specimen (*Arterial cord blood specimen*, *Darah
arteri*), volume 2,5 L/jam. Seluruhnya data uji.

| Perintah (2026-10-06, sesudah perubahan (d)–(f)) | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | **2308 lulus, 6 gagal** dari 2314 — keenamnya kegagalan baseline (Hemodialisa ×4, Bank Darah M0, petty cash); nol Laboratorium |
| `npm run lint:errors` | **0 error** |
| `npm run build` | **Hijau** (server BE/FE dimatikan lebih dulu) |

**Nol operasi Git dijalankan.**
