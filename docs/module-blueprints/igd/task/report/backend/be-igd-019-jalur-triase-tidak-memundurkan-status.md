# BE-IGD-019 — Jalur Triase Tidak Lagi Memundurkan Status Kunjungan

## Metadata

| Field | Nilai |
| --- | --- |
| Task | `BE-IGD-019` |
| Slice | `IGD-S01` · `EPIC IGD-03` · gelombang `MVP-0` |
| Requirement | `FR-IGD-013`, `FR-IGD-014`, `FR-IGD-015` |
| Keputusan | `IGD-GAP-014`, `IGD-CONF-05`, `IGD-DEC-093`, `IGD-DEC-094` |
| Kontrak | State `0.3.0` §1 dan Validation `0.3.0` §2 aturan 4–5 — **`approved`**, hash `a41efd8d…` / `0ee98b75…` |
| Uji | `AT-IGD-086`, `AT-IGD-087`, `AT-IGD-088` |
| Commit dasar | `300922c` |
| Tanggal | 26 Agustus 2026 |
| **Status** | **Selesai dan terbukti lewat test. Belum di-commit.** |

---

## 1. Ringkasan

Dua titik tulis `visit.VisitStatus = Triaged` pada `EmergencyTriageController` kini melewati
penjaga `TryApplyVisitStatus` yang dibuat `BE-IGD-018`. Ditambah satu lubang yang ditutup di
`EmergencyTriageService`: kunjungan berstatus `Completed` sebelumnya **tidak** dianggap
tertutup.

Keduanya dikerjakan **dalam satu task**, mengikuti pelajaran `BE-IGD-016` — memperbaiki satu
jalur dan melewatkan jalur kedua persis itulah yang membuat cacat ini bertahan.

---

## 2. Tiga cacat yang diperbaiki

### 2.1 Jalur create menulis status tanpa penjagaan

`EmergencyTriageController.Create` baris ~250. Setelah triase tersimpan, bila statusnya
`Completed`, kunjungan **selalu** ditulis menjadi `Triaged` — tanpa memeriksa apakah transisi
itu sah. Pasien yang sedang `InTreatment` mundur menjadi `Triaged`.

### 2.2 Jalur ubah status tidak memeriksa kunjungan sama sekali

`EmergencyTriageController.UpdateTriageStatus` baris ~356. Yang diperiksa hanya transisi
`TriageStatus`, **bukan** kunjungannya. Menyelesaikan penilaian lama pada kunjungan yang sudah
`Disposed` atau `Completed` **membuka kembali** kunjungan itu menjadi `Triaged`.

Ini jalur yang sepenuhnya tanpa penjagaan — bahkan tanpa pemeriksaan kunjungan tertutup yang
sudah ada di jalur create.

### 2.3 `Completed` tidak dianggap tertutup

`EmergencyTriageService.ValidateRequestAsync` memeriksa `Disposed` dan `Cancelled`, tetapi
**bukan** `Completed`. Kunjungan yang sudah benar-benar selesai masih menerima triase baru.

Cacat yang sama persis ada di `RetriageAsync` baris 141–143 dan **sengaja tidak diperbaiki di
sini** — itu `BE-IGD-020`, task tersendiri.

---

## 3. Yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Controllers/EmergencyTriageController.cs` | `EmergencyVisitService` di-inject; helper `KunjunganSudahDitutup`; kedua titik tulis memakai penjaga; jalur ubah status memeriksa kunjungan dan menolak `409`; `ProducesResponseType` `409` ditambahkan |
| `Services/EmergencyTriageService.cs` | `Completed` ditambahkan ke pemeriksaan kunjungan tertutup |

### 3.1 Aturan yang ditegakkan

Mengikuti `IGD-DEC-104` — lihat bagian 4.1 untuk tabel lengkapnya.

| Keadaan kunjungan | Yang terjadi |
| --- | --- |
| `Disposed`, `Completed`, `Cancelled` | **Ditolak `409`**, pesan kontrak *"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."* Pada create, `ValidateRequestAsync` menolaknya lebih dulu dengan `400` |
| `WaitingForTriage` | Menjadi `Triaged` lewat `CanTransition` |
| `Triaged`, `InTreatment`, `UnderObservation`, `AwaitingDisposition` | Penilaian tersimpan, status **tetap**. Penjaga **tidak dipanggil** |
| **`Arrived`** | Penjaga dipanggil dan **menolak** — melompati `WaitingForTriage` tidak sah → **`409`** |

