# Laporan Perubahan Backend — `BE-RWI-127`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-127` |
| Judul | Perbaikan temuan pengujian dokter rawat inap — culture invariant, isi resep, dan status code create |
| Slice | Perbaikan defect pasca-pengujian; bukan slice fitur baru |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap-v2.md` |
| Trace | `ISSUE-DOK-001` butir `ISS-01`, `ISS-03`, `ISS-04`, `ISS-06`; `BE-RWI-050`; `RWI-DEC-046` |
| Contract version | `0.6.0` → `0.6.1` — penambahan `Items` dan `Compounds` pada `CreatePrescriptionRequest` disetujui pemilik pada 23 September 2026 |
| Dependency | Tidak ada task pendahulu. `FE-RWI-095` adalah pasangan frontendnya dan dikerjakan setelah task ini |
| Klasifikasi | `MEDIUM` — skor 6 (repository 0, berkas diperiksa 1, berkas diubah 1, logika 1, kontrak API 2, database 1, keamanan 0, UI 0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi dan `docs/module-blueprints/rawat-inap/dokter-rawat-inap/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `dd860cd6847f637c90c60af225f6ba29420c54b9` |
| Tanggal | 23 September 2026 |
| Status | Selesai untuk lingkup backend; verifikasi runtime menunggu backend dijalankan ulang oleh pemilik |

---

## 1. Masalah yang diperbaiki

Pengujian pada 22 September 2026 menemukan fitur resep dokter rawat inap tidak dapat dipakai sama
sekali. Tiga masalah backend berikut diperbaiki pada task ini.

**Pertama, seluruh penyimpanan obat gagal dengan galat server.** Ketika dokter menambahkan obat ke
resep, server membalas `HTTP 500`. Penyebabnya bukan pada modul resep, melainkan pada cara aplikasi
membaca angka. Atribut pembatas nilai ditulis sebagai teks, misalnya batas terkecil `"0.0001"`. Pada
komputer yang bahasanya diatur Indonesia, titik dibaca sebagai pemisah ribuan — bukan pemisah
desimal — sehingga teks `"0.0001"` dianggap bukan angka yang sah dan aplikasi berhenti dengan galat
sebelum permintaan dokter sempat diproses. Kegagalannya tidak bergantung pada angka yang diketik
dokter: permintaan apa pun yang menyentuh formulir tersebut ikut gagal.

Cakupannya jauh melampaui resep. Di seluruh aplikasi terdapat **104** atribut pembatas bertipe
desimal, dan **54** di antaranya memakai batas pecahan sehingga berisiko — tersebar di **21 berkas**
milik Farmasi, Master Data, Billing, Kepegawaian, Hemodialisa, dan Keuangan. Batas bulat seperti
`"0"` atau `"999999999"` tetap terbaca benar dan tidak terdampak.

**Kedua, obat yang sudah dipilih dokter hilang tanpa kabar.** Saat menyimpan draf resep, frontend
mengirimkan daftar obat dan racikan, tetapi formulir penerima di backend tidak mengenal kedua daftar
itu. Data yang tidak dikenal dibuang diam-diam, sehingga yang tersimpan hanya kepala resepnya. Dokter
menerima notifikasi "berhasil disimpan" untuk resep yang isinya kosong. Inilah yang paling berbahaya
dari seluruh temuan: kegagalannya tidak terlihat. Contohnya, dokter meresepkan Ceftriaxone dan
Paracetamol, menekan simpan, melihat pesan sukses — dan resep yang sampai ke Farmasi tidak berisi
satu butir obat pun.

**Ketiga, jawaban server untuk operasi pembuatan data tidak seragam.** Sebagian endpoint menjawab
`201 Created` (kunjungan dokter, pesanan laboratorium, tindakan pasien), sebagian lagi `200 OK`
(radiologi, kajian pasien, resep). Perbedaan ini tidak menggagalkan apa pun, tetapi menyulitkan
siapa pun yang memeriksa keberhasilan secara seragam.

---

## 2. Proses bisnis

**Tujuan.** Dokter penanggung jawab pelayanan meresepkan obat untuk pasien rawat inap, dan resep itu
sampai ke Instalasi Farmasi lengkap beserta isinya.

**Pelaku.** Dokter rawat inap sebagai penulis resep; petugas Farmasi sebagai pembaca resep.

**Pemicu.** Dokter membuka tab Resep pada lembar kerja pasien, lalu menyusun draf resep.

**Langkah berurutan sesudah perbaikan:**

1. Dokter memilih catatan dokter (SOAP) yang menjadi dasar resep, dan menentukan jenis resepnya.
2. Dokter mencari obat pada katalog formularium, lalu menambahkannya beserta aturan pakai.
3. Dokter menekan simpan draf. Frontend mengirim kepala resep **beserta** daftar obat dan racikan.
4. Backend memeriksa kunci permintaan lebih dulu. Bila kunci yang sama pernah dipakai, resep yang
   sudah ada dikembalikan apa adanya — pengiriman ulang karena jaringan terputus tidak melahirkan
   resep kedua.
