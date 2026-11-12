# Acceptance Test Matrix — Modul Rekam Medis

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Contract version | `0.1.0` |
| Status | `draft` |
| Owner | Product/domain dan clinical governance authority: `OPEN` |
| Input revisions | `00-interview-decisions.md` revision `2`; `01-existing-capability-map.md` revision `1` |

> **PERINGATAN DASAR DESAIN.** Disusun di atas keputusan berstatus `draft`. Lihat `RM-DEC-025`.

---

## 1. Keadaan pengujian saat ini

Ini harus dinyatakan lebih dulu karena mengubah arti seluruh tabel di bawah.

| Kenyataan | Bukti |
|---|---|
| **Backend tidak memiliki project test sama sekali** | Penelusuran seluruh repository tidak menemukan project test apa pun |
| Frontend hanya memiliki 4 berkas test | `tests/e2e/auth-security.spec.mjs`, `tests/e2e/route-smoke.spec.mjs`, `tests/unit/auth-security.test.mjs`, `tests/unit/base-components-regression.test.mjs` |
| Tidak satu pun test menyentuh alur klinis | Penelusuran `tests/` tidak menemukan berkas bernama klinis |

Akibatnya: **membuat project test backend adalah prasyarat, bukan pelengkap.** Modul ini
menutup tiga celah pada kode yang sedang dipakai IGD dan antrean dokter (`RM-CAP-011`, `012`,
`013`). Mengubah kode berjalan tanpa jaring pengaman otomatis adalah risiko yang tidak
sebanding dengan penghematannya.

Apakah project test dibuat lebih dulu atau berjalan bersamaan adalah pertanyaan penyusunan
urutan kerja yang **belum diputuskan** — tercatat sebagai open question nomor 11 pada decision
log dan diteruskan ke `/plan-module-delivery`.

---

## 2. Matriks uji penerimaan

Kolom "Jenis test" memakai istilah: **Unit** untuk pengujian satu bagian kode, **Integrasi**
untuk pengujian yang menyentuh basis data, **E2E** untuk pengujian lewat antarmuka, dan
**Manual** untuk yang dibuktikan pengamatan orang.

### 2.1 Keutuhan dokumen

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-01` | `RM-DEC-003` | Mengubah CPPT berstatus `Signed` | Integrasi | Ditolak `400` dengan pesan yang mengarahkan ke addendum. Isi CPPT di basis data tidak berubah sedikit pun |
| `AT-RM-02` | `RM-DEC-003`, `RM-DEC-021` | Penulis menandatangani CPPT miliknya | Integrasi | Status menjadi `Signed`; `SignedAt`, `SignedByUserId`, dan `SignatureDeviceInfo` terisi; tidak ada permintaan kata sandi |
| `AT-RM-03` | `RM-DEC-003` lapis kedua | Kunjungan ditutup sementara ada dua CPPT `Draft` | Integrasi | Kedua CPPT menjadi `LockedUnsigned` dengan `LockTrigger = EncounterClosed`, dalam transaksi yang sama |
| `AT-RM-10` | `RM-DEC-003` | Mencoba mengembalikan dokumen `Signed` ke `Draft` | Integrasi | Ditolak. Tidak ada endpoint yang menyediakannya, dan pemanggilan langsung service ditolak |
| `AT-RM-11` | `RM-DEC-006` | Mencoba menandatangani dokumen `LockedUnsigned` | Integrasi | Ditolak `400` dengan pesan mengarahkan ke addendum |
| `AT-RM-18` | `RM-DEC-003` | Dokter membuka daftar catatan miliknya yang belum ditandatangani | Integrasi | Daftar hanya memuat dokumen `Draft` milik pengguna, bukan milik orang lain |
| `AT-RM-24` | `RM-DEC-013` | CPPT dibuat lewat endpoint yang sudah ada | Integrasi | Satu baris keutuhan berstatus `Draft` ikut tercipta; `AuthorUserId` terisi dari `ProviderUserId` |

### 2.2 Addendum dan kewenangan penulis

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-04` | `RM-DEC-004` | Penulis menambah addendum pada catatannya yang sudah `Signed` | Integrasi | Addendum tersimpan dengan `Sequence = 1`; isi CPPT asli **tidak berubah**; status dokumen tetap `Signed` |
| `AT-RM-05` | `RM-DEC-004` | Perawat mencoba menambah addendum pada catatan dokter yang akunnya masih aktif | Integrasi | Ditolak `403` dengan pesan bahwa hanya penulis yang dapat menambahkan koreksi |
| `AT-RM-14` | `RM-DEC-020` | Kepala unit menambah addendum pada catatan dokter yang akunnya sudah nonaktif | Integrasi | Diterima; `IsSubstituteAuthor` bernilai benar; `AuthorUserId` berisi kepala unit, **bukan** dokter yang nonaktif |
| `AT-RM-17` | `RM-DEC-004` | Menambah tiga addendum berturut-turut pada satu dokumen | Integrasi | `Sequence` berurut 1, 2, 3; status dokumen tetap `Signed` sepanjang ketiganya |
| `AT-RM-26` | `RM-DEC-020` | Kepala unit membuat penetapan berhalangan tanpa mengisi batas waktu | Integrasi | Ditolak `400`. Penetapan tanpa batas waktu tidak boleh tersimpan |
| `AT-RM-27` | `RM-DEC-020` | Menambah addendum memakai penetapan yang `ValidUntil`-nya sudah lewat | Integrasi | Ditolak `403` dengan pesan bahwa penetapan sudah berakhir |
| `AT-RM-28` | `RM-DEC-004` | Mencoba mengubah atau menghapus addendum yang sudah ada | Integrasi | Tidak ada endpoint yang menyediakannya |

