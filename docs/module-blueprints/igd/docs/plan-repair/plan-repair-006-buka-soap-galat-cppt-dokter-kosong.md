# PLAN-REPAIR-006 — Perbaiki Tombol Buka/Reset SOAP Dokter IGD, Tambah Selesaikan, dan Salin SOAP ke CPPT

```yaml
plan_id: PLAN-REPAIR-IGD-006
issue: ../issue/issue-006-buka-soap-galat-cppt-dokter-kosong.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-11-04"
diputuskan_oleh: "Rizki Gunawan (lisan di sesi kerja: \"setuju kerjakan sekarang\"), termasuk K-01 = saat diselesaikan"
tanggal_keputusan: "2026-11-04"
basis_source_backend: "30f1396d (rizkiG)"
basis_source_frontend: "739e05f68 (RizkiV2) + perubahan belum di-commit FIX-IGD-004/005"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-IGD-006-01` | `ISS-IGD-006-01` | Teruskan id ke `selectNote`; ganti `resetDraftDiagnoses` dengan `discardPending` | FE | 1 | — | 🟡 SEBAGIAN — AC 1–3 dari 4 terverifikasi di browser (AC 4 "Catatan baru" di riwayat tidak diuji); laporan task tracked belum ada | [laporan uji §9](../../testing/test-by-agy/laporan-live-testing-soap-dokter-igd.md) |
| `FIX-IGD-006-02` | `ISS-IGD-006-02`, `ISSUE-IGD-004-T2` (sisi IGD), `ISS-IGD-006-T1`, `-T2` | Tombol **Selesaikan Catatan**; props modal Selesaikan dibetulkan; `persistPending` pada simpan/selesaikan draf lama | FE | 2 | — | 🟡 SEBAGIAN — AC 1–3 dari 3 terverifikasi di browser; laporan task tracked belum ada | [laporan uji §9](../../testing/test-by-agy/laporan-live-testing-soap-dokter-igd.md) |
| `FIX-IGD-006-03` | `ISS-IGD-006-02` | Setelah selesai, salin SOAP ke CPPT (pola Rawat Jalan) lalu muat ulang CPPT | FE | 2 | — | 🟡 SEBAGIAN — AC 1 dan 3 (jalur sukses) terverifikasi; AC 2 (duplikat diabaikan) dan jalur gagal belum diuji; laporan task tracked belum ada | [laporan uji §9](../../testing/test-by-agy/laporan-live-testing-soap-dokter-igd.md) |

| `FIX-IGD-006-04` | Keputusan pemilik 4 Nov 2026 sore: "satu tombol saja" | IGD hanya punya **satu** tombol simpan: "Simpan Catatan CPPT" langsung menyimpan, menandatangani, dan memasukkan catatan ke CPPT setelah konfirmasi; bilah "Selesaikan Catatan"/"Simpan & Selesaikan" dihapus | FE | 1 | — | 🟡 SEBAGIAN — AC 1–4 terverifikasi di browser (draf baru dan draf lama); laporan task tracked belum ada | [laporan uji §9.3](../../testing/test-by-agy/laporan-live-testing-soap-dokter-igd.md) |

**Ringkasan: 0 dari 4 perbaikan selesai; keempatnya sebagian (lolos uji browser pada jalur utama, menunggu laporan task tracked dan commit).**

**Acceptance criteria `FIX-IGD-006-04`.** (1) Tab Catatan Dokter IGD hanya menampilkan satu tombol
simpan, "Simpan Catatan CPPT", di samping Reset. (2) Menekannya membuka konfirmasi "Catatan akan
disimpan, ditandatangani, dan langsung tampil di CPPT pasien…". (3) Pada draf baru tanpa simpan
sebelumnya: dibuat → diagnosis tersimpan → selesai → tersalin ke CPPT. (4) Pada draf lama yang dibuka
ulang: perubahan tersimpan → selesai → tersalin ke CPPT.

**Konsekuensi yang disepakati.** Tidak ada lagi tombol simpan draf di IGD. Draf hanya terbentuk bila
penyelesaian ditolak, misalnya karena diagnosis belum dipilih. Isi tetap tersimpan sebagai draf dan
dapat dibuka lagi lewat Riwayat.

---

## 2. Solusi terpilih per temuan

### ISS-IGD-006-01 — Galat Buka/Reset/Catatan baru

Hanya ada satu cara yang wajar: samakan dengan Rawat Inap.
- `emergency-doctor-progress-tab.jsx`: `onSelectNote={(id) => { selectNote(id); setActiveSubTab("form"); }}`.
- `use-emergency-progress-note.jsx`: `diagnosisApi.resetDraftDiagnoses()` → `diagnosisApi.discardPending()`.

Kedua berkas milik IGD. Komponen bersama Rawat Inap tidak disentuh.

