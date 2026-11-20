# Laporan Perubahan Backend — `BE-HRD-402`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-402` |
| Judul | Gerbang backend Menu 04: build, kontrak, uji negatif izin, dan konkurensi |
| Slice | `S-FIN-M04` / Verifikasi (alias `FIN-HRD-04-BE-016`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-402` |
| Trace | Seluruh task `BE-HRD-387` s.d. `BE-HRD-401`; `UAT-04-01` s.d. `48` |
| Contract version | Kontrak Data Induk dan Workforce Core existing |
| Dependency | `BE-HRD-391`, `393`, `394`, `395`, `398`, `401` — seluruhnya ✅ 20 November 2026 |
| Klasifikasi | `LIGHT` — verifikasi; tanpa perubahan source di luar task `387`–`401` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | Laporan ini, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), perubahan slice belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — build dan QBE lulus dengan log; uji negatif izin dan konkurensi dicatat apa adanya (`NOT RUN` pada runtime) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `MasterData/Workforce` + `WorkforceCore` |
| Keberlakuan | Verifikasi atas `TOUCHED LEGACY` dan `NEW CODE` slice `S-FIN-M04` |
| QBE yang diperiksa | Seluruh detektor checker atas 20 berkas source slice (`Strict`) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Ringkasan slice `S-FIN-M04`

Laporan agent sebelumnya menulis "0 compile errors" dan "48/56 UAT PASS" tanpa perintah yang dijalankan. Gerbang ini mengganti klaim itu dengan bukti perintah, dan mencatat apa yang belum dapat dibuktikan.

| Task | Hasil utama |
| --- | --- |
| `BE-HRD-387` ✅ | Resolver identitas: kunci per NIK/paspor, kode `WFP-` dari alokator atomik, paspor WNA untuk Pegawai |
| `BE-HRD-388` ✅ | Create atomik; akun login ganda per profil ditolak sebelum menabrak index |
| `BE-HRD-389` ✅ | Guard identitas (termasuk huruf paspor dan jenis identitas), status perkawinan, NPWP; audit `ChangedFields` |
| `BE-HRD-390` ✅ | Menu 06: target harus milik profil yang sama; apply tepat sekali |
| `BE-HRD-391` ✅ | Rekening: tanpa verifikasi diri lewat `POST`/`PUT`; rekening belum terverifikasi tidak mengambil alih rekening gaji |
| `BE-HRD-392` ✅ | Keluarga: jenis identitas ikut dikunci |
| `BE-HRD-393` ✅ | Tanggungan: pengosongan relasi anggota keluarga dikunci |
| `BE-HRD-394` ✅ | Pendidikan tertinggi tepat satu walau serentak |
| `BE-HRD-395` ✅ | Pengalaman eksternal terbukti (tanpa perubahan khusus) |
| `BE-HRD-396` ✅ | Riwayat SK: tiga jalan memutar (cabut approval, nonaktifkan, hapus berkas) ditutup |
| `BE-HRD-397` ✅ | Dokumen rahasia berbasis hak `WorkforceDocument : ViewConfidential`; berkas pegawai tidak lagi statis |
| `BE-HRD-398` ✅ | Perubahan isi dokumen mencabut verifikasi |
| `BE-HRD-399` ✅ | Status kredensial dokter tidak dapat diubah dari Data Induk |
| `BE-HRD-400` ✅ | Employee 360: hak atasan pada tanggal server; alasan SK disamarkan |
| `BE-HRD-401` ✅ | Histori profil nonaktif tetap terbuka; reaktivasi purnatugas dan pembukaan kunci skorsing ditolak |

## 2. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>/build-out/` (bin lokal terkunci oleh backend pengguna yang sedang berjalan, sehingga keluaran diarahkan ke folder sementara) | Berhasil, **0 error, 608 warning**; himpunan warning identik dengan build sebelum perubahan slice (satu warning nullable baru di `HrdEmployee360Service` sempat muncul dan diperbaiki sebelum build final) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` atas 20 berkas source slice (termasuk 3 berkas baru dan `Program.cs`) | **0 VIOLATION, 0 REVIEW, PASS** | `PASS` | Keluaran checker |
| Validator registri hak akses atas DLL hasil build (`PermissionRegistryDescriptor.BuildFromAssembly` + `PermissionRegistryValidator.Validate`, skrip `dotnet fsi` di folder sementara, tidak masuk repository) | `IsValid=True`; 2.635 identitas kanonik; 0 endpoint tak terdaftar, 0 identitas ganda, 0 `AccessType` tidak sah; `WorkforceDocument.ViewConfidential` terdaftar | `PASS` | Keluaran skrip |
| Verifikasi kontrak API (method, route, hak akses) | Tidak ada route/DTO baru; setiap endpoint yang disentuh tetap memakai `[AccessAction]` + `[AccessPermission]` yang sama; kode galat baru tercatat per laporan task | `PASS` | Laporan `387`–`401` bagian 4 |
| Uji negatif izin (403/409) via HTTP | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan; backend lokal milik pengguna sedang berjalan dan tidak dijalankan ulang tanpa izin |
| Uji konkurensi resolver (dua create serentak, NIK sama) via HTTP | Belum dijalankan | `NOT RUN` | Idem; dibuktikan lewat penelusuran source di `BE-HRD-387` |
| Startup aplikasi (seeder akses, Swagger) | Belum dijalankan | `NOT RUN` | Idem |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 3. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `dotnet build` dan QBE lulus dengan log perintah | Terpenuhi | Baris 1–2 tabel verifikasi |
| (2) Uji negatif izin dan konkurensi resolver dicatat PASS/FAIL/NOT RUN apa adanya | Terpenuhi | Baris 5–7 dicatat `NOT RUN` beserta alasan; penelusuran source per task |

## 4. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration / basis data | Tidak ada migration. Tidak ada perintah database yang dijalankan |
| Perubahan perilaku yang perlu diketahui admin | Hak baru **View Confidential Workforce Document** perlu dicentang untuk peran HR yang boleh membuka dokumen rahasia pegawai lain |
| Dampak ke frontend | (a) `FileUrl` dokumen/ijazah kini path API — frontend sudah menerjemahkan path relatif ke origin API; (b) centang "Terverifikasi" saat membuat rekening kini `403`; (c) status kredensial dokter di form Data Induk sebaiknya hanya-baca |
| Temuan di luar scope | `HiringHandoffService` (kode profil dan nomor `HND-` dari `Count+1`); `EmployeeProfileChange : Apply` tidak ditegakkan (memakai `Update`); `PayrollCalculationService` tidak mensyaratkan rekening terverifikasi; hak `HrdPersonnelActionOrder : Read` membuka mode HR penuh Employee 360 |
| Perubahan sampingan | `NONE`. Berkas tak terlacak `docs/module-blueprints/human-resource/docs/Blueprint_UI_ABK_Renbut_Quilvian.md` muncul selama sesi dan bukan bagian slice ini; tidak disentuh |
| Status Git | 17 berkas source/dokumen diubah, 3 berkas source baru, 16 laporan baru (lihat `git status --short` saat laporan ditulis); tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Langkah berikutnya | Slice `S-C5-SEP` (Menu 12: `BE-HRD-365`, `368`, `372`, `373`) lalu `S-FIN-M07` |