### 2.3 Jejak dan kewenangan akses

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-06` | `RM-DEC-016` | Membuka rekam medis pasien yang punya kunjungan aktif | Integrasi | Isi dikembalikan tanpa diminta alasan; satu baris jejak `AccessType = RoutineCare` tercatat |
| `AT-RM-07` | `RM-DEC-005`, `RM-DEC-016` | Membuka rekam medis pasien tanpa kunjungan aktif, tanpa mengisi alasan | Integrasi | Ditolak `400`; **isi rekam medis tidak dikembalikan sama sekali**; tidak ada kebocoran sebagian |
| `AT-RM-08` | `RM-DEC-015` | Mencoba mengubah atau menghapus baris jejak akses | Integrasi | Tidak ada endpoint yang menyediakannya |
| `AT-RM-12` | `RM-DEC-015` | Membuka rekam medis sepuluh kali berturut-turut | Integrasi | Sepuluh baris jejak tercatat, tidak sembilan dan tidak sebelas |
| `AT-RM-13` | `RM-DEC-017` | Pengguna ber-role `SuperAdmin` membuka rekam medis pasien tanpa kunjungan aktif | Integrasi | Tetap diminta alasan; aksesnya tercatat dan ditandai perlu ditinjau |
| `AT-RM-16` | `RM-DEC-022` | Membuka `PrivateNote` pada pasien yang **punya** kunjungan aktif | Integrasi | Tetap diminta alasan; jejak tercatat dengan `AccessScope = PrivateNote` |
| `AT-RM-25` | `RM-DEC-016` | Penilaian kunjungan aktif gagal karena gangguan basis data | Integrasi | Akses diperlakukan sebagai beralasan, bukan sebagai rawatan. Kegagalan teknis tidak melonggarkan kewenangan |
| `AT-RM-29` | `RM-DEC-005` | Petugas rekam medis membuka antrean tinjauan | Integrasi | Hanya baris `IsFlaggedForReview` bernilai benar yang muncul |
| `AT-RM-30` | `RM-DEC-015` | Penulisan jejak akses gagal | Integrasi | Permintaan dijawab `503`; **isi rekam medis tidak dikembalikan** |

### 2.4 Penelusuran berkas

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-09` | `RM-DEC-002` | Membuka riwayat pasien yang punya tiga kunjungan berbeda | Integrasi | Dokumen dari ketiga kunjungan tampil dalam satu daftar berurut waktu, tanpa perlu membuka kunjungan satu per satu |
| `AT-RM-22` | `RM-CAP-007` | Membuka rekam medis pasien yang `MergedToPatientId`-nya terisi | Integrasi | Ditolak `409` disertai nomor rekam medis pengganti. **Tidak** menampilkan riwayat yang terpotong |
| `AT-RM-31` | `RM-DEC-002` | Membuka riwayat pasien dengan sangat banyak dokumen | Integrasi | Jumlah baris dibatasi; penyaringan rentang tanggal berfungsi; tidak ada permintaan yang berjalan tanpa batas |
| `AT-RM-32` | Bagian 7 arsitektur backend | Membuka riwayat yang memuat jenis dokumen selain CPPT | E2E | Dokumen selain CPPT tampil **tanpa** status keutuhan, disertai keterangan bahwa jenis itu belum tunduk aturan keutuhan |

