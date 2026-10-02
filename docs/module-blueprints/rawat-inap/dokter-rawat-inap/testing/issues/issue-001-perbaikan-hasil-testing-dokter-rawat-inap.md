# ISSUE-001 — Perbaikan Temuan Pengujian Dokter Rawat Inap (Backend & Frontend)

```yaml
issue_id: ISSUE-DOK-001
module_id: rawat-inap
submodule: dokter-rawat-inap
blueprint_id: RWI-BP-001
contract_version: 0.6.0
sumber_temuan: docs/module-blueprints/rawat-inap/dokter-rawat-inap/testing/test-by-agy/
tanggal_pengujian: "2026-09-22"
tanggal_issue: "2026-09-23"
status: SEBAGIAN SELESAI   # source ISS-01..04, ISS-06 selesai; runtime belum diverifikasi; ISS-05 menunggu keputusan
write_authority: DIBERIKAN 23-09-2026 oleh pemilik
task_id_backend: BE-RWI-127   # 🟡 laporan: ../../task/report/backend/BE-RWI-127.md
task_id_frontend: FE-RWI-095  # 🟡 laporan: ../../task/report/frontend/FE-RWI-095.md
contract_version_sesudah: 0.6.1
verifikasi_source: "2026-09-23 — seluruh butir SUDAH-VERIFIKASI dicek langsung ke source, bukan hanya dibaca dari laporan"
```

## 1. Latar belakang

Tujuh laporan pengujian dijalankan pada 22 September 2026 terhadap sub-modul dokter rawat inap
(pasien uji Tn. Indra Gunawan, episode `RI-260909100035-F8D716`). Enam laporan berstatus lulus,
satu berstatus terhambat:

| Laporan | Status yang ditulis penguji |
| --- | --- |
| `laporan-testing-create-resep.md` | **DEFECTS FOUND — BLOCKED** |
| `laporan-testing-create-tindakan.md` | PASSED (1 defect ditemukan dan sudah diperbaiki saat pengujian) |
| `laporan-testing-create-visite.md` | PASSED |
| `laporan-testing-create-resume.md` | PASSED |
| `laporan-testing-create-penunjang.md` | PASSED |
| `laporan-testing-create-cppt.md` | PASSED |
| `laporan-testing-create-kajian-pasien.md` | PASSED |

Dokumen ini mengumpulkan seluruh butir yang perlu diperbaiki — backend maupun frontend — ke dalam
satu daftar kerja. Setiap butir sudah ditelusuri ulang ke source pada 23 September 2026; yang belum
bisa dipastikan dari source ditandai terpisah di bagian 4, jangan dikerjakan sebelum direproduksi.

### Koreksi terhadap laporan penguji

Laporan resep menyebut bug `RangeAttribute` sebagai masalah modul resep. Penelusuran source
menunjukkan **akarnya bukan di modul resep**, melainkan ketiadaan setelan culture di seluruh
aplikasi. Modul resep hanya kebetulan menjadi yang pertama tersentuh pengujian. Rinciannya di
`ISS-01`.

---

## 2. Daftar perbaikan

### ISS-01 — Batas pecahan pada `[Range(typeof(decimal), …)]` meledak di server ber-locale `id-ID`

| | |
| --- | --- |
| **Area** | Backend |
| **Keparahan** | **Blocker** |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Gejala** | `POST /prescription-items` dan `PATCH /prescription-workspaces/{id}/autosave` membalas **HTTP 500** untuk setiap permintaan |

**Akar masalah.** Overload `RangeAttribute(Type, string, string)` mem-parsing batas minimum/maksimum
memakai `CultureInfo.CurrentCulture`. Pada locale `id-ID`, titik adalah pemisah ribuan, sehingga
teks batas `"0.0001"` gagal di-parse dan melempar `FormatException` → `ArgumentException`. Kegagalan
terjadi saat validasi model, **sebelum** controller action dijalankan, dan tidak bergantung pada nilai
yang dikirim klien — request apa pun ke DTO tersebut ikut gagal.

Jejak dari log penguji (`Logs/quilvian-backend-20260922.json` baris 2348):

```text
System.ArgumentException: 0.0001 is not a valid value for Decimal. (Parameter 'value')
 ---> System.FormatException: The input string '0.0001' was not in a correct format.
   at System.ComponentModel.DataAnnotations.RangeAttribute.SetupConversion()
```

