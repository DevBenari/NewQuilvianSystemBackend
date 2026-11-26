# Laporan Perubahan Frontend — `FE-KSK-PO-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-PO-004` |
| Judul | Varian langkah pindai kartu identitas |
| Slice | `EPIC KSK-PO-06` — check-in kiosk frontend (`FR-KSK-PO-051`), gelombang 1, rilis `MVP-2` |
| Roadmap | [online-registration-frontend-roadmap.md](../../../roadmap/online-registration-frontend-roadmap.md) — kartu `FE-KSK-PO-004` |
| Trace | `KSK-DEC-048`, `053`; `CAP-PO-37`; `GATE-PO-14`; `KSK-VAL-PO-050`, `051` |
| Contract version | `NOT APPLICABLE` — frontend murni, tidak memanggil endpoint |
| Wewenang UI | `KSK-PO-FE-004` (gaya kiosk existing). Tampilan kolom terkunci memakai token CSS kiosk; tidak ada keputusan rupa baru |
| Dependency | — (tidak ada prasyarat) |
| Klasifikasi | `LIGHT` — satu komponen view kiosk diberi dua prop opsional, satu aturan CSS |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: komponen langkah pindai kiosk dan CSS module-nya; laporan dan baris status di blueprint kiosk (repository backend) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `5f18e9f04` (`sukmagpV2`) |
| Commit backend yang dijadikan rujukan | `a86001e2` (`sukmagp`), tidak dipakai karena task tanpa backend |
| Tanggal | 2026-11-19 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti |

## 1. Keadaan yang ditemukan di awal

Langkah pindai kartu di kiosk Pasien Baru menampilkan tiga pilihan kartu (KTP, KIA, SIM) dan **semua** isian hasil baca (OCR) sebagai kolom yang dapat diketik pasien, termasuk NIK (`CAP-PO-37`). Itu wajar untuk Pasien Baru biasa, karena pasien boleh membetulkan salah baca. Untuk Check-in Pasien Online, perilaku itu membuat syarat "NIK hasil pindai wajib sama dengan NIK online" (`KSK-DEC-048`) bisa diakali: orang dapat memindai kartu siapa saja lalu mengetik NIK pendaftaran. SIM juga tidak boleh dipakai karena tidak mencantumkan NIK (`KSK-DEC-053`).

## 2. Proses bisnis dari sisi pengguna

Task ini hanya menyiapkan komponen; layarnya dipasang di `FE-KSK-PO-006`. Nanti, saat pasien baru check-in:

1. Kiosk hanya menawarkan **KTP** dan **KIA**. Tombol SIM tidak muncul.
2. Pasien memindai kartu. Kolom NIK terisi dari hasil baca, tampil dengan bingkai putus-putus, dan **tidak dapat diketik**. Arahkan kursor ke kolom itu, keterangan "Diambil dari kartu, tidak dapat diubah" muncul.
3. Bila NIK tidak terbaca, kolomnya ditandai merah dengan isian "Pindai ulang kartu". Pasien memindai ulang, atau ke loket (`KSK-VAL-PO-051`).
4. Kolom lain (nama, alamat, dan seterusnya) tetap dapat dibetulkan seperti di Pasien Baru.

Contoh: Ibu Wati memindai KTP-nya. NIK `3174…0011` terkunci. Ia mencoba menghapus satu digit; nilainya tetap. Ia masih bisa membetulkan ejaan alamat yang salah baca.