### 2.5 Perubahan perilaku pada kode berjalan

Kelompok ini yang paling penting, karena menyentuh alur yang sedang dipakai.

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-19` | `RM-CAP-012` | Mengirim `ProviderUserId` orang lain pada permintaan ubah CPPT | Integrasi | Permintaan tidak gagal, tetapi `ProviderUserId` di basis data **tidak berubah**, dan `AuthorUserId` pada baris keutuhan juga tidak |
| `AT-RM-20` | `RM-CAP-013` | Mengirim `IsReadOnlyGenerated = false` pada CPPT yang read-only | Integrasi | Nilai di basis data **tidak berubah** |
| `AT-RM-21` | `RM-DEC-014` | Menjalankan migration pengisian data lama | Integrasi | CPPT pada kunjungan selesai bernilai `LockedUnsigned`; pada kunjungan berjalan bernilai `Draft`; yang dibatalkan bernilai `Cancelled` |
| `AT-RM-33` | `RM-DEC-014` | Migration pengisian data lama pada CPPT tanpa `ProviderUserId` | Integrasi | Baris tetap dibuat dengan `IsAuthorKnown = false`, tidak dilewati diam-diam |
| `AT-RM-34` | `RM-DEC-019` | Alur antrean dokter dijalankan penuh setelah perubahan | E2E | Menulis, menyimpan, dan menandatangani CPPT berjalan; alur IGD tidak terganggu |
| `AT-RM-35` | Integration contract 2.1 | Pendaftaran keutuhan gagal saat CPPT dibuat | Integrasi | Pembuatan CPPT ikut dibatalkan; tidak ada CPPT tanpa baris keutuhan |
| `AT-RM-36` | Integration contract 2.2 | Penguncian gagal saat kunjungan ditutup | Integrasi | Penutupan kunjungan ikut dibatalkan; kunjungan tetap terbuka |

### 2.6 Privasi dan pencatatan

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-23` | Aturan output dokumentasi | Membuat addendum lalu memeriksa isi log | Integrasi | `AddendumText`, `CorrectionReason`, dan `AccessReason` **tidak muncul** di log. Hanya `EntityId`, controller, action, dan status |
| `AT-RM-37` | `RM-DEC-022` | Memeriksa respons riwayat biasa | Integrasi | `PrivateNote` **tidak ada** di respons mana pun selain endpoint khususnya |
| `AT-RM-38` | Kamus data bagian 4 | Memeriksa isi tabel jejak akses | Integrasi | Tidak ada kolom yang memuat isi klinis |

