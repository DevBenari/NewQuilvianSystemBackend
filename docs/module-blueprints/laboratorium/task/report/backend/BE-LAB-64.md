# Laporan Perubahan Backend — `BE-LAB-64`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-64` (backend) |
| Judul | Permukaan baseline dua data induk Mikrobiologi — prasyarat `FE-LAB-34`, slice `S4b` |
| Trace | `rules/backend/master-data-endpoint-standard.md`; `rules/frontend/master-data-feature-standard.md` |
| Kontrak | `LAB-API-v1` **`r30`** bagian 25 — `approved` 2026-09-22 |
| Klasifikasi | `MEDIUM` — delapan endpoint, dua controller, nol migration |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-22 |
| Dependency | `BE-LAB-60` ✅, `BE-LAB-62` ✅ |
| Asal | Keputusan pemilik modul **"A. Lengkapi backend dahulu"**, 2026-09-22 |
| Status | ✅ **`SELESAI`** — kedelapan endpoint terbukti terhadap `QuilvianNewDevYoga`. **Nol batas diketahui** |

---

## 1. Kenapa task ini ada

`FE-LAB-34` hendak membangun tiga layar data induk Mikrobiologi. Standar frontend menuntut
setiap layar data induk memuat kartu ringkasan, penyaring yang bentuknya dibaca dari server,
dan sakelar aktif per baris. Ketiganya bersandar pada tiga endpoint yang **nol ada**.

| Grup | Dibangun | Dituntut standar backend |
|---|---|---|
| `lab-susceptibility-breakpoints` | 5 | 9 |
| `lab-procedure-microbiology-profiles` | 5 | 9 |
| `lab-discipline-settings` | 4 | 4 — varian sah, lihat bagian 4 |

**Ini kelalaian saya saat mengerjakan `BE-LAB-60` dan `BE-LAB-62`.** Keduanya dibangun ke
kontrak `r27`, yang menyebut lima endpoint — dan lima itulah yang dibuat.
`master-data-endpoint-standard.md` nol dibaca pada keduanya, padahal standar itulah yang
menentukan berapa endpoint yang wajib ada; kontrak hanya mencatat hasilnya.

Selisih ini karena itu melewati dua task, satu audit kesiapan, dan sapuan status bagian `6ab`
tanpa terlihat. Sapuan yang menyisir **daftar task** nol akan menemukan permukaan yang nol
pernah dijadikan task oleh siapa pun; yang menemukannya adalah upaya membangun layar di
atasnya.

---

## 2. Yang dibangun

| Berkas | Isi |
|---|---|
| `DTOs/LabSusceptibilityBreakpointDtos.cs` | `SummaryResponse` (6 angka), `OptionResponse`, `StatusRequest` |
| `DTOs/LabFilterAndSummaryDtos.cs` | `LabSusceptibilityBreakpointFilterMetadataResponse` + empat DTO profil |
| `Services/LabFilterMetadataFactory.cs` | `LabSusceptibilityBreakpoint()`, `LabProcedureMicrobiologyProfile()` |
| `Services/LabSusceptibilityBreakpointService.cs` | `GetSummaryAsync`, `GetOptionsAsync`, `SetStatusAsync` |
| `Services/LabProcedureMicrobiologyProfileService.cs` | Ketiganya, sama |
| `Controllers/LabSusceptibilityBreakpointController.cs` | Empat endpoint; **5 → 9** |
| `Controllers/LabProcedureMicrobiologyProfileController.cs` | Empat endpoint; **5 → 9** |

**Nol migration, nol kolom, nol tabel, nol index, nol permission baru.** Kedelapan endpoint
menumpang hak akses `Read` dan `Update` yang sudah ada — `QBE-SVC-001` dan `QBE-DTO-001`
dipatuhi: seluruh CRUD tetap di service, controller nol memuat query.

### Dua keputusan yang layak dibaca

**Keempat angka cakupan dihitung atas baris AKTIF saja.** `coveredOrganism` menjawab "berapa
kuman yang interpretasinya dapat dihitung hari ini". Penghitung interpretasi `r27` 22.4 nol
memakai breakpoint nonaktif, sehingga memasukkannya ke angka cakupan akan melaporkan kesiapan
yang tidak dimiliki. Perilaku ini **dibuktikan**, bukan diasumsikan — lihat bagian 3.

**`options` breakpoint tetap mengirim `labOrganismId` dan `labAntibioticId` di samping
`label`.** Pemanggil yang menyaring antibiogram per kuman membutuhkan penunjuknya, dan
menguraikannya kembali dari teks `"<kuman> — <antibiotik>"` adalah cara yang pasti pecah pada
nama kuman yang memuat tanda pisah.

---

## 3. Verifikasi

Seluruhnya lewat HTTP terhadap `QuilvianNewDevYoga`, 2026-09-22.

