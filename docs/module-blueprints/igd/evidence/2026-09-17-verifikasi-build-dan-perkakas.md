# Verifikasi Build Frontend, Perbaikan Perkakas Test, dan Penaikan Klasifikasi `BE-IGD-039`

| Field | Nilai |
| --- | --- |
| Tanggal | 17 September 2026 |
| Jenis | Evidence verifikasi + satu perbaikan perkakas. **Nol source aplikasi diubah**, nol migration, nol kueri basis data |
| Pemicu | Review modul IGD atas permintaan Product/Domain Owner, lalu perintah menjalankan urutan rekomendasinya |
| Backend | `NewQuilvianSystemBackend` branch `rizkiG` `94bf6ec5` |
| Frontend | `QuilvianSystemFrontendDev` branch `RizkiV2` `eee3f254d` |
| Batasan yang dipatuhi | `dotnet build` **tidak** dijalankan (milik Rizki); uji layar **tidak** dijalankan (butuh kredensial petugas); basis data **tidak** disentuh |

---

## 1. `IGD-EV-137` — `npm run build` frontend lulus bersih

Dijalankan pada `eee3f254d`, yaitu commit yang sudah memuat pekerjaan R3.9
(`FE-IGD-031` dan `FE-IGD-032`).

```
npm run build
→ exit 0
→ 0 error, 0 warning
→ postbuild: [prepare-standalone] Standalone runtime siap dijalankan.
```

Keempat route IGD terkompilasi:

| Route | Jenis |
| --- | --- |
| `/health-services/emergency-installation-management/emergency-assessment` | Static |
| `/health-services/emergency-installation-management/emergency-assessment/[slug]` | Dynamic |
| `/health-services/emergency-installation-management/emergency-triage` | Static |
| `/health-services/emergency-installation-management/emergency-triage/[slug]` | Dynamic |
| `/health-services/registration-management/emergency-registration` | Static |

**Task yang terdampak.** Penahan `npm run build` pada `FE-IGD-023`, `FE-IGD-024`,
`FE-IGD-031`, dan `FE-IGD-032` **lunas**. Keempatnya kini menyisakan **uji lewat layar** saja
— dan untuk `FE-IGD-032`, penilaian pemilik atas dua delta bagian 8.

Ini **bukan** bukti runtime dan **bukan** UAT. Build membuktikan kode terkompilasi, bukan
perilakunya benar.

## 2. `IGD-EV-138` — perkakas `npm test` diperbaiki, dan klaim lamanya dikoreksi

### 2.1 Koreksi klaim lama

`frontend-roadmap.md` bagian R3.4 menulis bahwa `npm test` **lulus dengan exit `0`** tanpa
menjalankan satu test pun. **Itu keliru.** Diukur ulang pada Node `v20.20.2`:

```
npm run test:unit  →  Could not find 'tests\unit\**\*.test.mjs'
                   →  exit code 1
```

Klaim lama kemungkinan besar lahir dari pembacaan yang salah: keluaran `npm test` dipipa ke
`tail`, sehingga exit code yang terbaca milik `tail`, bukan milik `npm`. Akibatnya bahaya ini
**dilaporkan jauh lebih gawat daripada kenyataannya** — CI akan merah, bukan hijau palsu.

### 2.2 Perbaikan

Satu baris pada `package.json`:

```diff
- "test:unit": "node --import ./tests/helpers/register.mjs --test \"tests/unit/**/*.test.mjs\""
+ "test:unit": "node --import ./tests/helpers/register.mjs --test tests/unit"
```

Direktori, bukan glob — benar pada Node 20 maupun Node 21. Verifikasi sesudahnya:

```
npm test  →  866 lulus, 0 gagal, exit 0
```

Angka `866` sama persis dengan yang dilaporkan `FE-IGD-031` dan `FE-IGD-032` lewat perintah
manual, jadi perbaikan ini **tidak mengubah cakupan test**, hanya membuat `npm test` benar-benar
menjalankannya.

**Perubahan ini di luar modul IGD** — `package.json` milik seluruh frontend. Ia dicatat di sini
karena ditemukan lewat review IGD, dan karena laporan task IGD selama ini memakai perintah
manual sebagai penggantinya.

