# PLAN-REPAIR-008 — General Consent di Workspace PPRI menjadi form edit berversi dengan riwayat pengubah

```yaml
plan_id: PLAN-REPAIR-EPS-008
issue: ../issue/issue-008-edit-riwayat-general-consent.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-13"
diputuskan_oleh: "Pemilik sistem (Muhammad Hamzah)"
tanggal_keputusan: "2026-10-13"
basis_source_backend: "4ec02370dfa2160a9b749f507f8ebd45b0a395db (MHamzah)"
basis_source_frontend: "f4495aa14c067c15e282203533e9c163363528ca (HamzahV2)"
```

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-008-01` | `ISS-EPS-008-01`, `ISS-EPS-008-02`, `ISS-EPS-008-T1` | Endpoint edit persetujuan berversi, endpoint riwayat, panduan rawat inap ikut tersimpan, dan simpan ulang dari admisi tidak lagi menimpa | BE | 1 | `BE-RWI-227` | ✅ SELESAI | [`BE-RWI-227.md`](../../task/report/backend/BE-RWI-227.md) — build 0 error, QBE PASS; uji PostgreSQL `NOT RUN` |
| `FIX-EPS-008-02` | `ISS-EPS-008-01`, `ISS-EPS-008-T1`, `ISS-EPS-008-T2` | Tab Formulir General Consent: mode lihat dan mode edit, panduan dari data tersimpan, hubungan tercentang pada cetakan | FE | 1 | `FE-RWI-241` | ✅ SELESAI | [`FE-RWI-241.md`](../../task/report/frontend/FE-RWI-241.md) — ESLint 0 error, unit 15/15, build PASS; uji layar `NOT FEASIBLE` |
| `FIX-EPS-008-03` | `ISS-EPS-008-02` | Panel Riwayat Perubahan: versi, pengubah, waktu, alasan, dan daftar perubahan | FE | 2 | `FE-RWI-242` | ✅ SELESAI | [`FE-RWI-242.md`](../../task/report/frontend/FE-RWI-242.md) — ESLint 0 error, unit 16/16, build PASS; uji layar `NOT FEASIBLE` |
| `FIX-EPS-008-04` | `ISS-EPS-008-01` | Revisi dokumen blueprint: keputusan baru, skema tampilan, arsitektur frontend, kontrak API, dan matriks validasi | Dokumen | 2 | — (perbaikan dokumen, bukan task roadmap) | ✅ SELESAI | Amandemen `RWI-DEC-295`/`296`, `RWI-AC-427` s.d. `431` di [`00-interview-decisions.md`](../../../00-interview-decisions.md) bagian "Amandemen PLAN-REPAIR-EPS-008"; lima dokumen pada bagian 7 direvisi |

**Ringkasan: 4 dari 4 perbaikan selesai pada tingkat source, lint, unit test, dan build. Uji transaksi PostgreSQL dan uji layar oleh pemilik belum dijalankan (bagian 8).**

`ISS-EPS-008-T3` sengaja tidak dimasukkan: berkasnya sedang diubah pekerjaan lain (issue bagian 6).

---

## 2. Solusi terpilih per temuan

### ISS-EPS-008-01 dan ISS-EPS-008-02 — Persetujuan dapat dikoreksi dan setiap koreksi tercatat

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Rantai versi pada `InpAdmissionDocument`: setiap koreksi = baris versi baru, versi lama menjadi `Superseded` | Memakai kolom dan pola yang sudah ada (`VersionNo`, `PreviousVersionId`, `CorrectionReason`, `ReviseAsync`); isi dan tanda tangan lama utuh; tanpa migration | Pembaca persetujuan wajib memilih versi aktif |
| B | Timpa di tempat ditambah tabel log perubahan terpisah | Pembaca tidak berubah | Tabel baru dan migration; isi lama hanya tersimpan sebagai potongan log; melanggar invariant model `InpAdmissionDocument.cs:11-15` |
| C | Edit hanya dengan membatalkan persetujuan lalu mengisi ulang di langkah admisi | Tanpa endpoint baru | Langkah admisi tidak tersedia sesudah admisi selesai; tidak menjawab permintaan "form edit" |

**Solusi terpilih: Opsi A.**

1. Tabel dan statusnya sudah dirancang untuk rantai koreksi (`Enums/InpAdmissionDocumentStatus.cs`:
   `Superseded` = "Digantikan versi koreksi; tetap terbaca di Riwayat").
2. Polanya sama dengan Versi Koreksi dokumen admisi lain (`RWI-DEC-240`, `RWI-AC-355`), sehingga
   petugas PPRI tidak perlu mempelajari aturan baru.
3. Tidak ada tabel atau migration baru. Riwayat cukup dibaca dari versi-versi yang ada.

**Kenapa bukan opsi lain.** Opsi B menambah tabel untuk menyimpan sesuatu yang sudah bisa disimpan
tabel yang ada, dan tetap membuang isi lama. Opsi C tidak memenuhi permintaan pemilik.

### ISS-EPS-008-T1 — Panduan rawat inap tidak tersimpan

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Simpan `GuidanceReceived` dan `GuidanceNote` di salinan isi persetujuan (`SnapshotJson`), dapat diubah lewat Edit | Tanpa kolom baru; ikut berversi dan masuk riwayat | Persetujuan lama tidak punya nilai panduan (tampil "Belum diisi") |
| B | Tetap di state layar | Tanpa perubahan | Cetakan tidak mencerminkan apa yang disepakati pasien |

**Solusi terpilih: Opsi A.** Panduan adalah bagian isi formulir yang dicetak dan ditandatangani,
jadi harus tersimpan dan berversi seperti isian lain.

### ISS-EPS-008-T2 — Hubungan tidak tercentang pada cetakan

Hanya ada satu cara yang wajar: centang hubungan diturunkan dari data persetujuan tersimpan
(`SignerType` + teks hubungan), bukan dari pilihan layar. Persetujuan bertanda tangan selalu punya
data itu.

---

## 3. Skema tampilan sebelum → sesudah

Tanda rangka mengikuti `05-skema-tampilan.md` bagian 1. Data contoh: pasien Tn. Budi Santoso; istri
Ny. Rina Santoso; petugas admisi Sari; petugas PPRI Dewi.

### 3.1 SEBELUM — tab Formulir General Consent, dokumen bertanda tangan

```text
Formulir General Consent                                            [Bertanda Tangan Digital]
✓ Dokumen Telah Bertanda Tangan Digital … (read-only / print-ready)
┌ Saya yang Bertanda Tangan di Bawah Ini ──[Pihak Penanda Tangan]┐ ┌ Informasi Kamar Rawat ──────[Umum]┐
│ Nama Penanda Tangan  Rina Santoso   Hubungan    Istri          │ │ Tipe Umum  Kelas I – Kamar 01 (Bed 1)│
│ No. Identitas        3171…9998      No. Telepon 0812…          │ ├ Penerimaan Panduan Rawat Inap ─────┤
│ Status Dokumen [Tanda Tangan Digital Tersimpan (Admisi)]       │ │ ( ) Ya, Sudah Menerima ( ) Tidak    │ ← dapat diklik, tidak tersimpan
│                                    (tidak ada tombol ubah)     │ │ Keterangan [                    ]   │
└────────────────────────────────────────────────────────────────┘ └────────────────────────────────────┘
[ pratinjau lembar cetak — hubungan: [ ] Diri Sendiri [ ] Suami [ ] Istri …  ← tidak ada yang tercentang ]
                                                                        [ Coba Lagi ] [ Cetak ]