### ISS-IGD-006-02 — SOAP tidak masuk CPPT

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Frontend IGD: tombol Selesaikan → `completeNote` → `POST …/from-consultation/{id}` (`ignoreDuplicate`) → muat ulang CPPT | Meniru Rawat Jalan; tanpa perubahan backend; berkas milik IGD | Bila penyalinan gagal setelah selesai, CPPT tertinggal (perlu pesan dan coba ulang) |
| B | Backend: finalisasi konsultasi IGD langsung membuat baris CPPT | Tidak bergantung layar | Mengubah layanan finalisasi bersama milik ClinicalManagement/Farmasi; perlu build backend dan persetujuan pemilik |
| C | CPPT dokter membaca langsung timeline konsultasi dokter | Tanpa salinan | Mengubah kontrak CPPT; draf ikut tampil |

**Solusi terpilih: Opsi A** (setelah K-01), karena paling dekat dengan pola yang sudah berjalan
(`useDoctorConsultationWorkspace.js:260-273`, `:326`) dan tidak menyentuh backend.

**Tombol Selesaikan** ditempatkan di tab IGD (`emergency-doctor-progress-tab.jsx`), bukan di
`soap-editor.jsx` milik Rawat Inap. Dengan begitu perbaikan tidak butuh izin pemilik Rawat Inap.
Tab IGD sudah punya `showCompleteModal`, `CompleteDocumentModal`, dan `completeNote`.

---

## 3. Skema tampilan sebelum → sesudah

### 3.1 Riwayat SOAP → Buka

**SEBELUM:** klik **Buka** → galat "diagnosisApi.resetDraftDiagnoses is not a function".

**SESUDAH:** klik **Buka** → pindah ke segmen *Form SOAP*, draf "SOAP #1" terbuka berisi S/O/A/P dan
diagnosis A02.0 (Utama), A17.0. Dokter dapat melanjutkan dan menyimpan.

### 3.2 Bilah aksi Form SOAP

**SEBELUM:**

```text
Ada perubahan yang belum disimpan.                 [ Reset ] [ Simpan Catatan CPPT ]
```

**SESUDAH:**

```text
Ada perubahan yang belum disimpan.                 [ Reset ] [ Simpan Catatan CPPT ]
──────────────────────────────────────────────────────────────────────────────────────
Selesaikan catatan untuk menandatanganinya dan menampilkannya di CPPT.   [ Selesaikan Catatan ]
```

**Tombol**

| Label | Jenis | Kapan tampil/aktif | Yang terjadi |
| --- | --- | --- | --- |
| Reset | Sekunder | Seperti sekarang | Kembali ke draf baru **tanpa galat**; diagnosis yang belum tersimpan dibuang |
| Selesaikan Catatan | Utama | Tampil bila catatan dapat disunting (`editable`); tetap dapat ditekan walau belum lengkap | Membuka modal "Selesaikan …?"; bila dikonfirmasi: simpan bila berubah → `PATCH …/complete` → (setelah K-01) salin ke CPPT → muat ulang riwayat dan CPPT. Kekurangan (S/O/A/P, diagnosis) tampil di kotak "Catatan belum dapat diselesaikan" yang sudah ada |

**Keadaan**

| Keadaan | Tampilan |
| --- | --- |
| Penyelesaian ditolak server | Pesan kekurangan dari server tampil; catatan tetap draf |
| Selesai, penyalinan CPPT gagal | Catatan tetap selesai; pesan "Catatan selesai, tetapi belum tampil di CPPT. Coba muat ulang CPPT." |
| Catatan sudah selesai | Tombol Selesaikan tidak tampil; koreksi lewat addendum seperti sekarang |

### 3.3 CPPT Pasien

**SESUDAH** (setelah catatan diselesaikan): kolom **Catatan Dokter (1)** memuat SOAP dokter dengan
penulis dr. Contoh A, waktu, dan isi S/O/A/P.

---

## 4. Rincian perbaikan

### FIX-IGD-006-01 — Buka/Reset/Catatan baru tanpa galat

| | |
| --- | --- |
| **Menutup** | `ISS-IGD-006-01` |
| **Area** | Frontend |
| **Berkas** | `emergency-doctor-progress-tab.jsx` (`onSelectNote`); `use-emergency-progress-note.jsx` (`selectNote`, cabang draf) |
| **Radius dampak** | Tab Catatan Dokter IGD |
| **Bergantung pada** | Tidak ada |

**Acceptance criteria.**

1. Riwayat SOAP → **Buka** pada "SOAP #1" → Form SOAP terbuka berisi isi dan diagnosis catatan itu,
   tanpa galat.
2. Mengubah Plan lalu **Simpan Catatan CPPT** → tersimpan; riwayat menampilkan Plan baru.
3. **Reset** → draf baru kosong (S/O terisi otomatis dari perawat bila ada), tanpa galat.
4. Riwayat → **Catatan baru** → draf baru, tanpa galat.

### FIX-IGD-006-02 — Tombol Selesaikan Catatan (sisi IGD)

| | |
| --- | --- |
| **Menutup** | `ISS-IGD-006-02` (prasyarat), `ISSUE-IGD-004-T2` sisi IGD |
| **Area** | Frontend |
| **Berkas** | `emergency-doctor-progress-tab.jsx` — tombol yang memanggil `setShowCompleteModal(true)` |
| **Radius dampak** | Tab Catatan Dokter IGD; komponen Rawat Inap tidak disentuh |
| **Bergantung pada** | `FIX-IGD-006-01` (draf tersimpan harus dapat dibuka dulu) |

