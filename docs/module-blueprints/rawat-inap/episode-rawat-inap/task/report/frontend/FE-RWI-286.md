# Laporan Perubahan Frontend — `FE-RWI-286`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-286` |
| Judul | Rencana kontrol terstruktur pada layar resume medis |
| Slice | Repair 043 episode rawat inap; Slice 2 [MVP Penuntasan Rawat Inap](../../../../MVP-Penuntasan-Rawat-Inap.md) |
| Roadmap | [roadmap-repair-043.md](../../../roadmap/roadmap-repair-043.md), kartu `FE-RWI-286` |
| Trace | `FIX-EPS-043-02` (`ISS-EPS-043-02`); rangka plan repair 043 bagian 3.B |
| Contract version | Delta `BE-RWI-256` ✅ pada source backend `53418d37` (`followUpDate`, `followUpServiceUnitId`, `followUpDoctorId`, nama poli/dokter) |
| Wewenang UI | Rangka 3.B plan repair 043 (disetujui 2026-11-02) |
| Dependency | `BE-RWI-256` ✅ 9 November 2026 |
| Klasifikasi | `MEDIUM` — form, detail, cetakan, dan validasi resume |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `HamzahV2` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | Source utama di `66b7e5600`; pemetaan galat server ke isian (sesi ini) di-commit pemilik `6053e347c`; perbaikan gerbang CSS di working tree |
| Commit backend yang dijadikan rujukan | `53418d372b6e37fa76aeabea9cf3b395c5534641` |
| Tanggal | 9 November 2026 |
| Status | 🟡 Sebagian — AC 4 terbukti di layar; AC 2 dan AC 3 terbukti di server dan unit test; AC 1, AC 5, dan tampilan form edit belum terlihat di peramban karena butuh akun DPJP |

---

## 1. Keadaan yang ditemukan di awal

Form, detail, cetakan, opsi poli/dokter (`useDischargeFollowUpOptions`), dan validasi sisi layar sudah ada
di `66b7e5600`. Satu celah ditemukan: penolakan server, misalnya "Tanggal kontrol tidak boleh sebelum
tanggal keputusan pulang.", hanya tampil sebagai galat umum, sehingga isian tanggal tidak ditandai
(AC 2).

Perbedaan dengan rangka 3.B: layar tidak memakai radio "Tidak perlu kontrol / Kontrol ke poliklinik".
Ketiga isian langsung tampil dengan keterangan "Kosongkan bila pasien tidak perlu kontrol ke
poliklinik." Pilihan radio itu juga tidak disimpan server (`BE-RWI-256` hanya punya tiga kolom). Bentuk
ini lebih sederhana dan tetap memenuhi inti AC 3: tanggal yang diisi tanpa poli ditandai wajib.

---

## 2. Proses bisnis dari sisi pengguna

1. DPJP membuka Detail Episode → Resume Pulang setelah menyatakan pasien boleh pulang.
2. Pada bagian 7 "Rencana Kontrol", DPJP mengisi tanggal kontrol (kalender tidak bisa memilih tanggal sebelum keputusan pulang), poliklinik, dokter (opsional, disaring menurut poli), dan catatan.
3. Sebelum dikirim, layar menahan isian yang tidak lengkap. Contoh: tanggal 16 November tanpa poli → isian poli ditandai "Poliklinik kontrol wajib dipilih bila tanggal kontrol diisi."
4. Bila server menolak, pesannya ditempel pada isian yang bersangkutan. Contoh: tanggal 1 November padahal keputusan pulang 2 November → isian tanggal ditandai.
5. Detail dan cetakan resume menampilkan "Jadwal Kontrol: Senin, 9 November 2026 — Poli Penyakit Dalam — dr. F".

