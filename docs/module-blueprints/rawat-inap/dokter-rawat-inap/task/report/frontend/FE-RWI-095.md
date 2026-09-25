# Laporan Perubahan Frontend — `FE-RWI-095`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-095` |
| Judul | Perbaikan temuan pengujian dokter rawat inap — katalog obat, jenis resep, dan duplikasi service |
| Slice | Perbaikan defect pasca-pengujian; bukan slice fitur baru |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` |
| Trace | `ISSUE-DOK-001` butir `ISS-02`, `ISS-04`, `ISS-05`; `BE-RWI-050`; `RWI-DEC-046` |
| Contract version | `0.6.1` — terkunci, sudah diimplementasikan backend pada `BE-RWI-127` |
| Dependency | `BE-RWI-127` — sudah selesai di tingkat source pada 23 September 2026 |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — source; laporan dan tautan bukti di `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/**` |
| Wewenang UI | Tidak ada elemen visual baru. Perubahan salinan teks terbatas pada satu pesan galat yang sebelumnya menyesatkan |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a03676d1d829f56e40b46bdc1747498b7f5dad99` |
| Tanggal | 23 September 2026 |
| Status | Sebagian. `ISS-02` dan `ISS-04` selesai di tingkat source; `ISS-05` **tidak dikerjakan** karena analisis awal pada dokumen issue terbukti keliru — rinciannya di bagian 7 |

---

## 1. Masalah yang diperbaiki

**Dokter tidak dapat memilih satu obat pun.** Saat mengetik nama obat pada modal pencarian, layar
selalu menjawab *"Tidak ada obat yang cocok atau dapat diresepkan untuk encounter ini"* — padahal
data obat di basis data lengkap. Penyebabnya, permintaan ke katalog tidak menyertakan identitas
kunjungan pasien. Backend membutuhkan identitas itu untuk menentukan obat mana yang boleh diresepkan
beserta penjaminannya, sehingga permintaan tanpa identitas itu ditolak. Pesan yang muncul di layar
menyesatkan: dokter membaca "obat tidak ada", padahal permintaannya yang ditolak.

**Resep obat pulang tercatat sebagai resep rutin.** Saat menyimpan draf, frontend mengirimkan jenis
resep dengan nama field `orderType`, sedangkan kontrak backend menamainya `prescriptionOrderType`.
Keduanya nama yang berbeda — bukan sekadar beda huruf besar-kecil — sehingga nilainya diabaikan dan
jenis resep selalu jatuh ke nilai bawaan "rutin". Akibatnya nyata bagi Instalasi Farmasi: penyaringan
obat pulang di layar mereka tidak dapat dipercaya, karena resep obat pulang tidak pernah benar-benar
ditandai sebagai obat pulang.

---

## 2. Proses bisnis

**Tujuan.** Dokter memilih obat dari formularium rumah sakit dan menyimpannya sebagai draf resep
yang jenisnya tercatat benar.

**Pelaku.** Dokter penanggung jawab pelayanan pada lembar kerja rawat inap.

**Langkah berurutan sesudah perbaikan:**

1. Dokter membuka tab Resep, lalu menekan "Cari dan Tambah Obat".
2. Dokter mengetik minimal dua huruf nama obat.
3. Sistem memeriksa lebih dulu apakah konteks kunjungan pasien sudah siap.
   - Bila belum siap, katalog **tidak** dipanggil dan layar menyebut keadaannya apa adanya:
     *"Konteks kunjungan pasien belum siap. Muat ulang halaman sebelum mencari obat."*
   - Bila sudah siap, sistem meminta katalog obat beserta identitas kunjungan tersebut.
4. Daftar obat yang boleh diresepkan untuk kunjungan itu muncul beserta harga dan status penjaminan.
5. Dokter memilih obat, melengkapi aturan pakai, lalu menentukan jenis resep — rutin atau obat pulang.
6. Dokter menekan simpan draf. Jenis resep dikirim memakai nama field yang dikenal backend, sehingga
   pilihan dokter benar-benar tersimpan.

**Jalur tidak normal.**

- Kata kunci kurang dari dua huruf → pencarian tidak dijalankan, daftar dikosongkan.
- Konteks kunjungan belum siap → pencarian tidak dijalankan, pesan jujur ditampilkan.
- Permintaan katalog gagal → daftar dikosongkan, dokter dapat mencoba lagi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`; `rules/frontend/test-policy.md`
- `src/lib/hooks/health-services/inpatient-management/use-inpatient-prescription-tab.jsx`
- `src/lib/services/health-services/pharmacy-management/` — `prescription.service.js`,
  `prescription-workspace-service.js`, `prescription-workspace.service.js`
- `src/lib/hooks/health-services/pharmacy-management/use-doctor-prescription.js`,
  `use-prescription-workspace.js`
- `src/components/features/health-services/pharmacy-management/prescription-workspace/prescription-template-dialog.jsx`
- `src/components/ui/doctor-clinical-base/DoctorDrugCatalogModal.jsx`
- Backend sebagai rujukan kontrak: `PrescribingDrugController.cs`, `PrescriptionDtos.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-prescription-tab.jsx` | `ISS-02` — `encounterId` disertakan pada pemanggilan `getPrescribingDrugs`, dan dimasukkan ke dependency array `useCallback` yang sebelumnya kosong; ditambah penjaga yang tidak memanggil katalog ketika konteks kunjungan belum siap, beserta pesan yang jujur. `ISS-04` — field payload `orderType` diganti menjadi `prescriptionOrderType` agar terikat ke properti kontrak backend |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Memakai kontrak `0.6.1` yang sudah terkunci; tidak ada kontrak yang didefinisikan ulang dari sisi frontend. Dua penyesuaian: `encounterId` kini dikirim sebagaimana diwajibkan `PrescribingDrugController`, dan jenis resep dikirim dengan nama properti yang benar |
| Database | `NOT APPLICABLE` — frontend tidak menyentuh basis data |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan pada otorisasi, sesi, maupun kepemilikan data. `encounterId` adalah konteks kunjungan yang sudah dimiliki halaman, bukan kredensial |

---

## 4. Tabel keputusan base component

`UI GATE: NOT APPLICABLE` — task ini tidak menulis satu baris JSX maupun CSS baru. Seluruh perubahan
berada pada lapisan hook dan payload. Satu-satunya perubahan yang terlihat pengguna adalah teks pesan
galat, yang memakai mekanisme pesan yang sudah ada (`setActionError`) tanpa elemen visual baru.

| Elemen | Status | Alasan |
| --- | --- | --- |
| Modal katalog obat | `REUSE` | `DoctorDrugCatalogModal` dipakai apa adanya; tidak ada prop visual yang diubah |
| Pesan galat pencarian | `REUSE` | Memakai kanal `actionError` yang sudah ada pada hook dan sudah dirender layar |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint src/lib/hooks/health-services/inpatient-management/use-inpatient-prescription-tab.jsx` | Bersih, exit code `0`, tanpa peringatan | `PASS` | Keluaran perintah |
| `npm run build` | **Tidak benar-benar dijalankan.** Next menolak dengan *"Another next build process is already running"*; perintah keluar dengan kode `0` tanpa membangun apa pun | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah. Kode keluar `0` di sini **bukan** tanda lulus, dan sengaja tidak dilaporkan sebagai `PASS` |
| Ketersediaan `encounterId` pada hook | Tersedia sebagai parameter hook (baris 60) dan sudah dipakai pada payload simpan draf (baris 289) | `PASS` | Pembacaan source |
| Kesesuaian nama properti dengan kontrak backend | `CreatePrescriptionRequest.PrescriptionOrderType` — dikonfirmasi langsung pada `PrescriptionDtos.cs` | `PASS` | Pembacaan source backend |
| Bentuk `items`/`compounds` tetap sesuai kontrak | Sesuai `AutosavePrescriptionItemRequest` dan `AutosavePrescriptionCompoundRequest`; tidak ada perubahan yang diperlukan karena `prescription.service.js` meneruskan payload apa adanya | `PASS` | Pembacaan source kedua repository |
| Dokter dapat mencari dan memilih obat di layar | Belum dijalankan | `NOT RUN` | Menunggu backend hasil `BE-RWI-127` dijalankan |
| Jenis resep obat pulang tersimpan benar | Belum dijalankan | `NOT RUN` | Sama seperti di atas |

`MANUAL TEST: NOT FEASIBLE` — verifikasi manual menuntut backend hasil `BE-RWI-127` berjalan,
sedangkan `bin` backend masih dikunci proses lama. Kedua perbaikan pada task ini baru terbukti
sesudah backend itu dijalankan ulang.

`AUTOMATED TEST: SKIPPED (opsional) — repository ini tidak memakai Jest; menulis test baru bukan
gerbang selesai menurut rules/frontend/test-policy.md`.

**Tidak dijalankan:**

- `npm run build` penuh, karena proses build lain sedang memegang kunci.
- Verifikasi manual kedua alur, karena backend belum dijalankan ulang.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `ISS-02` — mengetik minimal 2 huruf memunculkan daftar obat formularium | Belum terpenuhi pada tingkat runtime | `encounterId` sudah dikirim dan lint bersih; pembuktian runtime `NOT RUN` |
| `ISS-02` — tidak ada respons `400` pada `GET /prescribing-drugs` | Belum terpenuhi pada tingkat runtime | Penyebab `400` sudah dihapus; perlu log jaringan sesudah backend berjalan |
| `ISS-02` — UI menampilkan pesan jujur saat konteks kunjungan belum siap | Terpenuhi | Penjaga beserta pesannya ada pada `searchDrugs`; katalog tidak dipanggil pada keadaan itu |
| `ISS-02` — `encounterId` masuk dependency array | Terpenuhi | `useCallback(..., [encounterId])` |
| `ISS-04` — jenis resep obat pulang tersimpan sebagai obat pulang | Belum terpenuhi pada tingkat runtime | Nama field sudah cocok dengan kontrak; pembuktian runtime `NOT RUN` |
| `ISS-04` — hasil penelusuran resep lama yang jenisnya salah dilaporkan | Belum terpenuhi | Menuntut wewenang database yang belum diberikan; bukan lingkup frontend |
| `ISS-05` — tersisa satu berkas service, seluruh import menunjuk ke sana | **Tidak dikerjakan** | Analisis awal keliru; lihat bagian 7 |
| `ISS-03` — resep tanpa item tidak pernah berstatus tersimpan-sukses di UI | Terpenuhi lewat backend | `BE-RWI-127` menempatkan kepala dan isi resep pada satu transaksi, sehingga kegagalan penyimpanan isi ikut membatalkan kepalanya dan galat naik ke UI |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan lint pada berkas yang diubah |
| Masalah yang diketahui | **`ISS-05` tidak dikerjakan, dan deskripsinya pada dokumen issue perlu dikoreksi.** Kedua berkas itu bukan duplikat. `prescription-workspace.service.js` mengembalikan `null` ketika resep belum ada (`404` ditangkap), sedangkan `prescription-workspace-service.js` melempar galat. `use-doctor-prescription.js` baris 433–437 **bergantung** pada perilaku `null` itu untuk menampilkan keadaan kosong *"Belum ada resep. Draft akan dibuat saat dokter menambah obat…"*. Menyatukan keduanya seperti tertulis di dokumen issue akan mengubah keadaan kosong yang wajar menjadi banner galat pada layar resep dokter poliklinik — modul di luar sub-modul ini. Karena keparahan `ISS-05` hanya `Low` dan perbaikannya menuntut keputusan perilaku layar, penyatuan sengaja tidak dipaksakan |
| Risiko tersisa | (1) Kedua perbaikan belum terbukti pada runtime. (2) Duplikasi service masih ada, dan berkas mana yang menjadi acuan masih menunggu keputusan pemilik. (3) Resep lama yang jenisnya terlanjur salah belum ditelusuri |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M src/lib/hooks/health-services/inpatient-management/use-inpatient-prescription-tab.jsx`. Perubahan lain pada working tree — seluruhnya milik modul hemodialisa — sudah ada sebelum task ini dan tidak disentuh |
| Langkah berikutnya | (1) Jalankan ulang backend hasil `BE-RWI-127`, lalu uji ulang alur resep ujung ke ujung. (2) Putuskan perilaku mana yang menang untuk `ISS-05` — melempar galat atau mengembalikan keadaan kosong — sebelum kedua service disatukan. (3) Telusuri resep lama yang jenisnya salah sesudah wewenang database diberikan |