Pendaftaran Pasien Baru biasa **tidak berubah sama sekali**, karena ia tidak mengirim kedua prop baru.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `kiosk-new-patient-step-scan-identity.jsx` (satu-satunya pemakai: `kiosk-new-patient-view.jsx:2645`); `kiosk-new-patient-registration.helpers.jsx` (`IDENTITY_DOCUMENT_TYPES`, `IDENTITY_DOCUMENT_OPTIONS`, `getScanOcrEditFieldsByType`); `style/kiosk/registration/kiosk-new-patient-view.module.css` (aturan `.ocrEditableItem`); `src/app/globals.css` (token `--kiosk-*`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-scan-identity.jsx` | Prop opsional `lockedFieldNames` (bawaan larik kosong beku) dan `allowedIdentityTypes` (bawaan `null` = semua). Pilihan kartu disaring bila `allowedIdentityTypes` berisi. Kolom yang terkunci mendapat `readOnly`, `aria-readonly`, `title`, kelas `isOcrFieldLocked`, dan isian "Pindai ulang kartu" bila tidak terbaca |
| `src/style/kiosk/registration/kiosk-new-patient-view.module.css` | Aturan `.ocrEditableItem.isOcrFieldLocked`: latar `var(--kiosk-surface-panel)`, bingkai putus-putus `var(--kiosk-border-strong)`, kursor `not-allowed`. Tanpa warna literal |

### 3.3 Kepatuhan arsitektur frontend

| Butir | Hasil |
| --- | --- |
| Pola yang diikuti | Komponen view kiosk yang ada diperluas lewat props; tidak ada komponen, hook, service, atau state baru |
| Base component | Tidak disentuh |
| Token CSS | Hanya `--kiosk-*` yang sudah ada |
| Perilaku bawaan | Tidak berubah (dibuktikan R1/R1b) |

**Gerbang keputusan base component**

`UI GATE: 2 elemen — REUSE 0, EXTEND 2, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Pilihan jenis kartu terbatas KTP/KIA | Grid tombol kartu di `kiosk-new-patient-step-scan-identity.jsx` | Satu pemakai (`kiosk-new-patient-view.jsx:2645`) | `EXTEND` (komponen view kiosk, bukan base) | Prop opsional `allowedIdentityTypes`; bawaan tidak berubah |
| Kolom OCR terkunci | Kolom `ocrEditableItem` di komponen yang sama | Idem | `EXTEND` (komponen view kiosk, bukan base) | Prop opsional `lockedFieldNames`; bawaan tidak berubah |

Kedua `EXTEND` tidak mengubah perilaku bawaan dan tidak menyentuh base component, sehingga tidak memerlukan keputusan pengguna.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah dari langkah pindai yang ada |
| Kosong | NIK terkunci tidak terbaca → kolom merah, isian "Pindai ulang kartu" |
| Gagal | Tidak berubah dari langkah pindai yang ada |
| Tanpa hak akses | `NOT APPLICABLE` — langkah kiosk tanpa matriks Akses Role |

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — task ini tidak memanggil endpoint.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <komponen>` | 0 error, 1 warning `<img>` yang sudah ada sebelumnya | `PASS` / `EXISTING WARNING` | Keluaran ESLint |
| `npm run lint:errors` | 0 error | `PASS` | Keluaran perintah |
| `npm run test:unit` | `Could not find 'tests/unit/*.test.mjs'` — tidak ada berkas uji unit di repository | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah. `AUTOMATED TEST: SKIPPED (opsional) — tidak ada berkas uji unit; menulis uji baru opsional` |
| `npm run build` | `✓ Compiled successfully`, 854/854 halaman | `PASS` | Keluaran build |
| R1 render tanpa prop baru vs versi `HEAD` | HTML identik (2430 = 2430 karakter, sama byte demi byte) | `PASS` | Harness render SWC + `react-dom/server` di scratchpad, tidak di-commit |
| R1b kolom wajib kosong, tanpa prop baru, vs `HEAD` | HTML identik (2595 = 2595) | `PASS` | Harness |
| R2 `lockedFieldNames={["nik"]}` | Input NIK `readonly`, `aria-readonly="true"`, `title` terpasang | `PASS` | Harness |
| R2b kolom lain | Nama tetap tanpa `readonly` | `PASS` | Harness |
| R2c kelas penanda | Label NIK berkelas `isOcrFieldLocked` | `PASS` | Harness |
| R3 `allowedIdentityTypes={["KTP","KIA"]}` | Tombol hanya KTP, KIA | `PASS` | Harness |
| R4 `allowedIdentityTypes={[]}` | Kembali ke KTP, KIA, SIM | `PASS` | Harness |
| R5 NIK terkunci tidak terbaca | Isian "Pindai ulang kartu" | `PASS` | Harness |
| B1 uji browser: ketik dan hapus di NIK terkunci | Nilai tetap `3174000000000001` | `PASS` | Playwright Chromium pada HTML hasil render |
| B2 uji browser: kolom nama | Dapat diketik (`… X`) | `PASS` | Playwright |
| Grep warna literal pada aturan CSS baru | Tidak ada; hanya `var(--kiosk-…)` | `PASS` | Diff CSS |
| Uji di perangkat kiosk sungguhan | Belum, karena layar pemakainya baru dibuat di `FE-KSK-PO-006` | `NOT RUN` | — |

`MANUAL TEST`: dilakukan lewat uji browser Playwright pada hasil render komponen (B1, B2). Uji pada layar kiosk sungguhan dilakukan bersama `FE-KSK-PO-006`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Dengan `lockedFieldNames={["nik"]}`, kolom NIK tidak dapat diubah | Terpenuhi | R2, B1 |
| (2) Dengan `allowedIdentityTypes={["KTP","KIA"]}`, SIM tidak ditawarkan | Terpenuhi | R3 |
| (3) Kiosk Pasien Baru tanpa prop baru berperilaku sama persis | Terpenuhi | R1, R1b (HTML identik dengan `HEAD`) |
| DoD: kriteria 1–3 terbukti | Terpenuhi | Bagian 6 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Warning `<img>` di baris 240 komponen sudah ada sebelum task ini; tidak disentuh |
| Masalah yang diketahui | `npm run test:unit` gagal mencari berkas karena repository belum punya uji unit (kondisi lama) |
| Dependency backend | Tidak ada. Pemakai prop baru adalah `FE-KSK-PO-006` (menunggu `BE-KSK-PO-007`) |
| Perubahan sampingan | `NONE` — harness dan berkas render berada di scratchpad; build Next.js hanya memperbarui `.next/` yang tidak dilacak |
| Interupsi | `NONE` |
| Status Git | `M src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-scan-identity.jsx`, `M src/style/kiosk/registration/kiosk-new-patient-view.module.css` |
| Langkah berikutnya | `BE-KSK-PO-001` (skema; meminta konfirmasi sebelum membuat migration) |