Jalur tidak normal: resume lama yang hanya berisi teks tetap terbuka; "Jadwal Kontrol" tampil "-" dan teks lama tampil di "Catatan Tindak Lanjut". Resume yang sudah ditandatangani hanya dapat diubah supervisor lewat sesi koreksi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`inpatient-discharge-view.jsx`, `inpatient-discharge-utils.jsx`, `use-inpatient-discharge.jsx`, `use-discharge-follow-up-options`, `tests/unit/inpatient-discharge-follow-up.test.mjs`; backend `InpDischargeService.ValidateFollowUpPlanAsync`, `InpatientDischargeController` (`PUT {episodeId}/summary`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-discharge-utils.jsx` | `resolveFollowUpServerFieldErrors(message)`: pesan server berawalan "Tanggal kontrol" / "Poliklinik kontrol" / "Dokter kontrol" dipetakan ke isian masing-masing |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-discharge.jsx` | Saat simpan resume gagal, galat isian dari server ikut dipasang ke `summaryErrors` |
| `tests/unit/inpatient-discharge-follow-up.test.mjs` | Test AC-2 pemetaan pesan server |
| `src/components/view/health-services/inpatient-management/inpatient-discharge-view.jsx` | Gerbang CSS: `variant="outline"` → `secondary`; kelas status simpan dihitung di luar JSX |

### 3.3 Kepatuhan arsitektur frontend

Logika murni di `utils`, dipakai hook; view tidak berubah perilakunya. Tidak ada komponen baru.

### 3.4 Gerbang tampilan

**Modul referensi visual:** form resume pulang itu sendiri.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Penanda galat isian | `error` pada field `BaseFormControl` yang sudah ada | `REUSE` | — |
| Tombol tarik data usulan | `BaseButton variant="secondary"` | `REUSE` | Mengganti `outline` yang tidak sah |

`UI GATE: PASS — 2 elemen REUSE, 0 COMPOSE, 0 NEW`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs ... inpatient-discharge-view.jsx
OK — 6 berkas diperiksa, 0 temuan.
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| Resume lama (baca-saja) | `test-with-agy/screenshots/rawat-inap-mvp-1109/07-resume-kontrol/resume-lama-rencana-kontrol.png` | "Jadwal Kontrol: -", catatan "TEST" utuh |
| Form edit rencana kontrol | — | `VISUAL CHECK: NOT FEASIBLE` — akun uji superadmin bukan DPJP sehingga form tampil baca-saja; kata sandi akun DPJP (`rendi@admin.com`, dr. Rendy Pangalila, DPJP dua episode `DischargePending`) tidak tersedia di manifest uji |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka form seperti sekarang |
| Kosong | Resume lama: "Jadwal Kontrol" "-" |
| Gagal | Pesan galat umum + isian yang bersangkutan ditandai |
| Tanpa hak akses | Form baca-saja |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Discharge

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/v1/health-services/inpatient-management/discharges/{episodeId}/summary` | Simpan resume beserta rencana kontrol | Hak akses resume yang ada |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors`, `npm run build` | exit 0 | `PASS` | Run MVP |
| `npm run test:unit` | 4144 lulus / 56 gagal lama; `inpatient-discharge-follow-up.test.mjs` termasuk test AC-2 baru lulus | `PASS` untuk berkas terkait | `node --test` terfokus 23/23 |
| AC 1 — simpan tanggal/poli/dokter, tampil di detail dan cetakan | Tidak dapat dijalankan tanpa akun DPJP | `NOT RUN` | — |
| AC 2 — server menolak tanggal sebelum keputusan pulang | `PUT` → 400 "Tanggal kontrol tidak boleh sebelum tanggal keputusan pulang."; penandaan isian dibuktikan unit test | `PASS` (server + unit) | `07-resume-kontrol/network.log` |
| AC 3 — tanggal tanpa poli ditahan | `PUT` → 400 "Poliklinik kontrol wajib dipilih bila tanggal kontrol diisi."; validasi layar `validateDischargeSummaryForm` | `PASS` (server + unit) | Network log |
| AC 4 — resume lama terbuka tanpa galat | Tanpa galat halaman; "Jadwal Kontrol: -", catatan "TEST" | `PASS` | `resume-lama-rencana-kontrol.png` |
| AC 5 — revisi menyimpan nilai sebelum perubahan | Butuh sesi koreksi supervisor pada resume bertanda tangan | `NOT RUN` | Penelusuran source `BE-RWI-256` |

`AUTOMATED TEST: npm run test:unit — FAIL (56 kegagalan lama di luar cakupan; 0 kegagalan baru)`

`MANUAL TEST: NOT FEASIBLE` untuk AC 1, AC 5, dan form edit — butuh akun DPJP.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 1 | Belum terbukti di layar (source ada) | Bagian 6 |
| AC 2 | Terpenuhi (server + unit); penandaan di layar belum terlihat | Bagian 6 |
| AC 3 | Terpenuhi (server + validasi layar di unit) | Bagian 6 |
| AC 4 | Terpenuhi | Bagian 6 |
| AC 5 | Belum terbukti di layar (source ada) | Bagian 6 |
| DoD lint, unit, build, laporan | Terpenuhi | Bagian 6, berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | — |
| Masalah yang diketahui | Pilihan "Tidak perlu kontrol" tidak dibedakan dari "belum diisi" (keterbatasan kontrak `BE-RWI-256`) |
| Dependency backend | `BE-RWI-256` ✅ |
| Perubahan sampingan | `NONE`. Uji `PUT` ditolak validasi sebelum pemeriksaan wewenang, jadi tidak ada data resume yang berubah |
| Interupsi | `NONE` |
| Status Git | Berubah (belum di-commit): `inpatient-discharge-view.jsx` (gerbang CSS) |
| Langkah berikutnya | Uji AC 1 dan AC 5 dengan akun DPJP; lalu naikkan status ke ✅ |