**Cakupan sebenarnya.** Repo tidak memiliki setelan culture di mana pun — `Program.cs` tanpa
`CultureInfo.DefaultThreadCurrentCulture`, `.csproj` tanpa `InvariantGlobalization`, dan tanpa
`RequestLocalization`. Yang berisiko **hanya** atribut yang batasnya mengandung pecahan; batas bulat
seperti `"0"`/`"999999999"` tetap ter-parse benar di `id-ID`.

| Ukuran | Jumlah |
| --- | ---: |
| Atribut `Range(typeof(decimal), …)` seluruh `Areas/` | 104 |
| Di antaranya berbatas **pecahan** (berisiko) | **54** |
| File terdampak | **21** |

Terdampak jauh melampaui rawat inap: PharmacyManagement (6 file), MasterData (3), BillingManagement
(4), HumanResource (5), HemodialysisManagement, FinanceManagement. Pada jalur resep khususnya:

- `Areas/HealthServices/PharmacyManagement/DTOs/PrescriptionItemDtos.cs` — baris 113, 147, 164, 198
- `Areas/HealthServices/PharmacyManagement/DTOs/PrescriptionWorkspaceDtos.cs` — baris 247, 259, 270, 272, 275, 298, 302, 305, 309, 310
- `PrescriptionCompoundDtos.cs`, `PrescriptionCompoundItemDtos.cs`, `PrescriptionTemplateDtos.cs`, `PrescriptionPreparationDtos.cs`

**Usulan perbaikan — pilih satu, keputusan ada pada pemilik.**

*Opsi A (sistemik, satu baris, direkomendasikan).* Kunci culture di `Program.cs` sebelum
`builder.Build()`:

```csharp
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
```

Menutup seluruh 54 titik sekaligus dan mencegah kasus serupa lahir lagi. **Risiko yang harus
diperiksa sebelum diterapkan:** ini mengubah hasil `ToString()` untuk decimal dan tanggal di seluruh
aplikasi. Payload JSON tidak terpengaruh (`System.Text.Json` sudah invariant), tetapi setiap
pembentukan teks berformat Indonesia — cetak kuitansi, laporan, PDF, ekspor — wajib dicek ulang.

*Opsi B (bedah, tanpa efek global).* Tambahkan properti bawaan .NET 8+ pada 54 atribut berisiko
(project ini `net9.0`, jadi tersedia):

```csharp
[Range(typeof(decimal), "0.0001", "999999999", ParseLimitsInInvariantCulture = true)]
```

Tidak mengubah perilaku format aplikasi, tetapi harus disisipkan satu per satu dan titik baru di
masa depan tetap rawan mengulang bug yang sama.

**Acceptance criteria.**
1. `POST /prescription-items` dengan payload sah membalas 2xx, bukan 500, pada mesin ber-locale `id-ID`.
2. `PATCH /prescription-workspaces/{id}/autosave` membalas 2xx pada mesin yang sama.
3. Tidak tersisa `FormatException` dari `RangeAttribute.SetupConversion` di log.
4. Bila Opsi A dipilih: satu endpoint yang mencetak teks berformat rupiah/tanggal diverifikasi tidak berubah keluarannya, atau perubahannya disetujui pemilik.

---

### ISS-02 — `encounterId` tidak dikirim saat mencari obat formularium

| | |
| --- | --- |
| **Area** | Frontend |
| **Keparahan** | **Blocker** |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Gejala** | Modal "Cari dan Tambah Obat" selalu kosong; UI menampilkan *"Tidak ada obat yang cocok atau dapat diresepkan untuk encounter ini"* padahal master obat tersedia |

**Akar masalah.** `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-prescription-tab.jsx`
baris 162–165 memanggil katalog tanpa `encounterId`:

```javascript
const response = await getPrescribingDrugs({
  search: keyword.trim(),
  pageSize: 15,
});
```

Sedangkan `PrescribingDrugController.GetDrugs` menerima `[FromQuery] Guid encounterId` (baris 93) dan
menolak nilai kosong secara eksplisit (baris 107–111, pesan `"EncounterId wajib diisi."`), sehingga
backend membalas `400 Bad Request`. Pesan kosong di UI menyesatkan: dokter membaca "obat tidak
tersedia" padahal permintaannya ditolak.

