# Laporan Perubahan Frontend — `FE-RAD-08`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-08` |
| Judul | Verifikasi pasien dan gerbang keselamatan |
| Epic | `EPIC RAD-05` |
| Requirement | `FR-RAD-040` (penomoran blueprint) |
| Decision | `RAD-DEC-002` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 4, gelombang `MVP-3` |
| Contract version | `RAD-API-001` grup *Rad Study* (`v1`, berjalan); `RAD-VAL-001` bagian 4 |
| Acceptance criteria | `AC-6` — butir keselamatan berbeda antar alat |
| Test yang diminta roadmap | `UAT-10` butir MRI dan CT-Scan berbeda; `UAT-02` alat tanpa aturan aktif ditolak beserta pesannya |
| **Ketentuan mengikat** | `RAD-ARCH-FE-001` bagian 10 — isian wajib dapat dioperasikan dengan papan ketik, karena radiografer sering memakai sarung tangan |
| Dependency | `FE-RAD-01` **selesai**; `BE-RAD-06` **selesai** |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only kecuali laporan ini dan baris status roadmap |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2` |
| Tanggal | 2026-09-14 |
| Status | **Selesai.** 14 unit test baru lulus; 843 unit test repository lulus, 0 gagal |

---

## 1. Keputusan yang menentukan seluruh task ini

**Butir keselamatan dibaca dari study, bukan dari katalog.**

Roadmap memerintahkan memakai ulang `getRadStudySafetyRequirements`, dan docstring berkas
service — ditulis pada `FE-RAD-01` — menyebutnya *"Butir keselamatan yang berlaku untuk sebuah
study."* **Keduanya keliru bila diikuti apa adanya**, dan mengikutinya akan menghasilkan persis
kegagalan yang risiko task ini peringatkan.

`GET /rad-studies/safety-requirements` memanggil `GetSafetyRequirementsAsync`, yang
keterangannya di backend berbunyi:

> *Katalog butir keselamatan yang dikenal sistem. **Ini kosakata, bukan kebijakan.***

Query-nya tidak menerima `modalityId` sama sekali — ia mengembalikan **seluruh** baris
`MstRadSafetyRequirements`. Merendernya sebagai daftar centang akan menampilkan butir yang sama
untuk MRI dan untuk CT-Scan, dan butir MRI akan muncul pada pemeriksaan USG.

Yang benar-benar berlaku bagi sebuah study adalah **`study.safetyChecks`**.
`AttachSafetyChecksAsync` membekukannya saat study dibuat, dari aturan yang berstatus `Active`
untuk modalitas dan pemeriksaan study itu:

```csharp
var rules = await LoadApplicableRulesAsync(modalityId, procedureId, now, cancellationToken);