```

### 3.2 SESUDAH — mode lihat

```text
Formulir General Consent                                 [Bertanda Tangan Digital] [Versi 2]
Terakhir diubah Dewi · 13 Okt 2026 10.15
✓ Dokumen Telah Bertanda Tangan Digital …
┌ Saya yang Bertanda Tangan di Bawah Ini ─────────[ Edit Persetujuan ]┐ ┌ Informasi Kamar Rawat ────[Umum]┐
│ Nama  Rina Santoso       Hubungan  Suami / Istri — Istri            │ │ Tipe Umum  Kelas I – Kamar 01     │
│ NIK   3171…9999          Telepon   0812…                            │ ├ Penerimaan Panduan Rawat Inap ────┤
│ Alamat Jl. Kenanga No. 5, Bekasi                                    │ │ Sudah menerima panduan: Ya        │
│ Tanda tangan [citra]  ditandatangani 13 Okt 2026 09.40              │ │ Keterangan: brosur diserahkan     │
└─────────────────────────────────────────────────────────────────────┘ └───────────────────────────────────┘
┌ Riwayat Perubahan ──────────────────────────────────────────────────────────────────────────────┐
│ Versi 2 · Aktif        13 Okt 2026 10.15 · Dewi                                                  │
│   Alasan: NIK penanda tangan salah ketik saat admisi                                             │
│   • No. Identitas: 3171…9998 → 3171…9999                                                          │
│ Versi 1 · Digantikan   13 Okt 2026 09.40 · Sari — dibuat pada langkah admisi                      │
└─────────────────────────────────────────────────────────────────────────────────────────────────┘
[ pratinjau lembar cetak — hubungan: [ ] Diri Sendiri [ ] Suami [✓] Istri … ]
                                                                        [ Coba Lagi ] [ Cetak ]