### 2.7 Antarmuka

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-39` | `RM-FE-008` | Melihat satu dokumen pada layar rekam medis | E2E | Status keutuhan dan status alur kerja terlihat berbeda, tidak menyatu jadi satu penanda |
| `AT-RM-40` | `RM-FE-002` | Melihat dokumen yang punya addendum | E2E | Addendum tampil menempel pada dokumen induknya, bukan sebagai entri terpisah |
| `AT-RM-41` | `RM-FE-003` | Membuka pasien tanpa kunjungan aktif | E2E | Kotak isian alasan muncul **sebelum** isi rekam medis terlihat, bukan sesudah |
| `AT-RM-42` | `RM-FE-006` | Melihat dokumen bertanda `VeryConfidential` | E2E | Keterangan bahwa label kerahasiaan belum membatasi akses terlihat jelas |
| `AT-RM-43` | `RM-DEC-014` | Petugas rekam medis melihat laporan kelengkapan setelah migration | Manual | Banyaknya catatan bertanda tidak ditandatangani terlihat, dan sudah dijelaskan lebih dulu kepada petugas |

---

## 3. Jalur gagal yang wajib diuji

Diringkas terpisah karena inilah yang paling sering terlewat. Seluruhnya sudah ada di tabel
atas; daftar ini memastikan tidak ada yang luput saat penyusunan urutan kerja.

| Jalur gagal | ID |
|---|---|
| Mengubah dokumen terkunci | `AT-RM-01` |
| Menandatangani catatan orang lain | `AT-RM-02` sisi negatifnya |
| Menandatangani dokumen yang sudah terkunci | `AT-RM-11` |
| Addendum oleh yang tidak berwenang | `AT-RM-05` |
| Addendum memakai penetapan kedaluwarsa | `AT-RM-27` |
| Penetapan tanpa batas waktu | `AT-RM-26` |
| Akses tanpa alasan pada pasien tanpa kunjungan aktif | `AT-RM-07` |
| `SuperAdmin` tanpa alasan | `AT-RM-13` |
| Penilaian kunjungan gagal | `AT-RM-25` |
| Pencatatan jejak gagal | `AT-RM-30` |
| Pendaftaran keutuhan gagal | `AT-RM-35` |
| Penguncian saat penutupan gagal | `AT-RM-36` |
| Pasien hasil penggabungan | `AT-RM-22` |
| Data lama tanpa penulis | `AT-RM-33` |

---

## 4. Yang tidak dapat diuji otomatis

Dinyatakan terbuka supaya tidak dianggap sudah terbukti padahal belum.

| Hal | Alasan | Cara membuktikannya |
|---|---|---|
| Jumlah catatan lama yang akan berstatus `LockedUnsigned` | Data produksi tidak diaudit (batas audit nomor 3) | Dijalankan pada salinan data nyata sebelum rilis |
| Lama waktu migration pengisian data lama | Jumlah barisnya belum diketahui | Sama seperti di atas |
| Apakah dokter benar-benar menandatangani catatannya | Perilaku manusia | Diamati dari perbandingan `Signed` dan `LockedUnsigned` setelah beberapa pekan |
| Apakah alasan akses diisi bermakna atau asal | Perilaku manusia | Tinjauan berkala unit rekam medis |
| Apakah pasien dengan dua nomor rekam medis benar ada | `RM-CAP-007` masih `Unknown` | Penelusuran data sebelum rilis |

Baris ketiga dan keempat pantas diperhatikan. Keduanya adalah cara mengetahui apakah desain ini
benar-benar bekerja atau hanya tampak bekerja. Bila setelah sebulan hampir seluruh catatan
berstatus `LockedUnsigned`, berarti alur penandatanganan terlalu merepotkan dan perlu ditinjau
ulang — bukan berarti dokternya lalai.

---

## 5. Traceability ringkas

| Decision | Uji yang membuktikannya |
|---|---|
| `RM-DEC-003` | `AT-RM-01`, `02`, `03`, `10`, `11` |
| `RM-DEC-004` | `AT-RM-04`, `05`, `17`, `28` |
| `RM-DEC-005` | `AT-RM-07`, `29` |
| `RM-DEC-006` | `AT-RM-11` |
| `RM-DEC-013` | `AT-RM-24` |
| `RM-DEC-014` | `AT-RM-21`, `33`, `43` |
| `RM-DEC-015` | `AT-RM-08`, `12`, `30` |
| `RM-DEC-016` | `AT-RM-06`, `07`, `25` |
| `RM-DEC-017` | `AT-RM-13` |
| `RM-DEC-018` | `AT-RM-42` |
| `RM-DEC-019` | `AT-RM-19`, `20`, `34` |
| `RM-DEC-020` | `AT-RM-14`, `26`, `27` |
| `RM-DEC-021` | `AT-RM-02` |
| `RM-DEC-022` | `AT-RM-16`, `37` |


---

# Amandemen revision 4 — Uji penerimaan `AT-RM-44` sampai `AT-RM-74` (`approved`)

Disahkan Yoga Aji Pratama, 28 Oktober 2026 (`RM-DEC-062`).

Kolom "AC" menunjuk acceptance criteria pada `00-interview-decisions.md`. Jalur gagal diberi tanda **(gagal)**.

## Sambungan berkas

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-44` | AC-25, `RM-DEC-038` | Pasien dengan satu dokumen final di tiap modul membuka berkas | Integrasi (harness) | Setiap dokumen muncul di linimasa; nol baris salinan di tabel `Mrc*` |
| `AT-RM-45` | AC-26, `RM-DEC-041` **(gagal)** | Laporan radiologi `Drafted`, ringkasan pulang tanpa `SignedAt` | Integrasi | Keduanya **tidak** tampil |
| `AT-RM-46` | AC-27, `RM-DEC-040` | Pengguna hanya punya `MedicalRecord : Read` | Integrasi | Dokumen semua modul tampil; tepat satu baris `MrcAccessLog` |
| `AT-RM-47` | AC-28, `RM-DEC-043` | Hasil dirilis 1 detik sesudah `ClinicalClosedAt` dan 1 detik sebelumnya | Unit | Pertama `isLateEntry = true`, kedua `false` |
| `AT-RM-48` | AC-29, `RM-DEC-042` | Laporan radiologi versi 2; ringkasan pulang punya revisi | Integrasi | `isCorrected = true` pada keduanya |
| `AT-RM-49` | AC-30, `RM-DEC-044` **(gagal)** | Pembaca IGD dipalsukan melempar galat | Integrasi | Berkas `200`; IGD di `failedSources`; sumber lain tampil |
| `AT-RM-50` | `GAP-ARCH-RM-05` **(gagal)** | Pembaca Farmasi dipalsukan menunggu 10 detik | Integrasi | Farmasi di `failedSources` sebagai batas waktu; permintaan **tidak** batal seluruhnya |
| `AT-RM-51` | AC-39, AC-49, `RM-DEC-058` | Pasien dengan pemeriksaan PA yang dirilis | Integrasi | PA tidak tampil; `unavailableKinds` memuat `PathologyAnatomyResult` |
| `AT-RM-52` | AC-42, `RM-DEC-052` | Enam pengukuran gula darah satu hari, satu dibatalkan | Integrasi | Satu baris ringkasan berjumlah 5; rincian memuat 5 |
| `AT-RM-53` | AC-48, `RM-DEC-057` | Hasil PK dirilis, hasil Mikrobiologi belum | Integrasi | PK tampil; Mikrobiologi tidak |
| `AT-RM-54` | AC-51, `RM-DEC-059` | Pasien dengan catatan asuhan gizi | Integrasi | `isProvisional = true` |
| `AT-RM-55` | `VAL-RM-A12` **(gagal)** | Meminta rincian dokumen PA | API | `404` dengan pesan "belum tersedia" |