**Usulan perbaikan.** Sertakan `encounterId` pada pemanggilan. **Perhatikan:** `searchDrugs`
dibungkus `useCallback(…, [])` dengan dependency array kosong — `encounterId` wajib ikut masuk
dependency array, kalau tidak nilainya tertahan pada render pertama (`null`) dan bug-nya tetap ada
dalam bentuk lain.

**Acceptance criteria.**
1. Mengetik minimal 2 huruf di modal katalog memunculkan daftar obat dari formularium.
2. Tidak ada respons 400 pada `GET /prescribing-drugs` di log jaringan.
3. Saat `encounterId` memang belum tersedia, UI menampilkan pesan yang jujur ("konteks kunjungan belum siap"), bukan "obat tidak ditemukan".

---

### ISS-03 — `items` dan `compounds` dibuang diam-diam saat simpan draf resep

| | |
| --- | --- |
| **Area** | Kontrak Frontend ↔ Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Gejala** | Draf resep tersimpan dengan `TotalItemCount = 0` — header terbit, isi obat hilang tanpa pesan galat |

**Akar masalah.** `saveDraftPrescription` (hook yang sama, baris 255–330) menyusun payload lengkap
berisi `items` dan `compounds`, lalu mengirimkannya ke `createPrescription(payload)`
(`POST /prescriptions`). Namun `CreatePrescriptionRequest`
(`Areas/HealthServices/PharmacyManagement/DTOs/PrescriptionDtos.cs` baris 156–186) hanya mengenal
`EncounterId`, `ConsultationId`, `PrescriptionDateTime`, `PrescriptionOrderType`, `IdempotencyKey`,
`ClinicalNote`, dan `DoctorInstruction`. Properti yang tidak dikenal diabaikan model binder tanpa
galat, sehingga dokter menerima notifikasi "berhasil" untuk resep yang sebenarnya kosong.

Ini yang paling berbahaya secara klinis dari seluruh daftar: kegagalannya senyap.

**Usulan perbaikan.** Dua jalan, pilih berdasarkan kontrak yang ingin dikunci:

- *Jalan frontend:* setelah `createPrescription` mengembalikan id, panggil
  `autosavePrescriptionWorkspace(id, { items, compounds })`. Endpoint
  `PATCH /prescription-workspaces/{prescriptionId}/autosave` sudah ada
  (`PrescriptionWorkspaceController.cs` baris 86). Perlu ditangani: kegagalan langkah kedua harus
  memunculkan galat, jangan biarkan header telanjur terbit sementara UI berkata sukses.
- *Jalan backend:* tambahkan `Items`/`Compounds` ke `CreatePrescriptionRequest` agar sekali panggil.
  Lebih rapi secara transaksional, tetapi mengubah kontrak yang sudah terkunci `0.6.0` — **butuh
  persetujuan pemilik lebih dahulu.**

**Acceptance criteria.**
1. Menyimpan draf berisi 2 obat menghasilkan resep dengan `TotalItemCount = 2`.
2. Bila penyimpanan isi obat gagal, dokter melihat pesan galat — bukan notifikasi sukses.
3. Resep tanpa item tidak pernah berstatus tersimpan-sukses di UI.

---

### ISS-04 — `orderType` tidak terikat ke `PrescriptionOrderType`, jenis resep selalu jatuh ke `Routine`

| | |
| --- | --- |
| **Area** | Kontrak Frontend ↔ Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source — **belum tercatat di laporan penguji** |
| **Gejala** | Resep obat pulang tersimpan sebagai resep rutin |

**Akar masalah.** Frontend mengirim field bernama `orderType` (baris 291 pada hook), sedangkan DTO
backend menamainya `PrescriptionOrderType`. Keduanya nama yang berbeda, bukan sekadar beda
kapitalisasi, sehingga pengikatan case-insensitive ASP.NET Core tidak menolongnya. Field diabaikan
dan properti jatuh ke nilai bawaan `PrescriptionOrderType.Routine`.

Dokumentasi DTO sendiri menyatakan niat sebaliknya (`BE-RWI-050`, `RWI-DEC-046`): *"Obat pulang
menjadi jenis yang eksplisit, bukan disimpulkan dari waktu penulisan maupun dari status
perawatan. Petugas farmasi harus dapat menyaringnya di layar mereka sendiri."* Dengan bug ini,
penyaringan obat pulang di layar farmasi tidak dapat dipercaya.