```

### 3.3 SESUDAH — mode edit (sesudah menekan Edit Persetujuan)

```text
┌ Edit Persetujuan ───────────────────────────────────────────────────────────────────────────────┐
│ Jenis penanda tangan *  ( ) Pasien Sendiri  ( ) Orang Tua  ( ) Wali yang Sah  (•) Suami / Istri  ( ) Keluarga Lainnya │
│ Nama penanda tangan * [Rina Santoso            ]  Keterangan hubungan [Istri           ]          │
│ No. Identitas         [3171…9999               ]  No. Handphone       [0812…           ]          │
│ Alamat                [Jl. Kenanga No. 5, Bekasi                                     ]            │
│ Sudah menerima panduan rawat inap   (•) Ya, Sudah Menerima  ( ) Tidak / Belum Menerima           │
│ Keterangan panduan    [brosur diserahkan       ]                                                  │
│ Tanda tangan  [citra lama]                                  [ Ganti Tanda Tangan ]               │
│   └ bila ditekan: kanvas tanda tangan + [ Bersihkan ] [ Pakai Tanda Tangan Lama ]                 │
│ ⚠ Jenis penanda tangan berubah. Bubuhkan tanda tangan penanda tangan yang baru.   (bila berlaku)   │
│ Alasan perubahan *    [NIK penanda tangan salah ketik saat admisi                 ]               │
│                                                         [ Batal ]  [ Simpan Perubahan ]           │
└─────────────────────────────────────────────────────────────────────────────────────────────────┘
```

Untuk bayi baru lahir, pilihan "Pasien Sendiri" tampil `(×)` dengan keterangan "Tidak berlaku untuk
bayi baru lahir" (`RWI-DEC-273`).

**Wilayah**

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Kepala panel | Judul, lencana tanda tangan, lencana "Versi N", "Terakhir diubah <nama> · <waktu>" | `GET …/consent` (`VersionNo`, `LastChangedByName`, `LastChangedAt`) | `ClinicalContentPanel`, `StatusBadge` |
| Kartu penanda tangan (lihat) | Jenis + keterangan hubungan, nama, NIK, telepon, alamat, citra dan waktu tanda tangan | `GET …/consent` | Pola `readOnlyPanel` yang sudah ada |
| Kartu kamar | Tidak berubah (tetap baca-saja dari penempatan aktif) | `GET …/general-consent/print-data` | Yang sudah ada |
| Kartu panduan (lihat) | Ya / Tidak / "Belum diisi" dan keterangannya | `GET …/consent` (`GuidanceReceived`, `GuidanceNote`) | `readOnlyPanel` |
| Form edit | Isian pada 3.3 | Diisi awal dari versi aktif | `AdmissionRadioGroup`, `BaseTextField`, `BaseTextAreaField`, `react-signature-canvas` (dependency yang sudah ada) |
| Riwayat Perubahan | Satu baris per versi, terbaru di atas | `GET …/admission-workspace/general-consent/history` | Baru: `general-consent-history-panel.jsx` |
| Pratinjau cetak | Lembar V1 dengan hubungan tercentang dari data tersimpan | Versi aktif | `GeneralConsentFormPrint` |

**Tombol**

| Label | Jenis | Kapan tampil / aktif | Yang terjadi |
| --- | --- | --- | --- |
| Edit Persetujuan | Sekunder | Tampil bila persetujuan bertanda tangan, episode dapat ditulis (`isWritable`), dan pengguna punya `InpatientAdmissionDocument : Update` | Membuka mode edit dengan isian dari versi aktif |
| Ganti Tanda Tangan | Sekunder | Mode edit | Menampilkan kanvas; citra lama disimpan sebagai cadangan |
| Pakai Tanda Tangan Lama | Sekunder | Kanvas terbuka dan jenis penanda tangan tidak berubah | Menutup kanvas, kembali ke citra lama |
| Batal | Sekunder | Mode edit | Membuang perubahan dan kembali ke mode lihat tanpa permintaan apa pun |
| Simpan Perubahan | Primer | **Selalu dapat ditekan**; terkunci hanya selama permintaan berjalan | Validasi dulu. Bila ada isian kurang: ringkasan galat, lalu fokus ke isian pertama yang salah. Bila valid: `POST …/admission-workspace/general-consent/revisions`. Bila berhasil: toast "Persetujuan diperbarui menjadi versi N", kembali ke mode lihat, lalu data dan riwayat dimuat ulang |
| Cetak, Coba Lagi | Tetap | Mode lihat | Tidak berubah |

**Keadaan**

| Keadaan | Tampilan |
| --- | --- |
| Memuat | "Mengambil data cetak General Consent..." (yang sudah ada) |
| Persetujuan belum pernah disimpan (episode lama) | Perilaku cetak saja yang sekarang tetap berlaku; tombol Edit dan panel riwayat tidak tampil |
| Gagal memuat riwayat | Panel riwayat menampilkan "Riwayat perubahan tidak dapat dimuat" + Coba Lagi; form tetap dapat dipakai |
| Tidak berhak `Update` atau episode tertutup | Tombol Edit tidak tampil; riwayat tetap terbaca |
| Server menolak 409 (diubah petugas lain) | Mode edit tetap terbuka dengan isian petugas, pesan "Persetujuan sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda." |
| Server menolak 422 tanpa perubahan | Pesan "Tidak ada data yang berubah." |

---

## 4. Rincian perbaikan

### FIX-EPS-008-01 — Edit persetujuan berversi dan riwayat (backend)

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-008-01`, `ISS-EPS-008-02`, `ISS-EPS-008-T1` |
| **Area** | Backend `HealthServices/InPatientManagement` |
| **Jenis perubahan** | Source backend; **tanpa** entity baru dan **tanpa** migration |
| **Berkas yang diubah** | `Services/InpatientAdmissionConsentService.cs`, `Controllers/InpatientAdmissionConsentController.cs`, `DTOs/InpatientAdmissionConsentDtos.cs` |
| **Radius dampak** | Langkah Formulir Persetujuan admisi (`POST`/`GET …/consent`), kedua tab menu General Consent, lembar cetak persetujuan. Tidak menyentuh endpoint dokumen admisi generik |
| **Bergantung pada** | Tidak ada |