| Yang diuji | Hasil | Klasifikasi |
|---|---|---|
| `dotnet build -p:RunAnalyzers=False` | **0 error** | `PASS` |
| Kedelapan endpoint baru | `200` | `PASS` |
| Kelima endpoint lama kedua grup | `200` | `PASS` |
| `PATCH` breakpoint → `false` | `activeBreakpoint` 1→0, `inactiveBreakpoint` 0→1 | `PASS` |
| Akibatnya pada cakupan | `coveredOrganism` 1→0, `coveredAntibiotic` 1→0, `missingDiscContent` 1→0 | `PASS` — membuktikan keputusan bagian 2 |
| `options?onlyActive=true` sesudahnya | `totalData` 0 | `PASS` |
| `options?onlyActive=false` sesudahnya | `totalData` 1 | `PASS` |
| `PATCH` profil → `false` | `activeProfile` 1→0, `usesSusceptibilitySet` 1→0 | `PASS` |
| `options?search=Leuko` / `search=zzz` | `totalData` 1 / 0 | `PASS` |
| `PATCH` penunjuk asing | `404` beserta pesan Indonesia | `PASS` |
| Keduanya dikembalikan ke `true` | `activeBreakpoint` 1, `activeProfile` 1 | `PASS` — **data dev pulih seperti semula** |
| Baris `SysActionAccess` sesudah seeder | **4 per controller** | `PASS` — lihat bagian 5 |

**`AUTOMATED TEST: NOT APPLICABLE`** — nol uji unit backend disentuh; verifikasinya adalah
pemanggilan HTTP terhadap database sungguhan, sebagaimana dituntut `LAB-RDY-C04`.

**`MANUAL TEST: PASS`** — kedelapan endpoint dipanggil satu per satu beserta jalur gagalnya.

---

## 4. `lab-discipline-settings` sengaja TIDAK disentuh

`master-data-endpoint-standard.md` menyebut varian sah **"master data pengaturan tunggal"**:
entity yang hanya punya satu baris konfigurasi cukup `GET /` dan `PUT /{id}`, sebab
*"metadata, summary, options, dan delete tidak berlaku karena tidak ada koleksi data"*.

`LabDisciplineSetting` memuat **tiga baris tetap**, satu per disiplin, yang nol pernah bertambah
maupun berkurang. `summary` di atasnya akan melaporkan "3 dari 3 aktif" selamanya, dan `DELETE`
membuka jalan menghapus disiplin yang dipakai penghitung nomor cetak `r29`.

Keempat endpointnya — `GET /`, `GET /{discipline}`, `PUT /{discipline}`, `GET /options` —
karena itu **sudah memenuhi standar apa adanya**. Bagian `r29` 24 nol tersentuh.

---

## 5. Satu cacat pendaftaran hak akses ikut ditutup

`AccessMenuSeeder` berkunci pada **`(controller, ActionName)`** — satu baris `SysActionAccess`
per nama aksi, bukan per endpoint. Kedua controller sudah memuat dua method bernama `"Read"`
dengan `DisplayName`, `Description`, dan `SortOrder` **berbeda**, sehingga isi baris yang
tersimpan bergantung pada urutan refleksi menemukan method-nya.

Empat endpoint baru menaikkan taruhannya dari dua menjadi **lima** pesaing untuk satu baris
`Read`, dan dua untuk `Update`. Seluruh atribut `Read` dan `Update` pada kedua controller karena
itu diseragamkan — mengikuti `LabOrganismController` dan `LabAntibioticController`, yang memang
sudah menuliskan `DisplayName` dan `SortOrder` identik bagi nama aksi yang sama.

Dibuktikan atas database: tepat **empat** baris per controller, `DisplayName` dan `SortOrder`
nol lagi bergantung urutan.

> `RoutePath` baris `Read` menunjuk `/options`, yakni method yang terakhir ditemukan. **Itu
> perilaku repo yang sudah ada, bukan cacat baru** — baris `Read` milik `LabOrganism` dan
> `LabAntibiotic` menunjuk `/options` dengan cara yang sama persis, dan keduanya diperiksa
> untuk memastikannya. Kolom itu keterangan pada layar Akses Role; yang menjaga gerbang adalah
> `[AccessPermission]`, dan ia berkunci pada `(resource, action)`.

---

## 6. Yang sengaja TIDAK dikerjakan

| Butir | Alasan |
|---|---|
| Nama aksi tersendiri bagi `summary`/`options`/`status` | Akan melahirkan baris hak akses baru yang **nol tercentang pada role mana pun**, sehingga ketiga layar `FE-LAB-34` menjawab `403` sampai seseorang membukanya satu per satu. Konvensi repo menumpangkan `options` pada `Read` |
| `summary` dan `DELETE` pada `lab-discipline-settings` | Bagian 4 |
| Mengisi data induknya | `B3` pada audit kesiapan — satu organisme dan satu antibiotik masih menjadi seluruh isi tabelnya. Pengisian adalah pekerjaan data, bukan pekerjaan endpoint |
| Menyentuh kelima endpoint lama | Kontrak `r30` bersifat aditif; nol route, verb, bentuk, maupun hak akses yang berubah |

---

## 7. Koreksi status yang dibawa serta

Kontrak bagian 22.5 menandai kelima endpoint breakpoint **`Rencana (belum tersedia)`** padahal
`BE-LAB-60` membangunnya 2026-09-21. Kelimanya dipanggil, kelimanya menjawab `200`, dan kolom
statusnya dikoreksi menjadi **`Tersedia`**. Yang berubah hanya kolom status.

---

## 8. Akibatnya bagi `FE-LAB-34`

**Penahannya hilang.** Ketiga layar kini berdiri di atas sembilan endpoint penuh dan dapat
dibangun persis sesuai `master-data-feature-standard.md`, tanpa satu pun permukaan yang
dipalsukan di sisi layar.

**Nol operasi git dijalankan.**
