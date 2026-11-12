# PLAN-REPAIR-007 — Tampilkan Detail SOAP Selesai, Pulihkan Tombol Kembali/Catatan Baru, dan Tandai Draf di Riwayat

```yaml
plan_id: PLAN-REPAIR-IGD-007
issue: ../issue/issue-007-detail-soap-kosong-penanda-draf.md
status_rencana: DISETUJUI_SEBAGIAN
tanggal_rencana: "2026-11-04"
diputuskan_oleh: "Rizki Gunawan (lisan): IGD tanpa draf; pindah segmen menyegarkan; FIX-01 dikerjakan. FIX-02 tidak diteruskan; FIX-03 menunggu K-02"
tanggal_keputusan: "2026-11-04"
basis_source_backend: "30f1396d (rizkiG)"
basis_source_frontend: "739e05f68 (RizkiV2) + perubahan belum di-commit FIX-IGD-004/005/006"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-IGD-007-01` | `ISS-IGD-007-01`, `-02` | Props `ProgressNoteDetail` dan `SoapHistoryPanel` dibetulkan; bilah aksi "Kembali ke Riwayat" + "Catatan Baru"; **pindah segmen menyegarkan** (Riwayat dimuat ulang; kembali ke Form dari catatan selesai membuka form baru) | FE (milik IGD) | 1 | — | 🟡 SEBAGIAN — AC 1–5 terverifikasi di browser; laporan task tracked belum ada | [laporan uji §10](../../testing/test-by-agy/laporan-live-testing-soap-dokter-igd.md) |
| `FIX-IGD-007-02` | `ISS-IGD-007-03` | Penanda status berwarna + teks pada kartu Riwayat SOAP | FE (komponen bersama Rawat Inap) | — | — | DIBATALKAN — digantikan keputusan "tanpa draf" (`FIX-IGD-007-04`); draf lama tetap berlabel "Sedang ditulis" | — |
| `FIX-IGD-007-03` | `ISSUE-IGD-006-T4` | Tombol Koreksi (izin `isAllowed`), props modal Koreksi dan Salin, payload addendum `{ addendumText, correctionReason }` | FE (milik IGD) | 2 | — | ⛔ TERBLOKIR — menunggu K-02 | — |
| `FIX-IGD-007-04` | Keputusan pemilik "input langsung berhasil tanpa draf"; `ISS-IGD-007-T1`, `-T2` | Gerbang kelengkapan benar-benar menahan sebelum ada yang dikirim (`issues.length`, bukan `issues.hasErrors`); kekurangan tampil langsung tanpa konfirmasi; pesan penolakan server ditampilkan lewat `saveError`; kata "draf" dihapus dari teks layar IGD | FE (milik IGD) | 1 | — | 🟡 SEBAGIAN — AC terverifikasi di browser (T1 + regresi B1–B5); laporan task tracked belum ada | [laporan uji §10](../../testing/test-by-agy/laporan-live-testing-soap-dokter-igd.md) |

**Ringkasan: 0 dari 4 perbaikan selesai; 2 sebagian, 1 dibatalkan, 1 terblokir K-02.**

**Perilaku "tanpa draf" yang disepakati.** Menekan **Simpan Catatan CPPT**:
(1) bila S/O/A/P atau diagnosis belum lengkap → tidak ada yang dikirim ke server, kekurangan tampil
merah, isian tetap di layar; (2) bila lengkap → konfirmasi → tersimpan, ditandatangani, dan masuk
CPPT. Draf hanya mungkin tertinggal bila server menolak **setelah** catatan dibuat karena aturan yang
tidak dicerminkan layar (misalnya validasi resep). Dalam keadaan itu catatan tetap terbuka di form,
pesan "Catatan belum masuk CPPT…" tampil, dan menekan tombol lagi menyelesaikan catatan yang sama.

---

## 2. Solusi terpilih per temuan

### ISS-IGD-007-01/-02 — Detail kosong dan tanpa jalan keluar

Hanya ada satu cara yang wajar: samakan pemanggilan dengan Rawat Inap, seluruhnya di berkas IGD.
- `<ProgressNoteDetail detail={detail} isLockedUnsigned={isLockedUnsigned} />`.
- Tambah `ClinicalCompletionBar` di bawah Riwayat Koreksi, dengan meta "Dokumen terkunci sejak …"
  dan tombol **Kembali ke Riwayat** (sekunder) serta **Catatan Baru** (utama, bila boleh menulis).