**Grup Swagger `InpatientAdmissionConsent` dan `Health Services / Inpatient Management / Inpatient Admission Workspace`**

> **Delta implementasi 13-10-2026 (`BE-RWI-227`).** Edit dan riwayat tidak ditempatkan di controller consent, melainkan di `InpatientAdmissionDocumentController` (Workspace PPRI). Aturan akses role mewajibkan argumen ke-1 `[AccessPermission]` sama dengan `ControllerName` controller-nya, dan hak yang dipakai adalah `InpatientAdmissionDocument`. Karena koreksi adalah perintah yang menghasilkan baris riwayat, verb-nya `POST …/revisions` (standar endpoint transaksi bagian 3), sama dengan `documents/{id}/revisions`.

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/consent` (alias lama
`…/admission-consents` dan `api/v1/inpatient-admissions/{episodeId}/consent` tetap).

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Membaca persetujuan **versi aktif** | `InpatientEpisode : Read` (tetap) | — | `InpatientAdmissionConsentResponse` + `VersionNo`, `RowVersion`, `GuidanceReceived`, `GuidanceNote`, `LastChangedByName`, `LastChangedAt` |
| `POST` | `/` | Simpan dari langkah admisi. Bila sudah ada versi aktif: isi sama → kembalikan versi aktif tanpa perubahan; isi berbeda → versi baru beralasan otomatis | `InpatientEpisode : Create` (tetap) | `InpatientAdmissionConsentCreateDto` + `GuidanceReceived?`, `GuidanceNote?` | Sama |
| `POST` | `…/admission-workspace/general-consent/revisions` | Edit Persetujuan dari Workspace PPRI; selalu membuat versi baru | `InpatientAdmissionDocument : Update` | `InpatientAdmissionConsentRevisionRequest` | Sama |
| `GET` | `…/admission-workspace/general-consent/history` | Riwayat seluruh versi, terbaru di atas | `InpatientAdmissionDocument : Read` | — | `List<InpatientAdmissionConsentHistoryResponse>` |

Isi `InpatientAdmissionConsentRevisionRequest`:

- `RowVersion` (wajib) dan `CorrectionReason` (wajib, 10–500 karakter);
- isian penanda tangan seperti `CreateDto`: `SignerType`, `SignerName`, `SignerRelationship`,
  `SignerRelationshipText`, `SignerPhoneNumber`, `SignerIdentityNumber`, `SignerAddress`;
- `GuidanceReceived` (`bool?`) dan `GuidanceNote` (≤ 300 karakter);
- `SignatureImageBase64` opsional — kosong berarti tanda tangan lama dipakai.

`InpatientAdmissionConsentHistoryResponse` memuat:

- `VersionNo`, `Status` (`Aktif` atau `Digantikan`), `IsActive`;
- `ChangedAt`, `ChangedByName`, `ChangedByPosition`;
- `CorrectionReason` (pada versi 1 kosong, ditampilkan "dibuat pada langkah admisi");
- `IsSignatureReplaced`;
- `Changes[]` berisi {`Field`, `Label`, `Before`, `After`}.

Tidak ada UUID yang ditujukan untuk tampil.

**Kode status `POST …/general-consent/revisions`**

| Kode | Arti bagi petugas | Contoh |
| --- | --- | --- |
| 200 | Versi baru tersimpan | "Persetujuan diperbarui menjadi versi 2." |
| 400 | Isian kurang atau salah format | "Alasan perubahan minimal 10 karakter." Alasan "salah" (5 karakter) ditolak |
| 404 | Episode belum punya persetujuan tersimpan | "Formulir persetujuan rawat inap belum diisi atau tidak ditemukan." |
| 409 | Persetujuan sudah diubah petugas lain sejak dibuka, atau episode sudah ditutup/dibatalkan | `RowVersion` lama → "Persetujuan sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda." |
| 422 | Aturan bisnis menolak | `BAYI_PENANDATANGAN_WAJIB_WALI`; `TTD_BARU_WAJIB` (jenis penanda tangan berubah tanpa tanda tangan baru); `TIDAK_ADA_PERUBAHAN` |

**Perubahan status**

| Dari | Tindakan | Ke | Siapa | Syarat |
| --- | --- | --- | --- | --- |
| Versi N `Completed` | Edit Persetujuan (`POST …/revisions`) | Versi N `Superseded` + versi N+1 `Completed` | Pemegang `InpatientAdmissionDocument : Update` | Episode `Admitted`/`DischargePending`; `RowVersion` cocok; alasan 10–500; minimal satu isian berubah |
| Versi N `Completed` | Simpan ulang dari langkah admisi, isi berbeda | Sama, alasan otomatis "Disimpan ulang dari langkah Formulir Persetujuan admisi." | Pemegang `InpatientEpisode : Create` | Isi berbeda dari versi N |
| Versi N `Completed` | Simpan ulang dari langkah admisi, isi sama | Tetap versi N | — | — |

**Langkah.**

1. Ganti pencarian dokumen pada `GetConsentAsync` dan `SaveConsentAsync` agar hanya mengambil versi
   aktif (`Status == Completed`, tidak dihapus).
2. Tambah `GuidanceReceived` dan `GuidanceNote` ke `CreateDto` dan `Response`, lalu ikut disimpan
   ke salinan isi (`SnapshotJson`).
3. Tulis satu pembantu "buat versi berikutnya". Dalam satu transaksi, versi lama menjadi
   `Superseded` (`SupersededAt`, `Touch`) lalu disimpan. Sesudah itu baris baru ditambahkan dengan
   `VersionNo + 1`, `PreviousVersionId`, `CorrectionReason`, `Party` dan `Signature` baru,
   `DocumentNumber` yang sama, dan `CreateBy` = petugas. Urutan ini mengikuti `ReviseAsync`
   karena ada unique index dokumen aktif. Pelanggaran unique index `PreviousVersionId` atau
   `DbUpdateConcurrencyException` → 409.
4. `POST …/general-consent/revisions`:
   - pakai `InpAdmissionWriteGuard.EnsureWritableAsync`;
   - cocokkan `RowVersion`;
   - validasi alasan, nama, dan invariant bayi;
   - tolak bila jenis penanda tangan berubah tanpa tanda tangan baru;
   - tolak bila tidak ada isian yang berubah;
   - tanda tangan lama beserta `SignedAt`-nya disalin bila tidak diganti.
5. `POST` dengan versi aktif: bandingkan isi. Isi sama → kembalikan versi aktif (kode sukses tetap 201 seperti kontrak `BE-RWI-205`). Isi
   berbeda → pembantu langkah 3 dengan alasan otomatis.
6. `GET /history`:
   - ambil seluruh versi persetujuan episode, kecuali yang `IsDelete`;
   - nama pengubah dibaca lewat `InpAdmissionSourceReader.GetUsersAsync`;
   - `Changes` dihitung dengan membandingkan salinan isi versi itu dengan versi sebelumnya.
7. Lengkapi `[AccessAction]`/`[AccessPermission]` untuk dua action baru sesuai aturan izin
   repository.

**Acceptance criteria.**

1. Persetujuan versi 1 Ny. Rina (NIK `3171…9998`), dikoreksi lewat Edit Persetujuan dengan NIK `3171…9999` dan
   alasan "NIK penanda tangan salah ketik saat admisi":
   - versi 1 menjadi `Superseded` dengan isi dan citra tanda tangan utuh;
   - versi 2 `Completed` dengan `PreviousVersionId` = versi 1;
   - `GET …/consent` mengembalikan versi 2.
2. `GET …/general-consent/history` mengembalikan dua baris. Versi 2 memuat nama pengubah Dewi, waktu,
   alasan, dan `Changes` = [{No. Identitas, `3171…9998` → `3171…9999`}]. Versi 1 tanpa alasan.
3. Edit Persetujuan dengan alasan "salah" (5 karakter) → 400, tidak ada versi baru.
4. Dua petugas membuka versi 2. Petugas pertama menyimpan; petugas kedua menyimpan dengan
   `RowVersion` lama → 409, dan versi aktif tetap satu.
5. Edit Persetujuan yang mengganti jenis penanda tangan `Spouse` → `Family` tanpa `SignatureImageBase64` → 422
   `TTD_BARU_WAJIB`. Dengan tanda tangan baru → berhasil, `IsSignatureReplaced = true`, dan
   `SignedAt` = waktu simpan.
6. Edit Persetujuan tanpa satu pun isian berubah → 422 `TIDAK_ADA_PERUBAHAN`.
7. Episode `Closed` → Edit Persetujuan ditolak 409. `GET` dan `history` tetap dapat dibaca.
8. Langkah admisi menyimpan ulang isi yang sama → sukses (201, kode yang sama dengan sebelumnya), `VersionNo` tetap. Mengubah nomor HP
   lalu menyimpan ulang → versi baru beralasan otomatis.
9. `GuidanceReceived = true` dan `GuidanceNote` "brosur diserahkan" tersimpan, terbaca kembali
   lewat `GET`, dan perubahannya muncul pada `Changes`.
10. Pasien bayi baru lahir: Edit Persetujuan dengan `SignerType = Self` → 422 `BAYI_PENANDATANGAN_WAJIB_WALI`.

**Verifikasi.** Backend build (lihat memori: garis dasar 0 error), QBE conformance bila tersedia,
dan uji manual lewat Swagger pada database uji milik pemilik. Uji PostgreSQL yang tidak dijalankan
ditulis `NOT RUN`.

**Risiko.**

- Endpoint dokumen generik (`documents/{id}/revisions`) secara teknis masih bisa membuat konsep
  versi untuk jenis `GeneralConsent`. Konsep itu tidak punya salinan isi, sehingga `GET …/consent`
  akan mengembalikan 404. Layar tidak menyediakan jalur ke sana. Dicatat sebagai risiko, bukan
  diubah di sini.
- Citra tanda tangan tersalin per versi, sehingga ukuran baris bertambah (satu PNG per versi).

### FIX-EPS-008-02 — Tab Formulir General Consent: mode lihat dan mode edit (frontend)

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-008-01`, `ISS-EPS-008-T1`, `ISS-EPS-008-T2` |
| **Area** | Frontend Workspace PPRI |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `lib/services/.../inpatient-admission-workspace.service.js` (`reviseAdmissionConsent`); `lib/hooks/.../use-admission-general-consent.js`; `admission-workspace/sections/general-consent/general-consent-section.jsx`; baru `general-consent-edit-form.jsx`; `general-consent-form-print.jsx`; `utils/.../inpatient-admission-print-utils.js` (pemetaan hubungan cetak); konstanta jenis penanda tangan di `inpatient-admission-flow-constants.jsx` |
| **Radius dampak** | Hanya menu General Consent Workspace PPRI. Langkah admisi (`inpatient-admission-consent-form-step.jsx`) **tidak disentuh** karena sedang diubah pekerjaan lain |
| **Bergantung pada** | `FIX-EPS-008-01` |