## Penutupan klinis dan kelengkapan

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-56` | `AGG-RM-02` | Kunjungan ditutup dua kali lewat dua jalur | Integrasi | Tepat satu baris `MrcEncounterClinicalClosure`; waktu = penutupan pertama |
| `AT-RM-57` | AC-41, `RM-DEC-051` **(gagal)** | Rekonsiliasi IGD mengubah `CompletedAt` | Integrasi | `ClinicalClosedAt` tidak berubah; penanda `Susulan` tidak berubah |
| `AT-RM-58` | AC-40 | Episode RI ditutup Senin, tagihan final Kamis, hasil dirilis Selasa | Integrasi | Hasil `Susulan`; kunjungan masuk daftar kerja sejak Senin |
| `AT-RM-59` | `GAP-ARCH-RM-01` | Disposisi IGD ke rawat inap | Integrasi | Catatan konsep IGD menjadi `LockedUnsigned`; baris penutupan klinis tercatat |
| `AT-RM-60` | AC-31, AC-47 | Kunjungan RI tanpa ringkasan pulang, hanya persetujuan registrasi | Integrasi | `Incomplete`; dua rincian |
| `AT-RM-61` | AC-32 | Kunjungan RJ tanpa operasi; kunjungan RI dengan operasi tanpa catatan anestesi | Integrasi | RJ tidak dituntut laporan operasi; RI `Incomplete` "catatan anestesi belum ada" |
| `AT-RM-62` | AC-36, `RM-FE-023` | Ringkasan pulang tidak ada dan kultur belum dirilis | Integrasi | `AwaitingResult` dengan dua rincian |
| `AT-RM-63` | AC-30, `RM-DEC-044` **(gagal)** | Pembaca Kamar Operasi gagal saat menilai | Integrasi | `Undetermined`; **tidak pernah** `Complete` |
| `AT-RM-64` | AC-35, `VAL-RM-A08`, `A09` **(gagal)** | Nilai ulang kunjungan berjalan dan kunjungan batal | API | `409` dengan pesan masing-masing; keduanya tidak muncul di daftar kerja |
| `AT-RM-65` | AC-38, `RM-DEC-050` | Kunjungan yang tutup sebelum rilis | API | Tidak di daftar kerja; `evaluate` mengembalikan `isPersisted = false`; nol baris tersimpan |
| `AT-RM-66` | AC-52, AC-53, AC-54, `RM-DEC-060` | RJ di unit Laboratorium, Hemodialisa, dan poliklinik | Integrasi | Lab `Complete` setelah dirilis tanpa dokumen dokter; HD cukup sesi bertanda tangan; poliklinik tetap dituntut tiga butir |
| `AT-RM-67` | AC-45, AC-43, AC-44, AC-46 | Diagnosis `Primary` tanpa `IsPrimary`; asesmen ulang saja; konsultasi tanpa CPPT | Unit | Masing-masing: terpenuhi; tidak terpenuhi; terpenuhi |
| `AT-RM-68` | AC-34, `RM-DEC-046` | Kunjungan belum lengkap ditutup dan pasien dipulangkan | Integrasi | Keduanya berhasil; nol penolakan dari rekam medis |

## Profil dokumen wajib

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-69` | AC-33 | Menambah kriteria pada profil `RJ` lewat API | Integrasi | Kunjungan RJ yang belum `Complete` dinilai ulang dan menampilkan kekurangan baru, tanpa rilis program |
| `AT-RM-70` | `VAL-RM-A04`–`A07` **(gagal)** | Profil tanpa jenis kunjungan; kode kembar; kriteria kembar | API | `400`, `409`, `409` |
| `AT-RM-71` | `RuleNotFound` | Kunjungan berjenis `Unknown` | Integrasi | `Undetermined` dengan rincian aturan tidak ada |

