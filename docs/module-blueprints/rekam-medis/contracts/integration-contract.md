# Integration Contract — Modul Rekam Medis

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Contract version | `0.1.0` |
| Status | `draft` |
| Owner | Integration authority: `OPEN` |
| `approved_by` / `approved_at` | — / — |
| Input revisions | `00-interview-decisions.md` revision `2` |
| Compatibility impact | Dua modul berjalan mendapat kewajiban baru memanggil modul ini. Rinciannya pada bagian 2 |

> **PERINGATAN DASAR DESAIN.** Disusun di atas keputusan berstatus `draft`. Lihat `RM-DEC-025`.

---

## 1. Integrasi sistem luar

**Tidak ada.** Modul Rekam Medis pada rilis pertama tidak memanggil sistem luar mana pun, dan
tidak dipanggil sistem luar mana pun.

Ini bukan kelalaian melainkan batas scope yang disengaja. Integrasi SATUSEHAT dan FHIR
dinyatakan **di luar scope** pada decision log, dengan alasan: tidak ada implementasi apa pun
di source (hanya disebut pada dokumen keputusan IGD), dan integrasi seperti itu memerlukan
modul tersendiri beserta ownernya sendiri.

Bagian ini ditinjau ulang bila kebutuhan integrasi luar muncul.

---

## 2. Integrasi antar modul di dalam sistem

Meski tidak menyentuh sistem luar, modul ini punya tiga titik sentuh dengan modul yang sedang
berjalan. Ketiganya sinkron, di dalam satu proses, dan tanpa antrean pesan.

### 2.1 `ClinicalManagement` memanggil `MedicalRecordManagement` saat dokumen dibuat

| Aspek | Ketetapan |
|---|---|
| Pemanggil | `PatientIntegratedProgressNoteController` |
| Yang dipanggil | `ClinicalDocumentIntegrityService.RegisterAsync` |
| Kapan | Setelah CPPT tersimpan, di dalam transaksi yang sama |
| Sifat | Sinkron, wajib berhasil |
| Bila gagal | **Pembuatan CPPT ikut dibatalkan.** CPPT tanpa baris keutuhan akan luput dari seluruh aturan penguncian, dan itu keadaan yang tidak boleh ada |
| Idempotency | Dijamin index unik `(DocumentKind, DocumentId)`. Pemanggilan ulang untuk dokumen yang sama tidak membuat baris kedua |

### 2.2 `RegistrationManagement` memanggil `MedicalRecordManagement` saat kunjungan ditutup

| Aspek | Ketetapan |
|---|---|
| Pemanggil | `PatientEncounterController`, endpoint `PATCH /{id}/status` |
| Yang dipanggil | `ClinicalDocumentIntegrityService.LockOpenDocumentsForEncounterAsync` |
| Kapan | Saat status berpindah **menuju** `Completed`, di dalam transaksi yang sama |
| Sifat | Sinkron, wajib berhasil |
| Bila gagal | **Penutupan kunjungan ikut dibatalkan.** Kunjungan tertutup dengan dokumen masih terbuka melanggar `RM-DEC-003` |
| Idempotency | Aman dipanggil berulang. Hanya dokumen berstatus `Draft` yang terpengaruh; yang sudah terkunci dilewati |
| Batas waktu | Bila satu kunjungan memuat sangat banyak dokumen, penguncian dilakukan per potongan di dalam transaksi yang sama |

### 2.3 `MedicalRecordManagement` membaca milik modul lain

| Yang dibaca | Milik | Sifat | Bila gagal |
|---|---|---|---|
| Tiga belas tabel isi klinis | `ClinicalManagement` | Hanya baca, `AsNoTracking` | Bagian riwayat dari sumber itu ditandai gagal dimuat; bagian lain tetap tampil |
| `TrxPatientEncounter` | `RegistrationManagement` | Hanya baca | Penilaian kunjungan aktif gagal, sehingga akses **diperlakukan sebagai beralasan** — menutup rapat, bukan melonggarkan |
| `MstPatient` | `PatientManagement` | Hanya baca | Permintaan ditolak `404` |
| `ApplicationUser` | Identity | Hanya baca | Nama pengguna diambil dari salinan pada jejak akses |