foreach (var rule in rules)
{
    study.SafetyChecks.Add(new RadStudySafetyCheck
    {
        RequirementCodeSnapshot = rule.SafetyRequirement?.RequirementCode ?? string.Empty,
        IsMandatorySnapshot = rule.IsMandatory,
        RuleVersionSnapshot = rule.RuleVersion,
        CheckState = RadSafetyCheckState.Pending,
        ...
    });
}
```

Karena itu **butir mengikuti alat dengan sendirinya** — Definition of Done terpenuhi bukan
dengan menyaring di layar, melainkan dengan membaca sumber yang memang sudah tersaring. Kode,
nama, dan kewajibannya ikut dibekukan, sehingga perubahan master data kemudian tidak menulis
ulang apa yang ditanyakan hari itu.

**Docstring yang menyesatkan itu diperbaiki** pada `rad-study.service.js`. Ia berada di jalur
keselamatan pasien, dan membiarkannya berarti menunggu developer berikutnya membangun daftar
centang dari katalog.

---

## 2. Ketentuan mengikat: dapat dioperasikan dengan sarung tangan

`RAD-ARCH-FE-001` bagian 10. Dipenuhi pada empat titik, seluruhnya pada kartu butir:

| Yang dikerjakan | Alasannya |
| --- | --- |
| Kartu butir menerima fokus (`tabIndex={0}`), dan `1` / `2` / `3` menjawabnya | Angka dapat ditekan tanpa melihat dan tanpa melepas sarung tangan; huruf menuntut ketepatan yang tidak dimiliki jari bersarung |
| Pintasan dipasang **pada kartu**, bukan pada jendela | Angka yang ditekan selalu mengenai butir yang sedang disorot, tidak pernah butir lain |
| Peristiwa dari kolom catatan diabaikan | Petugas yang mengetik "1" pada catatan tidak sedang menjawab butir |
| Cincin fokus 3px yang tidak pernah dihilangkan; tombol jawaban `min-width: 150px` | Fokus harus terlihat dari jarak satu lengan, dan sasaran tekannya jauh di atas ukuran biasa |

Angka pintasannya tercetak pada tombolnya sendiri, sehingga tidak perlu dihafal.

---

## 3. `UAT-02` — alat tanpa aturan keselamatan aktif

Study yang lahir pada modalitas tanpa aturan `Active` punya **nol butir**. Layar
**tidak** memperlakukan daftar kosong sebagai "tidak ada syarat":

```js
if (list.length === 0) {
  return { policyConfigured: false, cleared: false, ... };
}
```

Yang tampil adalah kotak merah berisi pesan yang sama persis dengan yang akan dijawab server —
*"Aturan keselamatan untuk modalitas ini belum ditetapkan... Hubungi admin Radiologi..."* —
ditambah satu kalimat yang tidak ada di backend: **jangan memanggil pasien masuk sebelum
aturannya ada.** Backend menolak saat tombol ditekan; pada saat itu pasien sudah di ruang alat.

Ini perilaku fail-closed `RAD-DEC-002`: tidak adanya aturan berarti belum ada yang menetapkan
apa yang aman, **bukan** berarti semuanya aman.

---

## 4. Dua temuan

### 4.1 Kontrak menyebut `409`, jalur API menjawab `422`

`RAD-VAL-001` bagian 4 mencantumkan `409` untuk ketiga penolakan gerbang keselamatan.
`RadStudyController` memetakannya ke **`422`**, dan keterangannya menyatakan itu disengaja:

```csharp
RadOperationResultKind.SafetyBlocked or
RadOperationResultKind.PolicyNotConfigured or
    ... StatusCodes.Status422UnprocessableEntity