**Acceptance criteria.**

1. Draf berisi S/O/A/P dan minimal satu diagnosis → **Selesaikan Catatan** → konfirmasi → status
   menjadi selesai; riwayat menampilkan status selesai; editor menjadi baca-saja.
2. Draf tanpa diagnosis → konfirmasi → ditolak dengan pesan "Silakan pilih minimal satu diagnosa
   ICD-10…"; catatan tetap draf, isi tidak hilang.
3. Draf yang diubah lalu langsung diselesaikan → isi terbaru tersimpan (jalur `FIX-IGD-004-01`).

### FIX-IGD-006-03 — Salin SOAP ke CPPT setelah selesai

| | |
| --- | --- |
| **Menutup** | `ISS-IGD-006-02` |
| **Area** | Frontend |
| **Berkas** | `use-emergency-progress-note.jsx` (`completeNote`) atau tab IGD — memanggil `createPatientIntegratedProgressNoteFromConsultation(id, { useCurrentUserAsProvider: true, isReadOnlyGenerated: false }, { ignoreDuplicate: true })` |
| **Radius dampak** | Tab Catatan Dokter dan CPPT Pasien IGD |
| **Bergantung pada** | K-01, `FIX-IGD-006-02` |

**Acceptance criteria.**

1. Setelah AC 1 `FIX-02`, CPPT Pasien menampilkan **Catatan Dokter (1)** dengan isi S/O/A/P yang sama.
2. Menyelesaikan ulang atau menekan tombol dua kali tidak membuat dua entri CPPT (duplikat diabaikan).
3. Bila penyalinan gagal, catatan tetap selesai dan pesan yang jelas tampil.

**Verifikasi ketiga perbaikan.** `npx eslint` berkas yang diubah; uji browser akun Dokter IGD pada
pasien uji yang encounter-nya punya dokter.

**Risiko.** Penyelesaian mengunci catatan dan membuat entri CPPT pada database bersama. Uji
memakai pasien uji yang ditandai.

---

## 5. Urutan pengerjaan

```text
FIX-IGD-006-01   tanpa dependency — kerjakan pertama (Blocker)
└── FIX-IGD-006-02
    └── FIX-IGD-006-03   juga menunggu K-01
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Kapan SOAP dokter IGD masuk CPPT? | Saat diselesaikan / setiap kali disimpan | Saat diselesaikan, seperti Rawat Jalan | `FIX-IGD-006-03` |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `03-frontend-architecture.md` | §15, baris tab Catatan Dokter | "CPPT (dibaca dan ditulis)" | Ditambah: "SOAP dokter disalin ke CPPT saat catatan diselesaikan (`POST …/from-consultation/{id}`, duplikat diabaikan)" — setelah K-01 |

---

## 8. Verifikasi menyeluruh

1. Dokter membuka pasien uji → Catatan Dokter → isi S/O/A/P + diagnosis → Simpan.
2. Riwayat → Buka → lanjutkan → Simpan.
3. Selesaikan Catatan → konfirmasi → status selesai.
4. CPPT Pasien → Catatan Dokter (1) tampil.
5. Reset dan Catatan baru tanpa galat.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-04 | Rencana dibuat, 3 perbaikan, menunggu persetujuan | `diagnose-module-issue` |
| 2026-11-04 | Disetujui Rizki Gunawan beserta K-01 = "saat diselesaikan". Dikerjakan pada `emergency-doctor-progress-tab.jsx` dan `use-emergency-progress-note.jsx`. Perluasan yang ditemukan saat uji: props `CompleteDocumentModal` (T1), `persistPending` pada draf lama (T2), blok ICD-10 pada draf yang dibuka ulang (T3, perluasan `FIX-IGD-005-01`). ESLint kedua berkas: 0 masalah. Uji browser: Reset/Buka tanpa galat, Selesaikan 200, salin CPPT 200, CPPT menampilkan SOAP dokter, 0 galat halaman. Data uji: 1 catatan diselesaikan + 1 entri CPPT + 1 draf baru pada pasien uji K1-C | Claude Code |
| 2026-11-04 | Pelapor bertanya kenapa harus menekan Selesaikan agar masuk CPPT. Dijelaskan; pemilik memilih opsi B (satu tombol "Simpan & Selesaikan") lalu mengubahnya menjadi **satu tombol saja** ("jangan bikin 2 button… membingungkan dokter atau asisten dokter"). Dikerjakan sebagai `FIX-IGD-006-04` di tab IGD saja: aksi tombol "Simpan Catatan CPPT" editor bersama diarahkan ke konfirmasi penyelesaian; `soap-editor.jsx` milik Rawat Inap tidak diubah. ESLint 0 masalah. Uji: 5/5 lulus (draf baru); jalur draf lama lulus pada putaran opsi B sebelumnya. Data uji tambahan pada K1-C: 3 catatan selesai + 3 entri CPPT | Claude Code |