## Addendum pengesahan

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-72` | AC-37, `RM-DEC-049` | Penulis mengesahkan catatan `LockedUnsigned` | Integrasi | Addendum `Attestation` tersimpan; status tetap `LockedUnsigned`; rincian "tidak ditandatangani" hilang |
| `AT-RM-73` | `VAL-RM-A01`, `A02` **(gagal)** | Pengesahan atas catatan `Signed`; pengesahan kedua | API | `409` dua-duanya |
| `AT-RM-74` | `VAL-RM-A03` **(gagal)** | Pengguna lain tanpa jalur pengganti mencoba mengesahkan | API | `403` |


---

# Amandemen revision 5 — Uji penerimaan `AT-RM-75` sampai `AT-RM-92` (`approved`)

Disahkan Yoga Aji Pratama, 10 November 2026 (`RM-DEC-088`).

Kolom "AC" menunjuk acceptance criteria revision 14/15 pada `00-interview-decisions.md`. Jalur
gagal diberi tanda **(gagal)**.

## Profil dan penilaian Optik

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-75` | `RM-AC-R14-01`, `RM-DEC-080` | Kunjungan unit Optik, beli frame saja, tutup klinis | Integrasi (harness) | Profil `RJ-OPT`; keadaan `Complete`; tidak ada kekurangan asesmen/diagnosis/CPPT |
| `AT-RM-76` | `RM-AC-R14-02` **(gagal)** | Refraksi masih draf saat tutup klinis | Integrasi | `Incomplete`; kekurangan `UnsignedDocument` jenis `OpticalRefraction` |
| `AT-RM-77` | `RM-AC-R14-03` | Resep direvisi ke versi 2 | Integrasi | Linimasa versi 2, `isCorrected = true`; mengubah versi 1 ditolak `EnsureMutableAsync` |
| `AT-RM-78` | `RM-AC-R15-03` | Dokumen Optik versi 1 dengan addendum `Correction` lama | Unit pembaca Optik | `isCorrected = true` |
| `AT-RM-79` | `RM-AC-R14-07` | Kacamata `IN_WORK` sesudah tutup klinis, dokumen bertanda tangan | Integrasi | Keadaan bukan `AwaitingResult` |
| `AT-RM-80` | `RM-AC-R14-08` | Kunjungan Optik ber-`ReferralSourceEncounterId` | Integrasi | Kekurangan kunjungan asal dan tujuan tidak bertambah |

