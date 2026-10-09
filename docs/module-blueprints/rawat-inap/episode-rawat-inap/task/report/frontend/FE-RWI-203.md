# Laporan Perubahan Frontend — `FE-RWI-203`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-203` |
| Judul | Panel scan KTP admisi: tombol "Input Manual" yang tidak berfungsi disembunyikan, pesan galat scanner berbahasa Indonesia |
| Slice | Perbaikan defect pasca-pengujian; bukan slice fitur baru |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/frontend-roadmap-v2.md` |
| Trace | `ISSUE-EPS-003` butir `ISS-EPS-003-01`, `ISS-EPS-003-T2`; `PLAN-REPAIR-EPS-003` perbaikan `FIX-EPS-003-01`, `FIX-EPS-003-08`; skema tampilan 3.3 |
| Contract version | `0.10.0` — tidak disentuh. Task ini tidak mengubah panggilan backend mana pun |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — source; laporan dan tautan bukti di `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Wewenang UI | Skema tampilan 3.3 yang sudah ada (tanpa tombol Input Manual, dengan kalimat isi manual) dan `PLAN-REPAIR-EPS-003` yang diperintahkan untuk diimplementasikan pemilik 6 Oktober 2026 |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `b010ffb9722f236160477c95c931c22467550200` (`HamzahV2`), perubahan belum di-commit |
| Tanggal | 6 Oktober 2026 |
| Status | Selesai. Verifikasi manual di peramban `NOT RUN` |

---

## 1. Masalah yang diperbaiki

Saat pemindai belum siap, panel scan pada Langkah 2 admisi rawat inap menampilkan tombol "Input
Manual". Menekannya tidak berbuat apa-apa: panel memanggil `onUseManual`, sedangkan admisi rawat inap
tidak pernah mengirim handler itu. Tombol tersebut lahir untuk alur yang menyembunyikan formulir sampai
petugas memilih isi manual; di admisi rawat inap formulirnya selalu tampil.

Pada panel yang sama, kegagalan menghubungi Plustek Scanner Agent tampil sebagai pesan teknis peramban
*"Failed to fetch"*.

## 2. Proses bisnis

1. Petugas admisi membuka Langkah 2 Pendaftaran Pasien Baru.
2. Bila Plustek Scanner Agent tidak berjalan, panel kini menyebut masalahnya dalam Bahasa Indonesia
   beserta langkah perbaikannya, dan menambahkan kalimat *"Pemindai tidak tersedia? Isi formulir di
   bawah secara manual."*