5. Backend memvalidasi permintaan, menyiapkan konteks penjaminan pasien, lalu membuka satu transaksi.
6. Di dalam transaksi itu: nomor resep diterbitkan, kepala resep disimpan, lalu obat dan racikannya
   ikut disimpan lewat jalur penyimpanan yang sama persis dengan yang dipakai penyimpanan otomatis
   workspace.
7. Ringkasan resep dihitung ulang, transaksi dikunci, dan server menjawab `201 Created`.

**Aturan yang berlaku.**

- Isi resep bersifat opsional. Resep yang isinya disusun bertahap lewat workspace tetap sah dibuat
  sebagai kepala saja, dan perilaku lama itu tidak berubah.
- Kepala resep dan isinya berada pada **satu** transaksi. Bila penyimpanan obat gagal, kepala
  resepnya ikut dibatalkan. Tidak akan pernah ada resep yang kepalanya terbit tetapi obatnya hilang.
- Jenis resep tetap distempel dari `PrescriptionOrderType` sesuai `BE-RWI-050` dan `RWI-DEC-046`.
  Nama properti ini dipertahankan sebagai kontrak yang sah; frontend yang menyesuaikan diri.

**Status yang dihasilkan.** Resep berstatus `Draft`, pembayaran `NotBilled`, pemenuhan
`WaitingForPayment` — tidak berubah dari sebelumnya.

**Jalur tidak normal.**

- Kunci permintaan sudah pernah dipakai → `200 OK` beserta resep yang sudah ada, bukan `201`.
- Validasi permintaan gagal → `400 Bad Request`, tidak ada yang tersimpan.
- Konteks penjaminan tidak sah → `400 Bad Request`, tidak ada yang tersimpan.
- Penyimpanan obat gagal di tengah jalan → seluruh transaksi dibatalkan, termasuk kepala resepnya.

**Hasil akhir.** Resep beserta obatnya tersimpan utuh dan terbaca oleh Instalasi Farmasi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md`; `rules/backend/` — `TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `API_RULES.md`,
  `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`, `TEST_POLICY.md`,
  `engineering/BACKEND_ENGINEERING_CONTRACT.md`
- `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/issues/issue-001-perbaikan-hasil-testing-dokter-rawat-inap.md`
- `Program.cs`; `Responses/ApiResponse.cs`
- `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs`,
  `PrescriptionWorkspaceController.cs`
- `Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkspaceService.cs`
- `Areas/HealthServices/PharmacyManagement/DTOs/PrescriptionDtos.cs`,
  `PrescriptionWorkspaceDtos.cs`, `PrescriptionItemDtos.cs`
- `Areas/HealthServices/ClinicalManagement/Controllers/PhysicianVisitController.cs` (rujukan pola `201`),
  `PatientAssessmentController.cs`, `PrescribingDrugController.cs`
- `Areas/HealthServices/RadiologyManagement/Controllers/RadOrderController.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Program.cs` | `ISS-01`. Culture aplikasi dikunci ke `CultureInfo.InvariantCulture` sebelum `WebApplication.CreateBuilder`, beserta `using System.Globalization`. Satu titik ini menutup seluruh 54 atribut berisiko sekaligus dan mencegah atribut baru mengulang cacat yang sama |
| `.../PharmacyManagement/DTOs/PrescriptionDtos.cs` | `ISS-03`. `CreatePrescriptionRequest` menerima `Items` dan `Compounds`, memakai ulang tipe `AutosavePrescriptionItemRequest` dan `AutosavePrescriptionCompoundRequest` supaya bentuk isi resep hanya ada satu |
| `.../PharmacyManagement/Services/PrescriptionWorkspaceService.cs` | `ISS-03`. Badan penyimpanan isi resep dipisah ke `ApplyWorkspaceChangesAsync` yang tidak mengelola transaksi. `AutosaveAsync` tetap memegang transaksinya sendiri; `ApplyDraftContentAsync` ditambahkan untuk dipanggil dari transaksi yang sudah berjalan |
| `.../PharmacyManagement/Controllers/PrescriptionController.cs` | `ISS-03` dan `ISS-06`. `PrescriptionWorkspaceService` di-inject; isi resep disimpan di dalam transaksi yang sama dengan kepalanya; jawaban sukses menjadi `201 Created` |
| `.../RadiologyManagement/Controllers/RadOrderController.cs` | `ISS-06`. Helper `Execute` menerima `successStatusCode` dengan nilai bawaan `200` sehingga endpoint lain tidak terpengaruh; hanya `POST /rad-orders` yang memakai `201` |
| `.../ClinicalManagement/Controllers/PatientAssessmentController.cs` | `ISS-06`. `POST /patient-assessments` menjawab `201 Created` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Berubah, dan sudah disetujui pemilik. (1) `CreatePrescriptionRequest` bertambah dua properti opsional — bersifat menambah, permintaan lama tanpa `Items`/`Compounds` tetap sah dan perilakunya tidak berubah. (2) Tiga endpoint create berpindah dari `200` ke `201`. Konsumen frontend sudah diperiksa: tidak ada satu pun yang memeriksa nilai status code secara eksplisit, sehingga risikonya rendah. `contract_version` naik ke `0.6.1` |
| Database | Tidak ada perubahan schema, entity, maupun migration. Isi resep memakai tabel dan jalur persistensi yang sudah ada (`PhmPrescriptionItem`, `PhmPrescriptionCompound`, `PhmPrescriptionCompoundItem`). Status migration: `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE`. Tidak ada perubahan pada `[AccessAction]`, `[AccessPermission]`, `AccessTypes`, maupun resolusi pengguna. Hak akses `Prescription : Create`, `RadOrder : Create`, dan `PatientAssessment : Create` tetap persis seperti sebelumnya |