- `SoapHistoryPanel`: `onStartNewNote={canCreate ? startNewNote : undefined}`,
  `onRefresh={refreshTimeline}`, `loading={timelineState.loading}`. Props yang tidak dikenal
  dibuang.

### ISS-IGD-007-03 — Penanda draf

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Prop opt-in `statusTint` pada `SoapHistoryPanel`: latar tipis + garis kiri per status, plus teks "Draf — belum masuk CPPT…" untuk draf | Jelas; Rawat Inap tidak berubah selama prop tidak dinyalakan | Mengubah berkas milik Rawat Inap (perlu K-01) |
| B | CSS khusus IGD yang menargetkan kartu lewat selektor `:has()` pada badge | Tanpa menyentuh berkas Rawat Inap | Rapuh: bergantung pada struktur dalam komponen lain |
| C | Biarkan, hanya perbesar label | Paling kecil | Tidak menjawab usulan pelapor |

**Solusi terpilih: Opsi A** (setelah K-01).

---

## 3. Skema tampilan sebelum → sesudah

### 3.1 Catatan selesai dibuka dari Riwayat

**SEBELUM** (lampiran 1):

```text
[ Form SOAP ]  Riwayat SOAP (1)  CPPT Pasien
Riwayat Koreksi                                   0 Koreksi
  ✓ Belum ada koreksi pada catatan ini.
(tidak ada isi catatan, tidak ada tombol)
```

**SESUDAH:**

```text
[ Form SOAP ]  Riwayat SOAP (1)  CPPT Pasien
Dokumen asli                                   CNS-20261104-0007
  dr. Contoh A · 4 Nov 2026 14.31 · Selesai
  S  Keluhan Utama: …        O  Tekanan Darah: 112/72 mmHg …
  A  1. A02.0 - Salmonella enteritis (Diagnosa Utama) …
  P  …
Riwayat Koreksi                                   0 Koreksi
  ✓ Belum ada koreksi pada catatan ini.
────────────────────────────────────────────────────────────────
Dokumen terkunci sejak 4 Nov 2026 14.31.   [ Kembali ke Riwayat ] [ Catatan Baru ]
```

| Tombol | Jenis | Kapan tampil | Yang terjadi |
| --- | --- | --- | --- |
| Kembali ke Riwayat | Sekunder | Selalu | Pindah ke segmen Riwayat SOAP |
| Catatan Baru | Utama | Dokter boleh menulis | Draf baru di Form SOAP (S/O terisi dari perawat bila ada) |
| Koreksi | Sekunder | Setelah `FIX-IGD-007-03`, bila server mengizinkan | Membuka modal koreksi beralasan |

### 3.2 Kartu Riwayat SOAP (setelah K-01)

```text
┃ 14.14  SOAP #1  [Sedang ditulis]                 ← latar kuning tipis, garis kiri kuning
┃ dr. Contoh A
┃ ⚠ Draf — belum masuk CPPT. Buka lalu tekan Simpan Catatan CPPT.        [ Buka ]

┃ 14.31  SOAP #2  [Selesai]                        ← latar hijau tipis, garis kiri hijau
┃ dr. Contoh A                                                            [ Buka ]
```

| Status | Warna | Teks tambahan |
| --- | --- | --- |
| Draf / Sedang ditulis | Kuning lembut (token peringatan) | "Draf — belum masuk CPPT. Buka lalu tekan Simpan Catatan CPPT." |
| Selesai | Hijau lembut (token sukses) | — (label "Selesai" sudah cukup) |
| Dibatalkan / Terkunci tanpa TTD | Abu-abu / merah lembut | — |

Warna memakai token desain yang sudah ada, bukan nilai heksadesimal baru.

---

## 4. Rincian perbaikan

### FIX-IGD-007-01 — Detail tampil dan jalan keluar pulih

| | |
| --- | --- |
| **Berkas** | `emergency-doctor-progress-tab.jsx` saja |
| **Radius dampak** | Tab Catatan Dokter IGD |
| **Bergantung pada** | Tidak ada |

**Acceptance criteria.**

1. Riwayat SOAP → **Buka** pada catatan selesai → tampil "Dokumen asli" berisi S/O/A/P dan diagnosis
   catatan itu, lalu Riwayat Koreksi, lalu bilah aksi.