## Profil berpoli dan Rehab

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-81` | `RM-AC-R15-04`, A5.9.1 | Empat urutan pemilihan profil | Unit | Profil terpilih sesuai urutan |
| `AT-RM-82` | `RM-AC-R15-05`, `RM-AC-R14-09` | Kunjungan poli umum sesudah migration | Integrasi | Profil `RJ`; hasil sama dengan sebelum migration |
| `AT-RM-83` | `RM-AC-R14-05`, `RM-AC-R15-06` | Kunjungan poli rehab dengan (a) sesi final, (b) reasesmen saja, (c) ringkasan akhir saja | Integrasi | Ketiganya `Complete` tanpa CPPT/diagnosis |
| `AT-RM-84` | `RM-AC-R14-06`, `RM-AC-R15-07` **(gagal)** | Kunjungan poli rehab (a) tanpa dokumen, (b) edukasi saja, (c) program saja, (d) sesi draf | Integrasi | `Incomplete`, rincian butir `RehabClinicalDocument`; (d) juga `UnsignedDocument` |
| `AT-RM-85` | `RM-AC-R15-08` | Probe Rehab pada kunjungan dengan sesi berikutnya terjadwal | Unit probe | `PendingResults` kosong |
| `AT-RM-86` | `RM-AC-R15-09` **(gagal)** | Probe Rehab dibuat galat/lewat batas waktu | Integrasi | `Undetermined`; kekurangan `SourceUnavailable` |
| `AT-RM-87` | `RM-AC-R14-04` | Catatan sesi rehab bertanda tangan diubah, lalu dikoreksi lewat addendum | Integrasi | Ubah ditolak; addendum diterima; `isCorrected = true` |
| `AT-RM-88` | `DS-R5-04` | Catatan rehab `LockedUnsigned` hanya disahkan | Unit pembaca Rehab | `isCorrected = false` |

## Master dan addendum

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-89` | `VAL-RM-B02`, `B03` **(gagal)** | Profil berkondisi `Surgery` dengan `clinicType`; `clinicType = 999` | Integrasi controller | `400` dengan pesan kontrak |
| `AT-RM-90` | `VAL-RM-B04` **(gagal)** | Profil kedua dengan kunci empat kolom yang sama | Integrasi; **wajib** `TegakkanKunciUnikAsync` | `409` |
| `AT-RM-91` | `RM-AC-R15-01` **(gagal)** | Addendum `Correction` pada refraksi `Signed` dan `LockedUnsigned`; `GET /authority` | Integrasi controller | `400` `VAL-RM-B01`; `isCorrectionAllowed = false` |
| `AT-RM-92` | `RM-AC-R15-02` | Addendum `Attestation` pada refraksi `LockedUnsigned` oleh penulis | Integrasi | `201`; kekurangan tuntas pada penilaian berikutnya |

## Uji antarmuka (UAT)

| ID | Skenario | Bukti |
|---|---|---|
| `UAT-20` | Berkas pasien berisi dokumen Optik dan Rehab final | Delapan jenis tampil berlabel benar; saringan jenis bekerja (`RM-UN-R4-02`) |
| `UAT-21` | Dokumen Optik bertanda tangan dibuka | Tidak ada tombol koreksi; keterangan arahan ke layar Optik (`RM-FE-030`) |
| `UAT-22` | Profil `RJ-REHAB` dibuat lewat layar; pilih kondisi "Operasi" | Jenis poli terkunci kosong (`RM-FE-032`) |
| `UAT-23` | Kunjungan poli rehab dengan satu sesi final di daftar kerja | Tidak muncul sebagai belum lengkap. **Prasyarat:** poli rehab ada di master (`RM-UN-R4-01`) |

---

# Amandemen revision 6 — Uji penerimaan `AT-RM-93` sampai `AT-RM-108` (`approved`)

Disahkan Yoga Aji Pratama, 11 November 2026 (`RM-DEC-108`).

Kolom "AC" menunjuk acceptance criteria revision 19/20 pada `00-interview-decisions.md`. Jalur gagal
diberi tanda **(gagal)**. `RM-AC-R19-07` sudah `superseded` dan tidak diuji.