---

## 4. Dokumentasi endpoint

#### `Health Services / Pharmacy Management / Prescription`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/pharmacy-management/prescriptions` | Membuat resep dokter beserta obat dan racikannya sekaligus dalam satu transaksi. Menjawab `201 Created` untuk resep baru, dan `200 OK` bila kunci permintaan yang sama dikirim ulang | `Prescription : Create` |

#### `Health Services / Radiology Management / Rad Order`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/radiology-management/rad-orders` | Membuat pesanan pemeriksaan radiologi. Menjawab `201 Created` | `RadOrder : Create` |

#### `Health Services / Clinical Management / Patient Assessment`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/clinical-management/patient-assessments` | Membuat dokumen kajian medis pasien. Menjawab `201 Created` | `PatientAssessment : Create` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet msbuild QuilvianSystemBackend.csproj -t:Compile -p:Configuration=Debug` | Berhasil, exit code `0`, tanpa error | `PASS` | Keluaran perintah |
| `dotnet build` — dijalankan pemilik 23 September 2026 sesudah proses backend dihentikan | `Build succeeded in 3,5s` → `bin\Debug\net9.0\QuilvianSystemBackend.dll`, tanpa error | `PASS` | Keluaran perintah pemilik |
| `dotnet build QuilvianSystemBackend.csproj` — dijalankan agent, dua kali | Kompilasi berhasil, tetapi penyalinan ke `bin` gagal: `MSB3027`/`MSB3021`, berkas `QuilvianSystemBackend.exe` dikunci proses `QuilvianSystemBackend (PID 32544)` yang saat itu sedang berjalan | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah menyebut nama proses pengunci. Sudah **tidak berlaku lagi** sesudah pemilik menghentikan proses itu dan build hijau |
| Pemeriksaan warning pada berkas yang diubah | Tidak ada warning baru. Tiga warning yang muncul pada berkas tersentuh berada di baris 2891, 524, dan 582 — jauh dari baris yang disunting task ini, dan sudah ada sebelumnya | `PASS` | `dotnet msbuild -t:Rebuild -v:n` disaring pada keenam berkas |
| Cakupan sebenarnya bug culture | `104` atribut `Range(typeof(decimal), …)` di seluruh `Areas/`, `54` di antaranya berbatas pecahan, tersebar di `21` berkas | `PASS` | Pencarian pola pada `Areas/` |
| Penilaian risiko penguncian culture ke invariant | Aman. Backend sudah memakai `CultureInfo.InvariantCulture` secara sengaja di `123` tempat; dua pemakaian `id-ID` mengoper culture eksplisit per panggilan sehingga tidak terpengaruh; satu-satunya `CurrentCulture` adalah fallback sesudah `InvariantCulture` dicoba lebih dulu | `PASS` | Pencarian `CultureInfo` pada seluruh source |
| Penilaian konsumen atas perpindahan `200` → `201` | Tidak ada konsumen frontend yang memeriksa nilai status code secara eksplisit | `PASS` | Pencarian pola pemeriksaan status pada service frontend terkait |
| `POST /prescriptions` menyimpan obat dan `TotalItemCount` benar | Belum dijalankan | `NOT RUN` | Menunggu backend dijalankan ulang; `bin` masih dikunci proses lama |
| `POST /prescription-items` tidak lagi `500` di locale `id-ID` | Belum dijalankan | `NOT RUN` | Sama seperti di atas |

Uji manual: `REQUIRED` — perlu dijalankan pemilik sesudah backend di-restart.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Tidak dijalankan:**