Baris kedua perlu digarisbawahi. Bila sistem gagal menentukan apakah pasien punya kunjungan
aktif, ia **tidak** menganggap pengguna berhak. Ia menganggap akses sebagai beralasan dan
meminta alasan. Kegagalan teknis tidak boleh berubah menjadi pelonggaran kewenangan.

---

## 3. Ketergantungan urutan penerapan

Urutan ini mengikat karena satu modul memanggil yang lain.

```text
1. Tabel keutuhan dan addendum dibuat          (migration 1)
2. Tabel jejak akses dan masternya dibuat      (migration 2)
3. Data lama diisi untuk CPPT                  (migration 3)
4. ClinicalManagement mulai memanggil RegisterAsync
5. RegistrationManagement mulai memanggil LockOpenDocumentsForEncounterAsync
6. Layar penelusuran mulai dipakai
```

Langkah 3 wajib mendahului langkah 4 dan 5. Bila tidak, akan ada CPPT lama tanpa baris keutuhan
sementara CPPT baru sudah punya — dan layar penelusuran akan menampilkan sebagian dokumen tanpa
status, tanpa penjelasan mengapa.

Langkah 6 sengaja diletakkan paling akhir, sesuai `RM-DEC-019`: layar penelusuran menyajikan
catatan sebagai berkas resmi, sehingga keutuhannya harus dijamin lebih dulu.

---

## 4. Cara mundur bila gagal

| Tahap | Cara mundur | Risiko |
|---|---|---|
| Migration 1 dan 2 | Hapus tabel | Tidak ada. Belum ada yang memakainya |
| Migration 3 | Hapus seluruh baris yang dibuat migration ini | Rendah. Tidak menyentuh tabel klinis sama sekali |
| Langkah 4 dan 5 | Kembalikan perubahan controller | Rendah. Tidak ada perubahan skema yang perlu dibatalkan |
| Langkah 6 | Sembunyikan menu rekam medis | Tidak ada |

Sifat yang membuat pembatalan mudah: **tidak ada satu kolom pun berubah pada tabel yang sedang
dipakai.** Seluruh perubahan berupa tabel baru ditambah perubahan perilaku dua controller.
Karena itu pembatalan tidak pernah menyentuh data klinis yang sudah ada.

---

## 5. Yang sengaja tidak dipakai

| Yang ditolak | Alasan |
|---|---|
| Antrean pesan untuk penguncian saat kunjungan ditutup | Penguncian harus terjadi bersama penutupan kunjungan, tidak boleh tertunda. Jeda sekecil apa pun menciptakan celah waktu ketika kunjungan sudah tertutup tetapi dokumen masih dapat diubah |
| Pencatatan jejak akses secara asinkron | Sama alasannya. Bila pencatatan tertunda, ada pembacaan yang isinya sudah dikembalikan sebelum jejaknya tertulis |
| Event bus antar modul | Sistem belum memakainya. Menambahkannya untuk satu modul menciptakan pola yang tidak dipahami modul lain |
| Pemanggilan HTTP antar modul | Modul berada dalam satu proses. Memanggil lewat HTTP menambah kerumitan tanpa manfaat |

---

## 6. Traceability

| Titik integrasi | Decision | Acceptance test |
|---|---|---|
| Pendaftaran keutuhan saat dokumen dibuat | `RM-DEC-013` | `AT-RM-24` |
| Penguncian saat kunjungan ditutup | `RM-DEC-003` lapis kedua | `AT-RM-03` |
| Kegagalan penilaian kunjungan menutup rapat | `RM-DEC-016` | `AT-RM-25` |
| Urutan penerapan | `RM-DEC-014`, `RM-DEC-019` | `AT-RM-21` |