## Indikator ketepatan ringkasan pulang

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-93` | `RM-AC-R19-04`, `RM-DEC-101` | Tarik Oktober 2026 untuk satu dokter | Integrasi controller | Angka per kolom sesuai data uji; JSON mentah tanpa `patientId`, `encounterId`, `episodeId`, nomor RM; satu baris audit dengan periode di teks pesan |
| `AT-RM-94` | `RM-AC-R19-05` **(gagal)** | Pengguna tanpa `DischargeSummaryTimeliness : Read`; pengguna yang hanya punya `MedicalRecordCompleteness : Read` | Integrasi controller | `403` untuk keduanya |
| `AT-RM-95` | `RM-AC-R19-06` | Pulang 09.00, tanda tangan 08.59 esok hari, lalu dikoreksi dan ditandatangani ulang 2 hari kemudian | Integrasi (harness) | `within24h`; tetap `within24h` sesudah koreksi |
| `AT-RM-96` | `RM-AC-R20-02` | Tanda tangan, koreksi dalam sesi, tanda tangan ulang | Unit pembaca | Jam selesai = `PreviousSignedAt` versi terawal; dokter = penanda tangan pertama |
| `AT-RM-97` | `RM-AC-R20-03` | Keluar 51 jam sebelum `waktuTarik`, belum ditandatangani | Unit pengelompokan | `over48h` dan `unsignedOver48h` naik satu, atas DPJP aktif |
| `AT-RM-98` | `RM-AC-R20-04` | Keluar 4 jam sebelum `waktuTarik`, belum ditandatangani | Unit pengelompokan | Hanya `notYetDueCount`; tidak masuk `assessedCount` |
| `AT-RM-99` | `RM-AC-R20-05` | Ditandatangani 2 jam sebelum keluar | Unit | `within24h`; `signedBeforeDepartureCount` naik |
| `AT-RM-100` | `RM-AC-R20-06` | Episode ditutup, `PhysicallyLeftAt` kosong | Unit pembaca | Jam mulai `ClosedAt`; `closedAtFallbackCount` naik |
| `AT-RM-101` | `RM-AC-R20-07` | Episode `DischargePending` tanpa kepergian; episode `Cancelled` | Unit pembaca | Keduanya tidak terbaca |
| `AT-RM-102` | `DS-R6-07` | Selisih tepat 24:00:00 dan tepat 48:00:00 | Unit | `within24h` dan `between24And48h` |
| `AT-RM-103` | `DS-R6-03` | Belum ditandatangani, tanpa DPJP aktif | Unit | Item `doctorId = null` "Tanpa DPJP aktif" di urutan terakhir |
| `AT-RM-104` | `VAL-RM-C01`–`C04` **(gagal)**; `RM-AC-R19-08` | Periode kosong, terbalik, 367 hari, `pageSize = 0`; kunjungan Rawat Jalan/IGD dengan CPPT terlambat | Integrasi controller | `400` dengan pesan kontrak; Rawat Jalan/IGD tidak memengaruhi angka |
| `AT-RM-108` | Integration "Kegagalan" **(gagal)** | Pembaca Rawat Inap dibuat galat | Integrasi | `503`; tidak ada angka sebagian; tidak ada audit |

## Rencana kontrol dan tanda tangan ulang

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-105` | `RM-AC-R19-11` | Ringkasan dengan tanggal, poli, dokter; lalu tanggal + poli saja | Unit pembaca (`InpMedicalRecordDocumentSourceTests.cs`) | Bagian "Rencana kontrol" berisi teks A6.9.4 |
| `AT-RM-106` | `RM-AC-R19-12` | Ringkasan tanpa rencana kontrol | Unit pembaca | Bagian tidak ada; status kelengkapan tidak berubah |
| `AT-RM-107` | `RM-AC-R20-01` **(gagal)** | `PATCH .../summary/sign` pada ringkasan bertanda tangan tanpa sesi koreksi | Integrasi (lokasi uji milik Rawat Inap) | `409`; `SignedAt` tidak berubah. **Hanya dijalankan setelah persetujuan pemilik Rawat Inap** |

## Batas yang dinyatakan

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| — | `RM-AC-R19-01`–`03`, `09`, `10` | Tidak ada perubahan kode | Uji arsitektur ringan (A6.10) + regresi `MedicalRecordTests` | Tidak ada rujukan tipe HR terlarang; akun nonaktif ditolak (`RM-CAP-094`, sudah hidup) |

## Uji antarmuka (UAT)

| ID | Skenario | Bukti |
|---|---|---|
| `UAT-24` | Berkas pasien Rawat Inap dengan rencana kontrol dibuka | Bagian "Rencana kontrol" tampil di panel detail tanpa perubahan frontend |