2. **Kembali ke Riwayat** → segmen Riwayat SOAP aktif.
3. **Catatan Baru** → draf baru di Form SOAP, tanpa memuat ulang halaman.
4. Di Riwayat SOAP, **Buat SOAP Baru** membuka draf baru, dan **Refresh** memuat ulang daftar.
5. Tidak ada galat halaman.

### FIX-IGD-007-02 — Penanda status di Riwayat SOAP

| | |
| --- | --- |
| **Berkas** | `soap-history-panel.jsx` (prop opt-in `statusTint`, bawaan `false`), stylesheet `physician-progress-note.module.css` (kelas baru), `emergency-doctor-progress-tab.jsx` (menyalakan prop) |
| **Radius dampak** | Kartu Riwayat SOAP IGD. Rawat Inap tidak berubah selama prop tidak dinyalakan |
| **Bergantung pada** | K-01 |

**Acceptance criteria.**

1. Draf tampil berlatar kuning lembut dengan garis kiri kuning dan teks "Draf — belum masuk CPPT…".
2. Catatan selesai tampil berlatar hijau lembut dengan garis kiri hijau.
3. Kartu Riwayat SOAP Rawat Inap tidak berubah.
4. Teks tetap terbaca (kontras cukup) dalam tema terang.

### FIX-IGD-007-03 — Koreksi (addendum) dan Salin berfungsi

| | |
| --- | --- |
| **Berkas** | `emergency-doctor-progress-tab.jsx` (tombol Koreksi dengan `addendumAuthority?.isAllowed`; props `CorrectionModal` dan `ConfirmModal` mengikuti Rawat Inap); `use-emergency-progress-note.jsx` (`submitAddendum` menerima dan mengirim `{ addendumText, correctionReason }`) |
| **Radius dampak** | Tab Catatan Dokter IGD |
| **Bergantung pada** | K-02, `FIX-IGD-007-01` |

**Acceptance criteria.**

1. Catatan selesai milik dokter yang masuk → tombol **Koreksi** tampil → modal terbuka → isi koreksi dan
   alasan → tersimpan → Riwayat Koreksi menjadi "1 Koreksi".
2. "Salin dari catatan sebelumnya" pada draf yang Assessment/Plan-nya sudah terisi → modal konfirmasi
   terbuka → Salin dan Timpa → isi tersalin.

**Verifikasi ketiga perbaikan.** `npx eslint` berkas yang diubah; uji browser akun Dokter IGD pada
pasien uji K1-C (sudah punya beberapa catatan selesai).

---

## 5. Urutan pengerjaan

```text
FIX-IGD-007-01   tanpa dependency — kerjakan pertama
├── FIX-IGD-007-03   juga menunggu K-02
K-01 (pemilik Rawat Inap)
└── FIX-IGD-007-02
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Rekomendasi | Menahan |
| ---: | --- | --- | --- |
| K-01 | Izin prop opt-in penanda warna di `soap-history-panel.jsx` (milik Rawat Inap) | Setujui opt-in | `FIX-IGD-007-02` |
| K-02 | Sertakan perbaikan Koreksi/Salin (`ISSUE-IGD-006-T4`) sekarang | Ya | `FIX-IGD-007-03` |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Perubahan |
| --- | --- | --- |
| `03-frontend-architecture.md` | §15, baris tab Catatan Dokter | Tambah: "Riwayat SOAP menandai status dengan warna + teks; draf disertai keterangan belum masuk CPPT" — setelah K-01 |

---

## 8. Verifikasi menyeluruh

1. Buka catatan selesai → detail tampil → Kembali ke Riwayat → Buat SOAP Baru → draf baru.
2. Catatan selesai → Catatan Baru → draf baru tanpa refresh.
3. (Setelah K-01) Kartu draf kuning + teks; kartu selesai hijau.
4. (Setelah K-02) Koreksi tersimpan dan tampil di Riwayat Koreksi.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-04 | Rencana dibuat, 3 perbaikan, menunggu persetujuan | `diagnose-module-issue` |
| 2026-11-04 | Keputusan pemilik: "input langsung berhasil tanpa draf" dan "pindah tab maka refresh". `FIX-01` diperluas dengan penyegaran saat pindah segmen; `FIX-02` dibatalkan; `FIX-04` ditambahkan. Dikerjakan pada `emergency-doctor-progress-tab.jsx` dan `use-emergency-progress-note.jsx`; ESLint 0 masalah. Uji: 7/7 lulus tanpa data baru; regresi jalur sukses satu tombol 5/5 (menambah 1 catatan selesai + 1 CPPT pada K1-C) | Claude Code |