## 3. `IGD-EV-139` — `BE-IGD-039` dinaikkan menjadi blocker lintas gelombang

Pembacaan source pada `94bf6ec5`, tanpa menjalankan apa pun.

### 3.1 Dua lapis, dan lapis kedua tidak pernah tercapai

`EmergencyUnitAuthorityService.PeriksaAsync`:

| Lapis | Isi | Keadaan |
| ---: | --- | --- |
| 1 | Gerbang fail-closed `IGD-DEC-092` untuk unit tanpa `OrganizationUnitId` | **Selalu menyala** — pemetaan 0 dari 18 |
| 2 | `ApplicationUserOrganization.DepartmentId` vs `MstServiceUnit.OrganizationUnitId` | **Tidak pernah dieksekusi** |

Konsekuensinya penting untuk perencanaan: memperbaiki cacat perbandingan identitas **saja**
tidak membuka apa pun selama pemetaan unit masih kosong. Kedua pekerjaan itu harus turun
bersamaan.

### 3.2 Jalan keluar beralasan `IGD-DEC-092` tidak punya pemakai

Lapis 1 mengembalikan `Hasil(Berwenang: false, MembutuhkanAlasan: true, …)`. Flag
`MembutuhkanAlasan` itulah jalan keluar yang dijanjikan keputusan tersebut.

Keempat titik panggil pada `EmergencyDepartureService` — `arrive`, `accept-handover`, sikap
pesanan, dan pemeriksaan unit asal — **hanya** memeriksa `!authority.Berwenang` lalu
mengembalikan `403`. Tidak ada jalur pengisian alasan, dan tidak ada tempat alasan itu
disimpan.

### 3.3 Akibat pada klasifikasi gelombang

| Yang tertahan | Requirement | Klasifikasi lama |
| --- | --- | --- |
| `arrive` — kedatangan memindahkan pemilik klinis | `FR-IGD-023`…`025`, `027` | hanya `MVP-6` |
| `accept-handover` | `EPIC IGD-05` | hanya `MVP-6` |
| Sikap pesanan pada penutupan | `FR-IGD-045`, `047`…`051` | hanya `MVP-6` |
| `EPIC IGD-08` | `FR-IGD-053`…`059` | `MVP-6` — **sudah benar** |

`MVP-4` dan `MVP-5` **tetap** 🟡, bukan ⛔ — implementasinya ada dan boleh dilanjutkan. Yang
tidak dapat dilakukan adalah **membuktikannya lewat layar**. Keduanya karena itu tidak akan
mencapai ✅ lewat jalur uji layar selama blocker ini terbuka.

Dicatat pada `MODULE-STATUS.md` bagian *Kewenangan unit memblokir lintas gelombang*.

## 4. Angka progres dikoreksi

`MODULE-STATUS.md` masih memuat angka 15 September yang sudah usang.

| Lapisan | Tertulis | Benar per register roadmap |
| --- | --- | --- |
| Backend | 18 / 29 = 62% | **20 / 30 = 67%** |
| Frontend | 6 / 17 = 35% | **10 / 22 = 45%** |

Penyebabnya penambahan `BE-IGD-046` ✅ ke register dan naiknya `FE-IGD-023`, `024`, `028`,
`030` menjadi ✅, ditambah masuknya `FE-IGD-029`…`032` ke penyebut.

## 5. Yang **tidak** dikerjakan, beserta alasannya

| Pekerjaan | Alasan tidak dikerjakan |
| --- | --- |
| `dotnet build` untuk `BE-IGD-040`/`041` | Instruksi tetap pemilik: build backend dijalankan Rizki sendiri |
| Uji API 3 skenario `BE-IGD-041` | Menuntut backend berjalan, dan itu menuntut build di atas |
| Uji layar `FE-IGD-029` kriteria 2 | Butuh kredensial petugas; `NOT FEASIBLE` bagi agent |
| Uji layar `FE-IGD-023`, `024`, `031`, `032` | Sama |
| `BE-IGD-044` (`EPIC IGD-04`) | Gerbang urutan pemilik 16 September 2026 belum lunas: butir 1 (`FE-IGD-029`) dan butir 2 (`BE-IGD-041`) menuntut bukti runtime lebih dulu |