**Langkah.**

1. Service: `reviseAdmissionConsent(episodeId, payload)` → `POST …/admission-workspace/general-consent/revisions`.
2. Hook: sediakan `versionNo`, `rowVersion`, nilai panduan tersimpan, `lastChangedBy`, dan fungsi
   `saveEdit`. Pilihan panduan di layar hanya dipakai bila persetujuan belum pernah disimpan.
3. Section, tab Formulir General Consent:
   - mode lihat sesuai 3.2 (panduan baca-saja dari data tersimpan, lencana versi, baris "Terakhir
     diubah");
   - tombol Edit Persetujuan sesuai tabel tombol;
   - mode edit sesuai 3.3 di komponen baru `general-consent-edit-form.jsx`, memakai base component
     dan `react-signature-canvas` yang sudah ada.
4. Validasi layar mencerminkan server: nama ≥ 3, telepon angka ≤ 13, alasan 10–500, keterangan
   panduan ≤ 300, bayi tidak boleh "Pasien Sendiri", tanda tangan baru wajib bila jenis berubah.
   Tombol Simpan tidak pernah dimatikan karena isian kurang; validasi saat klik lalu fokus ke
   isian pertama yang salah.
5. Cetakan V1: hubungan tercentang diturunkan dari data tersimpan bila persetujuan bertanda tangan
   (pemetaan pada issue `ISS-EPS-008-T2`); panduan dari data tersimpan.
6. Ganti teks subjudul dan komentar "cetak saja / read-only" yang tidak lagi benar untuk dokumen
   bertanda tangan.

**Acceptance criteria.**

1. Persetujuan bertanda tangan + pengguna berhak `Update` + episode berjalan → tombol "Edit
   Persetujuan" tampil. Pengguna tanpa `Update`, atau episode `Closed`, tidak melihat tombol itu.
2. Menekan Edit mengisi form dengan data versi aktif (nama "Rina Santoso", jenis "Suami / Istri",
   NIK, telepon, alamat, panduan).
3. Menekan Simpan dengan alasan kosong tidak mengirim permintaan. Pesan "Alasan perubahan minimal 10
   karakter" tampil dan fokus pindah ke isian alasan. Tombol tidak pernah abu-abu sebelum diklik.
4. Mengubah NIK lalu menyimpan dengan alasan valid mengirim tepat satu `POST …/revisions`. Sesudah berhasil, mode
   lihat menampilkan NIK baru dan lencana "Versi 2", lalu tab Surat Persetujuan menampilkan data
   yang sama.
5. Mengubah jenis penanda tangan tanpa membubuhkan tanda tangan baru → peringatan tampil dan Simpan
   tidak mengirim permintaan.
6. Persetujuan dengan `SignerType = Spouse` dan teks "Istri" → cetakan V1 mencentang "Istri".
   `Self` → "Diri Sendiri". `Guardian` → "Lainnya".
7. Persetujuan dengan `GuidanceReceived = true` → kartu panduan menampilkan "Ya" baca-saja dan
   cetakan mencentang "Ya". Tidak ada radio panduan yang dapat diklik di mode lihat.
8. Episode lama tanpa persetujuan tersimpan → perilaku cetak saja yang sekarang tidak berubah.
9. Tidak ada UUID yang tampil di layar.

**Verifikasi.** `npx eslint` pada berkas yang disentuh (0 error) dan `npm run build`, mengikuti
memori pemilik bahwa uji `.mjs` dan E2E bukan gerbang selesai.

**Risiko.** Kanvas tanda tangan pada layar kecil. Dimensi kanvas mengikuti pola langkah admisi.

### FIX-EPS-008-03 — Panel Riwayat Perubahan (frontend)

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-008-02` |
| **Area** | Frontend Workspace PPRI |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | Service (`fetchAdmissionConsentHistory`); baru `general-consent-history-panel.jsx`; `general-consent-section.jsx` |
| **Radius dampak** | Menu General Consent saja |
| **Bergantung pada** | `FIX-EPS-008-01`, `FIX-EPS-008-02` (muat ulang sesudah simpan) |

**Langkah.**

1. Service `GET …/admission-workspace/general-consent/history`.
2. Panel di bawah formulir (tab Formulir General Consent). Satu baris per versi, terbaru di atas.
   Setiap baris memuat:
   - lencana "Aktif" atau "Digantikan";
   - waktu dalam format lokal dan nama pengubah;
   - alasan perubahan;
   - daftar "Label: lama → baru";
   - "Tanda tangan diganti" bila berlaku.
3. Versi 1 menampilkan "Dibuat pada langkah admisi".
4. Panel dimuat ulang sesudah edit berhasil.

**Acceptance criteria.**

1. Sesudah koreksi NIK oleh Dewi, panel menampilkan dua baris:
   - "Versi 2 · Aktif · 13 Okt 2026 10.15 · Dewi" dengan alasan dan "No. Identitas: 3171…9998 →
     3171…9999";
   - "Versi 1 · Digantikan · … · Sari — Dibuat pada langkah admisi".
2. Gagal memuat riwayat → pesan dan tombol Coba Lagi di dalam panel; form dan cetak tetap bekerja.
3. Pengguna tanpa `Update` tetap melihat riwayat bila punya `Read`.

**Verifikasi.** Sama dengan `FIX-EPS-008-02`.

**Risiko.** Tidak ada yang berarti; panel hanya baca.

### FIX-EPS-008-04 — Revisi dokumen blueprint

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-008-01` (sisi desain) |
| **Area** | Dokumen blueprint `rawat-inap` |
| **Jenis perubahan** | Dokumen (MODULE BLUEPRINT MODE) |
| **Berkas yang diubah** | Lihat bagian 7 |
| **Radius dampak** | Agent berikutnya membaca desain yang sama dengan source |
| **Bergantung pada** | `FIX-EPS-008-01` (kontrak final) |

**Acceptance criteria.**

1. `00-interview-decisions.md` memuat keputusan baru berisi permintaan pemilik 13-10-2026, contoh,
   dan acceptance criteria turunannya.
2. Tidak ada lagi kalimat dokumen desain yang menyatakan menu General Consent *read-only* untuk
   persetujuan bertanda tangan digital.

---

## 5. Urutan pengerjaan

```text
✅ FIX-EPS-008-01   tanpa dependency
├── ✅ FIX-EPS-008-02   memakai POST …/revisions, VersionNo, RowVersion, dan panduan dari FIX-01
│   └── ✅ FIX-EPS-008-03   memuat ulang riwayat sesudah edit FIX-02
└── ✅ FIX-EPS-008-04   mencatat kontrak final FIX-01
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- |
| K-01 | Apakah koreksi persetujuan memakai tanda tangan lama? | Tanda tangan lama dipakai untuk koreksi data; tanda tangan baru wajib bila jenis penanda tangan berganti; petugas boleh mengganti kapan pun | Tidak. Rekomendasi dipakai sebagai bawaan pada `FIX-EPS-008-01` langkah 4 dan dicatat di decision log; bila pemilik memilih lain, cukup mengubah satu validasi |

---

## 7. Dokumen hulu yang ikut direvisi

Status 13-10-2026: kelima baris di bawah sudah direvisi (`FIX-EPS-008-04`).

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `05-skema-tampilan.md` | Baris 705, prinsip satu pintu | "Menu PPRI berstatus **read-only / print-ready** sehingga staf PPRI hanya perlu mencetak …" | "Menu PPRI menampilkan dokumen siap cetak dan menyediakan **Edit Persetujuan** berversi untuk koreksi; setiap koreksi tercatat di Riwayat Perubahan (`PLAN-REPAIR-EPS-008`)" |
| `03-frontend-architecture.md` | 14.4.3 `FE-INP-36` | "cetak saja … tidak mengirim satu pun permintaan tulis" | Dua keadaan: (a) belum ada persetujuan tersimpan → cetak saja seperti sekarang; (b) bertanda tangan → mode lihat, mode edit, dan riwayat sesuai bagian 3 rencana ini |
| `contracts/api-contract.md` | Grup `Inpatient Admission Workspace` (12.2) dan `InpatientAdmissionConsent` | Belum tercatat | Tabel API `FIX-EPS-008-01` beserta delta route |
| `contracts/validation-matrix.md` | Sesudah `VAL-ADM-05` | — | Validasi edit: alasan 10–500, `RowVersion`, `TTD_BARU_WAJIB`, `TIDAK_ADA_PERUBAHAN` |
| `../00-interview-decisions.md` | Decision log | — | Keputusan baru: edit berversi + riwayat + K-01; acceptance criteria turunannya |

---

## 8. Verifikasi menyeluruh

1. Admisi Tn. Budi; langkah Formulir Persetujuan ditandatangani Ny. Rina (istri), NIK salah satu
   angka.
2. Workspace PPRI → General Consent → tab Formulir General Consent: tampil versi 1, hubungan
   "Istri" tercentang di pratinjau, dan panduan "Belum diisi".
3. Edit Persetujuan: perbaiki NIK, isi panduan "Ya", alasan "NIK penanda tangan salah ketik saat
   admisi", lalu Simpan. Lencana menjadi "Versi 2" dan riwayat menampilkan dua perubahan.
4. Tab Surat Persetujuan dan cetakannya memakai data versi 2.
5. Petugas lain yang masih membuka versi 1 lalu menyimpan → pesan "sudah diubah petugas lain".
6. Ganti jenis penanda tangan ke "Keluarga Lainnya" tanpa tanda tangan baru → ditolak; dengan
   tanda tangan baru → versi 3, "Tanda tangan diganti" tercatat.
7. Tutup episode → tombol Edit hilang, riwayat tetap terbaca.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-13 | Rencana dibuat, 4 perbaikan. Disetujui langsung oleh pemilik lewat permintaan "tolong di perbaiki ya"; K-01 memakai rekomendasi sebagai bawaan | `diagnose-module-issue` |
| 2026-10-13 | `FIX-EPS-008-01` ✅ sebagai `BE-RWI-227`. Delta: route edit/riwayat pindah ke grup Workspace PPRI (`POST …/general-consent/revisions`, `GET …/general-consent/history`) karena aturan akses role; AC 8 memakai kode sukses 201 yang sudah ada | `build-module-backend` |
| 2026-10-13 | `FIX-EPS-008-02` ✅ sebagai `FE-RWI-241`. Kotak tanda tangan memakai class modul Workspace PPRI, bukan class langkah admisi, karena sebagian class itu baru ada di working tree pekerjaan lain | `build-module-frontend` |
| 2026-10-13 | `FIX-EPS-008-03` ✅ sebagai `FE-RWI-242`; `FIX-EPS-008-04` ✅ lewat amandemen decision log (`RWI-DEC-295`, `RWI-DEC-296`, `RWI-AC-427` s.d. `431`) dan revisi `05-skema-tampilan.md`, `03-frontend-architecture.md` 14.4.3, `contracts/api-contract.md`, `contracts/validation-matrix.md` (`VAL-ADM-06` s.d. `09`). Status rencana `SELESAI` pada tingkat source; uji runtime bagian 8 masih menunggu pemilik | `build-module-frontend`, blueprint |