- Verifikasi runtime ketiga endpoint. Backend lama masih berjalan dan mengunci `bin`, sehingga biner
  hasil perbaikan belum dapat dijalankan. Ini butir verifikasi yang tersisa, dan sengaja ditulis apa
  adanya sebagai `NOT RUN`, bukan disimpulkan lulus.
- Eksekusi database apa pun. Task ini tidak menyentuh schema dan tidak memiliki wewenang database.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `ISS-01` — `POST /prescription-items` membalas 2xx, bukan `500`, pada mesin ber-locale `id-ID` | Belum terpenuhi pada tingkat runtime | Penyebabnya sudah dihapus di `Program.cs` dan kompilasi lulus, tetapi pembuktian runtime `NOT RUN` |
| `ISS-01` — `PATCH /prescription-workspaces/{id}/autosave` membalas 2xx | Belum terpenuhi pada tingkat runtime | Sama seperti di atas |
| `ISS-01` — tidak tersisa `FormatException` dari `RangeAttribute.SetupConversion` di log | Belum terpenuhi pada tingkat runtime | Perlu log sesudah backend dijalankan ulang |
| `ISS-01` — dampak penguncian culture pada keluaran berformat diperiksa | Terpenuhi | Tidak ada pemakaian culture `id-ID` yang bergantung pada culture bawaan; seluruh format sudah eksplisit |
| `ISS-03` — draf berisi 2 obat menghasilkan `TotalItemCount = 2` | Belum terpenuhi pada tingkat runtime | Jalur penyimpanan sudah tersambung dan satu transaksi; pembuktian runtime `NOT RUN` |
| `ISS-03` — kegagalan penyimpanan isi memunculkan galat, bukan notifikasi sukses | Terpenuhi | Kepala dan isi resep berada pada satu transaksi; kegagalan membatalkan keduanya dan galat naik ke pemanggil |
| `ISS-03` — resep tanpa item tidak pernah berstatus tersimpan-sukses di UI | Belum terpenuhi | Bagian frontend, dikerjakan pada `FE-RWI-095` |
| `ISS-04` — nama properti kontrak dipertahankan sebagai `PrescriptionOrderType` | Terpenuhi | Tidak ada perubahan pada properti tersebut; penyesuaian ada di sisi frontend |
| `ISS-06` — operasi create menjawab `201` secara seragam | Terpenuhi pada source | Tiga controller diubah dan kompilasi lulus; pembuktian runtime `NOT RUN` |

Butir yang belum terpenuhi seluruhnya bertumpu pada satu hal yang sama: verifikasi runtime belum
dapat dijalankan karena backend lama masih memegang `bin`. Tidak ada satu pun di antaranya yang
dinyatakan lulus tanpa bukti.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kompilasi tidak menghasilkan warning baru. Warning yang ada pada berkas tersentuh sudah ada sebelum task ini |
| Masalah yang diketahui | Pembungkus `ApiResponse<T>.Ok()` selalu menuliskan `statusCode: 200` pada badan jawaban, termasuk ketika HTTP status-nya `201`. Sifat ini sudah ada sebelumnya dan berlaku pada seluruh endpoint `201` di repository — termasuk `physician-visits` yang sudah lebih dulu memakai `201`. Tidak diubah pada task ini karena menyentuh berkas bersama di luar lingkup yang diberi wewenang; dilaporkan sebagai temuan. Akibatnya, laporan pengujian yang mengutip `"statusCode": 201` pada badan jawaban `physician-visits` tidak sesuai dengan keluaran sebenarnya |
| Risiko tersisa | (1) Verifikasi runtime ketiga endpoint belum dijalankan. (2) Penguncian culture ke invariant berlaku seluruh aplikasi; penilaian source menunjukkan tidak ada yang bergantung pada format `id-ID` bawaan, tetapi pemantauan pertama sesudah rilis tetap dianjurkan. (3) Resep lama yang jenisnya terlanjur salah akibat `ISS-04` belum ditelusuri — penelusuran data menunggu wewenang database terpisah |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs`, `M Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs`, `M Areas/HealthServices/PharmacyManagement/DTOs/PrescriptionDtos.cs`, `M Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkspaceService.cs`, `M Areas/HealthServices/RadiologyManagement/Controllers/RadOrderController.cs`, `M Program.cs`. Perubahan lain pada working tree — roadmap hemodialisa, tiga laporan pengujian yang terhapus, dan berkas tak terlacak lain — sudah ada sebelum task ini dan tidak disentuh |
| Langkah berikutnya | (1) Pemilik menghentikan proses backend yang berjalan, menjalankan `dotnet build`, lalu menjalankan ulang skenario resep ujung ke ujung. (2) Kerjakan `FE-RWI-095` untuk `ISS-02`, `ISS-04`, dan `ISS-05` di frontend. (3) Telusuri resep lama yang jenisnya salah sesudah wewenang database diberikan |