```

**Yang berlaku adalah source.** Layar memperlakukan `409` sebagai bentrok konkurensi dan
`422` sebagai penolakan gerbang, mengikuti perilaku nyata. **Kontraknya yang perlu diperbaiki**,
bukan kodenya — dan selama belum, pembaca kontrak akan menyiapkan penanganan yang salah.

### 4.2 Study dapat terkunci permanen ketika aturan berubah

Runtut kejadiannya:

1. Study dibuat; butirnya dibekukan dari aturan yang berlaku saat itu.
2. Admin **menambah** aturan wajib baru pada modalitas yang sama.
3. `ClearSafetyAsync` menilai terhadap aturan **sekarang**. Butir baru itu tidak punya baris
   pemeriksaan, dan `RadSafetyGateEvaluator` memperlakukannya sebagai belum dijawab — *"Butir
   wajib yang tidak punya baris pemeriksaan sama sekali diperlakukan sebagai belum dijawab...
   Ketiadaan jawaban bukan jawaban."*
4. `DecideSafetyCheckAsync` **menolak menjawabnya**: *"Butir keselamatan yang dimaksud tidak
   berlaku untuk study ini."*

Study itu terkunci. Tidak ada layar mana pun yang dapat menuntaskannya, dan petugas hanya
melihat penolakan yang menyebut kode butir yang tidak ada di hadapannya — sementara pasien
menunggu.

**Yang dikerjakan layar.** `findUnanswerableCodes` membandingkan kode yang disebut pesan server
dengan kode yang dimiliki study. Bila ada yang tidak dimiliki, layar menyatakannya beserta jalan
keluar satu-satunya: **rencanakan study baru**, yang membekukan ulang butir dari aturan terkini.

Pembacaan kalimat server memang rapuh. Karena itu hasilnya **hanya menambah keterangan**, tidak
pernah menyembunyikan atau mengganti pesan aslinya; bila polanya tidak cocok, hasilnya kosong
dan pesan server tetap tampil apa adanya. `S11` mengunci perilaku itu.

**Perbaikan sebenarnya milik backend** — menyelaraskan baris pemeriksaan dengan aturan yang
berlaku saat gerbang dinilai — dan **perlu keputusan pemilik modul**.

---

## 5. Perubahan yang dikerjakan

| Berkas | Keadaan |
| --- | --- |
| `src/lib/hooks/health-services/radiology-management/rad-safety-gate-rules.js` | Baru — fungsi murni; pembaca butir, pratinjau gerbang, pendeteksi butir terkunci, pembentuk muatan |
| `src/lib/hooks/health-services/radiology-management/use-rad-safety-gate.jsx` | Baru — controller |
| `src/components/view/health-services/radiology-management/rad-safety-gate/rad-safety-gate-view.jsx` | Baru — layar dua langkah |
| `src/style/health-services/radiology-management/rad-safety-gate/rad-safety-gate.module.css` | Baru — 22 kelas, seluruhnya design token |
| `src/app/health-services/radiology-management/rad-studies/[radOrderId]/page.jsx` | Baru — route |
| `src/lib/constants/health-services/radiology-management/rad-order-constants.jsx` | Diubah — `RAD_STUDY_STATUS_META`, `RAD_SAFETY_CHECK_STATE_META`, `RAD_SAFETY_CHECK_CHOICES`, `RAD_SAFETY_GATE_COPY` |
| `src/lib/constants/health-services/radiology-management/radiology-constants.jsx` | Diubah — `RADIOLOGY_ROUTES` menambah `orders`, `worklists`, `safetyGate()` |
| `src/lib/services/health-services/radiology-management/rad-study.service.js` | Diubah — **hanya docstring**; lihat bagian 1 |
| `src/components/view/…/rad-worklists/rad-worklist-view.jsx` | Diubah — kolom tindakan menuju layar ini, yang `FE-RAD-07` tunda ke task ini |
| `tests/unit/rad-safety-gate-rules.test.mjs` | Baru — 14 test |

`store.jsx` **tidak disentuh**, dan **tidak ada potongan Redux baru**. Rincian pesanan sudah
membawa konteks pasien **dan** study beserta butir keselamatannya dalam satu balasan —
`GetDetailAsync` memuatnya lewat `ThenInclude(s => s.SafetyChecks)` — sehingga satu permintaan
cukup. Setiap tindakan study menjawab dengan `RadStudyResponse` terbaru, dan jawaban itu dipakai
apa adanya sebagai keadaan layar berikutnya; tidak ada keadaan buatan layar yang bisa berselisih
dengan server.

Menu sidebar **tidak diubah**: layar ini beralamat pada satu pesanan dan dicapai dari daftar
kerja, bukan dari menu.

---

## 6. Mengapa tombol "Nyatakan Lolos" tidak dinonaktifkan saat butir belum tuntas

Layar menghitung pratinjau penahan dan menampilkannya sebelum tombol ditekan. Tombolnya tetap
dapat ditekan.

Menonaktifkannya akan menyembunyikan pesan server — dan pesan server adalah satu-satunya
keterangan yang berwenang. Pratinjau di layar menilai terhadap butir yang **terbawa balasan**;
server menilai terhadap aturan yang **berlaku saat itu**. Ketika keduanya berselisih — persis
keadaan bagian 4.2 — yang benar adalah server, dan petugas perlu membacanya.

Yang dicegah hanya pengiriman yang statusnya memang belum sampai di gerbang: `canClearSafety`
menuntut `PatientVerified`, sama seperti `ClearSafetyAsync`.

---

## 7. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-safety-gate-rules.test.mjs` | **14 lulus, 0 gagal** |
| `node --test tests/unit/` (seluruh repository) | **843 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **0 error, 0 warning** |
| `npm run build` | **Compiled successfully in 32.3s.** Route terdaftar: `/health-services/radiology-management/rad-studies/[radOrderId]` (dynamic) |