Payload frontend juga menyertakan `episodeId` yang tidak ada di DTO — ikut diabaikan. Perlu
diputuskan apakah field itu memang tidak diperlukan, atau backend yang kurang.

**Usulan perbaikan.** Samakan penamaan pada satu sisi, lalu telusuri apakah ada resep tersimpan
yang jenisnya sudah terlanjur salah sejak fitur ini dipakai.

**Acceptance criteria.**
1. Menyimpan resep bertipe obat pulang menghasilkan `prescriptionOrderType` obat pulang di basis data.
2. Layar farmasi dapat menyaring resep obat pulang dan hasilnya cocok dengan yang dibuat dokter.
3. Hasil penelusuran data lama dilaporkan — berapa resep yang jenisnya salah, dan apakah perlu perbaikan data.

---

### ISS-05 — Dua berkas service prescription workspace yang tumpang tindih

| | |
| --- | --- |
| **Area** | Frontend |
| **Keparahan** | Low (kebersihan kode) |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Status pengerjaan** | **Tidak dikerjakan** pada `FE-RWI-095` — lihat koreksi di bawah |

Terdapat dua berkas berdampingan yang sama-sama mengekspor `autosavePrescriptionWorkspace`:

- `src/lib/services/health-services/pharmacy-management/prescription-workspace-service.js` (baris 84)
- `src/lib/services/health-services/pharmacy-management/prescription-workspace.service.js` (baris 43)

> **Koreksi 23 September 2026.** Deskripsi awal butir ini — "berduplikat", cukup sisakan satu — **keliru**,
> dan ditemukan saat `FE-RWI-095` dikerjakan. Kedua berkas itu **bukan duplikat**; semantik galatnya
> berbeda:
>
> | Perilaku | `prescription-workspace.service.js` | `prescription-workspace-service.js` |
> | --- | --- | --- |
> | Resep belum ada (`404`) | Mengembalikan `null` | Melempar galat |
> | Id kosong | Mengembalikan `null` | Melempar lewat `assertUuid` |
>
> `use-doctor-prescription.js` baris 433–437 **bergantung** pada perilaku `null` itu untuk menampilkan
> keadaan kosong *"Belum ada resep. Draft akan dibuat saat dokter menambah obat, racikan, atau catatan."*
> Menyatukan keduanya seperti tertulis semula akan mengubah keadaan kosong yang wajar menjadi banner
> galat pada layar resep dokter **poliklinik** — modul di luar sub-modul ini.

**Keputusan yang dibutuhkan sebelum dikerjakan.** Perilaku mana yang menang ketika resep belum ada:
mengembalikan keadaan kosong, atau melempar galat? Ini keputusan perilaku layar, bukan sekadar
kebersihan kode, sehingga tidak diputuskan sepihak.

**Acceptance criteria.** Tersisa satu berkas, seluruh import menunjuk ke sana, lint bersih, **dan**
keadaan kosong pada layar resep poliklinik tetap tampil sebagaimana sebelumnya.

---

### ISS-06 — Status code operasi create tidak seragam

| | |
| --- | --- |
| **Area** | Backend (kontrak) |
| **Keparahan** | Low |
| **Status bukti** | Dari laporan penguji, belum ditelusuri ke source |

Operasi pembuatan membalas status yang berbeda-beda antar modul:

| Endpoint | Status |
| --- | :---: |
| `POST /lab-orders` | 201 Created |
| `POST /physician-visits` | 201 Created |
| `POST /patient-procedures/inpatient-orders` | 201 Created |
| `POST /rad-orders` | 200 OK |
| `POST /patient-assessments` | 200 OK |
| `POST /prescriptions` | 200 OK |

Bukan kegagalan fungsional, tetapi menyulitkan klien yang memeriksa status secara seragam. Perlu
keputusan pemilik: seragamkan ke 201, atau catat 200 sebagai ketentuan yang disengaja.

---

## 3. Urutan pengerjaan yang disarankan

`ISS-01` lebih dahulu, dan **jangan diperlakukan sebagai bug modul resep** — perbaikannya bersifat
lintas modul dan menutup 54 titik sekaligus. Selama ISS-01 terbuka, ISS-03 dan ISS-04 tidak dapat
diverifikasi tuntas karena penyimpanan item masih dijegal 500.