3. Petugas langsung mengisi formulir di bawah panel, atau memperbaiki agent lalu menekan Cek Scanner.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/registration-management/emergency-registration/plustek-scan-panel.jsx` | Tombol "Input Manual" hanya dirender bila `onUseManual` berupa fungsi — penjaga yang sama dengan "Cek Scanner" dan "Hapus Hasil". Prop baru opsional `manualFallbackHint` dirender sebagai satu baris petunjuk saat pemindai belum siap |
| `src/lib/hooks/health-services/registration-management/emergency-registration/use-plustek-ktp-scanner.js` | `getErrorMessage` mengenali kegagalan jaringan (`TypeError`, "Failed to fetch", "NetworkError", "Load failed") yang tidak membawa `status`, lalu mengembalikan kalimat *"Plustek Scanner Agent tidak dapat diakses. Pastikan aplikasi agent berjalan di komputer ini, lalu tekan Cek Scanner."* Galat HTTP dari agent tetap memakai pesannya sendiri |
| `src/components/view/health-services/inpatient-management/inpatient-admission-registration-step.jsx` | Mengirim `manualFallbackHint` ke panel |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Konstanta `INPATIENT_PATIENT_SCANNER_MANUAL_HINT` |

### 3.2 Radius dampak

| Pemakai | Dampak |
| --- | --- |
| `plustek-scan-panel.jsx` | Hanya dipakai admisi rawat inap. Pendaftaran IGD punya panel scan sendiri di `patient-selection-step.jsx` |
| `use-plustek-ktp-scanner.js` | Dipakai admisi rawat inap dan pendaftaran IGD. Pada IGD, kegagalan jaringan ke agent kini juga tampil sebagai kalimat Bahasa Indonesia — perubahan yang menguntungkan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Tabel keputusan base component

| Elemen | Status | Bukti |
| --- | --- | --- |
| Tombol Cek Scanner dan Scan eKTP | `REUSE` | Tidak berubah |
| Tombol Input Manual | Disembunyikan bila tidak ada handler | Tidak ada komponen baru |
| Kalimat petunjuk isi manual | `REUSE` | Kelas `stepActionHint` yang sudah ada pada `emergency-registration.module.css`; tidak ada CSS baru |
| Pesan galat scanner | `REUSE` | `EmergencyInlineAlert` yang sudah dipakai panel |

`UI GATE: PASS` — seluruh elemen `REUSE`, tidak ada komponen maupun CSS baru.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` pada berkas yang diubah | 0 error. Dua warning pada `use-plustek-ktp-scanner.js` (`react-hooks/set-state-in-effect`, `react-hooks/exhaustive-deps` pada efek `autoCheck`) **identik** dengan versi `HEAD` | `PASS` / `EXISTING WARNING` |
| `npx eslint src --quiet` (pengganti `npm run lint:errors` yang gagal oleh sebab lingkungan) | Exit `0` | `PASS` |
| `npm run build` | Exit `0`; kompilasi berhasil; 474 halaman; `postbuild` — *"Standalone runtime siap dijalankan"* | `PASS` |
| Grep anti-regresi UI pada baris baru | Nol nilai warna/ukuran literal, nol `!important`, nol `<button>` mentah | `PASS` |
| Tombol Input Manual tidak tampil saat agent mati | Dibuktikan dari source: syarat render kini `shouldShowManualAction && canUseManual`, dan admisi tidak mengirim `onUseManual` | `PASS` (source) |
| Uji peramban dengan agent dimatikan lalu dihidupkan | Belum dijalankan | `NOT RUN` |

`MANUAL TEST: NOT RUN` — verifikasi peramban tidak dijalankan. Butir DoD uji manual dikecualikan atas
keputusan pemilik 1 September 2026 dan 10 September 2026; bukti source, lint, dan build memadai.

`AUTOMATED TEST: SKIPPED (opsional) — perilaku tampilan; tidak ada test baru untuk task ini`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Agent mati: hanya "Cek Scanner" dan "Scan eKTP" tampil; "Input Manual" tidak ada | Terpenuhi | `plustek-scan-panel.jsx` — `canUseManual` |
| Kalimat "Pemindai tidak tersedia? Isi formulir di bawah secara manual." tampil saat pemindai belum siap | Terpenuhi | `manualFallbackHint` dirender bila `shouldShowManualAction` |
| Pemindai siap: tampilan tidak berubah | Terpenuhi | Kalimat dan tombol hanya bergantung pada `shouldShowManualAction` |
| Pemakai yang memberi `onUseManual` tetap melihat tombolnya | Terpenuhi | Penjaga berbasis `typeof onUseManual === "function"` |
| Agent mati: tidak ada lagi "Failed to fetch" | Terpenuhi | `isNetworkError` + `AGENT_UNREACHABLE_MESSAGE` |
| Pesan dari agent sendiri tetap tampil apa adanya | Terpenuhi | Galat ber-`status` tidak dicegat |
| Lint dan build hijau | Terpenuhi | Bagian 5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan di luar cakupan | Panel membaca `scanner.notice`, `scanner.hasIdentityData`, dan `scanner.ocrReady`, tetapi hook tidak pernah mengembalikan ketiganya. Akibatnya peringatan OCR kosong dari hook (`warning`) tidak pernah tampil di panel. Dicatat sebagai technical debt; tidak diperbaiki di sini |
| Risiko tersisa | Verifikasi peramban belum dijalankan |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat laporan `FE-RWI-207` bagian 7 untuk daftar lengkap berkas `PLAN-REPAIR-EPS-003`. Perubahan lain pada working tree — enam berkas `nursing-workspace` — bukan milik task ini dan tidak disentuh |