### 3.2 Urutan pemeriksaan pada kedua jalur

Kunjungan diperiksa **sebelum** apa pun disimpan atau disentuh.

Pada jalur **ubah status**, pemeriksaan mendahului perubahan `entity`, sehingga penolakan
tidak meninggalkan perubahan menggantung pada change tracker dan tidak ada `SaveChangesAsync`
yang dipanggil.

Pada jalur **create**, pemeriksaan dipindahkan ke **sebelum** `Add(entity)`. Sebelumnya triase
disimpan lebih dulu lalu status ditulis pada `SaveChangesAsync` kedua — susunan yang membuat
`409` meninggalkan baris triase yang terlanjur tersimpan. Kini keduanya masuk **satu**
`SaveChangesAsync`, sehingga kegagalan menyisakan nol baris.

---

## 4. Ketidakcocokan kontrak — **sudah ditutup `IGD-DEC-104`**

> **Diperbarui 26 Agustus 2026, sebelum di-commit.** Bagian 4 di bawah menggambarkan
> penafsiran yang dipakai implementasi pertama. Product/Domain Owner **menolak rumusan itu
> karena terlalu luas**, dan menggantinya dengan `IGD-DEC-104`. Kode dan test sudah
> disesuaikan; lihat bagian 4.1.

### 4.1 `IGD-DEC-104` — aturan per status

| Status kunjungan | Perlakuan |
| --- | --- |
| `WaitingForTriage` | Triase selesai → menjadi `Triaged` **melalui `CanTransition`** |
| `Triaged`, `InTreatment`, `UnderObservation`, `AwaitingDisposition` | Penilaian **disimpan**, `VisitStatus` **tetap**. Sistem **tidak mencoba** mengubahnya |
| `Disposed`, `Completed`, `Cancelled` | Penyelesaian triase ditolak **`409`** |
| **`Arrived`** | **Tidak boleh melompat ke `Triaged`.** Perubahan status memang diminta, `CanTransition` menolak → **`409`** |

Arti aturan 5 yang kini terkunci: *setiap perubahan status yang **benar-benar dilakukan**
akibat triase wajib melewati `CanTransition`; penilaian ulang yang **tidak mengubah status**
bukan transisi ilegal.*

**Yang membedakan bukan terbuka lawan tertutup, melainkan sudah lawan belum melewati tahap
triase.** `Arrived` adalah satu-satunya tempat rumusan lama dan `IGD-DEC-104` berbeda hasilnya:
rumusan lama membiarkannya berhasil diam-diam dengan status tetap `Arrived`; `IGD-DEC-104`
menolaknya `409`.

### 4.2 Yang berubah pada kode

| Perubahan | Isi |
| --- | --- |
| Helper `KunjunganSudahMelewatiTriase` | Memisahkan empat status yang sudah melewati triase dari `Arrived` dan `WaitingForTriage` |
| Kedua jalur | Penjaga **hanya dipanggil** bila kunjungan belum melewati triase; penolakannya menghasilkan `409` |
| Jalur create | Pemeriksaan kunjungan **dipindahkan ke sebelum penyimpanan**. `409` tidak pernah meninggalkan baris triase yang terlanjur tersimpan, dan perubahan status ikut `SaveChangesAsync` yang sama |
| Test | **18 → 27** |

Semantik `AT-IGD-086`, UAT "Ny. Sari", dan `IGD-DEC-083` dipertahankan seluruhnya. **Nol**
acceptance test yang perlu ditinjau ulang.

---

## 4-lama. Penafsiran pertama — digantikan `IGD-DEC-104`

**Validation-matrix bagian 2 aturan 5 tampak bertentangan dengan `AT-IGD-086`.**

Aturan 5 berbunyi: *"Perubahan status kunjungan akibat triase wajib transisi yang sah"* →
`409` → *"Penilaian ini tidak dapat mengubah status kunjungan dari {status}."*

Dibaca harfiah, penilaian ulang pasien `InTreatment` harus ditolak `409`. Tetapi:

| Sumber | Isi |
| --- | --- |
| `AT-IGD-086` | *"Menilai ulang pasien yang sudah `InTreatment`"* → **Berhasil** → *"Status kunjungan **tetap** `InTreatment`"* |
| `04-prd-to-mvp.md` EPIC IGD-03, UAT berhasil | *"Ny. Sari sedang ditangani. Perawat menilainya ulang karena kondisinya memburuk. Status kunjungan tetap sedang ditangani."* |

Keduanya menuntut **berhasil**, bukan `409`.

**Implementasi mengikuti `AT-IGD-086` dan skenario UAT**, karena keduanya menggambarkan
perilaku yang diinginkan secara konkret, sementara aturan 5 menggambarkannya secara ringkas
dan dapat dibaca dua arti. Penolakan penjaga pada kunjungan yang **masih terbuka** karena itu
diabaikan — status tidak berubah, penilaian tetap tersimpan.

`409` tetap diterbitkan untuk kunjungan yang **sudah tertutup**, sesuai aturan 4.

Penafsiran ini **belum dikonfirmasi owner** dan dicatat sebagai `IGD-OQ-079`. Bila aturan 5
memang dimaksudkan menolak, `AT-IGD-086` dan skenario UAT-nya harus ditinjau ulang — dan
perawat tidak akan dapat menilai ulang pasien yang sedang ditangani, yang tampaknya bukan yang
dikehendaki.

Test `PenilaianUlang_PenolakanPenjagaBukanKegagalanPenilaian` sengaja diberi komentar yang
menunjuk ke sini, supaya siapa pun yang kelak mengubahnya membaca alasannya lebih dulu.

---

## 5. Verifikasi

| Kriteria acceptance roadmap | Hasil |
| --- | --- |
| 1. Pasien `InTreatment` yang dinilai ulang tetap `InTreatment` | **Ya** — diuji untuk `InTreatment`, `UnderObservation`, `AwaitingDisposition` |
| 2. Triase pada kunjungan `Disposed` ditolak, kunjungan tidak terbuka kembali | **Ya** |
| 3. Triase pada kunjungan `Completed` ditolak | **Ya** — lubang `Completed` ditutup |
| 4. Pasien `WaitingForTriage` yang triasenya selesai **tetap** menjadi `Triaged` | **Ya** — diuji tersendiri, ditambah enam status terbuka yang tetap menerima triase |
| 5. Pesan penolakan persis seperti kontrak | **Ya** — *"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."* |

```
dotnet test --filter "FullyQualifiedName~EmergencyTriageVisitStatusTests"
→ Passed!  Failed: 0, Passed: 27, Skipped: 0, Total: 27

dotnet test ./QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj
→ Failed: 2, Passed: 711, Skipped: 0, Total: 713

dotnet build ./QuilvianSystemBackend.sln --configuration Release
→ Build succeeded. 0 Error(s), 150 Warning(s)
```

Suite naik dari **686 menjadi 713**. Dua yang gagal adalah **dua yang sama** milik
`InPatientManagement` yang sudah gagal sebelum task ini. **Nol regresi.**

### 5.1 Batas pembuktian

Test berjalan pada lapisan service dan penjaga, **bukan** controller lewat HTTP — provider
InMemory tidak menjalankan pipeline MVC. Karena itu kode balik `409` dibuktikan lewat
penelusuran kode, bukan lewat test. Yang dibuktikan test adalah keputusan yang mendasarinya:
kunjungan mana yang dianggap tertutup, dan status mana yang boleh berubah.

Pembuktian ujung-ke-ujung lewat HTTP menunggu kredensial petugas — utang yang sama yang
berlaku sejak roadmap revision `1`.

---

## 6. Yang sengaja tidak dikerjakan

| Yang tidak dikerjakan | Alasan |
| --- | --- |
| `RetriageAsync` yang juga melewatkan `Completed` | **`BE-IGD-020`**, task tersendiri |
| Lima titik tulis pada observasi, resusitasi, disposisi | **`BE-IGD-021`** |
| Penyelesaian kunjungan lewat penjaga | **`BE-IGD-022`** |
| Menggabungkan dua `SaveChangesAsync` pada jalur create | Di luar cakupan. Kunjungan tertutup sudah ditolak sebelum penyimpanan, sehingga tidak ada keadaan separuh jadi yang tersisa |
| `git commit` / `push` | Tidak diminta |