1. `ISS-01` (backend) — buka jalur penyimpanan.
2. `ISS-05` (frontend) — tentukan berkas service yang sah.
3. `ISS-02` (frontend) — dokter bisa memilih obat.
4. `ISS-03` (frontend/backend) — isi resep benar-benar tersimpan.
5. `ISS-04` (kontrak) — jenis resep tercatat benar + telusur data lama.
6. `ISS-06` (kontrak) — keputusan pemilik, boleh menyusul.
7. Jalankan ulang skenario resep ujung ke ujung, lalu perbarui `laporan-testing-create-resep.md`.

---

## 4. Anomali yang perlu direproduksi lebih dahulu

Butir berikut terbaca dari isi laporan yang justru berstatus "BERHASIL 100%". Belum saya telusuri ke
source, jadi **belum layak dijadikan task** sampai ada yang mereproduksinya.

### ANM-01 — CPPT kedua tidak muncul di lini masa

`laporan-testing-create-cppt.md` mencatat dua dokumen terbit dengan respons 200 OK —
`CPPT-20260922-0001` (langsung) dan `CPPT-20260922-0002` (dari konsultasi SOAP). Namun counter UI
hanya menunjukkan **`1 catatan`** dan lini masa hanya me-render satu kartu. `CPPT-0002` tidak pernah
muncul pada verifikasi visual, dan laporan tidak menjelaskan ke mana perginya, tetapi tetap
menyimpulkan fitur berjalan 100% sempurna.

*Yang perlu dipastikan:* apakah `POST /from-consultation/{id}` benar-benar menulis baris yang terbaca
`GET /episodes/{episodeId}`, atau ada penyaringan yang menyembunyikannya.

### ANM-02 — Kolom penulis kosong pada riwayat kajian medis

`laporan-testing-create-kajian-pasien.md` bagian 4.2 menampilkan baris riwayat dengan kolom
**Dokter / Penulis** berisi `-`. Untuk dokumen yang di kesimpulan laporan dinyatakan sudah memenuhi
standar dokumentasi medis rumah sakit, penulis yang kosong adalah cacat medikolegal, bukan detail
tampilan.

*Yang perlu dipastikan:* apakah backend tidak mengirim nama penulis, atau frontend tidak memetakannya.

### ANM-03 — Konteks pasien saling bertabrakan antar laporan

Untuk episode yang sama persis (`c3fe1370-18f0-42fb-8d9f-01449212828e`):

| Laporan | Ruang & bed | Penjamin |
| --- | --- | --- |
| `create-visite`, `create-tindakan`, `create-resep` | Kelas I 1 / BED 001 | BPJS / AdMedika |
| `create-penunjang` | Ruang Melati / Bed 02, kelas `UNIQUE` | AdMedika |

Salah satu laporan salah data. Perlu dipastikan apakah ini keliru tulis penguji, atau data ruang
pasien memang tidak konsisten antar layar. Kelas bernilai `UNIQUE` sendiri patut dicurigai sebagai
nilai master yang tidak semestinya tampil ke pengguna.

Laporan `create-penunjang` juga mencantumkan dua ID sekaligus untuk satu DPJP
(`19130ac0-…` dan `bc389b2c-…`), padahal di laporan lain keduanya berbeda peran — `19130ac0-…`
adalah `userId` dan `bc389b2c-…` adalah `doctorId`. Laporan `create-resep` melakukan kekeliruan
label yang sama. Ini kekeliruan dokumentasi, bukan cacat sistem, tetapi menyesatkan siapa pun yang
memakai laporan itu sebagai rujukan.

---

## 5. Catatan tata kelola

- Dokumen ini **belum** memuat wewenang tulis. Perubahan source backend dijalankan lewat
  `quilvian-engineering-skills:build-module-backend` setelah task ID dialokasikan dan disetujui;
  frontend lewat `build-module-frontend`.
- `ISS-03` jalan backend dan `ISS-04` menyentuh kontrak terkunci `0.6.0` — butuh persetujuan pemilik
  sebelum dikerjakan, bukan sekadar alokasi task.
- Deret task bebas berikutnya saat dokumen ini ditulis: `BE-RWI-127` dan `FE-RWI-095`. Konfirmasikan
  ulang ke metadata roadmap sebelum memakai, karena nomor tidak pernah dipakai ulang.
- Setelah tiap butir selesai, perbarui register pemicunya: roadmap terkait, laporan pengujian yang
  bersangkutan, dan status pada dokumen ini.