Cakupan test: `UAT-10` pada `S1` dan `S2`; `UAT-02` pada `S3` dan `S4`; pesan yang menyebut
butir penahan pada `S5`; urutan tindakan pada `S7` dan `S8`; butir terkunci pada `S9`–`S11`.

**`MANUAL TEST: NOT FEASIBLE`.** Menjalankan layar ini menuntut pesanan radiologi pada alat yang
punya aturan keselamatan berstatus `Active` beserta sesi petugas yang memegang `RadStudy :
Verify` dan `RadStudy : Safety`. Keduanya tidak tersedia dari sini: sesi login tidak dapat
dibuat, dan — lebih menentukan — **belum ada satu pun aturan keselamatan `Active` di lingkungan
mana pun**, karena `RAD-OPEN-011` masih terbuka. Setiap jalur yang diminta roadmap karena itu
dikunci unit test terhadap bentuk balasan yang diverifikasi langsung dari DTO backend, bukan
ditebak.

---

## 8. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Daftar centang dari `GET /safety-requirements` | Katalog, bukan kebijakan. Lihat bagian 1 — inilah kegagalan yang task ini cegah |
| Memaksa catatan terisi saat butir `Failed` | Backend tidak mewajibkannya, dan `RadSafetyRequirement.RequiresNote` **tidak ikut** pada `RadStudySafetyCheckResponse`. Layar menganjurkannya lewat keterangan, tidak menegakkan aturan yang tidak dimiliki server |
| Mulai acquisition, mutu, bahan terpakai | Milik `FE-RAD-09` |
| Potongan Redux study | Tidak ada yang perlu dibagi antar layar. Lihat bagian 5 |
| Base component baru | Dilarang `AGENTS.md`. Kartu identitas memakai `BasePatientVerificationCard` yang sudah ada |

---

## 9. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| **Study terkunci saat aturan berubah** | Temuan bagian 4.2. Layar menyatakannya; **perbaikan backend perlu keputusan pemilik modul** |
| **Kontrak `409` vs jalur API `422`** | Temuan bagian 4.1. Perbaikan dokumen `RAD-VAL-001` |
| **Tidak ada aturan keselamatan `Active`** | `RAD-OPEN-011` terbuka. **Selama belum ada, setiap pemeriksaan pada setiap alat akan ditolak gerbang** — layar ini terbuka, tetapi tidak satu pun pasien dapat melewatinya |
| **`RequiresNote` tidak terbawa balasan study** | Layar tidak dapat membedakan butir yang wajib bercatatan. Penambahan ruas pada `RadStudySafetyCheckResponse` akan menyelesaikannya |
| **Daftar kerja tanpa identitas pasien** | Temuan `FE-RAD-07`, masih terbuka. Layar ini **menutup sebagiannya**: identitas lengkap tampil di sini sebelum pemeriksaan dijalankan |
| **`HoldAsync` menerima status terminal** | Temuan `FE-RAD-06`, masih terbuka |

---

## 10. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-14 | Laporan dibuat. 6 berkas baru, 4 diubah. 14 test baru lulus; 843 test repository lulus; lint 0 error; build lulus. Definition of Done dipenuhi dengan membaca butir dari `study.safetyChecks`, dan docstring `FE-RAD-01` yang menyesatkan diperbaiki. Dua temuan dicatat: kontrak `409` vs jalur API `422`, dan study yang dapat terkunci permanen saat aturan keselamatan berubah. | `draft` |
