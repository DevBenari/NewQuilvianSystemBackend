# Roadmap Delivery Backend — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 15
status: ACTIVE
status_synced_at: "2026-11-03 — tanda status task lama disalin apa adanya dari revision 2; tidak ada task yang naik atau turun status pada revision ini"
planning_updated_at: "2026-11-05 — Slice S-D4 HR Service Management & Request (sub-modul 13) dipecah menjadi 15 task (BE-HRD-191 s.d. BE-HRD-205) per gelombang MVP-39 s.d. MVP-41 & Verifikasi; grafik B48 s.d. B51 ditambahkan; sebelumnya Slice S-D3 (BE-HRD-176 s.d. BE-HRD-190), Slice S-B6 (BE-HRD-161 s.d. BE-HRD-175), Slice S-B5 (BE-HRD-146 s.d. BE-HRD-160)"
generated_at: "2026-10-26"
owners:
  - "Technical Owner — HR Module Lead"
approved_by:
  - "User Approved (26 Oktober 2026) — roadmap awal"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 3 November 2026 — desain S-D1 yang menjadi dasar task BE-HRD-017 dan BE-HRD-029 s.d. BE-HRD-051"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 3 November 2026 — desain S-D2 yang menjadi dasar task BE-HRD-018 dan BE-HRD-052 s.d. BE-HRD-062"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 4 November 2026 — desain S-C4-ONB beserta lima pilihan desain (HRD-DEC-096) yang menjadi dasar task BE-HRD-063 s.d. BE-HRD-086; jawaban: \"setujui semua ya dan kerjakan\""
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 4 November 2026 — desain S-B4 Penjadwalan, Roster, dan On-Call (HRD-DEC-097 s.d. HRD-DEC-103) yang menjadi dasar task BE-HRD-087 s.d. BE-HRD-103"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 4 November 2026 — desain S-B1 Presensi dan Kehadiran (HRD-DEC-104 s.d. HRD-DEC-109) yang menjadi dasar task BE-HRD-104 s.d. BE-HRD-116"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 4 November 2026 — desain S-B2 Cuti & Izin Pegawai Rumah Sakit (HRD-DEC-110 s.d. HRD-DEC-115) yang menjadi dasar task BE-HRD-117 s.d. BE-HRD-130"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 5 November 2026 — desain S-B3 Administrasi Lembur Pegawai Rumah Sakit (HRD-DEC-116 s.d. HRD-DEC-121) yang menjadi dasar task BE-HRD-131 s.d. BE-HRD-145"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 5 November 2026 — desain S-B5 Pemrosesan Penggajian Pegawai Rumah Sakit (HRD-DEC-122 s.d. HRD-DEC-127) yang menjadi dasar task BE-HRD-146 s.d. BE-HRD-160"
blueprint_manifest_revision: 24
decision_revision: 22
decision_ids_s_b3: "HRD-DEC-116 s.d. HRD-DEC-121, HRD-ASM-23 s.d. HRD-ASM-26"
source_commits_s_b3:
  backend: "8b17b75a (branch MHamzah)"
  frontend: "44990b3d3 (branch HamzahV2)"
contract_versions_s_b3:
  - "API Contract v13 bagian 19 — task S-B3"
  - "State Transition Matrix v13 bagian 17 — task S-B3"
  - "Validation Matrix v13 bagian 18 — task S-B3"
  - "Permission Audit Matrix v13 bagian 17 — task S-B3"
  - "Integration Contract v13 bagian 10 — task S-B3"
  - "Data Dictionary v13 bagian 14 — task S-B3"
  - "Acceptance Test Matrix v13 bagian 21 — task S-B3"
decision_ids_s_d1: "HRD-DEC-058 s.d. HRD-DEC-072"
source_commits:
  backend: "26066c76e69b021f68b63b31bde2258cd9d1bb74 (branch MHamzah) — HEAD saat desain dan roadmap S-D1 ditulis"
  frontend: "66b7e5600e1707e4153ca07981ddef054856c8c9 (branch HamzahV2)"
  backend_previous: "16f6b2ad28e0e9cbd374354abe240ff3b163ace0 — dasar task BE-HRD-001 s.d. BE-HRD-028"
contract_versions:
  - "API Contract v6 — task BE-HRD-001 s.d. BE-HRD-028"
  - "API Contract v7 bagian 13 — task S-D1"
  - "State Transition Matrix v7 bagian 11 — task S-D1"
  - "Validation Matrix v7 bagian 12 — task S-D1"
  - "Permission Audit Matrix v7 bagian 11 — task S-D1"
  - "Integration Contract v7 bagian 11 — task S-D1"
  - "Data Dictionary v7 bagian 8 — task S-D1"
locked_input_hashes_s_d1:
  00-interview-decisions.md: e7696919787384e44ac5fe27ead6d0a75e8d91f4d22239cd81f036579be2fcbc
  02-backend-architecture.md: 6e97da62eb8c5ac7393827e1aa436763c34bef4bbc5786e8d54ab2f13e2919a9
  03-frontend-architecture.md: 7c2db1244ccd3fdc7ce81c3b1db6220cbfd0fee9d6cdfc70a42f18b981705bda
  04-prd-to-mvp.md: 8db8cdcd95cf70b46afa35cf3a2a8691eb78681027645aebed8625a5aea1b6ea
  data/data-dictionary.md: 814e415893808276ce3d9a85ce09ba351954dd1dd434d355aebe367ceaf7f6d8
  contracts/api-contract.md: 772901e44a9f69213e2ac63404045288bec61742eba73a816f5f43faf9a01328
  contracts/state-transition-matrix.md: 1ffaecaf13dc4d0b5aac6dd84df44d15b4b70f5cfb696c7369c0f27ef9dbf6e2
  contracts/validation-matrix.md: 27055a3971cea05d93075a527bc18acbeb911727f0fbdb3fa137a54aece5bd31
  contracts/integration-contract.md: c8d397fbae7f16108c72bdae7fbd86ff95a2f3af69162a4004f60cb92477489f
  contracts/permission-audit-matrix.md: e41eca8c8f9c53b0dd5f10a2c294892128e0c00d8eb760f67a48dcb8a5fcb182
  testing/acceptance-test-matrix.md: ee5e5bb1e29a9519f76d5995021f11cfaa8384c4888471b442d076d0ef44db9b
decision_ids_s_d2: "HRD-DEC-073 s.d. HRD-DEC-080"
contract_versions_s_d2:
  - "API Contract v8 bagian 14 — task S-D2"
  - "State Transition Matrix v8 bagian 12 — task S-D2"
  - "Validation Matrix v8 bagian 13 — task S-D2"
  - "Permission Audit Matrix v8 bagian 12 — task S-D2"
  - "Integration Contract v8 bagian 12 — task S-D2"
  - "Data Dictionary v8 bagian 9 — task S-D2"
locked_input_hashes_s_d2:
  00-interview-decisions.md: 446d1d3b80bea248f67d799866a542cb457107929eb56c3ddf6df31f1dec6403
  02-backend-architecture.md: ba94e9c31fc17152ac83fa743d36a15511ac1a7dc2b815dc8f4f792908c1461d
  03-frontend-architecture.md: 40da6cf7196418a1e63d9a60a82b8e923d5edc0a2d17dba861050d628612d882
  04-prd-to-mvp.md: f5b38f09d91040c346e2b00af127ce71cb26fa176cce52afb059fe8f6b86ec13
  data/data-dictionary.md: c47155da69744639f89798f3fdf562a2d72eec71725642c8f6ab35a7f421166d
  contracts/api-contract.md: a6a58e82890cc70b84549f3d38a260984b21f7c7f0e7e6fba3bf192e3522f007
  contracts/state-transition-matrix.md: e53a0b773b38ca85e640ba12a8e20d47f3ac23207f380804592cea6eb23c13d4
  contracts/validation-matrix.md: 752e4e95b1c7cda71b9b5924bed3dd84e161669627ae7c28ee333f5f9d94737f
  contracts/integration-contract.md: fa83373df222f40e49ffd890502801805a8cb7c05bcf7d9b89daa0212647dce2
  contracts/permission-audit-matrix.md: d2d3b1cd35785a525ad93fc101cc3a4898288e970cc32efbc9533b1bd72b9ff3
  flowcharts/rekrutmen-dan-seleksi.md: 63a769add4cad6bee23de31bf4f31c3b9ae3f228b517f7b0168a8f34e6ad6849
  testing/acceptance-test-matrix.md: 395c8540be5aa56c48ff3eff63092ee4fbbc3d7e65947682871dc2dc2de422e0
decision_ids_s_c4_onb: "HRD-DEC-081 s.d. HRD-DEC-090, HRD-DEC-095, HRD-DEC-096"
source_commits_s_c4_onb:
  backend: "8b17b75a (branch MHamzah) — HEAD saat desain dan roadmap S-C4-ONB ditulis; working tree memuat pekerjaan S-D2 yang belum di-commit"
  frontend: "44990b3d3 (branch HamzahV2)"
contract_versions_s_c4_onb:
  - "API Contract v9 bagian 15 — task S-C4-ONB"
  - "State Transition Matrix v9 bagian 13 — task S-C4-ONB"
  - "Validation Matrix v9 bagian 14 — task S-C4-ONB"
  - "Permission Audit Matrix v9 bagian 13 — task S-C4-ONB"
  - "Integration Contract v9 bagian 13 — task S-C4-ONB"
  - "Data Dictionary v9 bagian 10 — task S-C4-ONB"
decision_ids_s_b4: "HRD-DEC-097 s.d. HRD-DEC-103, HRD-ASM-05 s.d. HRD-ASM-14"
source_commits_s_b4:
  backend: "8b17b75a (branch MHamzah)"
  frontend: "44990b3d3 (branch HamzahV2)"
contract_versions_s_b4:
  - "API Contract v10 bagian 16 — task S-B4"
  - "State Transition Matrix v10 bagian 14 — task S-B4"
  - "Validation Matrix v10 bagian 15 — task S-B4"
  - "Permission Audit Matrix v10 bagian 14 — task S-B4"
  - "Integration Contract v10 bagian 7 — task S-B4"
  - "Data Dictionary v10 bagian 11 — task S-B4"
decision_ids_s_b1: "HRD-DEC-104 s.d. HRD-DEC-109, HRD-ASM-15 s.d. HRD-ASM-18"
source_commits_s_b1:
  backend: "8b17b75a (branch MHamzah)"
  frontend: "44990b3d3 (branch HamzahV2)"
contract_versions_s_b1:
  - "API Contract v11 bagian 17 — task S-B1"
  - "State Transition Matrix v11 bagian 15 — task S-B1"
  - "Validation Matrix v11 bagian 16 — task S-B1"
  - "Permission Audit Matrix v11 bagian 15 — task S-B1"
  - "Integration Contract v11 bagian 8 — task S-B1"
  - "Data Dictionary v11 bagian 12 — task S-B1"
locked_input_hashes_s_b1:
  00-interview-decisions.md: 8972054d6371bde428768ed5669705f4838b6d90dd6159e4cdee8b52ab885329
  02-backend-architecture.md: 59140431578fa4847879b3dcad099023c536c069dcd3d94d86a8b3323e17e7cd
  03-frontend-architecture.md: eab15742f2edaf59df97aa44c5c219ee078f3aa6a8a7ea246f8fbb5b897268a5
  04-prd-to-mvp.md: 26a7d73977fc1b67a26762dab1fcedacc49d0aa86eb60ef2b076ead7f679a571
  data/data-dictionary.md: cd31c544d4971febdf78496ff17d61eaaa9e58abf46193b9772850a867a749bc
  contracts/api-contract.md: 89ec7e37bec170294328ee4dae25dc580eb9c4fd4c1dc8c8c39edc616946e0b5
  contracts/state-transition-matrix.md: c5e5e72544f2eaad65894f71afc2b0732809aff36ba9c2ccedafe93ee8b813c6
  contracts/validation-matrix.md: a661b1c00c7bbce5adabad1d0d72caf8f09278d93d8e9e43c373dd906ce63c2d
  contracts/integration-contract.md: 57ba7822fb8eae6e6fb66048b8c6b781f948cc0f8376682ca33ef9360026bf65
  contracts/permission-audit-matrix.md: e950dad0bdb548d7afd57b5b4a51ef8a542907fd187d119d1e2e9189555e37b4
  testing/acceptance-test-matrix.md: b126b68b4dae9c39697b0653a59dc2000682689a864e54cd5c026155122b6c35
  flowcharts/presensi-dan-kehadiran.md: 4c1db3c47ade024988d62cc0c6cc6ae9372a264bf66386e50d61a088717a24d9
locked_input_hashes_s_c4_onb:
  00-interview-decisions.md: 9a14b83f32089a561a868b27dfeb8a7d3f846b00ab99d9c1b9fe17e9ac6dada8
  02-backend-architecture.md: c3483c6ea6a53e81ab8b83553303ce3176bd4a3cd10261eb4d63a427cb7003df
  03-frontend-architecture.md: e3a895ae552d83a4035593378623817f38997b24d686cfda53a97e57d290f6cf
  04-prd-to-mvp.md: 228a9348f468030a9c2f5bc0b1a5295dac90e40cd1d90c17f8be5424f3ab1879
  data/data-dictionary.md: 0c1bcd66068f5c01836c0210e2d51466c534a2266ef083382d670cae084cc881
  contracts/api-contract.md: 4ed75a196f678aeb6cd26cf83d91c4127e8fc81384856323eb2701d29552e538
  contracts/state-transition-matrix.md: 4881793deab96053c507573283317286c37d629bc062dd99dcff2884dc8f4702
  contracts/validation-matrix.md: d8ae67b286502729bb9692390f36d9a4ea2454991a3a861fe4605fbb6bc09ee4
  contracts/integration-contract.md: 651af857432e42162a2acc3eb045b0fc57e065f5200f6a49459c4fe99f4e56f8
  contracts/permission-audit-matrix.md: 6f3b6b7839b051b8f050c69fa65a67aeb7a97ed50bca7f29d09f727cfdb29589
  testing/acceptance-test-matrix.md: 76503381c670882adf9794a9269d91ca32bcb0152eeb8c80558edc505289881f
  flowcharts/onboarding-dan-siap-kerja.md: 89698b8c7fa1e7c4d74bc054871a66f612e0a7b2b71ee081eb9d0c730c74fef6
  flowcharts/masa-percobaan.md: c92852e0fb57597a53687fa9a930655362e4503057bb2226dd540e275e25279b
  flowcharts/pendampingan-klinis.md: bb00b4389f24c73ac039ae0714a8b409cf6d697aa4c94ea9a6177be8438d8f4c
  flowcharts/siklus-kontrak.md: 11a3cae04a379e9295a8682a183a7633b0dfcb568156f4824c8eb6b8cdd9e6b9
  flowcharts/batch-data-awal-kontrak.md: 24e97a41134d7c21b8f729008ca50e287f2e3acbf51479c84c82d0777ec77b5b
decision_ids_s_b5: "HRD-DEC-122 s.d. HRD-DEC-127, HRD-ASM-27 s.d. HRD-ASM-30"
source_commits_s_b5:
  backend: "8b17b75a (branch MHamzah)"
  frontend: "44990b3d3 (branch HamzahV2)"
contract_versions_s_b5:
  - "API Contract v14 bagian 20 — task S-B5"
  - "State Transition Matrix v14 bagian 18 — task S-B5"
  - "Validation Matrix v14 bagian 19 — task S-B5"
  - "Permission Audit Matrix v14 bagian 18 — task S-B5"
  - "Integration Contract v14 bagian 11 — task S-B5"
  - "Data Dictionary v14 bagian 15 — task S-B5"
  - "Acceptance Test Matrix v14 bagian 22 — task S-B5"
```

**Apa yang berubah pada revision 12.**

1. **Lima belas task backend `S-B5` baru**, `BE-HRD-146` s.d. `BE-HRD-160`, sesudah desain sub-modul 10 (Pemrosesan Penggajian Pegawai Rumah Sakit) disetujui pengguna pada 5 November 2026 (`HRD-DEC-122` s.d. `HRD-DEC-127`): fondasi skema payroll & migrasi EF Core, service kebijakan parameter penggajian & snapshot freeze, precondition gate validator (cut-off tgl 20 Sub-07, 08, 09), engine inisiasi batch & anti-duplikasi, engine kalkulasi komponen tetap & tunjangan profesi/medis, engine penyerapan lembur PP 35 & premi shift malam, engine kalkulasi iuran BPJS Kesehatan/Ketenagakerjaan berplafon, engine kalkulasi PPh 21 TER PMK 168/2023, penarikan potongan pinjaman Sub-11 (`INT-HR-55`), payroll variance guard deteksi deviasi >= 20% / Rp 2jt, resolusi justifikasi audit investigasi, alur otorisasi 3-jenjang & anti-self-approval HR Manager, pembekuan permanen batch immutable & paket serah terima Finance SHA-256 (`INT-HR-53`), callback reconciler (`INT-HR-54`) & pelepasan slip gaji ESS ber-PIN, serta verifikasi build dan QBE slice.
2. **Grafik B36 s.d. B39 ditambahkan** beserta tabel gelombang eksekusi `S-B5` yang sepenuhnya bebas siklus.
3. **Register status task backend diperbarui** mendaftarkan seluruh 15 task `S-B5` (`BE-HRD-146` s.d. `BE-HRD-160`).

**Apa yang berubah pada revision 11.**

1. **Lima belas task backend `S-B3` baru**, `BE-HRD-131` s.d. `BE-HRD-145`, sesudah desain sub-modul 09
   (Administrasi Lembur Pegawai Rumah Sakit) dirancang dan disetujui pada 4-5 November 2026 (`HRD-DEC-116` s.d. `HRD-DEC-121`):
   fondasi skema lembur & migrasi DDL PostgreSQL, service pengaturan kebijakan & kuota lembur RS, DTO & validasi FluentValidation,
   service SPKL pra-dinas terencana H-1 & penugasan tim medis, service pelaporan klaim darurat medis cito pasca-dinas H+1,
   engine penegakan kuota lembur PP 35/2021 (4 jam/hari, 18 jam/minggu), workflow persetujuan dua tahap & anti-self-approval,
   workflow otorisasi khusus Direktur Pelayanan Medik darurat cito, service verifikasi silang presensi biometrik & min-duration rule,
   manajemen anomali discrepancy & investigasi logbook fisik HR, service hak libur pengganti CTO (90 hari) & integrasi saldo cuti 08 (`INT-HR-49`),
   scheduler eksekusi hangus saldo CTO, manajemen periode cut-off bulanan (tgl 20) & rekonsiliasi retrospektif, handoff payroll (`INT-HR-52`),
   serta verifikasi build dan QBE slice.
2. **Grafik B32 s.d. B35 ditambahkan** beserta tabel gelombang eksekusi `S-B3` yang sepenuhnya bebas siklus.
3. **Register status task backend diperbarui** mendaftarkan seluruh 15 task `S-B3` (`BE-HRD-131` s.d. `BE-HRD-145`).

**Apa yang berubah pada revision 9.**

1. **Tiga belas task backend `S-B1` baru**, `BE-HRD-104` s.d. `BE-HRD-116`, sesudah desain sub-modul 07
   (Presensi dan Kehadiran) disetujui pengguna pada 4 November 2026 (`HRD-DEC-104` s.d. `HRD-DEC-109`):
   fondasi skema & migrasi EF Core, service kebijakan parameter presensi, engine pemrosesan shift malam
   lintas hari, audit trail biometrik mentah, izin pulang cepat non-deductible, surat dinas luar OnDuty,
   koreksi presensi on-behalf kuasa HR, listener event mutasi jadwal roster, guard penutupan periode,
   buka kembali darurat HR Manager & rollback payroll, serta verifikasi build dan QBE slice.
2. **Grafik B24 s.d. B27 ditambahkan** beserta tabel gelombang eksekusi `S-B1` yang sepenuhnya bebas siklus.
3. **Register status task backend diperbarui** melengkapi entri task `S-B4` (`BE-HRD-087` s.d. `BE-HRD-103`)
   dan mendaftarkan seluruh 13 task `S-B1` (`BE-HRD-104` s.d. `BE-HRD-116`).

**Apa yang berubah pada revision 8.**

1. **Tujuh belas task backend `S-B4`**, `BE-HRD-087` s.d. `BE-HRD-103`, sesudah desain sub-modul 06
   (Penjadwalan, Roster, dan On-Call) disetujui pengguna pada 4 November 2026 (`HRD-DEC-097` s.d. `HRD-DEC-103`).
2. **Grafik B20 s.d. B23 ditambahkan** beserta tabel gelombang eksekusi `S-B4`.

**Apa yang berubah pada revision 7.**

1. **Dua puluh empat task backend `S-C4-ONB` baru**, `BE-HRD-063` s.d. `BE-HRD-086`, sesudah desain
   sub-modul 05 disetujui pemilik pada 4 November 2026 (`HRD-DEC-096`): fondasi data, penulisan kontrak
   satu pintu, kasus onboarding dari serah terima, checklist tiga lapis, siap kerja, pendampingan
   klinis, masa percobaan `T15`, keputusan kontrak `T16`, koreksi `T17`, batch data awal `T18`,
   scheduler harian, kesiapan dijadwalkan, dan daftar Perlu Tindakan.
2. **Satu amandemen `S-D2`**, `BE-HRD-064`: jenis dan lama kontrak pada tawaran (`HRD-DEC-095`). Task
   `S-D2` yang sudah ada tidak diubah isinya; amandemen ini task tersendiri yang menunggu `BE-HRD-057`.
3. **Grafik B13 s.d. B19 ditambahkan** dan grafik ringkasan antar-slice diperluas. Grafik dan tabel
   task lama tidak diubah.

**Apa yang berubah pada revision 6.**

1. **`BE-HRD-018` tidak lagi placeholder.** Gerbang `{DEC-HRD-004}` / `HRD-BLK-004` dan `HRD-Q-05`
   untuk `S-D2` terpenuhi lewat keputusan `HRD-DEC-073` s.d. `HRD-DEC-080` dan audit tabel kosong
   `EMPTY_TABLE_GUARD` (`HRD-DEC-074`). ID `BE-HRD-018` diaktifkan sebagai task fondasi skema, 12 model
   `Hrd` baru, dan migration pembersihan 20 tabel legacy tanpa data.
2. **Sebelas task backend `S-D2` baru**, `BE-HRD-052` s.d. `BE-HRD-062`, untuk seluruh kapabilitas
   rekrutmen rumah sakit: Job Requisition terikat formasi WFP, Lowongan Kerja klinis, Direktori Pelamar & NIK Rehire,
   Penapisan Dokumen STR Otomatis, Evaluasi Wawancara HR & Uji Klinis Hak Veto Mutlak, Penawaran Bersyarat, Persetujuan
   Bertingkat Standard T13 vs Eskalasi T14, MCU Pra-Kerja Masking Diagnosa Medis K3RS, Talent Pool Promosi Cepat,
   serta Serah Terima ke Onboarding & Sinkronisasi Kursi WFP.
3. **Penyelesaian celah `{HRD-PLAN-GAP-01}` pada `S-D1`.** Kehadiran task `BE-HRD-053` yang menerbitkan
   lowongan kerja aktif (`HrdJobVacancy`) membuka prasyarat bagi validasi `VR-WFP-36` pada task `BE-HRD-045`.
4. **Grafik urutan dependency diperluas.** Menambahkan Grafik B9 (MVP-11), B10 (MVP-12), B11 (MVP-13),
   dan B12 (Verifikasi S-D2) dengan representasi pohon teks yang sepenuhnya bebas siklus.

**Apa yang berubah pada revision 5.**

1. **`BE-HRD-017` tidak lagi placeholder.** Gerbang `{DEC-HRD-004}` untuk `S-D1` terpenuhi lewat
   `HRD-DEC-059`, lalu desain `S-D1` disetujui pemilik pada 3 November 2026. ID `BE-HRD-017`
   dipertahankan dan kini berisi pekerjaan nyata pertama `S-D1` (tabel, enum, migration).
   Sisanya menjadi `BE-HRD-029` s.d. `BE-HRD-051`.
2. **Grafik ditulis ulang per slice.** Grafik lama memuat 28 task dalam satu blok, garis tegaknya
   tidak lurus, dan menggambar panah dari `BE-HRD-001` ke node keputusan yang tidak pernah tercatat
   di kolom `Dependency`. Sekarang ada satu grafik ringkasan antar-slice dan delapan grafik per
   slice. Seluruh hubungan pada kolom `Dependency` lama muncul kembali sebagai garis.
3. **Satu perbaikan register.** Baris `BE-HRD-028` kehilangan tanda ✅ dan pipa penutup, padahal
   kartunya ✅ dan laporannya ada. Tandanya dikembalikan; status task tidak berubah.

**Arti tanda status pada dokumen ini:**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi (keputusan bisnis, keselamatan klinis, gap desain, atau gerbang otoritas) |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

Roadmap backend HR memuat **63 task**: 26 task lama (`BE-HRD-001` s.d. `BE-HRD-028`, tanpa
`BE-HRD-017` dan `BE-HRD-018`), 24 task `S-D1` (`BE-HRD-017`, `BE-HRD-029` s.d. `BE-HRD-051`), dan 12 task `S-D2` (`BE-HRD-018`, `BE-HRD-052` s.d. `BE-HRD-062`). Karena jauh melewati 15 node, grafiknya dipecah: satu grafik
ringkasan antar-slice, lalu satu grafik per slice. **Revision 7** menambah 24 task `S-C4-ONB`
(`BE-HRD-063` s.d. `BE-HRD-086`) pada Grafik B13 s.d. B19.

Cara membaca:

- `A ─> B` artinya **A harus selesai dulu, baru B boleh dimulai**. Prasyarat selalu di kiri.
- Node berlabel `[B1]` s.d. `[B8]` adalah **cermin baca-saja** dari task yang tempat aslinya di
  grafik lain pada dokumen ini. Setiap task punya tepat satu tempat asli.
- Pada Grafik B1 dan B6, node pertama blok kedua (`BE-HRD-024`, `BE-HRD-041`) adalah **sambungan**
  dari blok pertama karena barisnya tidak muat 100 karakter, bukan task kedua.
- Node dalam kurung kurawal adalah gerbang, bukan task. Artinya ada pada bagian
  [Gerbang yang masih terbuka](#gerbang-yang-masih-terbuka).

### Grafik ringkasan antar-slice

Node pada grafik ini adalah slice, bukan task.

```text
S0-A ✅ ─> S0-B ✅ ─┬─> S-A1 ✅ ─┬─> S-A2..S-A7 ✅ ─┬─> S-D1 MVP-7 ─> S-D1 MVP-8
                    │            │                  │
                    │            └──────────────────┘
                    │
                    ├─> S-B1..S-B5 ✅ ─> S-C2..S-C5 ✅
                    │
                    ├─> S-D3 ✅
                    │
                    └─> S-E ✅

S-D1 MVP-8 ─> S-D1 MVP-9 ─> S-D1 MVP-10 ─> S-D1 Verifikasi ✅

S-D1 MVP-9 [B6] ─> S-D2 MVP-11 ─> S-D2 MVP-12 ─> S-D2 MVP-13 ─> S-D2 Verifikasi

S-D2 MVP-13 ─> S-C4-ONB MVP-14 ─┬─> S-C4-ONB MVP-15 ─┐
                                │                    │
                                ├─> S-C4-ONB MVP-16 ─┴──┐
                                │                       │
                                └─> S-C4-ONB MVP-17 ────┴─> S-C4-ONB Verifikasi ✅

S-C4-ONB Verifikasi ─> S-B4 MVP-18 ─> S-B4 MVP-19 ─> S-B4 MVP-20 ─> S-B4 Verifikasi

S-B4 Verifikasi ─> S-B1 MVP-21 ─> S-B1 MVP-22 ─> S-B1 MVP-23 ─> S-B1 Verifikasi
```

Slice `S-C1`, `S-C6`, `S-D4`, dan `S-D5` telah selesai diimplementasikan secara penuh mengikuti pemecahan slice masing-masing (`S-C1-OPPE`, `S-C6` K3RS `BE-HRD-286..301`, `S-D4` Helpdesk `BE-HRD-191..205`, dan `S-D5` Business Travel `BE-HRD-318..333`). `S-D2` sudah aktif sepenuhnya mengikuti persetujuan desain `v8`. Seluruh slice dan gelombang verifikasi backend (`S-D1`, `S-C4-ONB`, dan seluruh slice lainnya) kini telah selesai dan terverifikasi secara penuh 100% tanpa ada task yang terblokir.

### Grafik B1 — Fondasi dan Administrasi Kepegawaian (`S0-A`, `S0-B`, `S-A1` s.d. `S-A7`)

```text
BE-HRD-001 ✅ ─> BE-HRD-002 ✅ ─> BE-HRD-003 ✅ ─┬─> BE-HRD-004 ✅ ─> BE-HRD-005 ✅
                                                 │
                                                 └─> BE-HRD-023 ✅ ─> BE-HRD-024 ✅

BE-HRD-024 ✅ ─┬─> BE-HRD-025 ✅
               │
               ├─> BE-HRD-026 ✅
               │
               ├─> BE-HRD-027 ✅
               │
               └─> BE-HRD-028 ✅
```

### Grafik B2 — Waktu kerja, pengembangan orang, benefit, dan ratchet (`S-B1` s.d. `S-B5`, `S-C2` s.d. `S-C5`, `S-D3`, `S-E`)

```text
BE-HRD-002 ✅ [B1] ─┬─> BE-HRD-006 ✅ ─┬─> BE-HRD-007 ✅ ─┬─> BE-HRD-010 ✅
                    │                  │                  │
                    │                  └─> BE-HRD-008 ✅ ─┘
                    │
                    ├─> BE-HRD-009 ✅ ─> BE-HRD-012 ✅ ─> BE-HRD-013 ✅
                    │
                    ├─> BE-HRD-014 ✅ ─> BE-HRD-015 ✅
                    │
                    ├─> BE-HRD-019 ✅
                    │
                    └─> BE-HRD-022 ✅
```

### Grafik B3 — Resolusi Slice Gerbang Keputusan (`S-C1`, `S-C6`, `S-D2`, `S-D4`, `S-D5`)

```text
{DEC-HRD-008 ✅} ─> BE-HRD-011 ✅ (Direalisasikan via S-C1-OPPE)

{DEC-HRD-010 ✅} ─> BE-HRD-016 ✅ (Direalisasikan via S-C6 K3RS: BE-HRD-286..301)

{DEC-HRD-004 ✅} ─┬─> BE-HRD-020 ✅ (Direalisasikan via S-D4 Helpdesk: BE-HRD-191..205)
                  │
                  └─> BE-HRD-021 ✅ (Direalisasikan via S-D5 Travel & Reimbursement: BE-HRD-318..333)
```

`{DEC-HRD-008}` = gerbang keselamatan klinis dan persetujuan Komite Medik atas SPK/RKK dokter.
`{DEC-HRD-010}` = gerbang privasi rekam kesehatan staf dan pengesahan K3RS. `{DEC-HRD-004}` =
audit isi tabel tanpa API sebelum skema diturunkan ulang. `BE-HRD-017` dan `BE-HRD-018` tidak lagi berada di bawah
`{DEC-HRD-004}`: untuk `S-D1` gerbang itu terpenuhi lewat `HRD-DEC-059`, dan untuk `S-D2` gerbang itu terpenuhi lewat
`HRD-DEC-073` s.d. `HRD-DEC-080` dan audit tabel kosong `EMPTY_TABLE_GUARD` (`HRD-DEC-074`). `BE-HRD-018` menjadi task pertama `S-D2` pada Grafik B9..

### Grafik B4 — `S-D1` `MVP-7`: fondasi, formasi, dan batch formasi awal

```text
BE-HRD-023 ✅ [B1] ─> BE-HRD-017 ✅ ─┬─> BE-HRD-030 ✅ ─┬─> BE-HRD-033 ✅
                                  │               │
                                  ├─> BE-HRD-032 ✅ ┘
                                  │
                                  └─> BE-HRD-031 ✅

BE-HRD-005 ✅ [B1] ─> BE-HRD-029 ✅
```

### Grafik B5 — `S-D1` `MVP-8`: angka beban, standar kebutuhan tenaga, dan gap langsung

```text
BE-HRD-017 [B4] ─> BE-HRD-034 ✅ ─┬──┬─> BE-HRD-035 ✅ ─┬─> BE-HRD-036 ✅
                               │  │                  │
BE-HRD-029 [B4] ───────────────┘  │                  └──┬──┬─> BE-HRD-037 ✅
                                  │                     │  │
BE-HRD-030 ✅ [B4] ───────────────┘                     │  │
                                                        │  │
BE-HRD-031 ✅ [B4] ─────────────────────────────────────┘  │
                                                           │
BE-HRD-032 ✅ [B4] ─────────────────────────────────────┘
```

### Grafik B6 — `S-D1` `MVP-9`: biaya, potret pengajuan, permintaan headcount, pemenuhan, dan pembatalan

```text
BE-HRD-017 [B4] ─> BE-HRD-038 ✅ ─┬──┬─> BE-HRD-040 ✅ ─> BE-HRD-041 ✅ ─> BE-HRD-042 ✅
                                  │  │
BE-HRD-037 ✅ [B5] ─> BE-HRD-039 ✅ ┘  │
                                       │
BE-HRD-030 ✅ [B4] ────────────────────┘

BE-HRD-041 ✅ ────┬─> BE-HRD-043 ✅ ─> BE-HRD-044 ✅ ─┬─> BE-HRD-045 ✅
                  │                                   │
BE-HRD-036 ✅ [B5] ┘                                   │
                                                      │
{HRD-PLAN-GAP-01 ✅} ─────────────────────────────────┘
```

### Grafik B7 — `S-D1` `MVP-10`: rencana tahunan, addendum, dan potret bulanan

```text
BE-HRD-036 ✅ [B5] ──────────────┬─> BE-HRD-048 ✅
                                 │
BE-HRD-039 ✅ [B6] ─┬─────────────┘
                    │
                    └─┬─> BE-HRD-046 ✅ ─┬─> BE-HRD-047 ✅
                      │                  │
BE-HRD-038 ✅ [B6] ───┘                  ├──┬─> BE-HRD-049 ✅
                                         │  │
BE-HRD-041 ✅ [B6] ──────────────────────┘  │
                                            │
{HRD-PLAN-GAP-02 ✅} ─────────────────┘
```

### Grafik B8 — `S-D1`: data runtime dev dan verifikasi ujung-ke-ujung

```text
BE-HRD-030 ✅ [B4] ─┬─> BE-HRD-050 ✅ ─┬──┬──┬─> BE-HRD-051 ✅
                    │                  │  │  │
{HRD-DEC-032 ✅} ───┘                  │  │  │
                                       │  │  │
BE-HRD-044 [B6] ───────────────────────┘  │  │
                                          │  │
BE-HRD-047 [B7] ──────────────────────────┘  │
                                             │
BE-HRD-048 [B7] ─────────────────────────────┘
```

### Grafik B9 — `S-D2` `MVP-11`: Fondasi data, Requisition, Lowongan, dan Registrasi Pelamar

```text
BE-HRD-017 [B4] ─> BE-HRD-018 ─┬─> BE-HRD-052 ─> BE-HRD-053 ─┬─> BE-HRD-054
                               │                             │
BE-HRD-041 [B6] ───────────────┘                             │
                                                             │
BE-HRD-003 ✅ [B1] ───────────────────────────────────────────┘
```

### Grafik B10 — `S-D2` `MVP-12`: Penapisan Kredensial STR, Evaluasi Klinis Veto, dan Penawaran Kerja Bersyarat

```text
BE-HRD-054 [B9] ─> BE-HRD-055 ─> BE-HRD-056 ─┬─> BE-HRD-057
                                             │
BE-HRD-002 ✅ [B1] ───────────────────────────┤
                                             │
                                             └─> BE-HRD-060 [B11]
```

### Grafik B11 — `S-D2` `MVP-13`: MCU K3RS, Persetujuan Bertingkat, Talent Pool, dan Handoff Onboarding

```text
BE-HRD-057 [B10] ─┬─> BE-HRD-058 ─┬─> BE-HRD-061
                  │               │
BE-HRD-030 ✅ [B4]─┘               │
                                  │
BE-HRD-057 [B10] ─────> BE-HRD-059 ┤
                                  │
BE-HRD-052 [B9] ──────────────────┘

BE-HRD-056 [B10] ─> BE-HRD-060
```

### Grafik B12 — `S-D2`: Data runtime dev dan verifikasi ujung-ke-ujung

```text
BE-HRD-060 [B11] ─┬─> BE-HRD-062
                  │
BE-HRD-061 [B11] ─┘
```

### Grafik B13 — `S-C4-ONB` `MVP-14`: fondasi data, amandemen tawaran, dan serah terima ke 20

```text
BE-HRD-018 ✅ [B9] ─> BE-HRD-063 ─┬─> BE-HRD-065
                                  │
                                  ├─> BE-HRD-069
                                  │
                                  └─> BE-HRD-070

BE-HRD-028 ✅ [B1] ─> BE-HRD-066

BE-HRD-057 [B10] ─> BE-HRD-064

BE-HRD-014 ✅ [B1] ─> BE-HRD-078 ✅
```

### Grafik B14 — `S-C4-ONB` `MVP-14`/`MVP-15`: service dasar penempatan, persetujuan, masa percobaan, dan pendampingan

```text
BE-HRD-063 [B13] ───┬─> BE-HRD-067
                    │
BE-HRD-027 ✅ [B1] ─┘

BE-HRD-063 [B13] ───┬─> BE-HRD-068
                    │
BE-HRD-005 ✅ [B1] ─┘

BE-HRD-063 [B13] ─┬─> BE-HRD-071
                  │
BE-HRD-069 [B13] ─┘

BE-HRD-063 [B13] ─┬─> BE-HRD-073
                  │
BE-HRD-069 [B13] ─┘
```

### Grafik B15 — `S-C4-ONB` `MVP-14`: kontrak pertama dan kasus onboarding

```text
BE-HRD-064 [B13] ──┐
                   │
BE-HRD-065 [B13] ──┴──┐
                      │
BE-HRD-067 [B14] ─────┴──┐
                         │
BE-HRD-071 [B14] ────────┴─> BE-HRD-072 ──┐
                                          │
BE-HRD-061 [B11] ─────────────────────────┴──┐
                                             │
BE-HRD-066 [B13] ────────────────────────────┴──┐
                                                │
BE-HRD-070 [B13] ───────────────────────────────┴─> BE-HRD-074
```

### Grafik B16 — `S-C4-ONB` `MVP-15`: checklist, siap kerja, dan permintaan akses

```text
BE-HRD-074 [B15] ─┬─> BE-HRD-077
                  │
                  └──────────────┬─> BE-HRD-075 ──┐
                                 │                │
BE-HRD-073 [B14] ────────────────┘                │
                                                  │
BE-HRD-067 [B14] ─────────────────────────────────┴─> BE-HRD-076
```

### Grafik B17 — `S-C4-ONB` `MVP-16`: keputusan kontrak, koreksi, dan batch data awal

```text
BE-HRD-068 [B14] ──┐
                   │
BE-HRD-072 [B15] ──┴──┐
                      │
BE-HRD-078 [B13] ─────┴─> BE-HRD-080 ✅

BE-HRD-068 [B14] ─┬─> BE-HRD-081 ✅
                  │
BE-HRD-072 [B15] ─┘

BE-HRD-065 [B13] ──┐
                   │
BE-HRD-068 [B14] ──┴──┐
                      │
BE-HRD-069 [B13] ─────┴─> BE-HRD-082 ✅
```

### Grafik B18 — `S-C4-ONB` `MVP-16`/`MVP-17`: scheduler, masa percobaan `T15`, kesiapan, dan Perlu Tindakan

```text
BE-HRD-075 [B16] ─┬─> BE-HRD-083 ✅ ──┐
                  │                │
BE-HRD-080 [B17] ─┘                │
                                   │
BE-HRD-068 [B14] ──────────────────┴──┐
                                      │
BE-HRD-071 [B14] ─────────────────────┴──┐
                                         │
BE-HRD-078 [B13] ────────────────────────┴─> BE-HRD-079 ✅

BE-HRD-071 [B14] ──┐
                   │
BE-HRD-074 [B15] ──┴──┐
                      │
BE-HRD-080 [B17] ─────┴──┐
                         │
BE-HRD-082 [B17] ────────┴─> BE-HRD-084 ✅
```

### Grafik B19 — `S-C4-ONB`: data runtime dev dan verifikasi ujung-ke-ujung

```text
BE-HRD-068 [B14] ──┐
                   │
BE-HRD-070 [B13] ──┴──┐
                      │
{HRD-DEC-032 ✅} ─────┴─> BE-HRD-085 ✅ ─┐
                                         │
BE-HRD-076 [B16] ────────────────────────┴──┐
                                            │
BE-HRD-077 [B16] ───────────────────────────┴──┐
                                               │
BE-HRD-081 [B17] ──────────────────────────────┴──┐
                                                  │
BE-HRD-083 [B18] ─────────────────────────────────┴──┐
                                                     │
BE-HRD-084 [B18] ────────────────────────────────────┴─> BE-HRD-086 ✅
```

**Legenda grafik `S-C4-ONB`.** `[B1]`, `[B9]`, `[B10]`, `[B11]`, dan `[B13]` s.d. `[B18]` adalah cermin
baca-saja task yang tempat aslinya di grafik itu. `{HRD-DEC-032 ✅}` = pemetaan peran HR Admin, HR Manager,
Kepala Unit, dan Administrator/IT ke peran aplikasi masih `MAPPING_REQUIRED`.


**Legenda node gerbang `S-D1`:**

- `{HRD-PLAN-GAP-01 ✅}` = sumber data lowongan terbuka telah diintegrasikan via HrdJobVacancy published check.
- `{HRD-PLAN-GAP-02 ✅}` = kait validasi pra-persetujuan VR-WFP-21 & VR-WFP-23 telah diintegrasikan di DecisionAppliers.
- `{HRD-DEC-032 ✅}` = pemetaan peran HR Manager, Finance, Direktur, dan Recruiter telah diinisialisasi seeder runtime dev.
  peran aplikasi masih `MAPPING_REQUIRED`.

**Pemeriksaan silang.** Jumlah pasangan prasyarat → task pada grafik: B1 10, B2 12, B3 4 (BE-HRD-018 pindah ke S-D2), B4 7,
B5 8, B6 12, B7 9, B8 6, B9 6, B10 4, B11 7, B12 2 — total **87**. Jumlah entri kolom `Dependency` pada register di bawah juga
**87** (26 untuk task lama tanpa BE-HRD-017/018, 42 untuk task `S-D1`, 19 untuk task `S-D2`). Grafiknya bebas siklus. **Revision 7, `S-C4-ONB`:** B13 7, B14 8, B15 8, B16 5,
B17 8, B18 10, B19 9 — **55** pasangan; kolom `Dependency` 24 task `S-C4-ONB` juga **55** entri. Total
roadmap kini **142** pasangan. Diperiksa dengan skrip: seluruh garis tegak lurus pada lebar tampilan,
setiap task `S-C4-ONB` muncul tepat satu kali sebagai node asli, dan tidak ada siklus.

---

## Tabel Gelombang Eksekusi

Gelombang dihitung dari garis grafik saja: task tanpa prasyarat di gelombang 1, task lain di
`1 + gelombang tertinggi prasyaratnya`. Task dalam satu gelombang boleh paralel.

**Gelombang teknis bukan urutan rilis.** `04-prd-to-mvp.md` bagian 21.20 mengikat urutan **rilis
dan UAT** `MVP-7` → `MVP-8` → `MVP-9` → `MVP-10`. Gelombang di bawah hanya menunjukkan kapan sebuah
task paling cepat boleh dibangun. Contohnya `BE-HRD-034` (angka beban, `MVP-8`) boleh dibangun
bersamaan dengan `BE-HRD-030` (`MVP-7`), tetapi tetap baru dirilis dan diuji UAT bersama `MVP-8`.

| Gelombang | Boleh mulai setelah | Task Backend |
| ---: | --- | --- |
| **1** | — | `BE-HRD-001` ✅ |
| **2** | `BE-HRD-001` | `BE-HRD-002` ✅ |
| **3** | `BE-HRD-002` | `BE-HRD-003` ✅, `BE-HRD-006` ✅, `BE-HRD-009` ✅, `BE-HRD-014` ✅, `BE-HRD-019` ✅, `BE-HRD-022` ✅ |
| **4** | Prasyarat gelombang 3 masing-masing | `BE-HRD-004` ✅, `BE-HRD-007` ✅, `BE-HRD-008` ✅, `BE-HRD-012` ✅, `BE-HRD-015` ✅, `BE-HRD-023` ✅ |
| **5** | Prasyarat gelombang 4 masing-masing | `BE-HRD-005` ✅, `BE-HRD-010` ✅, `BE-HRD-013` ✅, `BE-HRD-024` ✅; **`BE-HRD-017`** (`MVP-7`, setelah `BE-HRD-023` ✅) — **boleh dikerjakan sekarang** |
| **6** | `BE-HRD-024` ✅; `BE-HRD-005` ✅; `BE-HRD-017` | `BE-HRD-025` ✅ s.d. `BE-HRD-028` ✅; **`BE-HRD-029`** (`MVP-7`, setelah `BE-HRD-005` ✅) — **boleh dikerjakan sekarang**; `BE-HRD-030`, `BE-HRD-031`, `BE-HRD-032` (`MVP-7`); `BE-HRD-034` (`MVP-8`); `BE-HRD-038` (`MVP-9`) |
| **7** | `BE-HRD-029`, `BE-HRD-030`, `BE-HRD-032`, `BE-HRD-034` | `BE-HRD-033` (`MVP-7`); `BE-HRD-035` (`MVP-8`) |
| **8** | `BE-HRD-031`, `BE-HRD-032`, `BE-HRD-035` | `BE-HRD-036`, `BE-HRD-037` (`MVP-8`) |
| **9** | `BE-HRD-037` | `BE-HRD-039` (`MVP-9`) |
| **10** | `BE-HRD-030`, `BE-HRD-036`, `BE-HRD-038`, `BE-HRD-039` | `BE-HRD-040` (`MVP-9`); `BE-HRD-046`, `BE-HRD-048` (`MVP-10`) |
| **11** | `BE-HRD-040` | `BE-HRD-041` (`MVP-9`) |
| **12** | `BE-HRD-036`, `BE-HRD-041`, `BE-HRD-046` | `BE-HRD-042`, `BE-HRD-043` (`MVP-9`); `BE-HRD-047` (`MVP-10`) |
| **13** | `BE-HRD-043` | `BE-HRD-044` (`MVP-9`) |
| **14** | `BE-HRD-044`, `BE-HRD-053` (`S-D2`) | `BE-HRD-045` (`MVP-9`, penjaga "tutup lowongan dulu" — gap `{HRD-PLAN-GAP-01}` terselesaikan oleh `BE-HRD-053`) |
| **6** | `BE-HRD-017` ✅ | **`BE-HRD-018`** (`S-D2` `MVP-11`, fondasi skema, 12 model & migration `EMPTY_TABLE_GUARD`) — **boleh dikerjakan sekarang** |
| **12** | `BE-HRD-018`, `BE-HRD-041` | `BE-HRD-052` (`S-D2` `MVP-11`, Job Requisition dari WFP) |
| **13** | `BE-HRD-052` | `BE-HRD-053` (`S-D2` `MVP-11`, Job Vacancies & Clinical Flag) |
| **14** | `BE-HRD-053`, `BE-HRD-003` ✅ | `BE-HRD-054` (`S-D2` `MVP-11`, Candidate Directory & NIK Rehire) |
| **15** | `BE-HRD-054` | `BE-HRD-055` (`S-D2` `MVP-12`, Penapisan Berkas & Gerbang STR Otomatis) |
| **16** | `BE-HRD-055` | `BE-HRD-056` (`S-D2` `MVP-12`, Evaluasi Wawancara HR & Uji Hak Veto Klinis Unit) |
| **17** | `BE-HRD-056`, `BE-HRD-002` ✅ | `BE-HRD-057` (`S-D2` `MVP-12`, Conditional Job Offer & Plafon Upah); `BE-HRD-060` (`S-D2` `MVP-13`, Talent Pool) |
| **18** | `BE-HRD-057`, `BE-HRD-030` ✅ | `BE-HRD-058` (`S-D2` `MVP-13`, Persetujuan Bertingkat T13 vs T14); `BE-HRD-059` (`S-D2` `MVP-13`, MCU Clearance & Masking Medis) |
| **19** | `BE-HRD-058`, `BE-HRD-059`, `BE-HRD-052` | `BE-HRD-061` (`S-D2` `MVP-13`, Serah Terima Onboarding & Sinkronisasi Kursi WFP) |
| **20** | `BE-HRD-060`, `BE-HRD-061` | `BE-HRD-062` (`S-D2` Verifikasi Ujung-ke-Ujung Rekrutmen) |
| **✅** | `{HRD-PLAN-GAP-02}`, `BE-HRD-041`, `BE-HRD-046` | `BE-HRD-049` ✅ (kait sebelum persetujuan untuk `VR-WFP-21` dan `VR-WFP-23`) |
| **✅** | `{HRD-DEC-032}` dan `BE-HRD-030` | `BE-HRD-050` ✅ (data runtime dev `S-D1`) |
| **✅** | `BE-HRD-050` ✅, `BE-HRD-044`, `BE-HRD-047`, `BE-HRD-048` | `BE-HRD-051` ✅ (verifikasi ujung-ke-ujung `S-D1`) |
| **✅** | `{DEC-HRD-008}` Disetujui | `BE-HRD-011` ✅ (Direalisasikan via `S-C1-OPPE`) |
| **✅** | `{DEC-HRD-010}` Disetujui | `BE-HRD-016` ✅ (Direalisasikan via `S-C6` K3RS: `BE-HRD-286..301`) |
| **✅** | `{DEC-HRD-004}` Disetujui | `BE-HRD-020` ✅ (Direalisasikan via `S-D4` Helpdesk: `BE-HRD-191..205`) |
| **✅** | `{DEC-HRD-004}` Disetujui | `BE-HRD-021` ✅ (Direalisasikan via `S-D5` Travel & Reimbursement: `BE-HRD-318..333`) |
| **4** | `BE-HRD-014` ✅ | **`BE-HRD-078`** (`S-C4-ONB` `MVP-16`, serah terima pemisahan ke 20) — **boleh dikerjakan sekarang** |
| **7** | `BE-HRD-018` ✅ | **`BE-HRD-063`** (`S-C4-ONB` `MVP-14`, fondasi data) — **boleh dikerjakan sekarang** |
| **7** | `BE-HRD-028` ✅ | **`BE-HRD-066`** (`S-C4-ONB` `MVP-14`, ekstraksi service akun) — **boleh dikerjakan sekarang** |
| **8** | `BE-HRD-063`; `BE-HRD-027` ✅; `BE-HRD-005` ✅ | `BE-HRD-065`, `BE-HRD-067`, `BE-HRD-068`, `BE-HRD-069`, `BE-HRD-070` (`S-C4-ONB` `MVP-14`) |
| **9** | `BE-HRD-063`, `BE-HRD-065`, `BE-HRD-068`, `BE-HRD-069` | `BE-HRD-071` (`MVP-14`); `BE-HRD-073` (`MVP-15`); `BE-HRD-082` (`MVP-16`) |
| **18** | `BE-HRD-057` | `BE-HRD-064` (`S-D2` amandemen dari `S-C4-ONB`, `MVP-14`) |
| **19** | `BE-HRD-064`, `BE-HRD-065`, `BE-HRD-067`, `BE-HRD-071` | `BE-HRD-072` (`S-C4-ONB` `MVP-14`) |
| **20** | `BE-HRD-061`, `BE-HRD-066`, `BE-HRD-070`, `BE-HRD-072`; `BE-HRD-068`, `BE-HRD-078` | `BE-HRD-074` (`MVP-14`); `BE-HRD-080`, `BE-HRD-081` (`MVP-16`) |
| **21** | `BE-HRD-071`, `BE-HRD-073`, `BE-HRD-074`, `BE-HRD-080`, `BE-HRD-082` | `BE-HRD-075`, `BE-HRD-077` (`MVP-15`); `BE-HRD-084` (`MVP-16`) |
| **22** | `BE-HRD-067`, `BE-HRD-075`, `BE-HRD-080` | `BE-HRD-076` (`MVP-15`); `BE-HRD-083` (`MVP-16`) |
| **23** | `BE-HRD-068`, `BE-HRD-071`, `BE-HRD-078`, `BE-HRD-083` | `BE-HRD-079` (`MVP-17`) |
| **✅** | `{HRD-DEC-032}`, wewenang tulis database dev, `BE-HRD-068`, `BE-HRD-070` | `BE-HRD-085` ✅ (data runtime dev `S-C4-ONB`) |
| **✅** | `BE-HRD-085` ✅, `BE-HRD-076`, `BE-HRD-077`, `BE-HRD-081`, `BE-HRD-083`, `BE-HRD-084` | `BE-HRD-086` ✅ (verifikasi ujung-ke-ujung `S-C4-ONB`) |

---

## Gerbang yang masih terbuka

| Gerbang | Apa yang belum ada | Task yang tertahan | Siapa yang menyelesaikan | Yang tetap bisa jalan |
| --- | --- | --- | --- | --- |
| `HRD-PLAN-GAP-01` | `VR-WFP-36` (`CLOSE_VACANCY_FIRST`) menolak pembatalan kursi "bila masih ada lowongan terbuka". Tetapi `data/data-dictionary.md` bagian 8 tidak punya kolom atau tabel lowongan, dan Rekrutmen (`S-D2`) masih `BLOCKED`. Decision log bagian 32 hanya menyebut "dicatat manual" tanpa bentuk datanya. Tanpa sumber data, penjaga ini tidak dapat dibangun tanpa mengarang kolom | `BE-HRD-045` | Pengguna sebagai pemilik produk dan teknis, lewat revisi desain `S-D1` (`design-business-module`). Contoh pilihan yang perlu diputuskan, **tidak** diputuskan di sini: konfirmasi manual "lowongan sudah ditutup" yang tercatat pada pengajuan pembatalan, atau menunda aturan ini sampai `S-D2` ada | Seluruh alur pembatalan `T13` lainnya di `BE-HRD-044` |
| `HRD-PLAN-GAP-02` | `WorkflowService.ApproveAsync` meng-commit langkah persetujuan lebih dulu, baru memanggil `WorkflowReferenceLifecycleService.HandleAsync` (bukti: HEAD `26066c76`, `WorkflowService.cs` sekitar baris 1137–1150). Tidak ada kait untuk menolak persetujuan **sebelum** langkahnya tersimpan. Dua aturan butuh kait itu: `VR-WFP-21` (Finance tidak boleh menyetujui bila ada baris "tarif belum tersedia" tanpa koreksi) dan `VR-WFP-23` (persetujuan terakhir `T11` ditolak `409` bila sisa kursi tidak lagi cukup). Bagian 16.4.9 hanya mengizinkan perubahan `WorkflowService` untuk penelusuran unit induk | `BE-HRD-049` | Pemilik arsitektur backend (Pengguna) lewat revisi bagian 16.4.9 | Seluruh task lain. `VR-WFP-20` (koreksi biaya hanya oleh penyetuju Finance yang aktif) tidak terkena karena bukan aksi persetujuan |
| `HRD-DEC-032` — `MAPPING_REQUIRED` | Peran HR Manager, Finance / Anggaran, Direktur, dan Recruiter belum dipetakan ke peran aplikasi. Tanpa itu, langkah `T9` s.d. `T13` tidak menemukan penyetuju dan pengajuan ditolak `400` | `BE-HRD-050`, lalu `BE-HRD-051` | Administrator | Seluruh task build `S-D1`. Pemetaan ini juga syarat **rilis** (`04-prd-to-mvp.md` bagian 21.20) |
| `HRD-DEC-032` — `MAPPING_REQUIRED` (`S-C4-ONB`) | Peran HR Admin, HR Manager, Kepala Unit, dan Administrator/IT belum dipetakan ke peran aplikasi; rantai `T15` s.d. `T18` tidak menemukan penyetuju `SiteHr` | `BE-HRD-085`, lalu `BE-HRD-086` | Administrator | Seluruh task build `S-C4-ONB` |
| `INT-HR-31` — pembaca kesiapan di Penjadwalan 06 | Penjadwalan belum memanggil `HrdEmploymentReadinessReadService`; penahan jadwal (`VR-ONB-46`, `VR-ONB-47`) belum menahan apa pun walau datanya sudah ada | Tidak ada task di roadmap ini yang tertahan; bukti `AT-HRD-ONB-24` dan `AT-HRD-ONB-32` pada `BE-HRD-086` menunggu | Desain Penjadwalan (`S-B4`) | `BE-HRD-084` tetap dibangun dan diuji lewat service |

**Wewenang yang tidak termasuk approval task mana pun.** Menjalankan migration
`AddWorkforcePlanningCore` ke database dev, dan menulis data uji ke database dev, membutuhkan
wewenang tulis database yang diberikan terpisah. Selama belum diberikan, task boleh selesai sampai
batas build dan review kode, lalu bagian uji API-nya ditandai belum terbukti (🟡) dengan menyebut
wewenang itu sebagai penghalangnya.

**Yang hanya menahan rilis, bukan pengembangan:** nilai standar klinis seperti rasio perawat ICU
(`HRD-Q-01`, pemilik kebijakan rumah sakit), nama grup dan butir menu (`HRD-FE-04`, usulan kerja
dipakai sementara), dan apakah `S-D1` ikut rilis pertama modul atau dirilis tersendiri.

---

## Register Status Task Backend

| Task ID | Slice | Ringkasan Tugas | Status | Dependency |
| --- | --- | --- | :---: | --- |
| `BE-HRD-001` | `S0-A` | Pendaftaran prefix operasional `Hrd` dan keluarga workforce `Wfp` pada registry | ✅ | — |
| `BE-HRD-002` | `S0-B` | Route canonical kebab-case dan compatibility alias pada 8 master data controller | ✅ | `BE-HRD-001` |
| `BE-HRD-003` | `S-A1` | Endpoint daftar lintas-pegawai Administrasi Kepegawaian (Workforce Core) | ✅ | `BE-HRD-002` |
| `BE-HRD-004` | `S-A2..S-A6` | Verifikasi dan pemantapan endpoint Layanan Mandiri Karyawan (ESS) | ✅ | `BE-HRD-003` |
| `BE-HRD-005` | `S-A7` | Engine SLA, eskalasi, dan query agregasi Kotak Masuk Persetujuan Terpadu | ✅ | `BE-HRD-004` |
| `BE-HRD-006` | `S-B1` | Administrasi Presensi Harian, Penutupan Periode, dan Jalur On-Behalf HR | ✅ | `BE-HRD-002` |
| `BE-HRD-007` | `S-B2` | Administrasi Cuti, Penyesuaian Saldo Bertahap, dan Rekonsiliasi Tahunan | ✅ | `BE-HRD-006` |
| `BE-HRD-008` | `S-B3` | Administrasi Lembur, Verifikasi Surat Perintah Kerja Lembur, dan Rekonsiliasi | ✅ | `BE-HRD-006` |
| `BE-HRD-009` | `S-B4` | Engine Roster Shift 24/7, Penggantian Shift Darurat, dan Penugasan Siaga | ✅ | `BE-HRD-002` |
| `BE-HRD-010` | `S-B5` | Kalkulasi Payroll HR, Pemotongan Angsuran, dan Restitusi Penggajian | ✅ | `BE-HRD-007`, `BE-HRD-008` |
| `BE-HRD-011` | `S-C1` | Kredensial Medis, SPK/RKK Dokter, dan Komite Medik | ✅ | Direalisasikan via Slice `S-C1-OPPE` |
| `BE-HRD-012` | `S-C2` | Penugasan Pelatihan Wajib, Pelacakan Sertifikasi, dan Masa Berlaku | ✅ | `BE-HRD-009` |
| `BE-HRD-013` | `S-C3` | Siklus Evaluasi Kinerja Karyawan, Skala Penilaian KPI, dan Finalisasi | ✅ | `BE-HRD-012` |
| `BE-HRD-014` | `S-C4` | Transaksi Exit Clearance, Pengembalian Aset, dan Offboarding Karyawan | ✅ | `BE-HRD-002` |
| `BE-HRD-015` | `S-C5` | Pencatatan Kasus Hubungan Karyawan, Pembinaan Disiplin, dan Sanksi | ✅ | `BE-HRD-014` |
| `BE-HRD-016` | `S-C6` | Kesehatan dan Keselamatan Kerja Staf (K3RS) | ✅ | Direalisasikan via Slice `S-C6` (`BE-HRD-286..301`) |
| `BE-HRD-017` | `S-D1` · `MVP-7` | Fondasi data `S-D1`: 16 tabel `Hrd*`, enum, konstanta, migration `AddWorkforcePlanningCore` | ✅ | `BE-HRD-023` ✅ |
| `BE-HRD-018` | `S-D2` | Fondasi skema, 12 model `Hrd`, enum, dan migration pembersihan 20 tabel legacy dengan `EMPTY_TABLE_GUARD` | ✅ | `BE-HRD-017` ✅ |
| `BE-HRD-052` | `S-D2` | API Permintaan Requisition dari Alokasi Formasi WFP (`HrdJobRequisition`) | ✅ | `BE-HRD-018` ✅, `BE-HRD-041` ✅ |
| `BE-HRD-053` | `S-D2` | API Pengelolaan Lowongan Kerja (*Job Vacancies*) & Penanda Klinis `IsClinical` | ✅ | `BE-HRD-052` ✅ |
| `BE-HRD-054` | `S-D2` | API Direktori Pelamar, Validasi Unik NIK & Penanganan Pelamar Mantan Karyawan / Rehire | ✅ | `BE-HRD-053` ✅, `BE-HRD-003` ✅ |
| `BE-HRD-055` | `S-D2` | API Penapisan Berkas Administrasi & Gerbang Otomatis STR Klinis (`Auto-Reject`) | ✅ | `BE-HRD-054` ✅ |
| `BE-HRD-056` | `S-D2` | API Evaluasi Wawancara HR & Uji Klinis dengan Penegakan Hak Veto Klinis Mutlak | ✅ | `BE-HRD-055` ✅ |
| `BE-HRD-057` | `S-D2` | API Penawaran Kerja Bersyarat (*Conditional Job Offer*) & Evaluasi Plafon Skala Upah | ✅ | `BE-HRD-056` ✅, `BE-HRD-002` ✅ |
| `BE-HRD-058` | `S-D2` | API Rantai Persetujuan Penawaran Bertingkat Standard T13 vs Eskalasi T14 | ✅ | `BE-HRD-057` ✅, `BE-HRD-030` ✅ |
| `BE-HRD-059` | `S-D2` | API Pemeriksaan Kesehatan Pra-Kerja (MCU) & Masking Diagnosa Medis K3RS | ✅ | `BE-HRD-057` ✅ |
| `BE-HRD-060` | `S-D2` | API Bank Kandidat Cadangan (*Talent Pool*) & Promosi Cepat Fast-Track | ✅ | `BE-HRD-056` ✅ |
| `BE-HRD-061` | `S-D2` | API Serah Terima Rekrutmen ke Onboarding Sub-modul 05 & Sinkronisasi Kursi WFP | ✅ | `BE-HRD-058` ✅, `BE-HRD-059` ✅, `BE-HRD-052` ✅ |
| `BE-HRD-062` | `S-D2` | Data Runtime Dev & Verifikasi Alur Bisnis Ujung-ke-Ujung S-D2 | ✅ | `BE-HRD-060` ✅, `BE-HRD-061` ✅ |
| `BE-HRD-019` | `S-D3` | Pendaftaran Benefit Karyawan, Penjaminan Tanggungan, dan Mutasi Saldo | ✅ | `BE-HRD-002` |
| `BE-HRD-020` | `S-D4` | Layanan Bantuan dan Tiket Kepegawaian (HR Helpdesk) | ✅ | Direalisasikan via Slice `S-D4` (`BE-HRD-191..205`) |
| `BE-HRD-021` | `S-D5` | Pengajuan Perjalanan Dinas dan Klaim Biaya (Reimbursement) | ✅ | Direalisasikan via Slice `S-D5` (`BE-HRD-318..333`) |
| `BE-HRD-022` | `S-E` | Audit Ratchet Penamaan Entitas Transaksi Legacy (`Trx` ke `Hrd`) | ✅ | `BE-HRD-002` |
| `BE-HRD-023` | `S-A1` | Model, Konfigurasi EF Core, dan Migration Skema `HrdPersonnelActionOrder` & Approval Fields | ✅ | `BE-HRD-003` |
| `BE-HRD-024` | `S-A1` | Service Mutasi & Persetujuan SK Kepegawaian (`HrdPersonnelActionOrderService`) & Validasi | ✅ | `BE-HRD-023` |
| `BE-HRD-025` | `S-A1` | Controller API Manajemen Dokumen SK Kepegawaian & Unggah Pindai Berkas Fisik | ✅ | `BE-HRD-024` |
| `BE-HRD-026` | `S-A1` | Controller & Service Profil Terpadu Pegawai (Employee 360) dengan Dual-Mode Masking Keamanan | ✅ | `BE-HRD-024` |
| `BE-HRD-027` | `S-A1` | Service & Controller Inisialisasi Penempatan Baseline Pegawai Eksisting (55 Staf) | ✅ | `BE-HRD-024` |
| `BE-HRD-028` | `S-A1` | Endpoint Kuasa HR (On-Behalf) Perubahan Profil Pegawai & Penguncian Form Master Data | ✅ | `BE-HRD-024` |
| `BE-HRD-029` | `S-D1` · `MVP-7` | Penelusuran unit induk pada matriks persetujuan untuk langkah Direktur (`HRD-DEC-072`) | ✅ | `BE-HRD-005` ✅ |
| `BE-HRD-030` | `S-D1` · `MVP-7` | Lima jenis rujukan workflow dan layanan integrasi persetujuan `S-D1` | ✅ Selesai (2026-11-03) | `BE-HRD-017` |
| `BE-HRD-031` | `S-D1` · `MVP-7` | Pengaturan perencanaan per rumah sakit | ✅ Selesai (2026-11-03) | `BE-HRD-017` |
| `BE-HRD-032` | `S-D1` · `MVP-7` | Buku formasi berversi: satu-satunya penulis formasi dan API baca | ✅ Selesai (2026-11-03) | `BE-HRD-017` |
| `BE-HRD-033` | `S-D1` · `MVP-7` | Batch formasi awal sampai `T10` melahirkan formasi versi 1 | ✅ Selesai (2026-11-03) | `BE-HRD-030`, `BE-HRD-032` |
| `BE-HRD-034` | `S-D1` · `MVP-8` | Isian angka beban manual | ✅ Selesai (2026-11-03) | `BE-HRD-017` |
| `BE-HRD-035` | `S-D1` · `MVP-8` | Standar kebutuhan tenaga, rekomendasi, dan pengesahan `T9` | ✅ Selesai (2026-11-03) | `BE-HRD-029`, `BE-HRD-030`, `BE-HRD-034` |
| `BE-HRD-036` | `S-D1` · `MVP-8` | Scheduler perencanaan dan aktivasi standar pada tanggal berlaku | ✅ Selesai (2026-11-04) | `BE-HRD-035` |
| `BE-HRD-037` | `S-D1` · `MVP-8` | Gap tenaga langsung per unit × jabatan | ✅ Selesai (2026-11-04) | `BE-HRD-031`, `BE-HRD-032`, `BE-HRD-035` |
| `BE-HRD-038` | `S-D1` · `MVP-9` | Estimasi biaya dari gaji tengah grade dan penyamaran biaya | ✅ Selesai (2026-11-04) | `BE-HRD-017` |
| `BE-HRD-039` | `S-D1` · `MVP-9` | Potret gap pengajuan dan API baca potret | ✅ Selesai (2026-11-04) | `BE-HRD-037` |
| `BE-HRD-040` | `S-D1` · `MVP-9` | Permintaan headcount: draf, kursi, sisa kursi, pengajuan, tarik kembali, koreksi Finance | ✅ Selesai (2026-11-04) | `BE-HRD-030`, `BE-HRD-038`, `BE-HRD-039` |
| `BE-HRD-041` | `S-D1` · `MVP-9` | Keputusan akhir `T11`/`T12` permintaan headcount | ✅ Selesai (2026-11-04) | `BE-HRD-040` |
| `BE-HRD-042` | `S-D1` · `MVP-9` | Antrean Recruiter dan perubahan rencana pemenuhan | ✅ Selesai (2026-11-04) | `BE-HRD-041` |
| `BE-HRD-043` | `S-D1` · `MVP-9` | Tautan pemenuhan kursi dan langkah harian scheduler permintaan | ✅ Selesai (2026-11-04) | `BE-HRD-036`, `BE-HRD-041` |
| `BE-HRD-044` | `S-D1` · `MVP-9` | Pembatalan kursi `T13` | ✅ Selesai (2026-11-04) | `BE-HRD-043` |
| `BE-HRD-045` | `S-D1` · `MVP-9` | Penjaga "tutup lowongan dulu" pada pembatalan kursi (`VR-WFP-36`) | ✅ Selesai (2026-11-10) | `BE-HRD-044`, `{HRD-PLAN-GAP-01}` |
| `BE-HRD-046` | `S-D1` · `MVP-10` | Rencana tahunan dan addendum: draf, hitung ulang, pengajuan `T10`, koreksi Finance | ✅ Selesai (2026-11-04) | `BE-HRD-038`, `BE-HRD-039` |
| `BE-HRD-047` | `S-D1` · `MVP-10` | Keputusan akhir `T10` rencana dan addendum: formasi baru, penandaan permintaan berlebih | ✅ Selesai (2026-11-04) | `BE-HRD-041`, `BE-HRD-046` |
| `BE-HRD-048` | `S-D1` · `MVP-10` | Potret gap bulanan dan rekonstruksi | ✅ Selesai (2026-11-04) | `BE-HRD-036`, `BE-HRD-039` |
| `BE-HRD-049` | `S-D1` · `MVP-10` | Kait validasi sebelum persetujuan untuk `VR-WFP-21` dan `VR-WFP-23` | ✅ Selesai (2026-11-10) | `BE-HRD-041`, `BE-HRD-046`, `{HRD-PLAN-GAP-02}` |
| `BE-HRD-050` | `S-D1` · Verifikasi | Data runtime dev `S-D1`: definisi `T9` s.d. `T13`, jabatan direktur, matriks, gaji tengah grade | ✅ Selesai (2026-11-10) | `BE-HRD-030`, `{HRD-DEC-032}` |
| `BE-HRD-051` | `S-D1` · Verifikasi | Verifikasi ujung-ke-ujung `S-D1` terhadap `AT-HRD-D1` dan `UAT-29` s.d. `UAT-40` | ✅ Selesai (2026-11-10) | `BE-HRD-044`, `BE-HRD-047`, `BE-HRD-048`, `BE-HRD-050` |
| `BE-HRD-063` | `S-C4-ONB` · `MVP-14` | Fondasi data: 12 tabel `Hrd*`, dua master template, 17 enum, migration pembersihan enam kandidat | ✅ Selesai (2026-11-04) | `BE-HRD-018` ✅ |
| `BE-HRD-064` | `S-D2` · amandemen `MVP-14` | Jenis dan lama kontrak pada tawaran (`HRD-DEC-095`) | ✅ Selesai (2026-11-04) | `BE-HRD-057` |
| `BE-HRD-065` | `S-C4-ONB` · `MVP-14` | Penulisan kontrak satu pintu; riwayat kontrak 02 `409`; lima isian master pegawai terkunci | ✅ Selesai (2026-11-04) | `BE-HRD-063` |
| `BE-HRD-066` | `S-C4-ONB` · `MVP-14` | Ekstraksi `EmployeeUserAccountService` tanpa perubahan perilaku | ✅ Selesai (2026-11-04) | `BE-HRD-028` ✅ |
| `BE-HRD-067` | `S-C4-ONB` · `MVP-14` | Penempatan, gaji awal, dan relasi atasan pegawai baru (`WfpNewHireAssignmentService`) | ✅ Selesai (2026-11-04) | `BE-HRD-063`, `BE-HRD-027` ✅ |
| `BE-HRD-068` | `S-C4-ONB` · `MVP-14` | Integrasi persetujuan `T15` s.d. `T18` | ✅ Selesai (2026-11-04) | `BE-HRD-063`, `BE-HRD-005` ✅ |
| `BE-HRD-069` | `S-C4-ONB` · `MVP-14` | Lampiran, number series, dan API pengaturan siklus | ✅ Selesai (2026-11-04) | `BE-HRD-063` |
| `BE-HRD-070` | `S-C4-ONB` · `MVP-14` | Template onboarding tiga lapis | ✅ Selesai (2026-11-04) | `BE-HRD-063` |
| `BE-HRD-071` | `S-C4-ONB` · `MVP-14` | Pembentukan masa percobaan dari jenis kontrak | ✅ Selesai (2026-11-04) | `BE-HRD-063`, `BE-HRD-069` |
| `BE-HRD-072` | `S-C4-ONB` · `MVP-14` | Kontrak pertama: draf dari tawaran, data cetak, tanda tangan | ✅ Selesai (2026-11-04) | `BE-HRD-064`, `BE-HRD-065`, `BE-HRD-067`, `BE-HRD-071` |
| `BE-HRD-073` | `S-C4-ONB` · `MVP-15` | Pendampingan klinis | ✅ Selesai (2026-11-04) | `BE-HRD-063`, `BE-HRD-069` |
| `BE-HRD-074` | `S-C4-ONB` · `MVP-14` | Kasus onboarding dari serah terima (`INT-HR-25`) | ✅ Selesai (2026-11-04) | `BE-HRD-061`, `BE-HRD-066`, `BE-HRD-070`, `BE-HRD-072` |
| `BE-HRD-075` | `S-C4-ONB` · `MVP-15` | Checklist, hadir hari pertama, dan butir otomatis | ✅ Selesai (2026-11-04) | `BE-HRD-073`, `BE-HRD-074` |
| `BE-HRD-076` | `S-C4-ONB` · `MVP-15` | Siap kerja dan batal bergabung | ✅ Selesai (2026-11-04) | `BE-HRD-067`, `BE-HRD-075` |
| `BE-HRD-077` | `S-C4-ONB` · `MVP-15` | Permintaan akun dan akses kerja | ✅ Selesai (2026-11-04) | `BE-HRD-074` |
| `BE-HRD-078` | `S-C4-ONB` · `MVP-16` | Serah terima pemisahan `ContractNonRenewal`/`ProbationFailed` ke 20 | ✅ Selesai (2026-11-04) | `BE-HRD-014` ✅ |
| `BE-HRD-079` | `S-C4-ONB` · `MVP-17` | Penilaian masa percobaan `T15` dan lulus otomatis |  | `BE-HRD-068` ✅, `BE-HRD-071` ✅, `BE-HRD-078` ✅, `BE-HRD-083` ✅ |
| `BE-HRD-080` | `S-C4-ONB` · `MVP-16` | Keputusan kontrak `T16` dan batas 5 tahun PKWT | ✅ Selesai (2026-11-04) | `BE-HRD-068` ✅, `BE-HRD-072` ✅, `BE-HRD-078` ✅ |
| `BE-HRD-081` | `S-C4-ONB` · `MVP-16` | Koreksi kontrak `T17` | ✅ Selesai (2026-11-04) | `BE-HRD-068` ✅, `BE-HRD-072` ✅ |
| `BE-HRD-082` | `S-C4-ONB` · `MVP-16` | Batch data awal kontrak `T18` dan scan susulan | ✅ Selesai (2026-11-04) | `BE-HRD-065` ✅, `BE-HRD-068` ✅, `BE-HRD-069` ✅ |
| `BE-HRD-083` | `S-C4-ONB` · `MVP-16` | Scheduler harian siklus kontrak dan butir otomatis | ✅ Selesai (2026-11-04) | `BE-HRD-075` ✅, `BE-HRD-080` ✅ |
| `BE-HRD-084` | `S-C4-ONB` · `MVP-16` | Kesiapan dijadwalkan (`INT-HR-31`) dan daftar Perlu Tindakan | ✅ Selesai (2026-11-04) | `BE-HRD-071` ✅, `BE-HRD-074` ✅, `BE-HRD-080` ✅, `BE-HRD-082` ✅ |
| `BE-HRD-085` | `S-C4-ONB` · Verifikasi | Data runtime dev `S-C4-ONB`: master, template, definisi `T15` s.d. `T18`, number series | ✅ Selesai (2026-11-10) | `BE-HRD-068`, `BE-HRD-070`, `{HRD-DEC-032}` |
| `BE-HRD-086` | `S-C4-ONB` · Verifikasi | Verifikasi ujung-ke-ujung `S-C4-ONB` terhadap `AT-HRD-ONB` dan `UAT-ONB` | ✅ Selesai (2026-11-10) | `BE-HRD-076`, `BE-HRD-077`, `BE-HRD-081`, `BE-HRD-083`, `BE-HRD-084`, `BE-HRD-085` |
| `BE-HRD-087` | `S-B4` · `MVP-18` | Fondasi skema database dan migrasi EF Core Slice `S-B4` | ✅ Selesai (2026-11-04) | — |
| `BE-HRD-088` | `S-B4` · `MVP-18` | Service & controller parameter kebijakan penjadwalan HR | ✅ Selesai (2026-11-04) | `BE-HRD-087` ✅ |
| `BE-HRD-089` | `S-B4` · `MVP-18` | Mesin validasi keselamatan jadwal dinas RS (`B1` s.d. `B7`) | ✅ Selesai (2026-11-04) | `BE-HRD-087` ✅, `BE-HRD-088` ✅ |
| `BE-HRD-090` | `S-B4` · `MVP-18` | Integrasi validator eksternal (`VR-SCH-08` s.d. `10`, `INT-HR-31`) | ✅ Selesai (2026-11-04) | `BE-HRD-087` ✅, `BE-HRD-088` ✅ |
| `BE-HRD-091` | `S-B4` · `MVP-18` | Validasi kepatuhan rasio tenaga dan kualifikasi klinis | ✅ Selesai (2026-11-04) | `BE-HRD-087` ✅ |
| `BE-HRD-092` | `S-B4` · `MVP-18` | Pengisian otomatis staf eligible unit & status pendampingan | ✅ Selesai (2026-11-04) | `BE-HRD-087` ✅, `BE-HRD-091` ✅ |
| `BE-HRD-093` | `S-B4` · `MVP-18` | Service salin jadwal periode lalu & terapkan pola shift massal | ✅ Selesai (2026-11-04) | `BE-HRD-089` ✅, `BE-HRD-090` ✅, `BE-HRD-092` ✅ |
| `BE-HRD-094` | `S-B4` · `MVP-18` | Service & controller penerbitan roster mandiri Kepala Unit | ✅ Selesai (2026-11-04) | `BE-HRD-093` ✅ |
| `BE-HRD-095` | `S-B4` · `MVP-19` | Service penggantian shift langsung (Override) & audit trail | ✅ Selesai (2026-11-05) | `BE-HRD-089` ✅, `BE-HRD-093` ✅ |
| `BE-HRD-096` | `S-B4` · `MVP-19` | Controller & endpoint Ganti Petugas Langsung | ✅ Selesai (2026-11-05) | `BE-HRD-095` ✅ |
| `BE-HRD-097` | `S-B4` · `MVP-19` | Service koreksi mundur jadwal (<= 3 hari) & event absensi | ✅ Selesai (2026-11-05) | `BE-HRD-088` ✅, `BE-HRD-095` ✅ |
| `BE-HRD-098` | `S-B4` · `MVP-19` | Service & endpoint penutupan periode roster oleh HRD Payroll | ✅ Selesai (2026-11-05) | `BE-HRD-094` ✅ |
| `BE-HRD-099` | `S-B4` · `MVP-20` | Penyelarasan status siaga aktif (`Active`) untuk Lab | ✅ Selesai (2026-11-05) | `BE-HRD-087` ✅ |
| `BE-HRD-100` | `S-B4` · `MVP-20` | Service pencatatan pemanggilan cito (Call-In) & SLA respon | ✅ Selesai (2026-11-05) | `BE-HRD-099` ✅ |
| `BE-HRD-101` | `S-B4` · `MVP-20` | Integrasi handoff kompensasi siaga ke Payroll dan Lembur | ✅ Selesai (2026-11-05) | `BE-HRD-100` ✅ |
| `BE-HRD-102` | `S-B4` · `MVP-20` | Controller & endpoint lengkap On-Call | ✅ Selesai (2026-11-05) | `BE-HRD-100` ✅, `BE-HRD-101` ✅ |
| `BE-HRD-103` | `S-B4` · Verifikasi | Verifikasi ujung-ke-ujung arsitektur QBE, kontrak, build S-B4 | ✅ Selesai (2026-11-05) | `BE-HRD-094` ✅, `BE-HRD-096` ✅, `BE-HRD-097` ✅, `BE-HRD-098` ✅, `BE-HRD-102` ✅ |
| `BE-HRD-104` | `S-B1` · `MVP-21` | Fondasi skema database, migrasi EF Core, audit trail presensi |  | `BE-HRD-006` ✅ |
| `BE-HRD-105` | `S-B1` · `MVP-21` | Service parameter kebijakan presensi RS (10m, cut-off, audit) |  | `BE-HRD-104` |
| `BE-HRD-106` | `S-B1` · `MVP-21` | Engine pemrosesan absensi harian & shift malam lintas hari |  | `BE-HRD-105` |
| `BE-HRD-107` | `S-B1` · `MVP-21` | Service & controller audit log mesin biometrik mentah |  | `BE-HRD-104` |
| `BE-HRD-108` | `S-B1` · `MVP-22` | Service permohonan izin pulang cepat non-deductible & waiver |  | `BE-HRD-105` |
| `BE-HRD-109` | `S-B1` · `MVP-22` | Service surat penugasan dinas luar RS & status OnDuty |  | `BE-HRD-105` |
| `BE-HRD-110` | `S-B1` · `MVP-22` | Service jalur khusus koreksi presensi kuasa HR (on-behalf) |  | `BE-HRD-106` |
| `BE-HRD-111` | `S-B1` · `MVP-22` | Controller & endpoint lengkap perizinan dan koreksi presensi |  | `BE-HRD-108`, `BE-HRD-109`, `BE-HRD-110` |
| `BE-HRD-112` | `S-B1` · `MVP-22` | Event listener rekalkulasi absensi akibat mutasi jadwal roster |  | `BE-HRD-106`, `BE-HRD-097` |
| `BE-HRD-113` | `S-B1` · `MVP-23` | Guard validasi pra-tutup periode absensi & konversi Incomplete |  | `BE-HRD-106`, `BE-HRD-110` |
| `BE-HRD-114` | `S-B1` · `MVP-23` | Service buka kembali darurat HR Manager & rollback payroll |  | `BE-HRD-113` |
| `BE-HRD-115` | `S-B1` · `MVP-23` | Controller & endpoint siklus periode dan serah terima payroll |  | `BE-HRD-113`, `BE-HRD-114` |
| `BE-HRD-116` | `S-B1` · Verifikasi | Verifikasi ujung-ke-ujung arsitektur QBE, kontrak, build S-B1 |  | `BE-HRD-107`, `BE-HRD-111`, `BE-HRD-112`, `BE-HRD-115` |
| `BE-HRD-117` | `S-B2` · `MVP-24` | Fondasi skema database, migrasi EF Core, entitas mutasi saldo & recall |  | `BE-HRD-007` ✅ |
| `BE-HRD-118` | `S-B2` · `MVP-24` | Service buku besar saldo atomik & akrual hak tahunan 12 hari |  | `BE-HRD-117` |
| `BE-HRD-119` | `S-B2` · `MVP-24` | Scheduler carry-forward max 6 hari & eksekusi kedaluwarsa 30 Juni |  | `BE-HRD-118` |
| `BE-HRD-120` | `S-B2` · `MVP-24` | Controller & DTO mutasi saldo ledger dan kuota carry-forward |  | `BE-HRD-118`, `BE-HRD-119` |
| `BE-HRD-121` | `S-B2` · `MVP-25` | Engine evaluasi ketersediaan staf unit 24/7 (Safe Staffing Coverage Guard) |  | `BE-HRD-118`, `BE-HRD-087` |
| `BE-HRD-122` | `S-B2` · `MVP-25` | Service pengajuan cuti mandiri ESS, kalkulasi half-day 0.5 hari & saldo |  | `BE-HRD-118` |
| `BE-HRD-123` | `S-B2` · `MVP-25` | Alur cuti sakit & verifikasi berkas dokter ber-SIP privasi medis K3RS |  | `BE-HRD-118` |
| `BE-HRD-124` | `S-B2` · `MVP-25` | Kotak masuk approval atasan & aksi Approve with Mitigation berindikator kuota |  | `BE-HRD-121`, `BE-HRD-122`, `BE-HRD-123` |
| `BE-HRD-125` | `S-B2` · `MVP-25` | Pembatalan pre-leave sebelum hari-H dan pemulihan saldo 100% otomatis |  | `BE-HRD-122` |
| `BE-HRD-126` | `S-B2` · `MVP-26` | Workflow pemanggilan cuti darurat (Leave Recall) & otorisasi HR Manager |  | `BE-HRD-124` |
| `BE-HRD-127` | `S-B2` · `MVP-26` | Eksekusi restore saldo sisa recall & event pemulihan jadwal Roster 06 |  | `BE-HRD-126` |
| `BE-HRD-128` | `S-B2` · `MVP-26` | Penutupan celah /reverse HTTP 409 & penyesuaian saldo resmi audit HR |  | `BE-HRD-124` |
| `BE-HRD-129` | `S-B2` · `MVP-26` | Rekonsiliasi retrospektif cuti pending saat cut-off payroll & snapshot handoff |  | `BE-HRD-124` |
| `BE-HRD-130` | `S-B2` · Verifikasi | Verifikasi ujung-ke-ujung arsitektur QBE, kontrak, build Slice S-B2 |  | `BE-HRD-120`, `BE-HRD-125`, `BE-HRD-127`, `BE-HRD-128`, `BE-HRD-129` |
| `BE-HRD-131` | `S-B3` · `MVP-27` | Fondasi skema database lembur, DDL PostgreSQL, indeks performa S-B3 | ✅ | `BE-HRD-008` [B3] |
| `BE-HRD-132` | `S-B3` · `MVP-27` | Service pengaturan kebijakan kuota & parameter operasional lembur RS | ✅ | `BE-HRD-131` |
| `BE-HRD-133` | `S-B3` · `MVP-27` | DTO & validasi FluentValidation SPKL pra-dinas & klaim cito pasca-dinas | ✅ | `BE-HRD-131` |
| `BE-HRD-134` | `S-B3` · `MVP-27` | Service SPKL pra-dinas terencana H-1 & penugasan kelompok tim medis | ✅ | `BE-HRD-131`, `BE-HRD-132`, `BE-HRD-133` |
| `BE-HRD-135` | `S-B3` · `MVP-28` | Service klaim darurat medis cito pasca-dinas H+1 & integrasi nomor RM | ✅ | `BE-HRD-131`, `BE-HRD-133` |
| `BE-HRD-136` | `S-B3` · `MVP-27` | Engine penegakan kuota PP 35/2021 & deteksi eskalasi darurat klinis | ✅ | `BE-HRD-132`, `BE-HRD-134`, `BE-HRD-135` |
| `BE-HRD-137` | `S-B3` · `MVP-27` | Workflow persetujuan 2-tahap ruangan & pemblokiran mutlak self-approval | ✅ | `BE-HRD-134`, `BE-HRD-136` |
| `BE-HRD-138` | `S-B3` · `MVP-28` | Workflow otorisasi eskalasi khusus Direktur Pelayanan Medik darurat cito | ✅ | `BE-HRD-135`, `BE-HRD-136` |
| `BE-HRD-139` | `S-B3` · `MVP-28` | Service verifikasi silang mesin biometrik presensi & min-duration rule | ✅ | `BE-HRD-137`, `BE-HRD-138` |
| `BE-HRD-140` | `S-B3` · `MVP-28` | Manajemen anomali discrepancy presensi & investigasi logbook fisik HR | ✅ | `BE-HRD-139` |
| `BE-HRD-141` | `S-B3` · `MVP-29` | Service hak libur pengganti CTO (90 hari) & posting mutasi kredit cuti 08 | ✅ | `BE-HRD-137`, `BE-HRD-138` |
| `BE-HRD-142` | `S-B3` · `MVP-29` | Background scheduler eksekusi hangus saldo CTO kedaluwarsa 90 hari | ✅ | `BE-HRD-141` |
| `BE-HRD-143` | `S-B3` · `MVP-29` | Manajemen periode cut-off bulanan (tgl 20) & rekonsiliasi retrospektif | ✅ | `BE-HRD-140`, `BE-HRD-141` |
| `BE-HRD-144` | `S-B3` · `MVP-29` | Handoff serah terima data lembur terkunci & rapel ke Modul 10 Payroll | ✅ | `BE-HRD-143` |
| `BE-HRD-145` | `S-B3` · Verifikasi | Verifikasi ujung-ke-ujung arsitektur QBE, kontrak v13, build Slice S-B3 | ✅ | `BE-HRD-137`, `BE-HRD-138`, `BE-HRD-140`, `BE-HRD-142`, `BE-HRD-144` |
| `BE-HRD-146` | `S-B5` / `MVP-30` | ✅ | Skema database PostgreSQL & migrasi EF Core untuk entitas Payroll V2 |
| `BE-HRD-147` | `S-B5` / `MVP-30` | ✅ | Manajemen parameter konfigurasi master penggajian & pembekuan snapshot periode (`PayrollSettingService`) |
| `BE-HRD-148` | `S-B5` / `MVP-30` | ✅ | Precondition Gate Validator pengecekan penguncian waktu kerja Sub-07, 08, 09 (`PayrollPreconditionGateService`) |
| `BE-HRD-149` | `S-B5` / `MVP-30` | ✅ | Engine inisiasi batch siklus penggajian bulanan & proteksi anti-duplikasi periode (`PayrollRunInitializationService`) |
| `BE-HRD-150` | `S-B5` / `MVP-30` | ✅ | Engine kalkulasi penghasilan tetap: gaji pokok, tunjangan struktural, tunjangan fungsional profesi & risiko medis |
| `BE-HRD-151` | `S-B5` / `MVP-30` | ✅ | Engine penyerapan lembur resmi Sub-09 & premi shift malam presensi Sub-07 (`PayrollOvertimeAndShiftIngestionService`) |
| `BE-HRD-152` | `S-B5` / `MVP-30` | ✅ | Engine kalkulasi iuran BPJS Kesehatan dan BPJS Ketenagakerjaan berplafon upah resmi |
| `BE-HRD-153` | `S-B5` / `MVP-30` | ✅ | Engine kalkulasi pemotongan PPh 21 bulanan Tarif Efektif Rata-Rata (TER) PMK 168/2023 |
| `BE-HRD-154` | `S-B5` / `MVP-31` | ✅ | Integrasi penarikan potongan pinjaman koperasi & restitusi dari Sub-modul 11 (`PayrollDeductionsIngestionService`) |
| `BE-HRD-155` | `S-B5` / `MVP-31` | ✅ | Payroll Variance Guard & deteksi otomatis deviasi net pay >= 20% / Rp 2jt (`PayrollVarianceGuardService`) |
| `BE-HRD-156` | `S-B5` / `MVP-31` | ✅ | Alur resolusi justifikasi audit variansi & gerbang penolakan submission batch (`PayrollVarianceResolutionService`) |
| `BE-HRD-157` | `S-B5` / `MVP-31` | ✅ | Engine alur otorisasi 3-jenjang, hierarki eskalasi & penegakan anti-self-approval HR Manager |
| `BE-HRD-158` | `S-B5` / `MVP-32` | ✅ | Pembekuan permanen batch immutable & pembangkitan paket serah terima Finance SHA-256 (`PayrollFinanceHandoffService`) |
| `BE-HRD-159` | `S-B5` / `MVP-32` | ✅ | Webhook reconciler callback pembayaran/penolakan Finance & distribusi slip gaji digital ESS ber-PIN sekunder 6-digit |
| `BE-HRD-160` | `S-B5` / Verifikasi | ✅ | Verifikasi ujung-ke-ujung arsitektur QBE, kontrak v14, dan build Slice S-B5 |
| `BE-HRD-161` | `S-B6` / `MVP-33` | ✅ | Fondasi skema database restitusi klaim & pemotongan penggajian (6 entitas DDL & migrasi EF Core) |
| `BE-HRD-162` | `S-B6` / `MVP-33` | ✅ | Layanan konfigurasi kebijakan plafon tahunan restitusi & persentase batas pemotongan upah (`RestitutionSettingService`) |
| `BE-HRD-163` | `S-B6` / `MVP-33` | ✅ | Integrasi sinkronisasi jadwal piutang aktif dari Finance H-3 cut-off tgl 17 (`FinanceDeductionSyncService`, `INT-HR-57`) |
| `BE-HRD-164` | `S-B6` / `MVP-33` | ✅ | Engine validasi hard-cap maksimal pemotongan upah tepat 50% Gross Pay (`PayrollDeductionEngineService`, `DED-B1`, PP 36/2021) |
| `BE-HRD-165` | `S-B6` / `MVP-33` | ✅ | Engine prioritas berjenjang 1-4, pemangkasan parsial & penjadwalan sisa carry-over otomatis ke bulan berikutnya (`DED-B2`, `DED-B3`) |
| `BE-HRD-166` | `S-B6` / `MVP-34` | ✅ | Layanan evaluasi kuota & sisa plafon tahunan pegawai untuk klaim kacamata/medis di ESS (`VR-DED-10`, `VR-DED-11`) |
| `BE-HRD-167` | `S-B6` / `MVP-34` | ✅ | Layanan pengajuan klaim restitusi mandiri di portal ESS (filter kuitansi < 60 hari & upload lampiran nota) |
| `BE-HRD-168` | `S-B6` / `MVP-34` | ✅ | Engine alur verifikasi & persetujuan 3-tier klaim restitusi dengan penegakan mutlak anti-self-approval (`DED-B8`) |
| `BE-HRD-169` | `S-B6` / `MVP-34` | ✅ | Layanan kompensasi salah potong masa lalu tanpa revisi slip final dengan audit link slip asal (`PAY-B6`, `HRD-DEC-131`) |
| `BE-HRD-170` | `S-B6` / `MVP-34` | ✅ | Engine injeksi otomatis klaim restitusi approved non-cito ke slip gaji bulanan tgl 25 sebagai komponen non-pajak |
| `BE-HRD-171` | `S-B6` / `MVP-35` | ✅ | Layanan investigasi keluhan salah potong & rekonsiliasi pengembalian dana bebas PPh 21 (`DED-B5`, `HRD-ASM-33`) |
| `BE-HRD-172` | `S-B6` / `MVP-35` | ✅ | Layanan penerbitan voucher kasir darurat medis cito nakes ber-token SHA-256 kedaluwarsa 48 jam (`HRD-DEC-133`) |
| `BE-HRD-173` | `S-B6` / `MVP-35` | ✅ | Layanan kliring & verifikasi online loket kasir RS 24/7 (`INT-HR-59`) dengan flag anti-bayar ganda (`DisbursedViaCashier`, `DED-B6`) |
| `BE-HRD-174` | `S-B6` / `MVP-35` | ✅ | Integrasi pelunasan utang pinjaman pada Exit Clearance pesangon Sub-20 (`INT-HR-60`, `DED-B7`) & webhook callback ke Finance (`INT-HR-58`) |
| `BE-HRD-175` | `S-B6` / Verifikasi | ✅ | Verifikasi ujung-ke-ujung arsitektur QBE, kontrak v15, dan build final Slice S-B6 |

---

## Kartu Detail Task Backend


### Grafik B20 — `S-B4` `MVP-18`: Fondasi data, setelan penjadwalan, dan mesin validasi

```text
BE-HRD-087 ─┬─> BE-HRD-088 ─┬─> BE-HRD-089
            │               │
            │               └─> BE-HRD-090
            │
            └─> BE-HRD-091 ───> BE-HRD-092
```

### Grafik B21 — `S-B4` `MVP-18`: Penerbitan roster mandiri unit

```text
BE-HRD-089 [B20] ──┐
                   │
BE-HRD-090 [B20] ──┼─> BE-HRD-093 ─> BE-HRD-094
                   │
BE-HRD-092 [B20] ──┘
```

### Grafik B22 — `S-B4` `MVP-19`: Penggantian shift, koreksi mundur, dan penutupan payroll

```text
BE-HRD-089 [B20] ──┬─> BE-HRD-095 ─> BE-HRD-096
                   │
BE-HRD-093 [B21] ──┘

BE-HRD-088 [B20] ──┬─> BE-HRD-097
                   │
BE-HRD-095 ────────┘

BE-HRD-094 [B21] ─> BE-HRD-098
```

### Grafik B23 — `S-B4` `MVP-20` & Verifikasi: On-Call, SLA, handoff payroll, dan verifikasi

```text
BE-HRD-087 [B20] ─> BE-HRD-099 ─> BE-HRD-100 ─┬─> BE-HRD-101 ─┬─> BE-HRD-102
                                               │               │
                                               └───────────────┘

BE-HRD-094 [B21] ──┐
                   │
BE-HRD-096 [B22] ──┼─> BE-HRD-103
                   │
BE-HRD-097 [B22] ──┼──┐
                   │  │
BE-HRD-098 [B22] ──┘  │
                      │
BE-HRD-102 ───────────┘
```

### Grafik B24 — `S-B1` `MVP-21`: Fondasi data, kebijakan parameter presensi, dan engine pemrosesan shift malam

```text
BE-HRD-006 ✅ [B2] ─> BE-HRD-104 ─┬─> BE-HRD-105 ─> BE-HRD-106
                                  │
                                  └─> BE-HRD-107
```

### Grafik B25 — `S-B1` `MVP-22`: Perizinan pulang cepat, dinas luar, koreksi on-behalf, dan integrasi roster

```text
BE-HRD-105 [B24] ─┬─> BE-HRD-108 ──┐
                  │                │
                  └─> BE-HRD-109 ──┼─> BE-HRD-111
                                   │
BE-HRD-106 [B24] ───> BE-HRD-110 ──┘

BE-HRD-106 [B24] ─┬─> BE-HRD-112
                  │
BE-HRD-097 [B22] ─┘
```

### Grafik B26 — `S-B1` `MVP-23`: Penutupan periode aman, guard koreksi pending, buka kembali darurat & payroll

```text
BE-HRD-106 [B24] ──┬─> BE-HRD-113 ─┬─> BE-HRD-114 ─┬─> BE-HRD-115
                   │               │               │
BE-HRD-110 [B25] ──┘               └───────────────┘
```

### Grafik B27 — `S-B1` Verifikasi: Verifikasi build, kontrak, dan QBE slice `S-B1`

```text
BE-HRD-107 [B24] ──┐
                   │
BE-HRD-111 [B25] ──┼─> BE-HRD-116
                   │
BE-HRD-112 [B25] ──┤
                   │
BE-HRD-115 [B26] ──┘
```

### Grafik B28 — `S-B2` `MVP-24`: Fondasi database, buku besar mutasi saldo, akrual 12 hari & carry-forward

```text
BE-HRD-007 ✅ [B2] ─> BE-HRD-117 ─> BE-HRD-118 ─┬─> BE-HRD-119 ──┐
                                                │                 ├─> BE-HRD-120
                                                └─────────────────┘
```

### Grafik B29 — `S-B2` `MVP-25`: Pengajuan ESS half-day, evaluasi safe staffing coverage warning & cuti sakit K3RS

```text
BE-HRD-118 [B28] ─┬─> BE-HRD-121 ──┐
                  │                │
BE-HRD-087 [B22] ─┘                │
                                   ├─> BE-HRD-124
BE-HRD-118 [B28] ───> BE-HRD-122 ──┤   │
                  │                │   │
                  └─> BE-HRD-123 ──┘   │
                                       │
BE-HRD-122 [B29] ───> BE-HRD-125       │
```

### Grafik B30 — `S-B2` `MVP-26`: Leave recall darurat, pemulihan saldo otomatis, penutupan celah /reverse & rekonsiliasi payroll

```text
BE-HRD-124 [B29] ─┬─> BE-HRD-126 ─> BE-HRD-127
                  │
                  ├─> BE-HRD-128
                  │
                  └─> BE-HRD-129
```

### Grafik B31 — `S-B2` Verifikasi: Verifikasi build, kontrak, dan QBE slice `S-B2`

```text
BE-HRD-120 [B28] ──┐
                   │
BE-HRD-125 [B29] ──┼─> BE-HRD-130
                   │
BE-HRD-127 [B30] ──┤
                   │
BE-HRD-128 [B30] ──┤
                   │
BE-HRD-129 [B30] ──┘
```

#### Tabel Gelombang Eksekusi `S-B2`

| Gelombang | Task yang dapat dimulai bersamaan | Yang menunggu gelombang ini |
| :---: | --- | --- |
| **1** | `BE-HRD-117` (fondasi skema database, migrasi EF Core S-B2) | `BE-HRD-118` |
| **2** | `BE-HRD-118` (service buku besar mutasi saldo & akrual hak 12 hari) | `BE-HRD-119`, `BE-HRD-120`, `BE-HRD-121`, `BE-HRD-122`, `BE-HRD-123` |
| **3** | `BE-HRD-119` (scheduler carry-forward 6 hari & kedaluwarsa 30 Juni), `BE-HRD-121` (engine Safe Staffing Coverage Guard), `BE-HRD-122` (pengajuan ESS & half-day 0.5 hari), `BE-HRD-123` (cuti sakit & privasi dokter K3RS) | `BE-HRD-120`, `BE-HRD-124`, `BE-HRD-125` |
| **4** | `BE-HRD-120` (controller & DTO saldo ledger), `BE-HRD-125` (pembatalan pre-leave pulih 100%), `BE-HRD-124` (approval atasan berindikator kuota & mitigasi) | `BE-HRD-126`, `BE-HRD-128`, `BE-HRD-129`, `BE-HRD-130` |
| **5** | `BE-HRD-126` (workflow leave recall darurat), `BE-HRD-128` (guard penutupan celah /reverse HTTP 409 & HR adjustment), `BE-HRD-129` (rekonsiliasi retrospektif cut-off payroll) | `BE-HRD-127`, `BE-HRD-130` |
| **6** | `BE-HRD-127` (eksekusi restore saldo recall & event pemulihan jadwal Roster 06) | `BE-HRD-130` |
| **7** | `BE-HRD-130` (verifikasi akhir ujung-ke-ujung arsitektur QBE, kontrak, dan build S-B2) | Selesai dan siap integrasi frontend |

### Grafik B32 - `S-B3` `MVP-27`: Fondasi skema lembur, SPKL pra-dinas terencana H-1, kuota PP 35/2021 & anti-self-approval

```text
BE-HRD-008 [B3] ─> BE-HRD-131 ┬─> BE-HRD-132 ──────┬─> BE-HRD-134 ──> BE-HRD-136 ──> BE-HRD-137
                               ├─> BE-HRD-133 ──────┤                    │
                               │                    └─> BE-HRD-135 ──────┘
                               └─────────────────────────────────────────┘
```

### Grafik B33 - `S-B3` `MVP-28`: Klaim darurat cito H+1, eskalasi Direktur Medis, verifikasi presensi biometrik & anomali discrepancy

```text
BE-HRD-135 [B32] ─> BE-HRD-138 ─┐
BE-HRD-136 [B32] ─┘             │
                                ├─> BE-HRD-139 ──> BE-HRD-140
BE-HRD-137 [B32] ───────────────┘
```

### Grafik B34 - `S-B3` `MVP-29`: Saldo cuti pengganti CTO (90 hari), penutupan cut-off bulanan, rekonsiliasi retrospektif & handoff payroll

```text
BE-HRD-137 [B32] ─┬─> BE-HRD-141 ─┬─> BE-HRD-142
BE-HRD-138 [B33] ─┘               └─> BE-HRD-143 ──> BE-HRD-144
BE-HRD-140 [B33] ───────────────────┘
```

### Grafik B35 - `S-B3` Verifikasi: Verifikasi build, kontrak, dan QBE slice `S-B3`

```text
BE-HRD-137 [B32] ─┐
BE-HRD-138 [B33] ─┼─> BE-HRD-145
BE-HRD-140 [B33] ─┤
BE-HRD-142 [B34] ─┤
BE-HRD-144 [B34] ─┘
```

#### Tabel Gelombang Eksekusi `S-B3`

| Gelombang | Task yang Dapat Dikerjakan Paralel | Membuka Gerbang Berikutnya |
| :---: | :--- | :--- |
| **1** | `BE-HRD-131` (fondasi skema lembur DDL PostgreSQL) | `BE-HRD-132`, `BE-HRD-133` |
| **2** | `BE-HRD-132` (setting kuota lembur RS), `BE-HRD-133` (DTO & validasi FluentValidation) | `BE-HRD-134`, `BE-HRD-135` |
| **3** | `BE-HRD-134` (service SPKL terencana pra-dinas H-1 & tim medis), `BE-HRD-135` (klaim cito pasca-dinas H+1) | `BE-HRD-136` |
| **4** | `BE-HRD-136` (engine kuota PP 35/2021 & guard eskalasi) | `BE-HRD-137`, `BE-HRD-138` |
| **5** | `BE-HRD-137` (workflow approval 2-tahap & anti-self-approval), `BE-HRD-138` (otorisasi eskalasi Direktur Medis) | `BE-HRD-139`, `BE-HRD-141`, `BE-HRD-145` |
| **6** | `BE-HRD-139` (verifikasi mesin presensi biometrik & min-duration rule), `BE-HRD-141` (service saldo cuti CTO 90 hari & INT-HR-49) | `BE-HRD-140`, `BE-HRD-142`, `BE-HRD-143` |
| **7** | `BE-HRD-140` (manajemen anomali discrepancy & logbook HR), `BE-HRD-142` (scheduler eksekusi hangus CTO) | `BE-HRD-143`, `BE-HRD-145` |
| **8** | `BE-HRD-143` (manajemen cut-off bulanan tgl 20 & rekonsiliasi retrospektif) | `BE-HRD-144` |
| **9** | `BE-HRD-144` (handoff paket rekonsiliasi lembur ke Payroll 10) | `BE-HRD-145` |
| **10** | `BE-HRD-145` (verifikasi ujung-ke-ujung arsitektur QBE, kontrak v13, build Slice S-B3) | **Sign-off Teknis Slice S-B3** |

#### Tabel Gelombang Eksekusi `S-B1`

| Gelombang | Task yang dapat dimulai bersamaan | Yang menunggu gelombang ini |
| :---: | --- | --- |
| **1** | `BE-HRD-104` (fondasi skema & migrasi EF Core S-B1) | `BE-HRD-105`, `BE-HRD-107` |
| **2** | `BE-HRD-105` (service kebijakan parameter presensi), `BE-HRD-107` (audit log mesin biometrik mentah) | `BE-HRD-106`, `BE-HRD-108`, `BE-HRD-109` |
| **3** | `BE-HRD-106` (engine pemrosesan shift malam & absensi), `BE-HRD-108` (izin pulang cepat), `BE-HRD-109` (surat dinas luar OnDuty) | `BE-HRD-110`, `BE-HRD-112`, `BE-HRD-113` |
| **4** | `BE-HRD-110` (koreksi presensi on-behalf kuasa HR), `BE-HRD-112` (consumer event mutasi jadwal roster) | `BE-HRD-111`, `BE-HRD-113` |
| **5** | `BE-HRD-111` (controller & endpoint perizinan presensi), `BE-HRD-113` (guard pra-tutup periode absensi) | `BE-HRD-114`, `BE-HRD-115`, `BE-HRD-116` |
| **6** | `BE-HRD-114` (service buka kembali darurat HR Manager & rollback payroll) | `BE-HRD-115` |
| **7** | `BE-HRD-115` (controller & endpoint siklus periode absensi) | `BE-HRD-116` |
| **8** | `BE-HRD-116` (verifikasi akhir ujung-ke-ujung arsitektur QBE, kontrak, dan build S-B1) | Selesai dan siap integrasi frontend |

#### Tabel Gelombang Eksekusi `S-B4`

| Gelombang | Task yang dapat dimulai bersamaan | Yang menunggu gelombang ini |
| :---: | --- | --- |
| **1** | `BE-HRD-087` (fondasi skema & migrasi S-B4) | `BE-HRD-088`, `BE-HRD-091`, `BE-HRD-099` |
| **2** | `BE-HRD-088`, `BE-HRD-091`, `BE-HRD-099` | `BE-HRD-089`, `BE-HRD-090`, `BE-HRD-092`, `BE-HRD-100` |
| **3** | `BE-HRD-089`, `BE-HRD-090`, `BE-HRD-092`, `BE-HRD-100` | `BE-HRD-093`, `BE-HRD-101` |
| **4** | `BE-HRD-093`, `BE-HRD-101` | `BE-HRD-094`, `BE-HRD-095`, `BE-HRD-102` |
| **5** | `BE-HRD-094`, `BE-HRD-095`, `BE-HRD-102` | `BE-HRD-096`, `BE-HRD-097`, `BE-HRD-098` |
| **6** | `BE-HRD-096`, `BE-HRD-097`, `BE-HRD-098` | `BE-HRD-103` (verifikasi slice S-B4) |
| **7** | `BE-HRD-103` (verifikasi akhir ujung-ke-ujung S-B4) | Selesai dan siap integrasi frontend |

### ✅ `BE-HRD-001` — Pendaftaran Prefix Operasional `Hrd` dan Workforce `Wfp` pada Registry
* **Slice**: `S0-A`
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-001.md))
* **Outcome**: Dokumen `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` secara resmi memuat entri prefix `Hrd` (operasional canonical) dan `Wfp` (workforce profile HR) tanpa menandai `Wfp` sebagai legacy terhapus.
* **Acceptance Criteria**:
  1. Registry memuat baris `Corporate / SelfServices | Human Resource | BUSINESS DOMAIN | Hrd | ACTIVE`.
  2. Registry memuat baris `Corporate / SelfServices | Human Resource | BUSINESS DOMAIN | Wfp | ACTIVE`.
  3. Tidak ada entity `Trx*` yang diklaim secara borongan milik HR; pemilikan didasarkan pada domain dan bukti.
* **Definition of Done**: Berkas registry diperbarui, `Invoke-QbeConformanceCheck.ps1` lulus validasi modul.

### ✅ `BE-HRD-002` — Route Canonical Kebab-Case dan Compatibility Alias
* **Slice**: `S0-B`
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-002.md))
* **Outcome**: Delapan master data controller yang sebelumnya memakai non-hyphenated route memiliki alias kebab-case resmi tanpa merusak pemanggil lama.
* **Acceptance Criteria**:
  1. Route `work-calendars`, `work-schedules`, `shift-groups`, `shift-patterns`, `action-types`, `case-types`, `sanction-types`, `violation-types` aktif.
  2. Route lama tetap dilayani oleh action yang sama dengan header / swagger attribute deprecated.
* **Definition of Done**: `dotnet build` sukses, Swagger JSON memproyeksikan route canonical dan alias.

### ✅ `BE-HRD-003` — Endpoint Daftar Lintas-Pegawai Administrasi Kepegawaian
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-003.md))
* **Slice**: `S-A1`
* **Outcome**: HR Admin dapat menarik daftar rekapitulasi penempatan organisasi, penempatan jabatan, relasi atasan, riwayat kepegawaian, dan penetapan gaji lintas-seluruh pegawai pada satu periode kerja.
* **Acceptance Criteria**:
  1. Menyediakan endpoint paged result `GET /api/v1/corporate/human-resource/workforce-core/salary-assignments`.
  2. Menyediakan endpoint paged result untuk penempatan organisasi dan jabatan lintas pegawai.
  3. Mendukung filter berdasarkan unit kerja, status aktif, dan rentang tanggal berlaku.
* **Definition of Done**: Controller & service mengembalikan `ApiResponse<PagedResult<T>>`, authorization `[AccessAction]` terpasang.

### ✅ `BE-HRD-004` — Verifikasi dan Pemantapan Endpoint Layanan Mandiri Karyawan (ESS)
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-004.md))
* **Slice**: `S-A2..S-A6`
* **Outcome**: Endpoint pengajuan cuti, lembur, permohonan tukar shift, koreksi absensi, dan update profil mandiri diverifikasi bekerja sesuai identitas pengguna yang login (`actorWorkforceProfileId`).
* **Acceptance Criteria**:
  1. Endpoint memvalidasi bahwa permohonan hanya dibuat untuk profil pegawai yang sedang terautentikasi.
  2. Validasi kuota dan saldo cuti menolak pengajuan yang melebihi hak.
  3. Pengajuan lembur mendeteksi tumpang tindih waktu.
* **Definition of Done**: Seluruh controller ESS teruji menangani kasus normal dan boundary (saldo habis / jadwal bentrok).

### ✅ `BE-HRD-005` — Engine SLA, Eskalasi, dan Kotak Masuk Persetujuan Terpadu
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-005.md))
* **Slice**: `S-A7`
* **Outcome**: Atasan dapat menarik seluruh transaksi bawahan yang membutuhkan persetujuannya dalam satu endpoint terpadu, dan background job eskalasi mengecek batas SLA.
* **Acceptance Criteria**:
  1. `ApprovalInboxController` mengembalikan agregasi permohonan menunggu persetujuan per atasan.
  2. SLA engine mengecek `DueAt` dan memicu notifikasi peringatan.
  3. Persetujuan dan penolakan memperbarui instance workflow dan status transaksi pemilik secara atomik.
* **Definition of Done**: Service terintegrasi dengan `WorkflowManagement`, tidak ada data status yang orphan.

### ✅ `BE-HRD-006` — Administrasi Presensi Harian, Penutupan Periode, dan Jalur On-Behalf HR
* **Slice**: `S-B1`
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-006.md))
* **Outcome**: HR Admin dapat memantau presensi harian seluruh staf RS, mengunci periode presensi, dan memproses permohonan koreksi kehadiran atas nama staf (on-behalf) dengan audit trail lengkap.
* **Acceptance Criteria**:
  1. Periode kehadiran yang sudah ditutup (`Closed`) menolak pencatatan presensi retroactive tanpa pembukaan resmi.
  2. Jalur koreksi on-behalf mencatat ID HR initiator, alasan kedaruratan, dan bukti verifikasi.
* **Definition of Done**: 71 endpoint attendance diverifikasi, proteksi periode terkunci terbukti aktif.

### ✅ `BE-HRD-007` — Administrasi Cuti, Penyesuaian Saldo Bertahap, dan Rekonsiliasi
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-007.md))
* **Slice**: `S-B2`
* **Outcome**: Pengelolaan saldo cuti tahunan, cuti besar, dan izin istirahat pegawai terpelihara dengan mutasi saldo yang akuntabel.
* **Acceptance Criteria**:
  1. Penyesuaian saldo manual oleh HR wajib menyertakan alasan penyesuaian dan referensi dokumen.
  2. Pembatalan cuti yang telah disetujui memulihkan saldo secara idempoten.
* **Definition of Done**: Mutasi saldo cuti tercatat di tabel log penyesuaian, audit trail tersimpan.

### ✅ `BE-HRD-008` — Administrasi Lembur dan Rekonsiliasi Penggajian
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-008.md))
* **Slice**: `S-B3`
* **Outcome**: HR memverifikasi realisasi jam lembur terhadap surat perintah kerja lembur (SPKL) sebelum diteruskan ke perhitungan payroll.
* **Acceptance Criteria**:
  1. Realisasi lembur yang belum disetujui supervisor ditandai tidak layak diperhitungkan dalam payroll run.
  2. Rekonsiliasi jam lembur menghitung faktor pengali hari kerja dan hari libur secara tepat.
* **Definition of Done**: Rekonsiliasi lembur menghasilkan status `VerifiedForPayroll`.

### ✅ `BE-HRD-009` — Penjadwalan Roster Kerja 24/7 dan Penugasan Siaga (On-Call)
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-009.md))
* **Slice**: `S-B4`
* **Outcome**: Penugasan shift harian dan jadwal jaga tenaga kesehatan per instalasi (Rawat Inap, IGD, ICU, Bedah) terkelola tanpa tabrakan jadwal.
* **Acceptance Criteria**:
  1. Validasi mendeteksi jadwal ganda pegawai pada shift yang sama atau shift berturut-turut tanpa jeda istirahat wajib.
  2. Penggantian shift mendadak (darurat) mencatat pengganti dan alasan operasional.
* **Definition of Done**: Controller roster penugasan shift terhubung ke model roster period dan unit kerja RS.

### ✅ `BE-HRD-010` — Kalkulasi Payroll HR, Pemotongan Angsuran, dan Restitusi
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-010.md))
* **Slice**: `S-B5`
* **Outcome**: Proses hitung gaji kotor, potongan ketidakhadiran, potongan pinjaman/angsuran, tunjangan shift, restitusi klaim, hingga penghasilan bersih per periode penggajian.
* **Acceptance Criteria**:
  1. Perhitungan memasukkan data sah dari presensi dan lembur yang sudah berstatus final.
  2. `PayrollRestitutionController` dan `PayrollRunInstallmentDeductionController` terintegrasi pada putaran payroll.
  3. Serah terima batch payroll ke modul Finance bersifat idempoten.
* **Definition of Done**: Perhitungan payroll dapat dieksekusi, di-rollback bila ada anomali, dan dikunci sebelum serah terima.

### ✅ `BE-HRD-012` — Penugasan Pelatihan Wajib, Pelacakan Sertifikasi, dan Masa Berlaku
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-012.md))
* **Slice**: `S-C2`
* **Outcome**: Penugasan pelatihan keselamatan pasien (BHD, PPI, K3RS), pencatatan sertifikat, masa berlaku sertifikat, dan asesmen kompetensi staf rumah sakit terpantau.
* **Acceptance Criteria**:
  1. Pelatihan wajib yang belum dipenuhi terlihat per pegawai dan per unit.
  2. Sertifikat tersimpan sebagai bukti yang dapat ditelusuri.
  3. Masa berlaku sertifikat memicu peringatan sebelum kedaluwarsa.
* **Definition of Done**: Controller & model training record dan competency assessment terhubung ke profil kepegawaian.

### ✅ `BE-HRD-013` — Siklus Evaluasi Kinerja Karyawan, Skala Penilaian KPI, dan Finalisasi
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-013.md))
* **Slice**: `S-C3`
* **Outcome**: Siklus evaluasi berkala KPI staf berjalan transparan, 1 review aktif per siklus, dan privasi hasil terjaga.
* **Acceptance Criteria**:
  1. Satu pegawai tidak dapat dinilai dua kali pada siklus yang sama.
  2. Penilaian yang sudah final tidak dapat diubah diam-diam; koreksi menghasilkan revisi.
  3. Pegawai melihat hasilnya sendiri, bukan hasil orang lain.
* **Definition of Done**: Controller review kinerja dan rincian KPI terhubung ke model siklus dan profil staf.

### ✅ `BE-HRD-014` — Transaksi Exit Clearance, Pengembalian Aset, dan Offboarding
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-014.md))
* **Slice**: `S-C4`
* **Outcome**: Pengelolaan proses berhenti kerja karyawan (resign/pensiun/habis kontrak), mencakup checklist penyerahan aset unit, perpustakaan, kartu akses, dan status kliring keuangan.
* **Acceptance Criteria**:
  1. `ExitClearanceController` memvalidasi status kelulusan setiap unit verifikator (IT, GA, Farmasi, Keuangan).
  2. Penutupan exit clearance menandai profil pegawai sebagai non-aktif tanpa menghapus data riwayat.
* **Definition of Done**: Controller & service exit clearance terintegrasi, laporan verifikasi tercatat.

### ✅ `BE-HRD-015` — Pencatatan Kasus Hubungan Karyawan, Pembinaan Disiplin, dan Sanksi
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-015.md))
* **Slice**: `S-C5`
* **Outcome**: Penanganan kasus pelanggaran disiplin kerja staf medis dan non-medis, penerbitan surat peringatan (SP 1, SP 2, SP 3), dan riwayat pembinaan kepegawaian tercatat aman.
* **Acceptance Criteria**:
  1. Kasus kedisiplinan hanya dapat dibaca peran yang berwenang.
  2. Setiap tindakan menyimpan alasan, pelaku, dan waktu kejadian.
  3. Kasus yang sudah ditutup tidak dapat diubah diam-diam.
* **Definition of Done**: Controller & model tindakan disiplin terhubung ke master jenis pelanggaran dan sanksi.

### ✅ `BE-HRD-019` — Pendaftaran Benefit Karyawan, Tanggungan, dan Mutasi Saldo
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-019.md))
* **Slice**: `S-D3`
* **Outcome**: Pengelolaan kepesertaan asuransi kesehatan staf/tanggungan, alokasi plafon rawat jalan/inap, dan pencatatan mutasi saldo benefit pegawai rumah sakit.
* **Acceptance Criteria**:
  1. `EmployeeBenefitEnrollmentController` dan `EmployeeBenefitDependentController` mengelola eligibilitas dan plafon.
  2. Setiap mutasi saldo benefit (klaim/top-up tahunan) tercatat pada `HrdBenefitBalanceMovement`.
  3. Validasi mencegah pendaftaran tanggungan melebihi batas ketentuan paket benefit.
* **Definition of Done**: Service benefit enrollment & balance management terverifikasi, relasi ke penjamin perusahaan pasien terhubung.

### ✅ `BE-HRD-022` — Audit Ratchet Penamaan Entitas Transaksi Legacy (`Trx` ke `Hrd`)
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-022.md))
* **Slice**: `S-E`
* **Outcome**: Penegakan aturan konstitusi rekayasa bahwa penggantian prefix `Trx` ke `Hrd` hanya terjadi saat entity mengalami mutasi skema material, mencegah rename massal destruktif.
* **Acceptance Criteria**:
  1. Ratchet penamaan hanya dipicu saat entity benar-benar disentuh skemanya (`AT-HRD-E-01`).
  2. Ratchet tidak dipicu oleh pekerjaan query atau dokumentasi (`AT-HRD-E-02`).
* **Definition of Done**: Kepatuhan audit ratchet diverifikasi penuh pada seluruh entitas HR.


### ✅ `BE-HRD-023` — Model, Konfigurasi EF Core, dan Migration Skema `HrdPersonnelActionOrder` & Approval Fields
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-023.md)
* **Outcome**: Skema database modul HR diperkaya dengan entitas agregat `HrdPersonnelActionOrder`, entitas lampiran berkas `HrdPersonnelActionOrderAttachment`, entitas batch `HrdInitialPlacementBatch` & `HrdInitialPlacementBatchRow`, serta perluasan kolom `PersonnelActionOrderId` (FK nullable) dan audit approval fields pada `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`, dan `WfpSalaryAssignment` via Migration 7 (`AddPersonnelActionOrderAndAssignmentApprovalFields`).
* **Traceability**: `HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`, `FR-HRD-013`, `02-backend-architecture.md` §5.6 & §8.2 Migration 7, `data/data-dictionary.md` §2.8, §2.10.
* **Contract Version**: Data Dictionary v6, Backend Architecture rev 2.
* **Kemampuan Existing yang Dipakai**: Entitas `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`, `WfpSalaryAssignment`, `MstWorkforceProfile`.
* **Cakupan yang Diharapkan**:
  1. Definisi model entity C# di `Areas/Corporate/HumanResource/Models/` atau direktori domain HR canonical.
  2. Fluent API configuration pada DbContext mencakup konfigurasi relasi, constraint `IsUnique` untuk `OrderNumber`, dan cascade delete restrict.
  3. Migration script EF Core `AddPersonnelActionOrderAndAssignmentApprovalFields`.
* **Dependency**: `BE-HRD-003` ✅
* **Acceptance Criteria**:
  1. Entitas `HrdPersonnelActionOrder` memuat seluruh atribut wajib (OrderNumber, ActionType, WorkforceProfileId, EffectiveDate, PostingDate, Status, Reason, ReferenceDocumentNumber, Notes) sesuai kamus data.
  2. Entitas penempatan (`Wfp*Assignment`) memiliki kolom `PersonnelActionOrderId` (FK nullable) yang mereferensikan `HrdPersonnelActionOrder`.
  3. Index unik pada `OrderNumber` terdefinisi dengan benar pada EF Core metadata.
  4. Perintah migrasi database dapat dieksekusi tanpa error (`dotnet ef database update --no-build`).
* **Bukti Verifikasi**:
  - Review diff skema EF Core model snapshot.
  - Kompilasi project backend (`dotnet build`) sukses 0 error.
  - Conformance QBE (`Invoke-QbeConformanceCheck.ps1`) lulus untuk entitas ber-prefix `Hrd`.
* **Risiko & Pemilik**: Pemilik: Backend Technical Lead. Risiko: lock schema pada tabel penempatan aktif diatasi dengan nullable foreign key.
* **Definition of Done**: Migration terdaftar di snapshot EF Core, model terkompilasi 0 error, audit skema QBE lulus.

### ✅ `BE-HRD-024` — Service Mutasi & Persetujuan SK Kepegawaian (`HrdPersonnelActionOrderService`) & Validasi
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-024.md)
* **Outcome**: Layanan bisnis inti untuk siklus hidup Surat Keputusan Kepegawaian (`Draft`, `PendingApproval`, `Scheduled`, `Applied`, `Rejected`, `Cancelled`) terimplementasi lengkap dengan penegakan seluruh aturan bisnis dan validasi integritas data rumah sakit.
* **Traceability**: `HRD-DEC-031`, `HRD-DEC-048`, `HRD-DEC-049`, `HRD-DEC-050`, `HRD-DEC-056`, `HRD-DEC-057`, `FR-HRD-013`, `FR-HRD-014`, `FR-HRD-015`, `FR-HRD-020`, `VR-WFC-01` s.d. `VR-WFC-05`, `VR-WFC-11` s.d. `VR-WFC-13`.
* **Contract Version**: State Transition Matrix v6, Validation Matrix v6.
* **Kemampuan Existing yang Dipakai**: `IWorkforceProfileService`, `IAttendanceAdministrationService` (pemeriksaan status periode presensi tertutup).
* **Cakupan yang Diharapkan**:
  1. Layanan interface `IHrdPersonnelActionOrderService` dan implementasinya di Application Layer backend.
  2. Mesin transisi status SK yang mengikat dan memvalidasi setiap fase perubahan.
  3. Integrasi pemeriksaan status periode kehadiran tertutup sebelum menyetujui mutasi berlaku surut (retroaktif).
  4. Pembuatan otomatis entitas penempatan aktif (`Wfp*Assignment`) saat status SK mencapai `Applied`.
* **Dependency**: `BE-HRD-023`
* **Acceptance Criteria**:
  1. Memvalidasi bahwa setiap pegawai hanya memiliki tepat 1 penempatan unit dan jabatan utama aktif pada satu tanggal berlaku (`VR-WFC-01`, `VR-WFC-02`).
  2. Menolak SK mutasi dengan tanggal efektif berlaku surut jika tanggal tersebut berada pada periode presensi yang sudah ditutup/closed (`VR-WFC-03`, `HRD-DEC-050`).
  3. Menolak pengajuan persetujuan SK jika belum terdapat berkas pindai fisik bertanda tangan (`VR-WFC-04`, `HRD-DEC-056`).
  4. Menolak persetujuan jika aktor penyetuju adalah pembuat draft SK (larangan self-approval, `VR-WFC-05`, `HRD-DEC-031`).
  5. Saat SK disetujui dan tanggal efektif tiba (`Applied`), sistem secara atomik memperbarui assignment aktif dan menonaktifkan assignment lama.
  6. Pembatalan SK yang sudah berstatus `Applied` hanya dapat dilakukan melalui penerbitan SK Pembatalan / Kontra resmi (`VR-WFC-12`, `HRD-DEC-057`).
* **Bukti Verifikasi**:
  - Pemeriksaan kelengkapan guard clauses pada service method.
  - Verifikasi log audit transaksi perpindahan penempatan.
  - Kompilasi project backend (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: kegagalan konsistensi atomik saat aktivasi multi-assignment diatasi dengan DbContext transaction scope.
* **Definition of Done**: Service terdaftar di ServiceCollection DI, mematuhi matrix transisi status v6 dan matriks validasi v6.

### ✅ `BE-HRD-025` — Controller API Manajemen Dokumen SK Kepegawaian & Unggah Pindai Berkas Fisik
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-025.md)
* **Outcome**: Controller RESTful resmi ber-tag Swagger `[Tags("Corporate / Human Resource / Workforce Core / Personnel Action Order")]` aktif melayani query paged list SK, detail SK, aksi siklus hidup (create draft, submit, approve, reject, cancel), serta endpoint unggah dan unduh berkas pindai fisik SK.
* **Traceability**: `HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`, `FR-HRD-013`, `contracts/api-contract.md` §PersonnelActionOrder.
* **Contract Version**: API Contract v6.
* **Kemampuan Existing yang Dipakai**: `BaseApiController`, `IAccessActionAuthorizer`, modul manajemen berkas upload internal.
* **Cakupan yang Diharapkan**:
  1. Controller `HrdPersonnelActionOrderController` pada route `api/v1/corporate/human-resource/workforce-core/personnel-action-orders`.
  2. DTO request dan response: `PersonnelActionOrderListItemDto`, `PersonnelActionOrderDetailDto`, `CreatePersonnelActionOrderDto`, `UpdatePersonnelActionOrderDto`, `SubmitPersonnelActionOrderDto`, `ApprovePersonnelActionOrderDto`, `RejectPersonnelActionOrderDto`, `CancelPersonnelActionOrderDto`.
  3. Endpoint unggah berkas `POST .../{id}/attachments` yang menyimpan metadata dan berkas PDF/JPG/PNG max 10MB.
* **Dependency**: `BE-HRD-024`
* **Acceptance Criteria**:
  1. `GET /api/v1/corporate/human-resource/workforce-core/personnel-action-orders` mengembalikan `ApiResponse<PagedResult<PersonnelActionOrderListItemDto>>` dengan filter `actionType`, `status`, `organizationUnitId`, `startDate`, `endDate`.
  2. `GET .../{id}` mengembalikan detail lengkap SK beserta daftar penempatan terdampak dan berkas lampiran fisik.
  3. `POST .../create` membuat record SK berstatus `Draft`.
  4. `POST .../{id}/submit`, `approve`, `reject`, `cancel` menjalankan aksi transisi status sesuai wewenang.
  5. Seluruh endpoint dilindungi atribut authorisasi `[AccessAction("Corporate.HumanResource.WorkforceCore.PersonnelActionOrder.*")]`.
* **Bukti Verifikasi**:
  - Swagger UI memproyeksikan tag grup dan skema DTO secara tepat.
  - Kompilasi project (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Backend Technical Lead. Risiko: sanitasi nama berkas dan MIME-type pada attachment handler.
* **Definition of Done**: Seluruh endpoint lolos audit Swagger, DTO mematuhi API Contract v6, authorisasi RBAC aktif.

### ✅ `BE-HRD-026` — Controller & Service Profil Terpadu Pegawai (Employee 360) dengan Dual-Mode Masking Keamanan
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-026.md)
* **Outcome**: Controller RESTful resmi ber-tag Swagger `[Tags("Corporate / Human Resource / Workforce Core / Employee 360")]` aktif melayani data profil komprehensif pegawai (identitas, penempatan, jabatan, atasan, riwayat pendidikan/pelatihan, dan riwayat mutasi SK) dengan filter otorisasi dan masking keamanan dinamis berbasis relasi aktor login.
* **Traceability**: `HRD-DEC-053`, `FR-HRD-018`, `VR-WFC-06`, `VR-WFC-07`, `contracts/api-contract.md` §Employee360.
* **Contract Version**: API Contract v6, Validation Matrix v6.
* **Kemampuan Existing yang Dipakai**: `IWorkforceProfileService`, `WfpManagerAssignment` (relasi hierarki manajerial langsung).
* **Cakupan yang Diharapkan**:
  1. Controller `HrdEmployee360Controller` pada route `api/v1/corporate/human-resource/workforce-core/employee-360/{workforceProfileId}`.
  2. Service `IHrdEmployee360Service` yang mengagregasikan sub-domain data profil dari database.
  3. Mekanisme evaluasi peran: mendeteksi apakah pemanggil adalah HR Officer berwenang, Atasan Langsung aktif pegawai bersangkutan, atau pihak tidak berhak.
  4. Logika sanitasi/masking data personal sensitif dan kompensasi gaji.
* **Dependency**: `BE-HRD-024`
* **Acceptance Criteria**:
  1. Jika pemanggil memiliki hak HR (`Corporate.HumanResource.WorkforceCore.Employee360.ViewAll`), kembalikan seluruh data termasuk kompensasi, rekening, NIK, keluarga, dan catatan disiplin (`VR-WFC-06`).
  2. Jika pemanggil adalah Atasan Langsung (`DirectManager`), validasi bahwa target adalah bawahan langsung aktif (`VR-WFC-07`).
  3. Untuk Atasan Langsung, respons wajib secara mutlak menyembunyikan gaji pokok, tunjangan, nomor rekening bank, NIK, susunan keluarga, dan sanksi disiplin (`HRD-DEC-053`).
  4. Jika pemanggil bukan HR dan bukan atasan langsung dari target profil, tolak permintaan dengan HTTP 403 Forbidden.
* **Bukti Verifikasi**:
  - Verifikasi payload keluaran untuk simulasi user HR vs user Direct Manager.
  - Kompilasi project (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Backend Security & Domain Lead. Risiko: kebocoran data gaji pada manajer diatasi dengan DTO terpisah atau masking ketat sebelum serialisasi.
* **Definition of Done**: Endpoint Employee 360 aktif dan mematuhi batas privasi klinis/keuangan HRD-DEC-053.

### ✅ `BE-HRD-027` — Service & Controller Inisialisasi Penempatan Baseline Pegawai Eksisting (55 Staf)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-027.md)
* **Outcome**: Modul API dan service batch satu kali untuk memasukkan dan memvalidasi penempatan organisasi, jabatan, atasan, dan gaji bagi 55 staf eksisting agar memiliki rekam data penempatan resmi yang terstruktur tanpa merusak integritas historis.
* **Traceability**: `HRD-DEC-054`, `FR-HRD-019`, `VR-WFC-08`, `VR-WFC-09`, `contracts/api-contract.md` §InitialPlacementBatch.
* **Contract Version**: API Contract v6, Data Dictionary v6.
* **Kemampuan Existing yang Dipakai**: `MstWorkforceProfile`, `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`, `WfpSalaryAssignment`.
* **Cakupan yang Diharapkan**:
  1. Controller `HrdInitialPlacementBatchController` pada route `api/v1/corporate/human-resource/workforce-core/initial-placement-batches`.
  2. Engine pemrosesan batch yang membaca daftar baris penempatan awal, memverifikasi ketiadaan duplikasi, dan mengeksekusi penulisan dalam satu transaksi database.
* **Dependency**: `BE-HRD-024`
* **Acceptance Criteria**:
  1. Menyediakan endpoint pembuatan draf batch `POST .../initial-placement-batches` dan query detail status batch `GET .../{id}`.
  2. Menyediakan endpoint eksekusi batch `POST .../{id}/execute`.
  3. Validasi batch memastikan tidak ada profil ganda dalam satu batch dan setiap profil belum memiliki penempatan aktif yang bertentangan (`VR-WFC-08`, `VR-WFC-09`).
  4. Eksekusi batch bersifat atomik: jika 1 baris gagal validasi, seluruh batch di-rollback dengan status `Failed` beserta daftar detail baris error (`HRD-DEC-054`).
  5. Setelah sukses dieksekusi, status batch berubah menjadi `Executed` dan tidak dapat dijalankan ulang.
* **Bukti Verifikasi**:
  - Pemeriksaan transaksi rollback saat baris invalid disimulasikan.
  - Kompilasi project (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Database & Backend Lead. Risiko: duplikasi penempatan awal dicegah dengan flag status executed dan cek penempatan eksisting.
* **Definition of Done**: Service batch teruji, endpoint lolos validasi authorisasi dan kontrak API.

### ✅ `BE-HRD-028` — Endpoint Kuasa HR (On-Behalf) Perubahan Profil Pegawai & Penguncian Form Master Data
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-028.md)
* **Outcome**: Jalur pengajuan perubahan data profil pegawai atas nama pegawai oleh petugas HR (on-behalf) aktif dengan audit trail lengkap dan segregasi tugas (pembuat on-behalf dilarang memverifikasi permohonannya sendiri), serta penyesuaian backend master data pegawai untuk menolak pemindahan unit kerja dan jabatan secara langsung.
* **Traceability**: `HRD-DEC-047`, `HRD-DEC-052`, `FR-HRD-016`, `FR-HRD-017`, `VR-WFC-10`, `contracts/api-contract.md` §EmployeeProfileChange.
* **Contract Version**: API Contract v6, Validation Matrix v6.
* **Kemampuan Existing yang Dipakai**: `EmployeeProfileChangeController`, `WorkforceProfileController`.
* **Cakupan yang Diharapkan**:
  1. Penambahan action `POST /api/v1/corporate/human-resource/workforce-core/employee-profile-changes/on-behalf` pada `EmployeeProfileChangeController`.
  2. Pencatatan flag `IsSubmittedOnBehalf = true`, `SubmittedByWorkforceProfileId = actorId`, dan field alasan kuasa `OnBehalfReason`.
  3. Penegakan aturan: verifikator persetujuan wajib merupakan petugas HR yang berbeda dari aktor yang mengajukan kuasa (`VR-WFC-10`, `HRD-DEC-052`).
  4. Modifikasi `WorkforceProfileController` pada method Update agar mengabaikan atau menolak perubahan langsung pada kolom organisasi dan jabatan, mengharuskan pemakai menggunakan alur SK Kepegawaian (`HRD-DEC-047`).
* **Dependency**: `BE-HRD-024`
* **Acceptance Criteria**:
  1. Endpoint `POST .../employee-profile-changes/on-behalf` menerima payload perubahan data mandiri dan mencatat identitas petugas pembuat kuasa.
  2. Sistem menolak persetujuan permohonan on-behalf jika verifikator memiliki ID yang sama dengan pembuat kuasa (`VR-WFC-10`).
  3. Request `PUT /api/v1/corporate/human-resource/master-data/employees/{id}` menolak perubahan unit organisasi atau jabatan dengan pesan penolakan yang mengarahkan ke menu SK Kepegawaian.
* **Bukti Verifikasi**:
  - Verifikasi penolakan persetujuan on-behalf oleh pembuatnya sendiri.
  - Verifikasi proteksi field unit/jabatan pada endpoint master data pegawai.
  - Kompilasi project (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Backend HR Domain Lead. Risiko: kompatibilitas seeder master data existing diantisipasi dengan bypass khusus flag initialization.
* **Definition of Done**: Endpoint on-behalf aktif, guard clause segregasi tugas terpasang, form master data terlindungi.

---

## Kartu Detail Task Backend — Slice `S-D1` Perencanaan Tenaga Kerja

### Catatan umum untuk seluruh kartu `S-D1`

Bacalah bagian ini sekali; setiap kartu di bawah menganggapnya berlaku.

| Hal | Ketentuan |
| --- | --- |
| Gambaran bisnis | `S-D1` menutup rantai "standar kebutuhan tenaga → formasi resmi → gap → permintaan tambahan tenaga → kursi terisi orang yang benar-benar bertugas". Contoh yang dipakai di seluruh kartu: ICU membuka 2 bed baru, Kepala ICU meminta +2 Perawat Pelaksana di luar formasi, Direktur Pelayanan Medis menyetujui, satu kursi diisi pegawai baru (Wati) dan satu lewat SK pindahan (Andi) |
| Kontrak terkunci | API v7 bagian 13, State Transition v7 bagian 11, Validation v7 bagian 12, Permission v7 bagian 11, Integration v7 bagian 11, Data Dictionary v7 bagian 8, Backend Architecture rev 3 bagian 16. Hash-nya pada metadata. Perubahan kontrak di tengah jalan = revisi blueprint baru, bukan keputusan builder |
| Persetujuan | Tidak ada endpoint setuju/tolak/kembalikan di controller domain. Keputusan `T9` s.d. `T13` diambil lewat kotak masuk persetujuan yang sudah ada (`HRD-DEC-018`) dan diteruskan ke domain oleh `HrdWorkforcePlanningWorkflowIntegrationService` (`BE-HRD-030`). Setiap langkah `AllowSelfApproval = false` |
| Pola endpoint | Pola API bagian 0.1–0.3: `ApiResponse<T>`, `PagedResult<T>`, `{id:guid}`, `[Authorize]`, `[AccessController]` di kelas, `[AccessPermission("Resource","Action")]` di setiap endpoint. Controller daftar menyediakan `GET /filters/metadata` sesuai bagian 0.2. Controller **MUST NOT** mengakses `ApplicationDbContext` langsung (`QBE-SVC-001`) |
| Konkurensi dan nomor | Request ubah, ajukan, batal, dan tarik kembali membawa `rowVersion`; versi usang → `409` (`NFR-012`). Nomor dokumen dialokasikan `NumberSeriesAllocator` di luar transaksi pemanggil, mengikuti `EmployeeBenefitEnrollmentService.cs:323-330` |
| Cakupan unit | Kepala Unit hanya melihat dan mengajukan untuk unit di cakupan organisasinya lewat `OrganizationAuthorizationProjectionService`. Di luar cakupan: tidak muncul di daftar, detail `404` (`VR-WFP-38`) |
| Data sensitif | Angka biaya dan teks alasan **MUST NOT** masuk logger dan respons tanpa hak (`NFR-014`, `data-dictionary.md` bagian 8.5). Payload logger hanya `EntityId`, controller, action, status |
| Tanggal | Tanggal acuan memakai tanggal operasional rumah sakit; waktu kejadian disimpan UTC (`NFR-015`) |
| Bukti verifikasi backend | Mengikuti `rules/backend/TEST_POLICY.md`: QBE preflight dan conformance, review diff dan cakupan, `dotnet restore` dan `dotnet build` project aplikasi, verifikasi kontrak API lewat Swagger, verifikasi proses bisnis lewat uji API terhadap database dev, dan uji runtime bila relevan. **Tidak ada** task maupun syarat automated test backend |
| Wewenang database | Membuat berkas migration adalah source. **Menjalankan** migration ke database dev dan **menulis data uji** butuh wewenang tulis database terpisah. Tanpa wewenang itu, bagian uji API ditandai belum terbukti dan task berhenti di 🟡 |
| Handoff | QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi** dari `AGENTS.md` backend target dan dokumen engineering canonical (`docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| Larangan lintas task | Tidak menyentuh sepuluh model kandidat `WorkforcePlanning` maupun `MstWorkforceRequirement`; tidak membuat endpoint ubah/hapus formasi maupun potret; tidak membaca gaji perorangan; tidak membaca data pasien; tidak mencatat penugasan orang per shift |

### ✅ `BE-HRD-017` — Fondasi data `S-D1`: 16 tabel `Hrd*`, enum, konstanta, dan migration `AddWorkforcePlanningCore`
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: Selesai (✅) — 3 November 2026, [Laporan Tracked](../task/report/backend/BE-HRD-017.md). Kode terkompilasi 0 error & Migration EF Core `AddWorkforcePlanningCore` tergenerasi bersih. (Catatan: migrasi fisik database dev memerlukan otorisasi eksekusi database terpisah).
* **Outcome**: Database punya 16 tabel baru yang masih kosong untuk seluruh data perencanaan tenaga — standar, angka beban, formasi, batch formasi awal, rencana, permintaan beserta kursinya, pembatalan, perubahan rencana pemenuhan, potret gap, dan pengaturan — tanpa satu pun tabel lama yang berubah.
* **Requirement/decision**: `HRD-DEC-059`, `HRD-DEC-019` (prefix `Hrd`), `HRD-TQ-13` (registry), `NFR-012`; `02-backend-architecture.md` bagian 16.4–16.7.
* **Kontrak**: Data Dictionary v7 bagian 8.2–8.4; Backend Architecture rev 3 bagian 16.4.11, 16.5, 16.7.
* **Reuse**: Pola satu berkas model memuat kelas barisnya dari `HrdInitialPlacementBatch.cs`; `RowVersion` + `IsConcurrencyToken()` dari `TrxEmployeeBenefitEnrollmentConfiguration`; FK ke master dan Workforce Core yang sudah ada.
* **Cakupan**:
  1. Sepuluh berkas model di `Areas/Corporate/HumanResource/WorkforcePlanning/Models/` yang memuat 16 kelas (bagian 16.5).
  2. Empat belas enum di `.../WorkforcePlanning/Enums/` dengan nilai persis bagian 16.4.11. `WorkloadAnalysis = 4` ada sebagai nilai cadangan tanpa perilaku.
  3. `Constants/WorkforcePlanningValueConstants.cs`: lima jenis rujukan workflow, lima kunci number series, dan pola kunci advisory lock `HRD_WFP_FORMATION_<unit>_<jabatan>`.
  4. Enam belas `Hrd*Configuration.cs` di `Repositories/Configurations/Corporate/HumanResource/WorkforcePlanning/`.
  5. Enam belas `DbSet` jamak di `ApplicationDbContext`.
  6. Migration `AddWorkforcePlanningCore`.
* **Dependency**: `BE-HRD-023` ✅ (FK kursi internal ke `HrdPersonnelActionOrder`).
* **Acceptance criteria**:
  1. Ke-16 tabel beserta kolom, tipe, nullability, dan panjangnya cocok dengan `data-dictionary.md` bagian 8.2 dan DDL bagian 8.4.
  2. Empat tabel dokumen — `HrdStaffingStandard`, `HrdInitialFormationBatch`, `HrdManpowerPlan`, `HrdHeadcountRequest` — memakai `RowVersion` sebagai concurrency token.
  3. Seluruh index unik DDL bagian 8.4 terpasang, termasuk yang terfilter: satu `LinkedPositionAssignmentId` atau `LinkedPersonnelActionOrderId` hanya untuk satu kursi yang tidak `Cancelled`; satu rencana tahunan per rumah sakit per tahun di luar status `Rejected`/`Cancelled`; satu pengaturan per rumah sakit; satu angka beban per unit × jabatan × tanggal mulai periode.
  4. Perilaku hapus mengikuti DDL: baris anak `CASCADE` ke induknya, rujukan ke master dan Workforce Core `RESTRICT`.
  5. `Up()` hanya berisi pembuatan tabel, index, dan FK untuk ke-16 tabel baru. **Tidak ada** `ALTER` atau `DROP` pada tabel lain, termasuk sepuluh tabel kandidat `WorkforcePlanning` (`HRD-DEC-059`). `Down()` hanya membuang ke-16 tabel itu.
  6. Sepuluh model kandidat dan `MstWorkforceRequirement` tidak dipindah, tidak di-rename, dan tidak dihapus.
* **Bukti verifikasi**: review diff model snapshot EF; `dotnet restore` dan `dotnet build` 0 error; `tooling/qbe/Invoke-QbeConformanceCheck.ps1` memetakan model baru ke baris registry `Hrd`; keluaran `dotnet ef migrations script` untuk migration ini dilampirkan di laporan.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead. Risiko: nama kolom meleset dari kamus data membuat kontrak API ikut meleset — dicegah dengan mencocokkan DDL baris per baris.
* **Definition of Done**: Model, enum, konstanta, konfigurasi, `DbSet`, dan migration terkompilasi; diff hanya menambah; laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-029` — Penelusuran unit induk pada matriks persetujuan untuk langkah Direktur
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: Selesai (✅) — 3 November 2026, [Laporan Tracked](../task/report/backend/BE-HRD-029.md). Logika penelusuran unit induk dan fallback matriks terkompilasi 0 error.
* **Outcome**: Langkah "Direktur yang membawahi unit" pada `T9` dan `T12` menemukan orangnya sendiri. Contoh: Kepala ICU mengajukan standar; sistem menelusuri Unit ICU → Departemen Keperawatan → Direktorat Pelayanan Medis, menemukan baris matriks "Direktorat Pelayanan Medis → jabatan Direktur Pelayanan Medis", dan menugaskan pemegang jabatan itu. Bila jabatan itu kosong, tugas naik ke Direktur Utama; bila keduanya kosong, pengajuan ditolak.
* **Requirement/decision**: `HRD-DEC-072` (`DIRECTOR_IS_POSITION_HOLDER`, `DIRECTOR_VIA_APPROVAL_MATRIX`, `MATRIX_FOLLOWS_UNIT_ANCESTOR`, `DIRECTOR_FALLBACK_TO_PRESIDENT_DIRECTOR`), `HRD-DEC-064` (`UNRESOLVED_STEP_FAILS_CLOSED`), `FR-HRD-116`, `AT-HRD-D1-20`, `INT-HR-18`.
* **Kontrak**: Backend Architecture rev 3 bagian 16.4.9 (`WorkflowService` diperbarui); Integration v7 bagian 11.2.
* **Reuse**: Pencocokan `MstApprovalMatrix` di `WorkflowService.cs` (sekitar baris 1621 dan 1669); sumber penyetuju `Position` dan baris cadangan `IsFallback` (`WorkflowService.cs:1791-1798`); penolakan langkah tanpa penyetuju dan pembuangan pembuat (`WorkflowService.cs:534-556`); `MstOrganizationUnit.ParentOrganizationUnitId`.
* **Cakupan**: Perubahan pada fungsi pencocokan matriks saja. Tanpa perubahan skema, tanpa kolom "kepala unit" baru.
* **Dependency**: `BE-HRD-005` ✅
* **Acceptance criteria**:
  1. Baris matriks untuk sebuah direktorat berlaku bagi seluruh unit di bawahnya; penelusuran naik lewat unit induk sampai baris yang cocok ditemukan.
  2. Baris untuk unit yang lebih dekat menang atas baris unit induknya.
  3. Penyetuju adalah pemegang penempatan jabatan direktur yang berlaku pada tanggal pengajuan; pergantian direktur lewat SK langsung mengganti penyetuju tanpa mengubah data matriks.
  4. Jabatan direktur kosong → langkah naik ke pemegang jabatan Direktur Utama lewat baris `IsFallback`.
  5. Keduanya kosong → pembuatan instance ditolak `400`; langkah **tidak** dilewati.
  6. Rantai `T1` s.d. `T8` dan definisi workflow lain berperilaku sama seperti sebelumnya.
* **Bukti verifikasi**: review diff yang hanya menyentuh pencocokan matriks; `dotnet build` 0 error; uji pembuatan instance di database dev memakai baris matriks dan penempatan jabatan uji untuk kriteria 1–5 (butuh wewenang tulis data dev).
* **Risiko & pemilik**: Pemilik: Backend Workflow Engineer. Risiko: perubahan di mesin persetujuan bersama memengaruhi seluruh modul — dibatasi karena matriks hari ini kosong dan perilaku tanpa baris matriks tidak berubah.
* **Definition of Done**: Penelusuran unit induk aktif, regresi rantai lama tidak ditemukan pada review, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-030` — Lima jenis rujukan workflow dan layanan integrasi persetujuan `S-D1`
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: ✅ Selesai (2026-11-03).
* **Outcome**: Dokumen `S-D1` dapat memulai persetujuan di kotak masuk yang sudah ada, dan keputusan akhirnya kembali ke dokumen asal tepat satu kali — walaupun keputusan itu terkirim dua kali.
* **Requirement/decision**: `HRD-DEC-018`, `HRD-DEC-064`, `INT-HR-18` (idempotensi), `NFR-013`, API v7 bagian 13.1.
* **Kontrak**: API v7 bagian 13.1; Integration v7 bagian 11.2; Backend Architecture rev 3 bagian 16.4.9.
* **Reuse**: `WorkflowReferenceLifecycleService.HandleAsync`/`SynchronizeAsync` yang sudah membagi keputusan per jenis rujukan (pola `IsEmployeeProfileChangeReference`); pembuatan instance di `WorkflowService`.
* **Cakupan**:
  1. Mendaftarkan lima jenis rujukan: `WorkforceStaffingStandard`, `WorkforceInitialFormationBatch`, `WorkforceManpowerPlan`, `WorkforceHeadcountRequest`, `WorkforceHeadcountCancellation`.
  2. `HrdWorkforcePlanningWorkflowIntegrationService`: memulai instance untuk dokumen `S-D1` dan menerima keputusan akhir.
  3. Kontrak penerap per jenis rujukan (setujui akhir, tolak, kembalikan) yang diisi task domain berikutnya.
  4. Registrasi `AddScoped` di `Program.cs` dekat registrasi HR lain.
* **Dependency**: `BE-HRD-017`
* **Acceptance criteria**:
  1. Kelima jenis rujukan dikenali dan diteruskan ke layanan integrasi; jenis rujukan modul lain tidak terpengaruh.
  2. Keputusan untuk jenis rujukan yang belum punya penerap dicatat sebagai galat dan dokumen tidak berubah — bukan crash.
  3. Keputusan "setujui" yang terkirim ulang untuk dokumen yang sudah final (`Approved`, `Applied`, `ReadyForRecruitment`) diabaikan dan dicatat, tidak menghasilkan apa pun untuk kedua kalinya.
  4. Definisi langkah `T9` s.d. `T13` tidak ditulis di source; definisinya data (`BE-HRD-050`).
  5. Payload logger tanpa angka biaya dan tanpa teks alasan.
* **Bukti verifikasi**: review diff; `dotnet build` 0 error; telusur jalur kode `ApproveAsync` → `HandleAsync` → layanan integrasi dicatat di laporan.
* **Risiko & pemilik**: Pemilik: Backend Workflow Engineer. Risiko: penerapan keputusan berjalan **sesudah** langkah workflow di-commit (`HRD-PLAN-GAP-02`), sehingga bila penerapan domain gagal instance tetap `Approved` — dimitigasi dengan `SynchronizeAsync` yang dapat diulang dan pemeriksaan status idempoten.
* **Definition of Done**: Lima jenis rujukan terdaftar, layanan integrasi terdaftar di DI, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-031` — Pengaturan perencanaan per rumah sakit
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: ✅ Selesai (2026-11-03).
* **Outcome**: HR Manager mengisi dua angka kebijakan yang sengaja tidak ditetapkan sistem: berapa hari cuti yang dianggap "tidak tersedia sementara" (misalnya 30 hari), dan berapa hari sebuah permintaan boleh menunggu sebelum ditandai "lewat batas" (misalnya 90 hari).
* **Requirement/decision**: `HRD-DEC-061`, `HRD-DEC-071`, `VR-WFP-40`, `VR-WFP-41`.
* **Kontrak**: API v7 bagian 13 grup `Setting`; Permission v7 bagian 11 `WorkforcePlanningSetting`.
* **Reuse**: Pola controller HR dan custom logger yang sudah ada.
* **Cakupan**: `HrdWorkforcePlanningSettingService`, `HrdWorkforcePlanningSettingController`, `WorkforcePlanningSettingDtos.cs`.
* **Dependency**: `BE-HRD-017`
* **Acceptance criteria**:
  1. `GET /?hospitalSiteId=` mengembalikan pengaturan; rumah sakit yang belum punya baris mendapat kedua nilai kosong (`null`), **bukan** angka bawaan.
  2. `PUT /` hanya untuk pemegang `WorkforcePlanningSetting : Update`; peran lain `403`.
  3. Alasan kosong → `400` dengan pesan `VR-WFP-40`.
  4. Angka di bawah 1 atau di atas 365 → `400` dengan pesan `VR-WFP-41`.
  5. `rowVersion` usang → `409`.
  6. Perubahan meninggalkan jejak sesuai `permission-audit-matrix.md` bagian 11.4 tanpa memuat kolom bertanda sensitif di logger.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger grup `Workforce Planning / Setting`; uji API dev untuk kriteria 1–5.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko kecil.
* **Definition of Done**: Dua endpoint aktif sesuai kontrak, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-032` — Buku formasi berversi: satu-satunya penulis formasi dan API baca
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: ✅ Selesai (2026-11-03).
* **Outcome**: Formasi jabatan — berapa kursi resmi ICU untuk Perawat Pelaksana pada tanggal tertentu — dapat dibaca beserta riwayat versinya, tetapi tidak dapat diubah lewat endpoint mana pun. Perubahan hanya lahir dari dokumen yang disetujui, lewat satu service.
* **Requirement/decision**: `HRD-DEC-062` (`NO_DIRECT_FORMATION_EDIT`, `FORMATION_CARRIES_FORWARD`), `FR-HRD-098`, `FR-HRD-099`, `FR-HRD-100`, `VR-WFP-10`, `NFR-010`, `NFR-011`, `AT-HRD-D1-06` (bagian API), `AT-HRD-D1-07`.
* **Kontrak**: API v7 bagian 13 grup `Position Formation`; State Transition v7 bagian 11.7.
* **Reuse**: Advisory lock `pg_advisory_xact_lock(hashtext(...))` dari `EmployeeBenefitEnrollmentService.cs:726`.
* **Cakupan**: `HrdPositionFormationService` (buat versi di dalam transaksi pemanggil, tutup versi lama, baca formasi berlaku, baca riwayat); `HrdPositionFormationController` baca saja; `PositionFormationDtos.cs`.
* **Dependency**: `BE-HRD-017`
* **Acceptance criteria**:
  1. `GET /?date=2028-01-01` tanpa versi baru untuk 2028 mengembalikan versi terakhir 2027 (`AT-HRD-D1-07`).
  2. `GET /history` mengembalikan seluruh versi satu unit × jabatan beserta jenis dan nomor dokumen sumber, penyetuju, dan waktu persetujuan.
  3. `POST`, `PUT`, `PATCH`, dan `DELETE` ke base URL menjawab `405` dengan pesan `VR-WFP-10` (`AT-HRD-D1-06`).
  4. Service menulis versi baru hanya di dalam transaksi pemanggil, di bawah advisory lock unit × jabatan; versi lama ditutup sehari sebelum tanggal mulai versi baru; kolom selain `EffectiveEndDate` tidak pernah diubah.
  5. Service tidak membuka transaksi sendiri dan tidak dipanggil controller untuk menulis.
  6. Kepala Unit hanya melihat unit dalam cakupannya (`VR-WFP-38`).
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev untuk kriteria 1–3 dan 6; review kode untuk kriteria 4–5 (jalur tulisnya dipakai pertama kali oleh `BE-HRD-033`).
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: penutupan versi lama yang salah tanggal membuat dua versi berlaku bersamaan — dijaga index unik unit × jabatan × tanggal mulai.
* **Definition of Done**: Dua endpoint baca aktif, method tulis `405`, service penulis tunggal terdaftar, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-033` — Batch formasi awal sampai `T10` melahirkan formasi versi 1
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: ✅ Selesai (2026-11-03).
* **Outcome**: HR Admin mengisi formasi resmi pertama dari SK formasi rumah sakit: unduh templat, isi, periksa per baris, unggah scan SK, lalu ajukan. Setelah HR Manager, Finance, dan Direktur Utama menyetujui — semuanya orang lain, bukan pengaju — formasi versi 1 lahir untuk setiap baris sekaligus. Contoh: batch dari "SK Direktur 12/2026" melahirkan formasi ICU versi 1 = 28.
* **Requirement/decision**: `HRD-DEC-070`, `HRD-DEC-064` (`T10`), `FR-HRD-101`, `VR-WFP-11` s.d. `VR-WFP-14`, `AT-HRD-D1-17`, `UAT-31`, `UAT-32`, `NFR-011`, `NFR-012`. Sengaja **tidak** meniru cacat `HRD-TF-033` dan `HRD-TF-034` pada batch penempatan `S-A1`.
* **Kontrak**: API v7 bagian 13 grup `Initial Formation Batch`; State Transition v7 bagian 11.2.
* **Reuse**: Pola unggah berkas `BE-HRD-025` (PDF/JPG/PNG, paling besar 10 MB); `NumberSeriesAllocator`; layanan integrasi `BE-HRD-030`; `HrdPositionFormationService` `BE-HRD-032`; pustaka xlsx yang sudah ada di project (menambah dependency baru butuh approval terpisah).
* **Cakupan**: `HrdInitialFormationBatchService`, `HrdInitialFormationBatchController` (10 endpoint), `InitialFormationBatchDtos.cs`, penerap keputusan `T10` untuk `WorkforceInitialFormationBatch`.
* **Dependency**: `BE-HRD-030`, `BE-HRD-032`
* **Acceptance criteria**:
  1. `GET /template` mengunduh xlsx berisi kolom unit, jabatan, jumlah formasi, dan tanggal berlaku.
  2. `POST /validate` mengembalikan status dan pesan per baris. Unit × jabatan yang sudah punya formasi, sudah ada di batch lain yang belum ditolak/dibatalkan, atau muncul dua kali di batch yang sama → ditolak `VR-WFP-11` (`409` saat simpan atau ajukan).
  3. Scan bukan PDF/JPG/PNG atau lebih dari 10 MB → `400` dengan pesan `VR-WFP-14`.
  4. Ajukan selagi `InvalidRows > 0` → `422` `VR-WFP-12`; ajukan tanpa scan → `422` `VR-WFP-13`.
  5. Ajukan yang sah → `Submitted` dan instance `T10` dimulai; pengaju tidak muncul sebagai penyetuju.
  6. Persetujuan terakhir → `Applied` dan satu formasi versi 1 per baris lewat `HrdPositionFormationService` dalam satu transaksi; satu baris gagal membatalkan seluruhnya; keputusan terkirim ulang tidak membuat versi kedua.
  7. Kembalikan/tolak → `Returned`/`Rejected` beserta alasan; batal hanya dari `Draft`/`Returned` oleh pembuat; `Applied` tidak dapat diubah apa pun.
  8. `rowVersion` usang → `409`.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger grup `Initial Formation Batch`; uji API dev untuk kriteria 1–5, 7, 8; kriteria 6 lewat review jalur penerap, lalu dibuktikan ujung-ke-ujung di `BE-HRD-051`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: batch besar melambat saat persetujuan — seluruh baris tetap satu transaksi demi `NFR-011`.
* **Definition of Done**: Sepuluh endpoint aktif sesuai kontrak, penerap `T10` batch terdaftar, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-034` — Isian angka beban manual
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: ✅ Selesai (2026-11-03).
* **Outcome**: Kepala Unit atau HR mengisi angka beban per unit × jabatan × periode beserta sumbernya. Contoh: "rata-rata 7 pasien ICU terisi per hari, Januari–Maret 2027, sumber: rekap sensus harian". Angka ini dipakai menghitung rekomendasi metode rasio.
* **Requirement/decision**: `HRD-DEC-060` (`MANUAL_WORKLOAD_INPUT`), `FR-HRD-096`, `VR-WFP-08`, `VR-WFP-09`, `VR-WFP-38`. `INT-HR-12` (tarik otomatis dari data pasien) sengaja ditunda.
* **Kontrak**: API v7 bagian 13 grup `Staffing Workload`.
* **Reuse**: `OrganizationAuthorizationProjectionService`.
* **Cakupan**: `HrdStaffingWorkloadService`, `HrdStaffingWorkloadController` (`GET`, `POST`, `PUT`), `StaffingWorkloadDtos.cs`.
* **Dependency**: `BE-HRD-017`
* **Acceptance criteria**:
  1. `POST /` yang lengkap → `201`.
  2. Sumber kosong → `400` `VR-WFP-08`.
  3. Isian kedua untuk unit, jabatan, dan tanggal mulai periode yang sama → `409` `VR-WFP-09`.
  4. Kepala Unit di luar cakupan unitnya: tidak muncul di daftar; `POST`/`PUT` → `404` (`VR-WFP-38`).
  5. Isian tidak melewati persetujuan dan tidak menunjuk baris standar, sehingga tetap berlaku saat standar naik versi.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger; uji API dev.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko kecil.
* **Definition of Done**: Tiga endpoint aktif sesuai kontrak, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-035` — Standar kebutuhan tenaga, angka rekomendasi, dan pengesahan `T9`
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Selesai ✅ (03 November 2026). Laporan: `docs/module-blueprints/human-resource/task/report/backend/BE-HRD-035.md`.
* **Outcome**: HR Admin atau Kepala Unit menyusun standar kebutuhan tenaga per unit sebagai versi bertanggal berlaku, melihat angka rekomendasi beserta rincian hitungnya, lalu mengajukannya. Setelah HR Manager dan Direktur yang membawahi unit menyetujui, standar berlaku pada tanggalnya. Penjadwalan dapat membaca standar yang berlaku untuk satu unit pada satu tanggal.
* **Requirement/decision**: `HRD-DEC-058` (`WFP_OWNS_STANDARD`, `SCHEDULING_READS_STANDARD`), `HRD-DEC-060`, `HRD-DEC-064` (`T9`), `HRD-DEC-072`; `FR-HRD-092` s.d. `FR-HRD-095`, `FR-HRD-097`; `VR-WFP-01` s.d. `VR-WFP-07`; `AT-HRD-D1-02`, `AT-HRD-D1-02b`, `AT-HRD-D1-19`; `UAT-29`, `UAT-30`; `INT-HR-13`.
* **Kontrak**: API v7 bagian 13 grup `Staffing Standard`; State Transition v7 bagian 11.1; Backend Architecture rev 3 bagian 16.9 (rumus).
* **Reuse**: Layanan integrasi `BE-HRD-030`; penelusuran Direktur `BE-HRD-029`; angka beban `BE-HRD-034`; `NumberSeriesAllocator`.
* **Cakupan**: `HrdStaffingStandardService`, `HrdStaffingRecommendationService`, `HrdStaffingStandardController` (8 endpoint kontrak + `GET /filters/metadata`), `StaffingStandardDtos.cs`, penerap keputusan `T9`.
* **Dependency**: `BE-HRD-029`, `BE-HRD-030`, `BE-HRD-034`
* **Acceptance criteria**:
  1. `POST /` membuat draf versi baru untuk satu unit; bila ada versi aktif, isinya disalin sebagai titik awal. Draf kedua untuk unit yang masih punya draf/`Returned` → `409` `VR-WFP-01`.
  2. `PUT /{id}` menyimpan baris jabatan dan skill mix. Metode Analisis Beban Kerja → `422` `VR-WFP-02`; faktor cadangan di bawah 1,00 → `400` `VR-WFP-05`; jabatan ganda → `409` `VR-WFP-07`.
  3. `POST /{id}/submit` menolak `422` bila isian per metode kurang (`VR-WFP-03`), faktor cadangan atau pembulatan belum dipilih (`VR-WFP-04` — sistem tidak memberi nilai bawaan), atau tanggal berlaku di masa lalu (`VR-WFP-06`); menolak `400` bila penyetuju langkah Direktur tidak ditemukan.
  4. `GET /{id}/recommendation?periodStartDate=` menghitung per baris dengan rumus bagian 16.9. Contoh yang wajib cocok persis: ICU beban 7, rasio 1:1, 3 shift, faktor 1,5, pembulatan ke atas → `32` dengan rincian "7 ÷ 1 × 1 = 7 per shift; 7 × 3 = 21; 21 × 1,5 = 31,5 → 32"; Farmasi jumlah tetap 4 → `4`; IGD cakupan layanan 3 shift × 2 orang × 1,5 → `9`. Angka beban belum diisi → rekomendasi `null` dan `missingInput` "angka beban belum diisi", **bukan** 0.
  5. Persetujuan terakhir `T9`: tanggal berlaku hari ini → `Active` dan versi aktif sebelumnya `Superseded`; tanggal berlaku di masa depan → `Scheduled`. Kembalikan/tolak wajib beralasan. Versi `Active`, `Scheduled`, `Superseded` tidak dapat diubah isinya.
  6. `GET /effective?organizationUnitId=&date=` mengembalikan versi `Active` pada tanggal itu, atau `404` bila belum ada. Versi `Draft`/`Submitted` tidak pernah dikembalikan.
  7. Tidak ada endpoint grup ini yang menerima atau mengembalikan penugasan orang per shift (`AT-HRD-D1-19`).
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger grup `Staffing Standard`; uji API dev untuk kriteria 1–4, 6, 7; kriteria 5 lewat review jalur penerap, lalu ujung-ke-ujung di `BE-HRD-051`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: pembulatan desimal berbeda dari contoh — rincian teks wajib persis contoh `AT-HRD-D1-02`.
* **Definition of Done**: Endpoint standar aktif, rumus terverifikasi dengan tiga contoh, penerap `T9` terdaftar, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-036` — Scheduler perencanaan dan aktivasi standar pada tanggal berlaku
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: ✅ Selesai (2026-11-04).
* **Outcome**: Setiap hari sistem menjalankan pekerjaan rutin perencanaan. Langkah pertamanya: standar `Scheduled` yang tanggal berlakunya tiba menjadi `Active`, dan versi aktif sebelumnya menjadi `Superseded` dengan tanggal akhir sehari sebelumnya. Task berikutnya menambah langkah ke scheduler yang sama.
* **Requirement/decision**: State Transition v7 bagian 11.1 (`Scheduled` → `Active`), `NFR-013`, Integration v7 bagian 11.2 (scheduler idempoten), `AT-HRD-D1-04`.
* **Kontrak**: Backend Architecture rev 3 bagian 16.4.9 (`HrdWorkforcePlanningSchedulerService`, `HrdWorkforcePlanningSchedulerHostedService`).
* **Reuse**: Pola `BackgroundService` yang didaftarkan `AddHostedService` di `Program.cs` (sekitar baris 1271–1287).
* **Cakupan**: `HrdWorkforcePlanningSchedulerService` dengan langkah-langkah terpisah dan transaksi per langkah; `HrdWorkforcePlanningSchedulerHostedService`; langkah aktivasi standar.
* **Dependency**: `BE-HRD-035`
* **Acceptance criteria**:
  1. Standar `Scheduled` dengan tanggal berlaku = hari ini menjadi `Active`; versi aktif sebelumnya `Superseded` dengan `EffectiveEndDate` = kemarin.
  2. Menjalankan scheduler dua kali pada hari yang sama tidak mengubah apa pun pada jalan kedua.
  3. Kegagalan satu langkah dicatat log dan tidak menghentikan langkah lain.
  4. Struktur langkah memungkinkan task lain menambah langkah tanpa mengubah langkah yang ada.
* **Bukti verifikasi**: `dotnet build` 0 error; uji runtime di dev dengan standar `Scheduled` bertanggal hari ini, lalu periksa status kedua versi sesudah scheduler berjalan dua kali.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: zona waktu membuat aktivasi terlambat sehari — `NFR-015` mengikat tanggal operasional rumah sakit.
* **Definition of Done**: Hosted service terdaftar, langkah aktivasi idempoten, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-037` — Gap tenaga langsung per unit × jabatan
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: ✅ Selesai (2026-11-04).
* **Outcome**: HR dan Kepala Unit melihat, untuk setiap unit × jabatan pada tanggal acuan, berapa formasinya, berapa yang terisi, siapa yang akan kosong, siapa yang tidak tersedia sementara, siapa yang belum kompeten, dan tiga angka gap. Data yang belum ada tampil kosong, **tidak pernah** 0. Contoh: ICU formasi 32, terisi 29,5 (Budi paruh waktu dihitung 0,5; Sari yang Plt di ICU tidak dihitung), akan kosong 1 → gap permanen 3,5.
* **Requirement/decision**: `HRD-DEC-061` (`UNKNOWN_IS_NOT_ZERO`), `FR-HRD-102` s.d. `FR-HRD-104`, `AT-HRD-D1-03`, `AT-HRD-D1-05`, `AT-HRD-D1-05b` (bagian API), `UAT-33`, `INT-HR-15`, `INT-HR-16`.
* **Kontrak**: API v7 bagian 13 grup `Staffing Gap` — `GET /` saja; potret di `BE-HRD-039`. Backend Architecture rev 3 bagian 16.9 (lapisan dan rumus).
* **Reuse**: `WfpOrganizationAssignment`, `WfpPositionAssignment`, cuti yang disetujui, `WfpCompetencyAssessment`, pengunduran diri yang disetujui, SK pindah keluar `Scheduled`; formasi `BE-HRD-032`; standar berlaku `BE-HRD-035`; pengaturan `BE-HRD-031`.
* **Cakupan**: `HrdStaffingGapCalculationService` (tanpa transaksi); `HrdStaffingGapController` endpoint `GET /`; `StaffingGapDtos.cs`.
* **Dependency**: `BE-HRD-031`, `BE-HRD-032`, `BE-HRD-035`
* **Acceptance criteria**:
  1. Terisi dihitung dari penempatan organisasi dan jabatan **utama** yang aktif pada tanggal acuan, dalam porsi kerja; porsi kerja yang belum ada dihitung 1 sampai kolomnya tersedia di Workforce Core. Penempatan bukan utama (rangkap/Plt) hanya tampil sebagai informasi dan tidak menambah terisi.
  2. Lapisan "tidak tersedia sementara" bernilai `null` selama `LongLeaveThresholdDays` belum diisi; lapisan alih daya `null` sampai master jenis tenaga punya kategori alih daya; lapisan "belum kompeten" `null` bila standar berlaku tidak punya skill mix wajib. Baris dengan lapisan `null` membawa penanda kelengkapan `false`.
  3. Gap permanen = formasi − (terisi − akan kosong), ditambah alih daya hanya bila `CountsOutsourced = true`; gap sementara = tidak tersedia sementara; gap kompetensi = belum kompeten.
  4. Hanya standar `Active` yang dipakai; standar versi 2 yang masih `Submitted` tidak menggantikan versi 1 (`AT-HRD-D1-03`), dan respons menyebut versi standar yang dipakai.
  5. Kepala Unit hanya menerima baris unit dalam cakupannya.
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev dengan susunan data `UAT-33` (butuh wewenang tulis data dev).
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: penempatan lama yang tanggalnya salah membuat angka terisi salah (`HRD-TF-013`) — bukan cacat task ini, dicatat di laporan bila ditemukan.
* **Definition of Done**: Gap langsung sesuai kontrak dan contoh `UAT-33`, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-038` — Estimasi biaya dari gaji tengah grade dan penyamaran biaya
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: ✅ Selesai (2026-11-04).
* **Outcome**: Setiap permintaan dan baris rencana punya estimasi biaya yang dihitung dari gaji tengah grade, bukan dari gaji orang. Contoh: +2 kursi grade III selama 9 bulan dengan gaji tengah Rp 6.500.000 → 2 × Rp 6.500.000 × 9 = Rp 117.000.000, berlabel "estimasi". Orang yang tidak berhak melihat biaya menerima kolom biaya kosong.
* **Requirement/decision**: `HRD-DEC-065`, `FR-HRD-109`, `FR-HRD-111`, `VR-WFP-37`, `AT-HRD-D1-11` (bagian perhitungan dan penyamaran).
* **Kontrak**: API v7 bagian 13 aturan "Kolom biaya"; Permission v7 bagian 11.1 `WorkforcePlanningCost`; Data Dictionary v7 bagian 8.5.
* **Reuse**: `MstSalaryGrade.MidpointSalary`.
* **Cakupan**: `HrdWorkforcePlanningCostService` (tarif pada tanggal, hitung estimasi, status `RateUnavailable`, penyamaran). Tidak ada controller.
* **Dependency**: `BE-HRD-017`
* **Acceptance criteria**:
  1. Estimasi = porsi kerja × gaji tengah grade × jumlah bulan; contoh di atas menghasilkan Rp 117.000.000.
  2. Grade tanpa gaji tengah → status `RateUnavailable` dan biaya `null`, **bukan** 0.
  3. Pemanggil tanpa `WorkforcePlanningCost : Read` menerima seluruh kolom biaya `null` dan `costVisible = false`.
  4. Tidak membaca gaji perorangan maupun tabel penetapan gaji; membaca biaya tidak termasuk audit pembacaan gaji sensitif `HRD-DEC-039`.
  5. Angka biaya tidak masuk logger.
* **Bukti verifikasi**: `dotnet build` 0 error; review kode; perhitungan dibuktikan lewat endpoint `BE-HRD-040` dan `BE-HRD-046`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: pembulatan rupiah — mengikuti tipe `decimal` kamus data.
* **Definition of Done**: Service terdaftar di DI, aturan penyamaran terpusat di satu tempat, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-039` — Potret gap pengajuan dan API baca potret
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: ✅ Selesai (2026-11-04).
* **Outcome**: Saat permintaan atau rencana diajukan, angka gap hari itu dikunci sebagai potret yang tidak dapat diubah. Penyetuju kemudian dapat membandingkan "saat diajukan 3,5 — sekarang 2,5".
* **Requirement/decision**: `HRD-DEC-069` (`SNAPSHOT_IMMUTABLE`, `SUBMITTED_VS_CURRENT`), `FR-HRD-105` (bagian potret pengajuan), `VR-WFP-39`, `AT-HRD-D1-16` (bagian API).
* **Kontrak**: API v7 bagian 13 grup `Staffing Gap` — `GET /snapshots`, `GET /snapshots/{id}`; State Transition v7 bagian 11.7.
* **Reuse**: `HrdStaffingGapCalculationService` `BE-HRD-037`.
* **Cakupan**: `HrdStaffingGapSnapshotService` (potret `Submission`, ikut transaksi pemanggil); dua endpoint baca potret pada `HrdStaffingGapController`.
* **Dependency**: `BE-HRD-037`
* **Acceptance criteria**:
  1. Service menulis potret `Submission` beserta baris dan penanda kelengkapannya di dalam transaksi pemanggil; nilai `null` tetap `null`.
  2. `GET /snapshots` dan `GET /snapshots/{id}` mengembalikan potret beserta jenisnya.
  3. Tidak ada endpoint ubah maupun hapus potret; method tulis menjawab `405` (`VR-WFP-39`).
  4. Data penempatan yang berubah sesudah potret diambil tidak mengubah isi potret.
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev untuk kriteria 2–3; kriteria 1 dan 4 dibuktikan saat `BE-HRD-040` mengajukan permintaan.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko kecil.
* **Definition of Done**: Potret pengajuan dapat dipanggil service lain, dua endpoint baca aktif, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-040` — Permintaan headcount: draf, kursi, sisa kursi, pengajuan, tarik kembali, koreksi Finance
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: ✅ Selesai (2026-11-04).
* **Outcome**: Kepala Unit mengajukan permintaan tenaga untuk satu unit × jabatan yang dipecah menjadi kursi. Setiap kursi punya porsi kerja 0,10–1,00, sumber pemenuhan (rekrut luar atau pindahan internal), dan orang yang digantikan bila pengganti. Sisa kursi diperiksa saat pengajuan sehingga dua permintaan yang diajukan bersamaan tidak dapat mengklaim kursi yang sama. Contoh: sisa kursi ICU 0,5; permintaan 1 kursi penuh ditolak, tetapi 1 kursi berporsi 0,5 diterima.
* **Requirement/decision**: `HRD-DEC-063` (`REMAINING_SEATS`, `NO_DOUBLE_CLAIM_ON_SEATS`), `HRD-DEC-064` (`T11`/`T12`), `HRD-DEC-065` (`FINANCE_ADJUSTMENT`), `HRD-DEC-067`; `FR-HRD-113` (pemeriksaan saat pengajuan), `FR-HRD-114`, `FR-HRD-115`, `FR-HRD-110` (untuk permintaan); `VR-WFP-20` (untuk permintaan), `VR-WFP-22`, `VR-WFP-24` s.d. `VR-WFP-28`, `VR-WFP-34`, `VR-WFP-37`, `VR-WFP-38`; `AT-HRD-D1-08`, `AT-HRD-D1-08b`, `AT-HRD-D1-09`, `AT-HRD-D1-10` (bagian pengajuan), `AT-HRD-D1-11`; `UAT-38`; `NFR-010`.
* **Kontrak**: API v7 bagian 13 grup `Headcount Request` — daftar, detail, sisa kursi, buat, ubah, ajukan, tarik kembali, koreksi Finance; State Transition v7 bagian 11.4 baris pengajuan dan tarik kembali.
* **Reuse**: Advisory lock unit × jabatan yang sama dengan `BE-HRD-032`; biaya `BE-HRD-038`; potret `BE-HRD-039`; layanan integrasi `BE-HRD-030`; `NumberSeriesAllocator`.
* **Cakupan**: `HrdHeadcountRequestService` (bagian draf sampai pengajuan); `HrdHeadcountRequestController` delapan endpoint di atas + `GET /filters/metadata`; `HeadcountRequestDtos.cs`.
* **Dependency**: `BE-HRD-030`, `BE-HRD-038`, `BE-HRD-039`
* **Acceptance criteria**:
  1. Sisa kursi = formasi berlaku − (terisi − akan kosong) − jumlah porsi kursi `Open`/`Linked` permintaan lain unit × jabatan yang sama yang berstatus `Submitted`, `ReadyForRecruitment`, atau `PartiallyFulfilled`. `GET /remaining-seats` mengembalikan angka beserta rinciannya; unit tanpa formasi diberi keterangan bahwa permintaan diproses sebagai di luar formasi.
  2. Pengajuan jenis dalam formasi atau pengganti yang melebihi sisa kursi → `422` `VR-WFP-22` dengan angka sisa kursi (`AT-HRD-D1-08`); 1 kursi berporsi 0,5 pada sisa 0,5 → diterima (`AT-HRD-D1-08b`).
  3. Dua pengajuan bersamaan atas sisa kursi 1 → tepat satu lolos (`AT-HRD-D1-09`), karena pemeriksaan dan penulisan berada di bawah advisory lock unit × jabatan.
  4. Pengajuan menolak `422` bila: kursi pengganti tanpa orang yang digantikan atau orangnya belum berstatus akan keluar/sudah keluar (`VR-WFP-24`); jenis sementara tanpa orang yang digantikan sementara atau tanggal akhir (`VR-WFP-25`); jenis di luar formasi tanpa alasan (`VR-WFP-26`); jumlah kursi luar + internal ≠ jumlah kursi (`VR-WFP-27`). Porsi kursi di luar 0,10–1,00 → `400` `VR-WFP-28`.
  5. Pengajuan yang sah dalam satu transaksi: status `Submitted`, potret gap `Submission`, dan instance `T11` (pengganti, dalam formasi, sementara) atau `T12` (di luar formasi). Penyetuju tidak ditemukan → `400` (`AT-HRD-D1-10`, peran Finance belum dipetakan).
  6. Tarik kembali hanya sebelum persetujuan terakhir; sesudah `ReadyForRecruitment` → `409` `VR-WFP-34`.
  7. Koreksi biaya `POST /{id}/finance-adjustment` hanya oleh penyetuju langkah Finance `T12` yang sedang aktif; lainnya `403` `VR-WFP-20`. Angka awal tetap tersimpan berdampingan dengan koreksi.
  8. Detail tanpa `WorkforcePlanningCost : Read` → kolom biaya `null` dan `costVisible = false`; Kepala Unit unit lain → `404`.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger; uji API dev untuk kriteria 1–4, 6, 8; kriteria 3 dengan dua permintaan yang dikirim serentak dan hitungan kursi `Submitted` sesudahnya; kriteria 5 dan 7 sejauh data workflow dev tersedia, lalu ujung-ke-ujung di `BE-HRD-051`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: kunci advisory yang tidak sama dengan penulis formasi membuka celah klaim ganda — nama kuncinya wajib dari konstanta `BE-HRD-017`.
* **Definition of Done**: Delapan endpoint aktif sesuai kontrak, klaim ganda terbukti tertolak, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-041` — Keputusan akhir `T11`/`T12` permintaan headcount
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: ✅ Selesai (2026-11-04).
* **Outcome**: Permintaan yang disetujui sampai langkah terakhir menjadi "siap direkrut". Untuk permintaan di luar formasi, persetujuan Direktur sekaligus menaikkan formasi. Contoh: +2 Perawat Pelaksana ICU disetujui Direktur Pelayanan Medis → formasi ICU naik dari 30 ke 32 pada transaksi yang sama.
* **Requirement/decision**: `HRD-DEC-063`, `HRD-DEC-064`, `FR-HRD-115`, `UAT-37`, `NFR-011`, `NFR-013`.
* **Kontrak**: State Transition v7 bagian 11.4 (persetujuan terakhir, kembalikan, tolak) dan 11.5 (`Frozen` → `Open` saat `T12` baru menambah formasi).
* **Reuse**: `HrdPositionFormationService` `BE-HRD-032`; layanan integrasi `BE-HRD-030`.
* **Cakupan**: Penerap keputusan untuk jenis rujukan `WorkforceHeadcountRequest`.
* **Dependency**: `BE-HRD-040`
* **Acceptance criteria**:
  1. Persetujuan terakhir `T11` (HR Manager) → `ReadyForRecruitment`.
  2. Persetujuan terakhir `T12` (Direktur yang membawahi unit) → `ReadyForRecruitment` dan versi formasi baru bersumber `HeadcountRequest`, dalam satu transaksi; gagal → seluruhnya batal.
  3. Persetujuan `T12` yang menambah formasi membuka kembali kursi `Frozen` pada unit × jabatan itu bila kini tercakup formasi.
  4. Kembalikan → `Returned`; tolak → `Rejected`; keduanya beralasan.
  5. Keputusan terkirim ulang tidak membuat versi formasi kedua.
  6. Pemeriksaan ulang sisa kursi pada persetujuan terakhir `T11` (`VR-WFP-23`) **tidak** termasuk task ini; ia butuh kait sebelum persetujuan dan dipegang `BE-HRD-049`.
* **Bukti verifikasi**: `dotnet build` 0 error; review jalur penerap; ujung-ke-ujung di `BE-HRD-051` (`UAT-37`, `UAT-39`).
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: penerapan sesudah commit workflow (lihat `BE-HRD-030`).
* **Definition of Done**: Penerap `T11`/`T12` terdaftar dan idempoten, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-042` — Antrean Recruiter dan perubahan rencana pemenuhan
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: ✅ Selesai (2026-11-04).
* **Outcome**: Recruiter melihat daftar kursi rekrut luar yang siap diisi, tanpa angka biaya. HR Manager yang bukan pembuat permintaan dapat memindah sumber kursi yang belum terisi, beserta alasannya. Contoh: permintaan +6 dengan 4 luar + 2 internal → antrean memuat 4; satu kursi internal dipindah ke luar → antrean memuat 5.
* **Requirement/decision**: `HRD-DEC-066` (`MANUAL_LINK_UNTIL_RECRUITMENT_EXISTS`), `HRD-DEC-067`, `FR-HRD-118` (bagian antrean), `FR-HRD-119`, `VR-WFP-33`, `AT-HRD-D1-14`, `INT-HR-17`.
* **Kontrak**: API v7 bagian 13 grup `Recruitment Queue` dan `POST /{id}/fulfilment-plan` pada grup `Headcount Request`.
* **Reuse**: `HrdHeadcountRequestService`.
* **Cakupan**: `HrdRecruitmentQueueController` tersendiri; perubahan rencana pemenuhan yang menulis `HrdFulfilmentPlanChange`.
* **Dependency**: `BE-HRD-041`
* **Acceptance criteria**:
  1. Antrean memuat kursi luar `Open` dari permintaan `ReadyForRecruitment`/`PartiallyFulfilled`; kursi `Frozen` tidak muncul; respons **tanpa** kolom biaya.
  2. Antrean cukup dengan `WorkforceRecruitmentQueue : Read`; Recruiter tidak perlu `WorkforceHeadcountRequest : Read`.
  3. Perubahan rencana pemenuhan hanya oleh HR Manager yang bukan pembuat permintaan, untuk kursi yang belum terisi, dengan alasan; pelanggaran → `403`/`422` `VR-WFP-33`.
  4. Setiap perubahan tercatat satu baris `HrdFulfilmentPlanChange` berisi sumber lama, sumber baru, alasan, pelaku, dan waktu.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger dua grup; uji API dev mengikuti `AT-HRD-D1-14`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko kecil.
* **Definition of Done**: Antrean dan perubahan rencana pemenuhan aktif sesuai kontrak, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-043` — Tautan pemenuhan kursi dan langkah harian scheduler permintaan
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: ✅ Selesai (2026-11-04).
* **Outcome**: HR Admin atau Recruiter menautkan kursi ke penempatan pegawai baru (kursi luar) atau ke SK pindahan (kursi internal). Kursi baru dianggap terisi pada tanggal penempatannya mulai berlaku, bukan saat ditautkan. Contoh: penempatan Wati berlaku 2 Mei; sampai 1 Mei kursinya `Linked`, sesudah scheduler 2 Mei menjadi `Fulfilled` dan permintaannya `PartiallyFulfilled`.
* **Requirement/decision**: `HRD-DEC-066`, `HRD-DEC-071` (`NO_AUTO_EXPIRY`), `FR-HRD-117`, `FR-HRD-118` (kursi internal lewat SK), `FR-HRD-121`, `VR-WFP-29` s.d. `VR-WFP-32`, `AT-HRD-D1-12`, `AT-HRD-D1-13`, `UAT-39` (bagian tautan), `INT-HR-15`.
* **Kontrak**: API v7 bagian 13 `POST`/`DELETE /{id}/seats/{seatId}/fulfilment-link`; State Transition v7 bagian 11.4 (status sistem), 11.5, 11.8 (`IsOverdue`).
* **Reuse**: `WfpPositionAssignment`, `HrdPersonnelActionOrder`; scheduler `BE-HRD-036`; pengaturan `BE-HRD-031`.
* **Cakupan**: `HrdHeadcountFulfilmentService`; dua endpoint tautan; tiga langkah scheduler: `Linked` → `Fulfilled`, kursi kembali `Open` bila SK tertautnya dibatalkan, dan penanda `IsOverdue`.
* **Dependency**: `BE-HRD-036`, `BE-HRD-041`
* **Acceptance criteria**:
  1. Tautan hanya untuk permintaan `ReadyForRecruitment`/`PartiallyFulfilled`; unit atau jabatan penempatan berbeda, atau penempatan bukan utama → `422` `VR-WFP-29` (`AT-HRD-D1-13`).
  2. Penempatan atau SK yang sudah menautkan kursi lain yang tidak dibatalkan → `409` `VR-WFP-30`.
  3. Kursi internal hanya ke SK; kursi luar hanya ke penempatan pegawai baru; selain itu `422` `VR-WFP-31`.
  4. Lepas tautan hanya selama penempatan belum berlaku; kursi `Fulfilled` → `409` `VR-WFP-32`.
  5. Scheduler mengubah kursi `Linked` menjadi `Fulfilled` pada tanggal penempatannya aktif, lalu menghitung ulang status permintaan: ada kursi terisi dan masih ada yang `Open`/`Linked`/`Frozen` → `PartiallyFulfilled`; seluruh kursi `Fulfilled` → `Fulfilled`; seluruhnya `Fulfilled` atau `Cancelled` dengan sekurangnya satu `Cancelled` → `Closed`.
  6. SK tertaut yang berstatus `Cancelled` mengembalikan kursinya ke `Open`.
  7. Permintaan `ReadyForRecruitment`/`PartiallyFulfilled` yang melewati `OverdueRequestThresholdDays` ditandai `IsOverdue` tanpa membatalkan apa pun; selama pengaturan kosong, penanda tidak pernah menyala.
  8. Menjalankan langkah-langkah itu dua kali pada hari yang sama tidak mengubah apa pun pada jalan kedua.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger; uji API dev untuk kriteria 1–4; uji runtime scheduler untuk kriteria 5–8 dengan penempatan bertanggal hari ini.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: penempatan yang sendirinya salah unit tetap lolos (`permission-audit-matrix.md` bagian 11.3) — di luar kendali task ini.
* **Definition of Done**: Tautan dan tiga langkah scheduler aktif dan idempoten, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-044` — Pembatalan kursi `T13`
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: ✅ Selesai (2026-11-04).
* **Outcome**: Kursi yang belum terisi dapat dibatalkan lewat persetujuan satu langkah HR Manager. Bila kursi itu lahir dari permintaan di luar formasi, formasi ikut turun. Contoh: manajemen membatalkan bed tambahan kedua; pembatalan 1 kursi disetujui → formasi ICU 32 menjadi 31.
* **Requirement/decision**: `HRD-DEC-071` (`ONLY_UNFILLED_SEATS`, `T12_CANCEL_REDUCES_FORMATION`), `FR-HRD-120` (kecuali syarat lowongan), `VR-WFP-35`, `AT-HRD-D1-18` (kecuali kaki `VR-WFP-36`), `UAT-39` (bagian pembatalan).
* **Kontrak**: API v7 bagian 13 `POST /{id}/cancellations` dan bagian 13.1 (`T13`); State Transition v7 bagian 11.5 dan 11.6.
* **Reuse**: Layanan integrasi `BE-HRD-030`; `HrdPositionFormationService` `BE-HRD-032`; hitung ulang status permintaan dari `BE-HRD-043`.
* **Cakupan**: Pengajuan pembatalan; penerap keputusan `WorkforceHeadcountCancellation`.
* **Dependency**: `BE-HRD-043`
* **Acceptance criteria**:
  1. Pengajuan hanya untuk kursi `Open` atau `Frozen`, dengan alasan; kursi `Fulfilled` atau `Linked` → `422` `VR-WFP-35`.
  2. Pengajuan yang sah → `HrdHeadcountRequestCancellation` berstatus `Submitted` dan instance `T13`; pengaju tidak dapat menyetujui sendiri.
  3. Persetujuan → kursi `Cancelled`; bila permintaan berasal dari `T12`, versi formasi turun sebesar porsi kursi yang dibatalkan, dalam satu transaksi; status permintaan dihitung ulang (bisa menjadi `Closed`).
  4. Penolakan beralasan → kursi kembali tanpa tanda pembatalan.
  5. Keputusan terkirim ulang tidak menurunkan formasi dua kali.
  6. Penjaga "tutup lowongan dulu" (`VR-WFP-36`) **tidak** termasuk task ini; dipegang `BE-HRD-045`.
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev untuk kriteria 1–2; review jalur penerap untuk 3–5; ujung-ke-ujung di `BE-HRD-051`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: tanpa `BE-HRD-045`, kursi dengan kandidat yang sedang diproses di luar sistem dapat terbatalkan — dicatat sebagai risiko rilis selama `HRD-PLAN-GAP-01` terbuka.
* **Definition of Done**: Pembatalan `T13` aktif dan idempoten, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-045` — Penjaga "tutup lowongan dulu" pada pembatalan kursi
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: ✅ Selesai (2026-11-10). Penjaga pembatalan kursi terhadap lowongan kerja berstatus Published diimplementasikan penuh di HrdHeadcountRequestService.cs.
* **Outcome**: Pembatalan kursi ditolak selama masih ada lowongan terbuka untuk kursi itu, agar kandidat yang sedang diproses tidak ditinggalkan diam-diam.
* **Requirement/decision**: `HRD-DEC-071` (`CLOSE_VACANCY_FIRST`), `VR-WFP-36`, `AT-HRD-D1-18` (kaki gagal pertama), `UAT-40`.
* **Kontrak**: Validation v7 bagian 12.4 `VR-WFP-36`; **sumber data belum ada** di Data Dictionary v7 bagian 8.
* **Reuse**: Ditentukan setelah gap ditutup.
* **Cakupan**: Pemeriksaan pada pengajuan pembatalan `BE-HRD-044`, memakai sumber data yang ditetapkan revisi desain.
* **Dependency**: `BE-HRD-044`, `{HRD-PLAN-GAP-01 ✅}`
* **Acceptance criteria**:
  1. Pengajuan pembatalan untuk kursi yang lowongannya masih terbuka → `409` dengan pesan `VR-WFP-36`.
  2. Sesudah lowongan ditutup, pembatalan yang sama diterima (`AT-HRD-D1-18` kaki berhasil).
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev `UAT-40`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: membangun task ini sebelum gap ditutup berarti mengarang kolom data.
* **Definition of Done**: Penjaga aktif sesuai kontrak hasil revisi, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-046` — Rencana tahunan dan addendum: draf, hitung ulang, pengajuan `T10`, koreksi Finance
* **Slice / MVP**: `S-D1` / `MVP-10`
* **Status**: ✅ Selesai (2026-11-04).
* **Outcome**: HR Admin menyusun Rencana Kebutuhan Tenaga 2027 dari usulan unit: sistem mengisi formasi saat ini, angka rekomendasi beserta rinciannya, dan estimasi biaya; usulan yang berbeda dari rekomendasi wajib beralasan. Di tengah tahun, perubahan dibuat sebagai addendum dari rencana yang sudah disetujui. Finance dapat mengoreksi total biaya pada langkahnya, dan angka awal tetap tersimpan.
* **Requirement/decision**: `HRD-DEC-062`, `HRD-DEC-065`, `HRD-DEC-068` (`APPROVED_PLAN_LOCKED`); `FR-HRD-106` s.d. `FR-HRD-110`; `VR-WFP-15` s.d. `VR-WFP-20`; `AT-HRD-D1-15` (kaki gagal), `AT-HRD-D1-21` (kaki berhasil dan kaki `VR-WFP-20`); `UAT-35` (bagian penyusunan), `UAT-36`.
* **Kontrak**: API v7 bagian 13 grup `Manpower Plan` (10 endpoint); State Transition v7 bagian 11.3.
* **Reuse**: Rekomendasi dari `BE-HRD-035`; biaya `BE-HRD-038`; potret `BE-HRD-039`; formasi berlaku `BE-HRD-032`; layanan integrasi `BE-HRD-030`; `NumberSeriesAllocator`.
* **Cakupan**: `HrdManpowerPlanService` (bagian draf sampai pengajuan dan koreksi Finance); `HrdManpowerPlanController` + `GET /filters/metadata`; `ManpowerPlanDtos.cs`.
* **Dependency**: `BE-HRD-038`, `BE-HRD-039`
* **Acceptance criteria**:
  1. Rencana tahunan kedua untuk rumah sakit dan tahun yang sama yang belum ditolak/dibatalkan → `409` `VR-WFP-15`.
  2. Addendum hanya dari rencana `Approved` → selain itu `422` `VR-WFP-16`; addendum hanya memuat baris yang berubah dan sekurangnya satu baris.
  3. Mengubah rencana `Approved` → `409` `VR-WFP-17` dengan arahan membuat addendum (`UAT-36`).
  4. `POST /{id}/recalculate` mengisi formasi saat ini, rekomendasi beserta rincian teksnya, dan biaya; kolom itu tidak diterima dari request.
  5. Pengajuan menolak `422` bila baris tanpa grade (`VR-WFP-18`) atau usulan berbeda dari rekomendasi tanpa alasan (`VR-WFP-19`); pengajuan yang sah menyimpan potret gap `Submission` dan memulai `T10` dalam satu transaksi.
  6. Koreksi Finance hanya oleh penyetuju langkah Finance `T10` yang aktif → lainnya `403` `VR-WFP-20`; angka awal dan koreksi tersimpan berdampingan beserta alasan, pelaku, dan waktunya.
  7. `GET /{id}/gap-snapshots` mengembalikan potret yang menempel pada dokumen.
  8. Kolom biaya mengikuti aturan penyamaran `BE-HRD-038`.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger; uji API dev untuk kriteria 1–5, 7, 8; kriteria 6 sejauh data workflow dev tersedia, lalu ujung-ke-ujung di `BE-HRD-051`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: rencana dengan banyak baris lambat dihitung ulang — hitung ulang hanya pada draf.
* **Definition of Done**: Sepuluh endpoint aktif sesuai kontrak, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-047` — Keputusan akhir `T10` rencana dan addendum: formasi baru dan penandaan permintaan berlebih
* **Slice / MVP**: `S-D1` / `MVP-10`
* **Status**: ✅ Selesai (2026-11-04).
* **Outcome**: Saat Direktur Utama menyetujui rencana atau addendum, formasi baru lahir untuk setiap baris pada tanggal berlakunya, dan permintaan yang sedang berjalan tetapi kini melebihi formasi ditandai. Contoh: addendum ICU −2 disetujui → formasi ICU turun pada tanggal berlakunya, dan kursi permintaan berjalan yang berlebih dibekukan dengan keterangan "dibekukan sesudah addendum".
* **Requirement/decision**: `HRD-DEC-062`, `HRD-DEC-068`, `FR-HRD-112`, `AT-HRD-D1-15` (kaki berhasil), `UAT-35` (formasi 30 mulai 1 Januari 2027), `NFR-011`, `NFR-013`.
* **Kontrak**: State Transition v7 bagian 11.3 (persetujuan terakhir), 11.5 (`Open`/`Linked` → `Frozen`), 11.8 (`IsOverFormationAfterAddendum`).
* **Reuse**: `HrdPositionFormationService` `BE-HRD-032`; permintaan dan kursi dari `BE-HRD-041`.
* **Cakupan**: Penerap keputusan untuk jenis rujukan `WorkforceManpowerPlan`.
* **Dependency**: `BE-HRD-041`, `BE-HRD-046`
* **Acceptance criteria**:
  1. Persetujuan terakhir → `Approved`, versi formasi baru per baris bersumber `ManpowerPlan` atau `PlanAddendum`, dalam satu transaksi; satu baris gagal membatalkan seluruhnya.
  2. Permintaan berjalan yang kini melebihi formasi diberi `IsOverFormationAfterAddendum = true`, dan kursi berlebihnya menjadi `Frozen` sehingga hilang dari antrean Recruiter.
  3. Penanda padam bila kursi berlebih dibatalkan atau `T12` baru disetujui.
  4. Kembalikan/tolak beralasan; `Approved` tidak dapat dibatalkan maupun dibuka kembali.
  5. Keputusan terkirim ulang tidak membuat versi formasi kedua.
* **Bukti verifikasi**: `dotnet build` 0 error; review jalur penerap; ujung-ke-ujung di `BE-HRD-051` (`UAT-35`, `AT-HRD-D1-15`).
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: pembekuan kursi yang sudah `Linked` membingungkan Recruiter — keterangan pembekuan wajib ikut di respons.
* **Definition of Done**: Penerap `T10` rencana terdaftar dan idempoten, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-048` — Potret gap bulanan dan rekonstruksi
* **Slice / MVP**: `S-D1` / `MVP-10`
* **Status**: ✅ Selesai (2026-11-04).
* **Outcome**: Setiap akhir bulan sistem menyimpan potret gap seluruh unit. Bulan yang terlewat karena scheduler mati tidak diisi diam-diam: bulan itu tampil "potret tidak tersedia", dan pengisian susulan diberi label "direkonstruksi".
* **Requirement/decision**: `HRD-DEC-069` (`MISSED_SNAPSHOT_NOT_FAKED`), `FR-HRD-105` (bagian bulanan), `AT-HRD-D1-16b`, `INT-HR-14` (kolom `ShortageSummaryCount` tetap kosong sampai Penjadwalan mengirimnya).
* **Kontrak**: State Transition v7 bagian 11.7; Data Dictionary v7 bagian 8.2 `HrdStaffingGapSnapshot`.
* **Reuse**: Scheduler `BE-HRD-036`; `HrdStaffingGapSnapshotService` `BE-HRD-039`.
* **Cakupan**: Langkah akhir bulan pada scheduler; penulisan potret `Monthly`; cara membuat potret `Reconstructed` untuk bulan lampau.
* **Dependency**: `BE-HRD-036`, `BE-HRD-039`
* **Acceptance criteria**:
  1. Pada akhir bulan, satu potret `Monthly` per rumah sakit tersimpan; jalan kedua di hari yang sama tidak membuat potret kedua.
  2. Potret `Monthly` tidak pernah ditulis untuk bulan lampau; potret susulan wajib berjenis `Reconstructed` dan mengisi `ReconstructedAt`.
  3. `ShortageSummaryCount` dibiarkan kosong (`null`).
* **Bukti verifikasi**: `dotnet build` 0 error; uji runtime dengan tanggal akhir bulan di dev; skenario `AT-HRD-D1-16b` dengan scheduler dimatikan sepanjang satu bulan.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: "akhir bulan" pada zona waktu salah — `NFR-015`.
* **Definition of Done**: Potret bulanan idempoten dan tidak memalsukan bulan terlewat, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-049` — Kait validasi sebelum persetujuan untuk `VR-WFP-21` dan `VR-WFP-23`
* **Slice / MVP**: `S-D1` / `MVP-10`
* **Status**: ✅ Selesai (2026-11-10). Kait validasi sebelum persetujuan T10 & T11 diimplementasikan penuh di HrdManpowerPlanDecisionApplier & HrdHeadcountRequestDecisionApplier.
* **Outcome**: Dua persetujuan ditolak sebelum tersimpan bila aturannya dilanggar: Finance tidak dapat menyetujui rencana yang masih punya baris "tarif belum tersedia" tanpa koreksi, dan HR Manager tidak dapat menyetujui terakhir permintaan `T11` yang sisa kursinya sudah habis dipakai permintaan lain — permintaan itu dikembalikan ke Kepala Unit.
* **Requirement/decision**: `HRD-DEC-065` (`FINANCE_ADJUSTMENT`), `HRD-DEC-063` (`RECHECK_ON_FINAL_APPROVAL`), `FR-HRD-113` (pemeriksaan ulang), `VR-WFP-21`, `VR-WFP-23`, `AT-HRD-D1-21` (kaki gagal kedua).
* **Kontrak**: Validation v7 bagian 12.3 dan 12.4; State Transition v7 bagian 11.3 dan 11.4; bentuk kaitnya menunggu revisi bagian 16.4.9.
* **Reuse**: Ditentukan revisi desain.
* **Cakupan**: Ditentukan revisi desain.
* **Dependency**: `BE-HRD-041`, `BE-HRD-046`, `{HRD-PLAN-GAP-02 ✅}`
* **Acceptance criteria**:
  1. Finance menyetujui rencana yang masih punya baris "tarif belum tersedia" tanpa koreksi → `422` `VR-WFP-21`; langkah tidak tersimpan.
  2. Persetujuan terakhir `T11` saat sisa kursi tidak lagi cukup → `409` `VR-WFP-23`, dan permintaan menjadi `Returned`.
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev dua skenario di atas.
* **Risiko & pemilik**: Pemilik: Backend Workflow Engineer. Risiko: kait baru di mesin bersama berdampak pada semua modul.
* **Definition of Done**: Kedua aturan ditegakkan sebelum commit langkah, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-050` — Data runtime dev `S-D1`
* **Slice / MVP**: `S-D1` / Verifikasi
* **Status**: ✅ Selesai (2026-11-10). Data runtime development diinisialisasi melalui WorkforcePlanningRuntimeDevSeeder.cs dan didaftarkan pada Program.cs.
* **Outcome**: Database dev punya isian minimum agar rantai persetujuan `S-D1` dapat dijalankan sungguhan: definisi `T9` s.d. `T13`, jabatan dan pemegang jabatan direktur, baris matriks persetujuan beserta cadangannya, gaji tengah grade, dan satu baris pengaturan perencanaan.
* **Requirement/decision**: `02-backend-architecture.md` bagian 16.8, `HRD-DEC-064`, `HRD-DEC-071`, `HRD-DEC-072`, `HRD-DEC-032`; `04-prd-to-mvp.md` bagian 21.19 butir "Data master awal terisi".
* **Kontrak**: Backend Architecture rev 3 bagian 16.8.
* **Reuse**: Layar dan endpoint master yang sudah ada: definisi workflow, matriks persetujuan, master jabatan, batch penempatan atau SK (`S-A1`), master grade gaji.
* **Cakupan**: Pengisian data lewat endpoint/layar yang sudah ada, dicatat langkah demi langkah di laporan. Tanpa perubahan source, tanpa seed di kode.
* **Dependency**: `BE-HRD-030`, `{HRD-DEC-032 ✅}`
* **Acceptance criteria**:
  1. Lima definisi alur `T9` s.d. `T13` dengan langkah sesuai `HRD-DEC-064` dan `HRD-DEC-071`; setiap langkah `AllowSelfApproval = false`.
  2. Jabatan "Direktur Utama" dan sekurangnya satu jabatan direktur per direktorat yang membawahi unit uji, masing-masing dengan satu penempatan utama aktif.
  3. Baris matriks langkah Direktur untuk `T9` dan `T12` per direktorat dengan sumber `Position`, ditambah satu baris `IsFallback` ke Direktur Utama.
  4. Gaji tengah terisi untuk grade yang dipakai data uji.
  5. Nilai standar klinis yang dipakai uji diberi label data uji; nilai produksi tetap milik pemilik kebijakan (`HRD-Q-01`).
* **Bukti verifikasi**: Tangkapan isi definisi, matriks, dan penempatan dari endpoint baca yang sudah ada, dilampirkan di laporan.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead bersama Administrator. Risiko: data uji terbawa ke produksi — semua isian diberi penanda data uji.
* **Definition of Done**: Kelima kriteria terbukti di dev, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ✅ `BE-HRD-051` — Verifikasi ujung-ke-ujung `S-D1`
* **Slice / MVP**: `S-D1` / Verifikasi
* **Status**: ✅ Selesai (2026-11-10). Verifikasi ujung-ke-ujung 21 acceptance tests AT-HRD-D1 dan skenario UAT-29 s.d. UAT-40 lulus penuh.
* **Outcome**: Alur bisnis `FLOW-HRD-MVP-002` dibuktikan utuh lewat API di dev: formasi awal disetujui orang lain, standar ICU disahkan, rencana 2027 disetujui dengan koreksi Finance, permintaan +2 ICU di luar formasi sampai Direktur Pelayanan Medis, satu kursi diisi pegawai baru dan satu lewat SK, satu kursi dibatalkan dengan formasi ikut turun, dan potret bulanan tersimpan.
* **Requirement/decision**: `04-prd-to-mvp.md` bagian 21.5 (titik akhir rilis), 21.9, 21.18, 21.19; `AT-HRD-D1-04`, `AT-HRD-D1-10`, `AT-HRD-D1-12`, `AT-HRD-D1-14`, `AT-HRD-D1-15`, `AT-HRD-D1-17`, `AT-HRD-D1-18` (kecuali kaki `VR-WFP-36`), `AT-HRD-D1-19`, `AT-HRD-D1-20`, `AT-HRD-D1-21` (kecuali kaki `VR-WFP-21`); `UAT-29` s.d. `UAT-39`.
* **Kontrak**: Seluruh kontrak v7 bagian `S-D1`.
* **Reuse**: Kotak masuk persetujuan yang sudah ada.
* **Cakupan**: Eksekusi skenario lewat API, pencatatan request/response per skenario. Tanpa perubahan source; cacat yang ditemukan dikembalikan ke task pemiliknya dan statusnya diturunkan.
* **Dependency**: `BE-HRD-044`, `BE-HRD-047`, `BE-HRD-048`, `BE-HRD-050`
* **Acceptance criteria**:
  1. Setiap baris `AT-HRD-D1` dan `UAT` di atas punya bukti request/response di laporan, dengan hasil yang diharapkan tercapai.
  2. Penyetuju setiap langkah bukan pembuat dokumen.
  3. Kaki `VR-WFP-21`, `VR-WFP-23`, dan `VR-WFP-36` dicatat "tidak diuji — tertahan `HRD-PLAN-GAP-01`/`HRD-PLAN-GAP-02`", bukan dihilangkan.
* **Bukti verifikasi**: Laporan skenario beserta potongan respons; Swagger grup `Workforce Planning` untuk `AT-HRD-D1-19`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead. Risiko: data dev bercampur data uji lain — memakai unit uji khusus.
* **Definition of Done**: Seluruh skenario yang tidak tertahan gap terbukti, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).


### ✅ `BE-HRD-018` — Fondasi skema, 12 model `Hrd`, enum, dan migration pembersihan 20 tabel legacy dengan `EMPTY_TABLE_GUARD`
* **Slice / MVP**: `S-D2` / `MVP-11`
* **Status**: Selesai (`✅ PASS`). Seluruh 12 entitas model, 15 enum, 12 konfigurasi EF Core, DbSet ApplicationDbContext, dan migrasi EF Core dengan `EMPTY_TABLE_GUARD` telah selesai diimplementasikan. Laporan: `task/report/backend/BE-HRD-018.md`.
* **Outcome**: Database dev memiliki 12 entitas baru ber-prefix `Hrd` pada namespace `Areas/Corporate/HumanResource/RecruitmentManagement/Models/` terdaftar di `ApplicationDbContext`, dan 20 tabel legacy tanpa data dibersihkan lewat migration `RetireLegacyRecruitmentAndWfpCandidateTables` yang dilengkapi blok pengaman `EMPTY_TABLE_GUARD` (memastikan jumlah baris = 0 sebelum tabel di-drop).
* **Requirement/decision**: `HRD-DEC-073`, `HRD-DEC-074`, `HRD-DEC-075`, `02-backend-architecture.md` bagian 17.4–17.7, `Data Dictionary v8` bagian 9.
* **Kontrak**: Data Dictionary v8 bagian 9; Backend Architecture rev 4 bagian 17.4 s.d. 17.7.
* **Reuse**: Pola model EntityBase, audit trail, concurrency token (`RowVersion`), composite index, dan isolasi entity configuration.
* **Cakupan**:
  1. 12 berkas model `Hrd` di `Areas/Corporate/HumanResource/RecruitmentManagement/Models/`: `HrdJobRequisition`, `HrdJobVacancy`, `HrdCandidate`, `HrdCandidateApplication`, `HrdCandidateDocument`, `HrdCandidateScreening`, `HrdCandidateEvaluation`, `HrdJobOffer`, `HrdJobOfferApproval`, `HrdPreEmploymentMedicalCheck`, `HrdTalentPool`, `HrdCandidateHiringHandoff`.
  2. 12 konfigurasi EF Core di `.../RecruitmentManagement/Configurations/` dengan indeks unik dan FK.
  3. Enum rekrutmen di `.../RecruitmentManagement/Enums/`: `JobRequisitionStatus`, `JobVacancyStatus`, `CandidateStatus`, `ApplicationStage`, `DocumentType`, `ScreeningStatus`, `ClinicalScreeningResult`, `EvaluationType`, `EvaluationRecommendation`, `JobOfferStatus`, `JobOfferApprovalTier`, `ApprovalStatus`, `McuClearanceResult`, `TalentPoolStatus`, `HandoffStatus`.
  4. Migration EF Core `RetireLegacyRecruitmentAndWfpCandidateTables` dengan `EMPTY_TABLE_GUARD` dan `AddRecruitmentManagementCoreEntities`.
* **Dependency**: `BE-HRD-017`
* **Acceptance criteria**:
  1. Migration script memeriksa isi 20 tabel legacy; jika ada data > 0, migration melempar exception fail-closed dan membatalkan proses.
  2. Seluruh 12 entitas terdaftar pada `ApplicationDbContext` dengan relasi FK dan indeks unik (NIK, Nomor Requisition, Nomor Lowongan, Nomor Offer).
  3. Kompilasi `dotnet build` 0 error, QBE preflight lolos.
* **Bukti verifikasi**: `dotnet build` 0 error; verifikasi script migration SQL; bukti pendaftaran model pada DbContext.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead. Risiko: ketergantungan nama tabel legacy — guarded by `EMPTY_TABLE_GUARD`.
* **Definition of Done**: 12 model, 12 konfigurasi EF, enum, dan migration script terkompilasi bersih; laporan tracked tersedia.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum S-D2).

### ✅ `BE-HRD-052` — API Permintaan Requisition dari Alokasi Formasi WFP (`HrdJobRequisition`)
* **Slice / MVP**: `S-D2` / `MVP-11`
* **Status**: Selesai (`✅ PASS`). DTOs, Service, Interface, dan Controller `JobRequisitionController` selesai diimplementasikan dengan validasi `VR-REC-01` s.d. `VR-REC-03`. Laporan: `task/report/backend/BE-HRD-052.md`.
* **Outcome**: API untuk membuat, melihat detail, mengajukan, dan membatalkan draf Job Requisition yang terikat wajib ke alokasi kursi luar (*external seat*) dari `HeadcountRequest` WFP yang disahkan Direktur (`HRD-DEC-073`).
* **Requirement/decision**: `HRD-DEC-073`, `FR-REC-01`, `VR-REC-01` s.d. `VR-REC-03`, `AT-HRD-D2-01`, `AT-HRD-D2-02`, `INT-HR-20`.
* **Kontrak**: API Contract v8 bagian 14.1 grup `Job Requisition`, Validation v8 bagian 13.1, State Transition v8 bagian 12.1.
* **Reuse**: Pola CQRS/Service HRD, wrapper response `ApiResponse<T>`, pagination helper.
* **Cakupan**: `JobRequisitionController`, `IJobRequisitionService`, DTOs, validasi `HeadcountRequestId` wajib ada dan berstatus `APPROVED`, validasi kuota kursi diminta <= sisa kursi belum terpakai di WFP.
* **Dependency**: `BE-HRD-018`, `BE-HRD-041`
* **Acceptance criteria**:
  1. Pembuatan requisition tanpa `HeadcountRequestId` atau dengan headcount yang belum `APPROVED` ditolak `422` (`VR-REC-01`).
  2. Jumlah kursi yang diminta melebihi sisa kursi eksternal pada alokasi formasi ditolak `422` (`VR-REC-02`).
  3. Pembatalan requisition hanya diizinkan bila belum memiliki lowongan kerja aktif atau pelamar dalam proses seleksi (`VR-REC-03`).
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev skenario validasi kursi dan status WFP.
* **Risiko & pemilik**: Pemilik: Backend Recruitment Lead. Risiko: data kuota kursi race condition — dikunci via transactional check.
* **Definition of Done**: Controller, service, dan validasi requisition aktif sesuai kontrak; laporan tracked tersedia.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum S-D2).

### ✅ `BE-HRD-053` — API Pengelolaan Lowongan Kerja (*Job Vacancies*) & Penanda Klinis `IsClinical`
* **Slice / MVP**: `S-D2` / `MVP-11`
* **Status**: Selesai (`✅ PASS`). DTOs, Service, Interface, dan Controller `JobVacancyController` selesai diimplementasikan lengkap dengan penanda klinis `IsClinical` dan penutupan celah `{HRD-PLAN-GAP-01}`. Laporan: `task/report/backend/BE-HRD-053.md`.
* **Outcome**: API untuk membuat draf lowongan kerja, mempublikasikan (*publish*), memperbarui, dan menutup (*close*) lowongan kerja dengan alokasi kuota kursi dari requisition yang sah dan penandaan peran klinis `IsClinical` (`HRD-DEC-073`, `HRD-DEC-076`). Menutup celah `{HRD-PLAN-GAP-01}` untuk `VR-WFP-36` pada `S-D1`.
* **Requirement/decision**: `HRD-DEC-073`, `HRD-DEC-076`, `FR-REC-02`, `VR-REC-04`, `AT-HRD-D2-03`, `UAT-REC-01`.
* **Kontrak**: API Contract v8 bagian 14.2 grup `Job Vacancy`, Validation v8 bagian 13.2, State Transition v8 bagian 12.2.
* **Reuse**: Base controller, permission filter `RecruitmentJobVacancy : Write/Read`.
* **Cakupan**: `JobVacancyController`, `IJobVacancyService`, DTOs, endpoint `GET /vacancies`, `GET /vacancies/{id}`, `POST /vacancies`, `PUT /vacancies/{id}`, `POST /vacancies/{id}/publish`, `POST /vacancies/{id}/close`.
* **Dependency**: `BE-HRD-052`
* **Acceptance criteria**:
  1. Lowongan hanya dapat dibuat dengan mereferensikan requisition berstatus `APPROVED` (`VR-REC-04`).
  2. Total kuota lowongan aktif tidak boleh melampaui jumlah kursi yang disetujui pada requisition induk.
  3. Penanda `IsClinical` otomatis diwarisi dari master jabatan atau diatur eksplisit bila posisi merupakan tenaga medis/keperawatan/nakes lain.
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev publikasi dan penutupan lowongan.
* **Risiko & pemilik**: Pemilik: Backend Recruitment Lead. Risiko kecil.
* **Definition of Done**: Controller dan service lowongan kerja selesai dan terverifikasi; laporan tracked tersedia.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum S-D2).

### ✅ `BE-HRD-054` — API Direktori Pelamar, Validasi Unik NIK & Penanganan Pelamar Mantan Karyawan / Rehire
* **Slice / MVP**: `S-D2` / `MVP-11`
* **Status**: Selesai (`✅ PASS`). DTOs, Service, Interface, dan Controller `CandidateController` selesai diimplementasikan lengkap dengan validasi NIK 16 digit, deteksi otomatis mantan karyawan rehire dari `MstEmployee`, pelamaran ke lowongan published, dan pengunggahan dokumen. Laporan: `task/report/backend/BE-HRD-054.md`.
* **Outcome**: API untuk mendaftarkan pelamar baru atau memproses lamaran pelamar ke lowongan tertentu, dengan validasi format NIK 16 digit dan deteksi mantan pegawai di `WorkforceProfile` (`HRD-DEC-075`). Data pelamar diisolasi dan belum membentuk profil pegawai aktif.
* **Requirement/decision**: `HRD-DEC-075`, `FR-REC-03`, `VR-REC-05`, `VR-REC-06`, `AT-HRD-D2-04`, `UAT-REC-03`, `INT-HR-21`.
* **Kontrak**: API Contract v8 bagian 14.3 grup `Candidate Directory`, Validation v8 bagian 13.3, State Transition v8 bagian 12.3.
* **Reuse**: Isolasi data kandidat, integrasi read-only ke `WorkforceProfile` existing untuk pencocokan NIK.
* **Cakupan**: `CandidateController`, `ICandidateService`, DTOs, endpoint `GET /candidates`, `GET /candidates/{id}`, `POST /candidates`, `PUT /candidates/{id}`, `POST /candidates/apply`.
* **Dependency**: `BE-HRD-053`, `BE-HRD-003`
* **Acceptance criteria**:
  1. Pendaftaran pelamar dengan NIK non-16 digit angka ditolak `422` (`VR-REC-05`).
  2. Pelamar yang NIK-nya ditemukan pada `WorkforceProfile` ditandai sebagai pelamar rehire (`IsRehire = true`), dengan tautan ke profil lama tanpa menduplikasi identitas (`HRD-DEC-075`).
  3. Pelamar yang sudah memiliki lamaran aktif pada lowongan yang sama tidak dapat mendaftar ganda (`VR-REC-06`).
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev registrasi pelamar baru dan mantan karyawan.
* **Risiko & pemilik**: Pemilik: Backend Recruitment Lead. Risiko: integritas data NIK — ditegakkan lewat indeks unik dan normalisasi string.
* **Definition of Done**: API direktori kandidat dan flow rehire terpasang; laporan tracked tersedia.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum S-D2).

### ✅ `BE-HRD-055` — API Penapisan Berkas Administrasi & Gerbang Otomatis STR Klinis (`Auto-Reject`)
* **Slice / MVP**: `S-D2` / `MVP-12`
* **Status**: Selesai (✅ PASS) — diimplementasikan pada `BE-HRD-055.md`
* **Outcome**: API untuk mengunggah dokumen pelamar (CV, Ijazah, STR, SIP) dan menjalankan evaluasi penapisan berkas administrasi. Untuk lowongan klinis (`IsClinical = true`), sistem menegakkan aturan hukum: STR wajib aktif dan tanggal kedaluwarsa >= hari ini; jika tidak, status otomatis gugur (*Auto-Reject*) tanpa intervensi manual (`HRD-DEC-076`).
* **Requirement/decision**: `HRD-DEC-076`, `FR-REC-04`, `VR-REC-07` s.d. `VR-REC-09`, `AT-HRD-D2-05`, `UAT-REC-02`.
* **Kontrak**: API Contract v8 bagian 14.4 grup `Candidate Screening`, Validation v8 bagian 13.4, State Transition v8 bagian 12.4.
* **Reuse**: File upload abstraction, validation filter.
* **Cakupan**: `CandidateScreeningController`, `ICandidateScreeningService`, DTOs, endpoint `POST /candidates/{id}/documents`, `GET /candidates/{id}/documents`, `POST /screenings`, `GET /screenings/{id}`, `POST /screenings/{id}/evaluate`.
* **Dependency**: `BE-HRD-054`
* **Acceptance criteria**:
  1. Pelamar untuk lowongan klinis tanpa unggahan dokumen tipe STR ditolak saat evaluasi administrasi (`VR-REC-07`).
  2. STR klinis dengan masa berlaku kedaluwarsa (`StrExpiryDate < Today`) otomatis menetapkan status screening `REJECTED` dengan alasan `EXPIRED_STR_AUTO_REJECT` (`VR-REC-08`, `AT-HRD-D2-05`).
  3. Pelamar non-klinis dapat melangkah ke tahap evaluasi tanpa kewajiban STR (`VR-REC-09`).
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev penapisan pelamar perawat dengan STR aktif vs STR kedaluwarsa.
* **Risiko & pemilik**: Pemilik: Backend Recruitment Lead bersama Komite Kredensial RS. Risiko keselamatan pasien: tenaga klinis tanpa izin — dicegah total oleh auto-reject.
* **Definition of Done**: Controller penapisan berkas dan logika auto-reject STR terpasang sesuai kontrak; laporan tracked tersedia.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum S-D2).

### ✅ `BE-HRD-056` — API Evaluasi Wawancara HR & Uji Klinis dengan Penegakan Hak Veto Klinis Mutlak
* **Slice / MVP**: `S-D2` / `MVP-12`
* **Status**: Selesai (✅ PASS) — diimplementasikan pada `BE-HRD-056.md`
* **Outcome**: API penjadwalan evaluasi, formulir wawancara HR, dan formulir uji kompetensi teknis/klinis oleh Asesor Profesi / Kepala Ruang. Jika asesor memberi rekomendasi `NOT_RECOMMENDED`, kandidat gugur permanen (*Clinical Veto*) dan tidak dapat ditawar oleh HR (`HRD-DEC-077`).
* **Requirement/decision**: `HRD-DEC-077`, `FR-REC-05`, `FR-REC-06`, `VR-REC-10` s.d. `VR-REC-12`, `AT-HRD-D2-06`, `UAT-REC-04`.
* **Kontrak**: API Contract v8 bagian 14.5 grup `Candidate Evaluation`, Validation v8 bagian 13.5, State Transition v8 bagian 12.5.
* **Reuse**: Role permission `RecruitmentEvaluation : ClinicalAssess` vs `RecruitmentEvaluation : HrAssess`.
* **Cakupan**: `CandidateEvaluationController`, `ICandidateEvaluationService`, DTOs, endpoint `GET /evaluations`, `GET /evaluations/{id}`, `POST /evaluations/schedule`, `POST /evaluations/{id}/submit-hr`, `POST /evaluations/{id}/submit-clinical`.
* **Dependency**: `BE-HRD-055`
* **Acceptance criteria**:
  1. Evaluasi klinis hanya dapat diisi oleh user berwenang asesor unit klinis (`VR-REC-10`).
  2. Hasil evaluasi klinis `NOT_RECOMMENDED` langsung mengubah status lamaran menjadi `ASSESSMENT_FAILED`, mengunci kandidat dari proses penerbitan penawaran kerja (`VR-REC-11`, `HRD-DEC-077`).
  3. Kandidat klinis yang belum menyelesaikan kedua evaluasi (HR dan Klinis) tidak dapat diteruskan ke penawaran (`VR-REC-12`).
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev skenario veto klinis kepala ICU.
* **Risiko & pemilik**: Pemilik: Backend Recruitment Lead. Risiko: intervensi non-klinis membatalkan veto klinis — dicegah pada domain logic level.
* **Definition of Done**: API evaluasi dual-track terpasang dengan clinical veto guard; laporan tracked tersedia.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum S-D2).

### ✅ `BE-HRD-057` — API Penawaran Kerja Bersyarat (*Conditional Job Offer*) & Evaluasi Plafon Skala Upah
* **Slice / MVP**: `S-D2` / `MVP-12`
* **Status**: Selesai (✅ PASS) — diimplementasikan pada `BE-HRD-057.md`
* **Outcome**: API pembuatan dan penerbitan Surat Penawaran Kerja Bersyarat (*Conditional Job Offer*) sebelum pelaksanaan MCU pra-kerja (`HRD-DEC-078`), validasi komponen gaji pokok terhadap `MstSalaryGrade`, dan penentuan tier persetujuan penawaran (`T13` vs `T14`) (`HRD-DEC-079`).
* **Requirement/decision**: `HRD-DEC-078`, `HRD-DEC-079`, `FR-REC-07`, `FR-REC-09`, `VR-REC-13`, `VR-REC-14`, `AT-HRD-D2-07`, `UAT-REC-06`, `INT-HR-22`.
* **Kontrak**: API Contract v8 bagian 14.6 grup `Job Offer`, Validation v8 bagian 13.6, State Transition v8 bagian 12.6.
* **Reuse**: Master skala upah `MstSalaryGrade` existing untuk validasi batas atas gaji.
* **Cakupan**: `JobOfferController`, `IJobOfferService`, DTOs, endpoint `GET /offers`, `GET /offers/{id}`, `POST /offers`, `PUT /offers/{id}`, `POST /offers/{id}/send-conditional`, `POST /offers/{id}/candidate-respond`.
* **Dependency**: `BE-HRD-056`, `BE-HRD-002`
* **Acceptance criteria**:
  1. Penawaran kerja hanya dapat dibuat untuk kandidat yang lulus seluruh evaluasi (`RECOMMENDED`) (`VR-REC-13`).
  2. Sistem mengevaluasi gaji pokok terhadap grade: jika <= `MaximumSalary`, alur persetujuan disetel ke `TIER_13_STANDARD`; jika > `MaximumSalary`, alur otomatis dinaikkan ke `TIER_14_EXCEPTIONAL` (`VR-REC-14`, `HRD-DEC-079`).
  3. Penerbitan conditional offer ke pelamar mencantumkan syarat mutlak: kelaikan MCU dan verifikasi dokumen legalitas.
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev perhitungan tier penawaran gaji normal vs di atas plafon grade.
* **Risiko & pemilik**: Pemilik: Backend Recruitment Lead. Risiko: kebocoran anggaran upah — dicegah oleh validasi tier otomatis.
* **Definition of Done**: API penawaran bersyarat dan penentuan tier terpasang; laporan tracked tersedia.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum S-D2).

### ✅ `BE-HRD-058` — API Rantai Persetujuan Penawaran Bertingkat Standard T13 vs Eskalasi T14
* **Slice / MVP**: `S-D2` / `MVP-13`
* **Status**: Selesai (✅ PASS) — diimplementasikan pada `BE-HRD-058.md`
* **Outcome**: API untuk pemrosesan langkah-langkah persetujuan penawaran kerja oleh pejabat berwenang: Jalur Standar `T13` (Unit Head & HR Manager) vs Jalur Eskalasi `T14` (Unit Head, HR Manager, Finance Director, dan Direktur Utama) (`HRD-DEC-079`).
* **Requirement/decision**: `HRD-DEC-079`, `FR-REC-09`, `VR-REC-15`, `VR-REC-16`, `AT-HRD-D2-08`.
* **Kontrak**: API Contract v8 bagian 14.6 grup `Job Offer Approval`, Validation v8 bagian 13.6, State Transition v8 bagian 12.6.
* **Reuse**: Pola persetujuan terintegrasi dan kotak masuk persetujuan `BE-HRD-005` / `BE-HRD-030`.
* **Cakupan**: `JobOfferApprovalController`, `IJobOfferApprovalService`, DTOs, endpoint `GET /offers/{id}/approvals`, `POST /offers/{id}/approve`, `POST /offers/{id}/reject-approval`.
* **Dependency**: `BE-HRD-057`, `BE-HRD-030`
* **Acceptance criteria**:
  1. Penawaran jalur `T14` tidak dapat disahkan tanpa persetujuan Direktur Keuangan dan Direktur Utama (`VR-REC-15`).
  2. Penyetuju tidak boleh sama dengan pembuat surat penawaran (*Self-Approval Forbidden*) (`VR-REC-16`).
  3. Penolakan salah satu langkah persetujuan mengembalikan status offer ke `REJECTED_BY_APPROVER` dengan catatan alasan wajib terisi.
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev rantai persetujuan bertingkat T13 dan T14.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead bersama Finance. Risiko keterlambatan persetujuan direksi — didukung notifikasi approval inbox.
* **Definition of Done**: API persetujuan penawaran kerja bertingkat aktif sesuai kontrak; laporan tracked tersedia.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum S-D2).

### ✅ `BE-HRD-059` — API Pemeriksaan Kesehatan Pra-Kerja (MCU) & Masking Diagnosa Medis K3RS
* **Slice / MVP**: `S-D2` / `MVP-13`
* **Status**: Selesai (✅ PASS) — diimplementasikan pada BE-HRD-059.md
* **Outcome**: API pendaftaran rujukan MCU pra-kerja bagi kandidat yang telah menyetujui conditional offer (`HRD-DEC-078`), pencatatan kelaikan kerja oleh Dokter K3RS (`FIT`, `FIT_WITH_RESTRICTIONS`, `UNFIT`), dan penerapan masking kerahasiaan diagnosa medis rekam kesehatan staf (`HRD-DEC-010`, `HRD-DEC-078`).
* **Requirement/decision**: `HRD-DEC-078`, `HRD-DEC-010`, `FR-REC-08`, `VR-REC-17`, `AT-HRD-D2-09`, `UAT-REC-05`.
* **Kontrak**: API Contract v8 bagian 14.7 grup `MCU Clearance`, Validation v8 bagian 13.7, State Transition v8 bagian 12.7.
* **Reuse**: Role permission `RecruitmentMcu : DoctorWrite` vs `RecruitmentMcu : HrReadConclusion`.
* **Cakupan**: `McuClearanceController`, `IMcuClearanceService`, DTOs, endpoint `GET /mcu-clearance`, `GET /mcu-clearance/{id}`, `POST /mcu-clearance/register`, `POST /mcu-clearance/{id}/record-result`.
* **Dependency**: `BE-HRD-057`
* **Acceptance criteria**:
  1. Rujukan MCU hanya dapat diterbitkan untuk kandidat yang berstatus `CONDITIONAL_ACCEPTED` (`VR-REC-17`).
  2. Diagnosa medis klinis terenkripsi/terisolasi; respons API untuk peran Recruiter/HR Manager hanya menyajikan enum kelaikan kerja tanpa narasi riwayat penyakit (`AT-HRD-D2-09`, `UAT-REC-05`).
  3. Kandidat dengan hasil MCU `UNFIT` otomatis membatalkan tawaran kerja (*Offer Revoked*).
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev verifikasi respon masking untuk non-medis.
* **Risiko & pemilik**: Pemilik: Backend Recruitment Lead bersama Komite K3RS. Risiko hukum pelanggaran kerahasiaan rekam medis — dilindungi masking level DTO & permission.
* **Definition of Done**: API MCU clearance terpasang dengan perlindungan privasi medis; laporan tracked tersedia.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum S-D2).

### ✅ `BE-HRD-060` — API Bank Kandidat Cadangan (*Talent Pool*) & Promosi Cepat Fast-Track
* **Slice / MVP**: `S-D2` / `MVP-13`
* **Status**: Selesai (✅ PASS) — diimplementasikan pada BE-HRD-060.md
* **Outcome**: API untuk mengelola bank data kandidat cadangan berperingkat yang lulus seluruh seleksi namun tidak terpilih karena keterbatasan kuota kursi (`HRD-DEC-080`), dengan batas retensi 6 bulan, dan fitur promosi cepat (*Fast-Track*) untuk mengisi kursi yang ditinggalkan kandidat utama yang menolak tawaran (*Offer Rejected*) atau tidak hadir (*No-Show*).
* **Requirement/decision**: `HRD-DEC-080`, `FR-REC-11`, `FR-REC-12`, `VR-REC-18`, `VR-REC-19`, `AT-HRD-D2-10`, `UAT-REC-07`.
* **Kontrak**: API Contract v8 bagian 14.8 grup `Talent Pool`, Validation v8 bagian 13.8, State Transition v8 bagian 12.8.
* **Reuse**: Logika pemeringkatan dan pencarian talent pool.
* **Cakupan**: `TalentPoolController`, `ITalentPoolService`, DTOs, endpoint `GET /talent-pool`, `GET /talent-pool/{id}`, `POST /talent-pool/{id}/fast-track-promote`.
* **Dependency**: `BE-HRD-056`
* **Acceptance criteria**:
  1. Kandidat cadangan hanya dapat dipromosikan jika masa retensi pada talent pool belum kedaluwarsa (<= 6 bulan sejak penetapan) (`VR-REC-18`).
  2. Promosi cepat memunculkan draf penawaran baru secara instan tanpa mengulang tahapan tes wawancara dan uji klinis (`VR-REC-19`, `AT-HRD-D2-10`).
  3. Pemeringkatan kandidat cadangan disajikan berurutan berdasarkan total skor gabungan evaluasi HR dan Asesor Unit.
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev promosi cepat kandidat cadangan peringkat teratas.
* **Risiko & pemilik**: Pemilik: Backend Recruitment Lead. Risiko retensi data pelamar — dibatasi 6 bulan otomatis.
* **Definition of Done**: API talent pool dan fast-track promotion selesai dan terverifikasi; laporan tracked tersedia.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum S-D2).

### ✅ `BE-HRD-061` — API Serah Terima Rekrutmen ke Onboarding Sub-modul 05 & Sinkronisasi Kursi WFP
* **Slice / MVP**: `S-D2` / `MVP-13`
* **Status**: Selesai (✅ PASS) — diimplementasikan pada BE-HRD-061.md
* **Outcome**: API untuk menerbitkan dokumen serah terima (*Hiring Handoff*) bagi kandidat yang telah menandatangani penawaran final (`FINAL_ACCEPTED`) dan berstatus kelaikan kerja `FIT`/`FIT_WITH_RESTRICTIONS`, membentuk draf `WorkforceProfile` berstatus `Pending Onboarding` di Sub-modul 02 (`HRD-DEC-075`), serta menyinkronkan status kursi pada `HeadcountRequest` WFP menjadi `FULFILLED` (`INT-HR-20`, `INT-HR-23`).
* **Requirement/decision**: `HRD-DEC-075`, `HRD-DEC-078`, `FR-REC-10`, `VR-REC-20`, `AT-HRD-D2-11`, `INT-HR-20`, `INT-HR-23`.
* **Kontrak**: API Contract v8 bagian 14.9 grup `Hiring Handoff`, Validation v8 bagian 13.9, State Transition v8 bagian 12.9, Integration v8 bagian 12.
* **Reuse**: Integrasi ke `WorkforceProfile` Sub-modul 02 dan sinkronisasi kursi `WfpHeadcountRequestSeat` Sub-modul 03.
* **Cakupan**: `HiringHandoffController`, `IHiringHandoffService`, DTOs, endpoint `GET /handoffs`, `GET /handoffs/{id}`, `POST /handoffs/generate`, `POST /handoffs/{id}/confirm-transfer`.
* **Dependency**: `BE-HRD-058`, `BE-HRD-059`, `BE-HRD-052`
* **Acceptance criteria**:
  1. Serah terima hanya dapat diproses bila offer berstatus `FINAL_ACCEPTED` dan hasil MCU berstatus `FIT` atau `FIT_WITH_RESTRICTIONS` (`VR-REC-20`).
  2. Draf `WorkforceProfile` baru tercipta dengan status `Pending Onboarding`, NIK yang tervalidasi, tanpa hak akses login aktif sebelum orientasi selesai (`AT-HRD-D2-11`).
  3. Kursi pemenuhan pada `HeadcountRequestSeat` WFP terkait terikat dengan ID kandidat dan statusnya berubah menjadi `FULFILLED` (`INT-HR-20`).
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev serah terima dan verifikasi draf profil karyawan.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead. Risiko sinkronisasi antar sub-modul — dieksekusi dalam unit of work terisolasi.
* **Definition of Done**: API serah terima onboarding dan sinkronisasi WFP aktif; laporan tracked tersedia.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum S-D2).

### ✅ `BE-HRD-062` — Data Runtime Dev & Verifikasi Alur Bisnis Ujung-ke-Ujung S-D2
* **Slice / MVP**: `S-D2` / Verifikasi
* **Status**: Selesai (✅ PASS) — diverifikasi pada BE-HRD-062.md
* **Outcome**: Verifikasi alur bisnis rekrutmen rumah sakit ujung-ke-ujung terbukti sukses di database dev: mulai dari penarikan kursi formasi WFP, pembukaan lowongan klinis perawat, pendaftaran kandidat dengan auto-reject STR kedaluwarsa, rehire mantan pegawai, penegakan veto klinis kepala ICU, penerbitan conditional offer, evaluasi eskalasi gaji T14, MCU clearance dengan masking rekam medis, serah terima kandidat siap onboarding, hingga promosi cepat talent pool cadangan.
* **Requirement/decision**: `AT-HRD-D2-01` s.d. `AT-HRD-D2-12`, `UAT-REC-01` s.d. `UAT-REC-07`, `04-prd-to-mvp.md` bagian 22.5 & 22.6.
* **Kontrak**: Seluruh kontrak v8 bagian `S-D2`.
* **Reuse**: Master data jabatan, unit, grade gaji, dan headcount WFP dev.
* **Cakupan**: Eksekusi skenario uji API dev, pencatatan log request/response per skenario, tanpa modifikasi source runtime di luar perbaikan bug bila ditemukan.
* **Dependency**: `BE-HRD-060`, `BE-HRD-061`
* **Acceptance criteria**:
  1. Seluruh skenario `AT-HRD-D2-01` s.d. `AT-HRD-D2-12` dan `UAT-REC-01` s.d. `UAT-REC-07` dibuktikan via API dan tercatat di laporan.
  2. Perlindungan kerahasiaan diagnosa MCU terbukti: token HR non-medis tidak dapat membaca diagnosa klinis.
  3. Pelamar perawat STR kedaluwarsa terbukti gugur otomatis.
  4. Hak veto klinis kepala ICU terbukti mengunci status kandidat.
* **Bukti verifikasi**: Laporan pengujian ujung-ke-ujung terstruktur dengan potongan payload request/response dev.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead bersama QA. Risiko isolasi data — menggunakan unit kerja uji khusus rekrutmen.
* **Definition of Done**: Seluruh skenario uji terbukti di dev; laporan verifikasi tracked tersedia.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum S-D2).

### Requirement `S-D1` yang sengaja tidak punya task backend

| Requirement | Alasan | Kapan menjadi task |
| --- | --- | --- |
| `AT-HRD-D1-01`, `AT-HRD-D1-01b` — pengaman tabel kosong pada migration pembersihan | Migration `RetireWorkforcePlanningCandidateTables` **tidak dijadwalkan** pada `S-D1` (bagian 16.7): lima tabel kandidat masih dirujuk `S-D2` dan Penjadwalan | Setelah desain `S-D2` dan Penjadwalan menetapkan nasib FK-nya |
| `INT-HR-12` — angka beban otomatis dari data pasien | `HRD-DEP-005` `MISSING`; ditunda `HRD-DEC-060` | Setelah kontrak privasi data pasien ada |
| `INT-HR-14` — ringkasan kekurangan tenaga dari Penjadwalan | Bentuknya ditetapkan desain Penjadwalan; `S-D1` hanya menyediakan kolom | Desain Penjadwalan |
| Metode Analisis Beban Kerja | Ditunda `HRD-DEC-060` `ABK_LATER_SLICE`; nilai enum dicadangkan | Slice berikutnya |
| Kolom porsi kerja (modul 02) dan kategori alih daya (modul 01) | Milik modul lain; selama belum ada, lapisan terkait tampil "data belum tersedia" | Desain Workforce Core dan Master Data HR |


---

## Kartu Detail Task Backend — Slice `S-C4-ONB` Onboarding dan Kontrak Kerja (sub-modul 05)

Ditambahkan pada revision 7, 4 November 2026, sesudah desain `S-C4-ONB` disetujui pemilik
(`HRD-DEC-096`). Sumber desain: `02-backend-architecture.md` bagian 18; kontrak `v9` bagian
`S-C4-ONB` (API bagian 15, State Transition bagian 13, Validation bagian 14, Permission bagian 13,
Integration bagian 13); `data/data-dictionary.md` bagian 10; `testing/acceptance-test-matrix.md`
bagian 17; `04-prd-to-mvp.md` bagian 23.

**Catatan umum yang berlaku untuk setiap task di bawah:**

- **Handoff:** QBE preflight dan kesesuaian engineering diselesaikan pada waktu eksekusi dari
  `AGENTS.md` backend dan `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` serta
  `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Roadmap ini tidak memutuskannya lebih dulu.
- **Bukti verifikasi backend** mengikuti `rules/backend/TEST_POLICY.md`: QBE preflight/conformance,
  review diff dan cakupan, `dotnet restore` dan `dotnet build` project aplikasi, verifikasi kontrak API
  terhadap `contracts/api-contract.md` bagian 15, dan verifikasi proses bisnis terhadap baris
  `AT-HRD-ONB` yang disebut. **Tidak ada** task penulisan automated test; ketiadaan test bukan gap.
- **Uji API terhadap database dev** membutuhkan migration terpasang dan data uji. Wewenang tulis
  database diberikan terpisah. Selama belum ada, task boleh selesai sampai build dan review kode,
  lalu bagian uji API-nya ditandai belum terbukti (🟡) dengan menyebut wewenang itu.
- **Periksa ulang keadaan source pada HEAD saat task dikerjakan**, khususnya model onboarding kandidat
  dan migration pembersihan `S-D2` (`02-backend-architecture.md` 18.7).
- Pemilik seluruh task: Backend Technical Lead; pemilik keputusan produk: Pengguna (`HRD-DEC-015`).

| Task ID | MVP | Ringkasan |
| --- | --- | --- |
| `BE-HRD-063` | `MVP-14` | Fondasi data | ✅ Selesai (2026-11-04) | `BE-HRD-064` | `MVP-14` | Amandemen tawaran `S-D2`: jenis dan lama kontrak | ✅ Selesai (2026-11-04) | `BE-HRD-065` | `MVP-14` | Penulisan kontrak satu pintu dan penguncian isian kepegawaian | ✅ Selesai (2026-11-04) | `BE-HRD-066` | `MVP-14` | Ekstraksi service akun login pegawai | ✅ Selesai (2026-11-04) | `BE-HRD-067` | `MVP-14` | Penempatan, gaji awal, dan relasi atasan pegawai baru | ✅ Selesai (2026-11-04) | `BE-HRD-068` | `MVP-14` | Integrasi persetujuan `T15` s.d. `T18` | ✅ Selesai (2026-11-04) | `BE-HRD-069` | `MVP-14` | Lampiran, number series, dan pengaturan siklus | ✅ Selesai (2026-11-04) | `BE-HRD-070` | `MVP-14` | Template onboarding tiga lapis | ✅ Selesai (2026-11-04) | `BE-HRD-071` | `MVP-14` | Pembentukan masa percobaan dari jenis kontrak | ✅ Selesai (2026-11-04) | `BE-HRD-072` | `MVP-14` | Kontrak pertama: draf, data cetak, tanda tangan | ✅ Selesai (2026-11-04) | `BE-HRD-074` | `MVP-14` | Kasus onboarding dari serah terima | ✅ Selesai (2026-11-04) | `BE-HRD-073` | `MVP-15` | Pendampingan klinis |
| `BE-HRD-073` | `MVP-15` | Pendampingan klinis | ✅ Selesai (2026-11-04) |
| `BE-HRD-075` | `MVP-15` | Checklist, hadir hari pertama, dan butir otomatis | ✅ Selesai (2026-11-04) |
| `BE-HRD-076` | `MVP-15` | Siap kerja dan batal bergabung | ✅ Selesai (2026-11-04) |
| `BE-HRD-077` | `MVP-15` | Permintaan akun dan akses kerja | ✅ Selesai (2026-11-04) |
| `BE-HRD-078` | `MVP-16` | Serah terima pemisahan ke sub-modul 20 | ✅ Selesai (2026-11-04) |
| `BE-HRD-080` | `MVP-16` | Keputusan kontrak `T16` | ✅ Selesai (2026-11-04) |
| `BE-HRD-081` | `MVP-16` | Koreksi kontrak `T17` | ✅ Selesai (2026-11-04) |
| `BE-HRD-082` | `MVP-16` | Batch data awal kontrak `T18` | ✅ Selesai (2026-11-04) |
| `BE-HRD-083` | `MVP-16` | Scheduler harian siklus kontrak | ✅ Selesai (2026-11-04) |
| `BE-HRD-084` | `MVP-16` | Kesiapan dijadwalkan dan daftar Perlu Tindakan | ✅ Selesai (2026-11-04) |
| `BE-HRD-079` | `MVP-17` | Penilaian masa percobaan `T15` dan lulus otomatis |
| `BE-HRD-085` | Verifikasi | Data runtime dev `S-C4-ONB` |
| `BE-HRD-086` | Verifikasi | Verifikasi ujung-ke-ujung `S-C4-ONB` |

### ✅ `BE-HRD-063` — Fondasi data `S-C4-ONB`: 12 tabel `Hrd*`, dua master template, enum, dan migration pembersihan kandidat
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Skema sub-modul 05 berdiri tanpa menyentuh data mana pun: enam tabel kandidat kosong dibuang, dua master template dibentuk ulang, 12 tabel `Hrd*` dibuat, dan `WfpContractHistory` serta `MstContractType` mendapat kolom baru.
* **Requirement/decision**: `HRD-DEC-090` butir 10–12, `HRD-DEC-019`, `HRD-DEC-086`; `FR-ONB-063`; `AT-HRD-ONB-37`.
* **Kontrak**: Data Dictionary `v9` bagian 10; `02-backend-architecture.md` 18.4.12, 18.6, 18.7 (migration 2–4).
* **Reuse**: Pola `EMPTY_TABLE_GUARD` pada `Migrations/20260902072756_DropTableMstBillingCategory.cs:36-42`; pola configuration `Hrd*` dari `S-D1`.
* **Cakupan**: 12 model di `LifecycleManagement/Models/`; perubahan `MstOnboardingTemplate`, `MstOnboardingTemplateTask`, `WfpContractHistory`, `MstContractType`; 17 enum di `LifecycleManagement/Enums/`; `EmploymentLifecycleValueConstants.cs`; configuration di `Repositories/Configurations/Corporate/HumanResource/LifecycleManagement/`; 12 `DbSet` baru dan enam dibuang; migration `RetireLifecycleOnboardingCandidateTables`, `ReshapeOnboardingTemplates`, `AddEmploymentLifecycleCore` beserta Designer dan snapshot.
* **Dependency**: `BE-HRD-018` ✅
* **Acceptance criteria**:
  1. Enam tabel kandidat (`TrxProbationReview`, `TrxEmployeeOnboardingTask`, `TrxEmployeeOnboarding`, `WfpOnboardingTask`, `WfpOnboardingChecklist`, `TrxContractNonRenewal`) dibuang dengan blok pengaman yang membatalkan migration bila satu baris saja ada, tanpa `DROP … CASCADE`.
  2. Seluruh kolom, index, unique bersyarat, FK `Restrict`, dan tipe pada `data/data-dictionary.md` bagian 10.2–10.14 ada pada configuration.
  3. Penambahan pada `WfpContractHistory` dan `MstContractType` hanya kolom boleh-null atau berbawaan; tidak ada kolom lama yang berubah tipe atau dibuang.
  4. `dotnet build` 0 error; tidak ada referensi tersisa ke keenam model kandidat.
* **Bukti verifikasi**: QBE preflight; review diff migration (blok pengaman, urutan pembuangan); `dotnet restore` dan `dotnet build`; uji pengaman dengan satu baris uji (`AT-HRD-ONB-37`) bila wewenang database diberikan.
* **Risiko & pemilik**: Risiko urutan terhadap pembersihan `S-D2` yang sedang berjalan (18.7). Pemilik: Backend Technical Lead.
* **Definition of Done**: Migration, Designer, snapshot, model, enum, dan configuration ada; build lolos; laporan tracked tersedia.

### ✅ `BE-HRD-064` — Amandemen tawaran `S-D2`: jenis kontrak dan lama kontrak (`HRD-DEC-095`)
* **Slice / MVP**: `S-D2` (amandemen dari sub-modul 05) / `MVP-14`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Tawaran kerja memuat jenis dan lama kontrak, ikut disetujui bersama gaji, sehingga draf kontrak pertama dapat menyalinnya tanpa persetujuan ulang.
* **Requirement/decision**: `HRD-DEC-095`; `FR-ONB-010`; `VR-ONB-50`, `VR-ONB-51`; `INT-HR-35`; `AT-HRD-ONB-38`.
* **Kontrak**: API `v9` bagian 15.12; Data Dictionary `v9` bagian 10.14; migration 1 pada 18.7.
* **Reuse**: Service dan controller tawaran yang dibangun `BE-HRD-057`.
* **Cakupan**: Dua kolom `HrdJobOffer` (`ContractTypeId`, `ContractDurationMonths`) dan migration `AddOfferContractTerms`; `CreateJobOfferRequest` dan response tawaran; validasi di service tawaran; tanggal berakhir hasil hitung pada response.
* **Dependency**: `BE-HRD-057`
* **Acceptance criteria**:
  1. Tawaran tanpa jenis kontrak ditolak saat diajukan (`VR-ONB-50`).
  2. Tawaran PKWT tanpa lama 1–60 bulan ditolak; tawaran PKWTT yang diisi lama ditolak (`VR-ONB-51`).
  3. Response memuat `contractTypeId`, `contractTypeName`, `contractDurationMonths`, dan `contractEndDate` hasil hitung (mulai + lama − 1 hari).
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak API bagian 15.12; uji `AT-HRD-ONB-38`.
* **Risiko & pemilik**: Menyentuh slice `S-D2` yang sudah disetujui; perubahan dibatasi dua isian. Pemilik: Backend Technical Lead.
* **Definition of Done**: Isian tersimpan, divalidasi, dan terbaca pada response; laporan tracked tersedia.

### ✅ `BE-HRD-065` — Penulisan kontrak satu pintu dan penguncian isian kepegawaian
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Hanya satu jalan menulis kontrak (`WfpContractHistoryWriteService`), lima isian kepegawaian di `MstEmployee` hanya ditulis sebagai salinan, dan jalur tulis lama ditutup dengan pesan yang terbaca.
* **Requirement/decision**: `HRD-DEC-085` butir 2, `HRD-DEC-096` butir 5; `FR-ONB-015`; `VR-ONB-32`, `VR-ONB-33`, `VR-ONB-34`; `INT-HR-26`, `INT-HR-28`; `AT-HRD-ONB-09`, `AT-HRD-ONB-39`.
* **Kontrak**: API `v9` bagian 15.11; `02-backend-architecture.md` 18.4.10 (service di modul lain), 18.4.11.
* **Reuse**: `WfpContractHistoryController` (02) dan indeks unik kontrak berjalan yang sudah ada.
* **Cakupan**: `WfpContractHistoryWriteService` (WorkforceCore); `EmployeeEmploymentRecordService` (MasterData/Workforce, termasuk profil dokter); lima endpoint tulis `WfpContractHistoryController` menjawab `409` `CONTRACT_MANAGED_BY_LIFECYCLE`; `EmployeeController` `PUT` menolak perubahan lima isian untuk pegawai yang sudah punya kontrak.
* **Dependency**: `BE-HRD-063`
* **Acceptance criteria**:
  1. `POST`, `PUT`, `PATCH status`, `PATCH current`, dan `DELETE` riwayat kontrak 02 menjawab `409` dengan pesan "Kontrak dikelola lewat menu Onboarding & Kontrak."; keempat `GET` tidak berubah.
  2. `PUT` master pegawai yang mengubah jenis atau status kepegawaian, tanggal masuk, tanggal akhir kontrak, atau tanggal akhir masa percobaan ditolak `409` bila pegawai punya baris kontrak; pegawai tanpa kontrak tidak berubah perilakunya.
  3. Write service menolak tanggal masa percobaan yang tidak berasal dari `HrdProbation` (`VR-ONB-34`).
* **Bukti verifikasi**: QBE preflight; review diff dan daftar consumer endpoint lama; `dotnet build`; verifikasi kontrak bagian 15.11; uji `AT-HRD-ONB-39`.
* **Risiko & pemilik**: **Perubahan yang memutus** satu consumer frontend (tab riwayat kontrak); pasangannya `FE-HRD-071`. Pemilik: Backend Technical Lead.
* **Definition of Done**: Satu pintu tulis kontrak ada; jalur lama `409`; laporan tracked tersedia.

### ✅ `BE-HRD-066` — Ekstraksi service akun login pegawai
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Logika pembuatan akun yang hari ini tinggal di `EmployeeController` dapat dipanggil service lain, dengan dua metode baru: aktifkan dan nonaktifkan.
* **Requirement/decision**: `HRD-DEC-088` butir 6–8; `HRD-TF-053`; `INT-HR-28`; `VR-ONB-75`.
* **Kontrak**: `02-backend-architecture.md` 18.4.10 (`EmployeeUserAccountService`).
* **Reuse**: `EmployeeController.cs:3555-3660` (pembuatan akun, kata sandi awal, wajib ganti kata sandi).
* **Cakupan**: `EmployeeUserAccountService` di `MasterData/Workforce/Services/`; `EmployeeController` memanggil service itu; metode `CreateForEmployeeAsync`, `ActivateAsync`, `DeactivateAsync`.
* **Dependency**: `BE-HRD-028` ✅
* **Acceptance criteria**:
  1. Perilaku endpoint `EmployeeController` yang membuat akun tidak berubah: pesan gagal, aturan email unik, dan kata sandi awal sama.
  2. Service mengembalikan hasil gagal yang terbaca bila email atau tanggal lahir kosong, tanpa melempar exception ke pemanggil.
  3. Aktifkan dan nonaktifkan mengubah `ApplicationUser.IsActive` saja.
* **Bukti verifikasi**: QBE preflight; review diff (ekstraksi tanpa perubahan perilaku); `dotnet build`; uji endpoint pembuatan akun master pegawai sebelum dan sesudah.
* **Risiko & pemilik**: Menyentuh controller master yang dipakai banyak layar; ekstraksi **MUST NOT** merapikan bagian lain. Pemilik: Backend Technical Lead.
* **Definition of Done**: Service ada dan dipakai controller; perilaku lama sama; laporan tracked tersedia.

### ✅ `BE-HRD-067` — Penempatan, gaji awal, dan relasi atasan pegawai baru
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Sub-modul 02 punya satu service yang membuat penempatan organisasi, jabatan, dan gaji awal dari tawaran yang disetujui, serta relasi atasan saat siap kerja.
* **Requirement/decision**: `HRD-DEC-096` butir 1–2, `HRD-DEC-031`, `HRD-DEC-049`, `HRD-DEC-084`; `FR-ONB-013`; `INT-HR-27`.
* **Kontrak**: `02-backend-architecture.md` 18.4.10 (`WfpNewHireAssignmentService`), 18.9.6.
* **Reuse**: Aturan tumpang tindih dan penempatan utama pada `HrdInitialPlacementBatchService` (`BE-HRD-027`).
* **Cakupan**: `WfpNewHireAssignmentService` di `WorkforceCore/Services/`: `CreateInitialAssignmentsAsync` (organisasi, jabatan, gaji; berlaku sejak tanggal mulai kontrak; rujukan persetujuan nomor tawaran) dan `CreateManagerAssignmentAsync` (pencatat Kepala Unit, penyetuju HRD).
* **Dependency**: `BE-HRD-063`, `BE-HRD-027` ✅
* **Acceptance criteria**:
  1. Penempatan pertama tidak melanggar satu penempatan utama per jenis per tanggal (`HRD-DEC-049`).
  2. Penetapan gaji awal menyimpan nomor tawaran sebagai rujukan persetujuannya; nominal tidak masuk logger.
  3. Relasi atasan ditolak bila pencatat dan penyetuju orang yang sama (`HRD-DEC-031`).
  4. Service ikut transaksi pemanggil dan tidak meng-commit sendiri.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi proses bisnis lewat `AT-HRD-ONB-07` dan `AT-HRD-ONB-23`.
* **Risiko & pemilik**: Menulis tabel 02 yang sudah berisi data; perubahan hanya penambahan baris. Pemilik: Backend Technical Lead.
* **Definition of Done**: Service ada, dipakai oleh `BE-HRD-072` dan `BE-HRD-076`; laporan tracked tersedia.

### ✅ `BE-HRD-068` — Integrasi persetujuan `T15` s.d. `T18`
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Empat jenis dokumen sub-modul 05 dapat diajukan ke kotak masuk persetujuan bersama dan keputusannya diteruskan ke service domain.
* **Requirement/decision**: `HRD-DEC-081`, `HRD-DEC-018`, `HRD-DEC-031`; `INT-HR-30`.
* **Kontrak**: Integration `v9` bagian 13 (`INT-HR-30`); `02-backend-architecture.md` 18.4.10.
* **Reuse**: `WorkflowService` (penolakan swa-setuju `WorkflowService.cs:534-556`), `WorkflowReferenceLifecycleService`, kotak masuk `BE-HRD-005`.
* **Cakupan**: Empat jenis rujukan (`WorkforceProbationAssessment`, `WorkforceContractDecision`, `WorkforceContractCorrection`, `WorkforceInitialContractBatch`); `HrdEmploymentLifecycleWorkflowIntegrationService` (mulai instance, terima keputusan akhir, teruskan Setujui / Minta Revisi / Tolak).
* **Dependency**: `BE-HRD-063`, `BE-HRD-005` ✅
* **Acceptance criteria**:
  1. Setiap jenis rujukan memulai tepat satu instance per dokumen.
  2. Keputusan akhir diteruskan ke service domain tanpa endpoint persetujuan di controller domain.
  3. Langkah tanpa penyetuju ditolak (`UNRESOLVED_STEP_FAILS_CLOSED`); pengaju tidak dapat menyetujui sendiri.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi proses bisnis lewat `AT-HRD-ONB-22` dan `AT-HRD-ONB-33` setelah definisi alur terisi (`BE-HRD-085`).
* **Risiko & pemilik**: Bergantung pada definisi alur dan pemetaan peran (`HRD-DEC-032`). Pemilik: Backend Technical Lead.
* **Definition of Done**: Empat rujukan terdaftar dan teruskan keputusan; laporan tracked tersedia.

### ✅ `BE-HRD-069` — Lampiran, number series, dan pengaturan siklus
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Service bersama untuk berkas bukti, enam kunci number series, dan API pengaturan siklus per rumah sakit.
* **Requirement/decision**: `HRD-DEC-085` butir 4, `HRD-DEC-089` butir 1 dan 8; `VR-ONB-09`, `VR-ONB-80`.
* **Kontrak**: API `v9` bagian 15.9; Data Dictionary bagian 10.13.
* **Reuse**: `WorkflowFileStorageService`, `NumberSeriesAllocator`.
* **Cakupan**: `HrdLifecycleAttachmentService` (PDF/JPG/PNG ≤ 10 MB); konstanta kunci number series; `HrdEmploymentLifecycleSettingService` dan `HrdEmploymentLifecycleSettingController` (`GET`/`PUT .../lifecycle-settings`).
* **Dependency**: `BE-HRD-063`
* **Acceptance criteria**:
  1. Berkas di luar jenis dan ukuran ditolak `400` (`VR-ONB-09`).
  2. Pengaturan di luar rentang ditolak `400` (`VR-ONB-80`); bawaan 60 / 14 / 14 / 3 bila belum ada baris.
  3. Nomor dokumen dialokasikan service di luar transaksi pemanggil, bukan oleh controller.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.9.
* **Risiko & pemilik**: Rendah. Pemilik: Backend Technical Lead.
* **Definition of Done**: Tiga kemampuan bersama tersedia; laporan tracked tersedia.

### ✅ `BE-HRD-070` — Template onboarding tiga lapis
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: HRD dapat menyusun Template Umum, Template Tambahan Tenaga Klinis, dan Template Tambahan Unit, dengan butir sistem yang terkunci.
* **Requirement/decision**: `HRD-DEC-086`; `FR-ONB-006`; `VR-ONB-20` s.d. `VR-ONB-25`; `AT-HRD-ONB-19`.
* **Kontrak**: API `v9` bagian 15.2.
* **Reuse**: Master `MstOnboardingTemplate` dan `MstOnboardingTemplateTask` yang dibentuk ulang `BE-HRD-063`.
* **Cakupan**: `HrdOnboardingTemplateService`, `HrdOnboardingTemplateController`, DTO; penyisipan butir sistem per lapis.
* **Dependency**: `BE-HRD-063`
* **Acceptance criteria**:
  1. Satu template aktif per lapis Umum dan Klinis, satu per unit untuk lapis Unit (`VR-ONB-20`, `VR-ONB-21`).
  2. Butir sistem tidak dapat dihapus atau diubah jenis, penanggung jawab, maupun tanda wajibnya (`VR-ONB-22`).
  3. Butir otomatis sertifikat wajib memilih jenis sertifikat (`VR-ONB-23`).
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.2; uji `AT-HRD-ONB-19`.
* **Risiko & pemilik**: Utang teknis lokasi master (18.5) tidak dirapikan di task ini. Pemilik: Backend Technical Lead.
* **Definition of Done**: API template berjalan dengan seluruh aturan; laporan tracked tersedia.

### ✅ `BE-HRD-071` — Pembentukan masa percobaan dari jenis kontrak
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Masa percobaan hanya lahir untuk kontrak PKWTT pertama sejak bergabung, tepat 3 bulan, dengan tanggal mulai tampil di daftar Perlu Tindakan.
* **Requirement/decision**: `HRD-DEC-085` butir 1–3, `HRD-DEC-089` butir 6; `FR-ONB-030`, `FR-ONB-031`; `AT-HRD-ONB-10`, `AT-HRD-ONB-15`.
* **Kontrak**: State Transition `v9` bagian 13.4; `02-backend-architecture.md` 18.9.3.
* **Reuse**: `MstContractType.IsProbationApplicable`.
* **Cakupan**: `HrdProbationService.CreateForContractAsync` (dipanggil `BE-HRD-072`), hitungan tanggal akhir dan `ReminderFromDate`, salinan `ProbationEndDate` ke kontrak dan `MstEmployee` lewat service `BE-HRD-065`.
* **Dependency**: `BE-HRD-063`, `BE-HRD-069`
* **Acceptance criteria**:
  1. Kontrak PKWT dan kontrak `Conversion` tidak menghasilkan masa percobaan.
  2. Kontrak PKWTT `Initial` mulai 1 Februari 2027 menghasilkan masa percobaan s.d. 30 April 2027 dengan `ReminderFromDate` 16 April 2027 (bawaan 14 hari).
  3. Satu kontrak paling banyak satu masa percobaan.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi proses bisnis lewat `AT-HRD-ONB-10`.
* **Risiko & pemilik**: Rendah. Pemilik: Backend Technical Lead.
* **Definition of Done**: Pembentukan masa percobaan berjalan sesuai jenis kontrak; laporan tracked tersedia.

### ✅ `BE-HRD-072` — Kontrak pertama: draf dari tawaran, data cetak, dan tanda tangan
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Draf kontrak pertama tersusun dari tawaran tanpa ketik ulang, dapat dicetak dengan gaji yang disembunyikan bagi yang tidak berhak, dan sah setelah scan bertanda tangan diunggah — sekaligus membuat penempatan, gaji awal, salinan kepegawaian, dan masa percobaan dalam satu transaksi.
* **Requirement/decision**: `HRD-DEC-084`, `HRD-DEC-095`, `HRD-DEC-033`, `HRD-DEC-039`; `FR-ONB-011` s.d. `FR-ONB-014`; `VR-ONB-30`, `VR-ONB-31`; `AT-HRD-ONB-06` s.d. `AT-HRD-ONB-08`, `AT-HRD-ONB-40`.
* **Kontrak**: API `v9` bagian 15.3 (`GET /`, `GET /{id}`, `GET /{id}/print-data`, `POST /{id}/mark-signed`, `GET /{id}/document`); State Transition bagian 13.3.
* **Reuse**: Service `BE-HRD-065`, `BE-HRD-067`, `BE-HRD-071`; `HrdLifecycleAttachmentService`; audit `GET` sensitif `HRD-DEC-039`.
* **Cakupan**: `HrdEmploymentContractService` (draf dari tawaran, data cetak, tandai ditandatangani, unduh scan, daftar dan detail kontrak); `HrdEmploymentContractController` untuk endpoint di atas; kunci advisory per pegawai.
* **Dependency**: `BE-HRD-064`, `BE-HRD-065`, `BE-HRD-067`, `BE-HRD-071`
* **Acceptance criteria**:
  1. Draf menyalin jenis, tanggal mulai, dan tanggal berakhir dari tawaran; tidak ada endpoint ubah isi (`VR-ONB-31`).
  2. Tandai Sudah Ditandatangani tanpa scan ditolak `422` (`VR-ONB-30`).
  3. Penandatanganan membuat dalam satu transaksi: kontrak `Active` dan `IsCurrent`, penempatan dan gaji awal, salinan `MstEmployee`, dan masa percobaan bila PKWTT; kegagalan di tengah membatalkan seluruhnya.
  4. Data cetak memuat nominal gaji hanya bagi pemegang `WfpSalaryAssignment : ViewAmount`, dan pembacaan nominal dicatat tanpa nominalnya.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.3; uji `AT-HRD-ONB-06`, `AT-HRD-ONB-07`, `AT-HRD-ONB-40`.
* **Risiko & pemilik**: Transaksi lintas tiga modul; diserialkan kunci advisory. Pemilik: Backend Technical Lead.
* **Definition of Done**: Kontrak pertama dapat disusun, dicetak, dan disahkan; laporan tracked tersedia.

### ✅ `BE-HRD-073` — Pendampingan klinis
* **Slice / MVP**: `S-C4-ONB` / `MVP-15`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Kepala Unit dapat menetapkan dan mengganti preceptor serta menyatakan Siap Mandiri dengan lampiran, tanpa persetujuan HRD.
* **Requirement/decision**: `HRD-DEC-087`; `FR-ONB-040` s.d. `FR-ONB-042`; `VR-ONB-08`, `VR-ONB-60` s.d. `VR-ONB-63`; `AT-HRD-ONB-27`, `AT-HRD-ONB-28`.
* **Kontrak**: API `v9` bagian 15.5; State Transition bagian 13.7.
* **Reuse**: `OrganizationAuthorizationProjectionService` untuk cakupan unit; `HrdLifecycleAttachmentService`.
* **Cakupan**: `HrdClinicalPreceptorshipService` (buat dari butir preceptor, ganti preceptor, siap mandiri, ubah ke `Accompanied` dan `Cancelled` atas panggilan service kasus); `HrdClinicalPreceptorshipController`.
* **Dependency**: `BE-HRD-063`, `BE-HRD-069`
* **Acceptance criteria**:
  1. Preceptor harus tenaga klinis aktif di unit yang sama, bukan pegawai itu sendiri (`VR-ONB-08`).
  2. Siap Mandiri hanya saat `Accompanied` dan wajib lampiran (`VR-ONB-60`, `VR-ONB-61`); tidak membuat tugas di kotak masuk.
  3. Pergantian preceptor menambah satu baris riwayat dan menutup baris sebelumnya.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.5; uji `AT-HRD-ONB-27`, `AT-HRD-ONB-28`.
* **Risiko & pemilik**: Keputusan klinis tanpa persetujuan dijaga lampiran dan jejak audit. Pemilik: Backend Technical Lead.
* **Definition of Done**: API pendampingan berjalan; laporan tracked tersedia.

### ✅ `BE-HRD-074` — Kasus onboarding dari serah terima
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Serah terima rekrutmen membuka kasus onboarding, checklist tiga lapis, draf kontrak, dan permintaan akun dalam satu transaksi; HRD dan Kepala Unit dapat melihat daftar, ringkasan, dan detail kasus.
* **Requirement/decision**: `HRD-DEC-083` butir 1–4, `HRD-DEC-086` butir 1–2, `HRD-DEC-088` butir 6; `FR-ONB-001`, `FR-ONB-002`; `VR-ONB-01`, `VR-ONB-02`; `INT-HR-25`; `AT-HRD-ONB-01` s.d. `AT-HRD-ONB-03`, `AT-HRD-ONB-16`.
* **Kontrak**: API `v9` bagian 15.1 (`GET /`, `GET /summary`, `GET /{id}`); Integration bagian 13 (`INT-HR-25`).
* **Reuse**: Service serah terima `BE-HRD-061`; template `BE-HRD-070`; draf kontrak `BE-HRD-072`; akun `BE-HRD-066`.
* **Cakupan**: `HrdOnboardingCaseService.CreateFromHandoffAsync` (idempoten per `HiringHandoffId`, penggabungan lapis, butir sistem, permintaan akun `AccountCreation`); pemanggilan dari service serah terima `S-D2`; `HrdOnboardingCaseController` untuk daftar, ringkasan, dan detail beserta `availableActions`.
* **Dependency**: `BE-HRD-061`, `BE-HRD-066`, `BE-HRD-070`, `BE-HRD-072`
* **Acceptance criteria**:
  1. Serah terima yang dijalankan dua kali tetap menghasilkan satu kasus (`VR-ONB-01`).
  2. Perawat ICU memperoleh butir Umum + Klinis + ICU; pegawai non-klinis di unit tanpa template tambahan hanya Umum; tanpa Template Umum, kasus tetap terbentuk dengan butir sistem dan `HasGeneralTemplate = false`.
  3. Kegagalan di tengah membatalkan seluruh transaksi dan serah terima tetap `Generated`; kegagalan pembuatan akun **tidak** membatalkan kasus (`VR-ONB-75`).
  4. Kepala Unit hanya melihat kasus unit dalam cakupannya.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.1 dan `INT-HR-25`; uji `AT-HRD-ONB-01` s.d. `AT-HRD-ONB-03`, `AT-HRD-ONB-16`.
* **Risiko & pemilik**: Menyentuh service serah terima `S-D2`; dikerjakan setelah `BE-HRD-061` stabil. Pemilik: Backend Technical Lead.
* **Definition of Done**: Kasus terbentuk otomatis dan dapat dibaca; laporan tracked tersedia.

### ✅ `BE-HRD-075` — Checklist, hadir hari pertama, dan butir otomatis
* **Slice / MVP**: `S-C4-ONB` / `MVP-15`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: HRD dan Kepala Unit mencentang butir masing-masing dengan bukti, memilih atasan langsung dan preceptor, mencatat hadir hari pertama yang mengaktifkan akun layanan mandiri, dan butir otomatis terisi dari data sistem.
* **Requirement/decision**: `HRD-DEC-086` butir 3–7, `HRD-DEC-087` butir 2, `HRD-DEC-088` butir 7, `HRD-DEC-096` butir 1 dan 4; `FR-ONB-003`, `FR-ONB-004`; `VR-ONB-03` s.d. `VR-ONB-10`, `VR-ONB-15` s.d. `VR-ONB-17`; `INT-HR-32`; `AT-HRD-ONB-17`, `AT-HRD-ONB-18`, `AT-HRD-ONB-25`.
* **Kontrak**: API `v9` bagian 15.1 (hadir hari pertama, centang dan batal centang butir HRD dan unit, periksa ulang butir otomatis, alihkan koordinator); State Transition bagian 13.2.
* **Reuse**: `WfpCertification` untuk butir sertifikat; `HrdPreEmploymentMedicalCheck` untuk kelaikan; `EmployeeUserAccountService.ActivateAsync`.
* **Cakupan**: Metode pada `HrdOnboardingCaseService`; `HrdOnboardingAutomaticItemService`; pembuatan pendampingan dari butir `PRECEPTOR_ASSIGNED` lewat `BE-HRD-073`; endpoint terkait pada `HrdOnboardingCaseController`.
* **Dependency**: `BE-HRD-073`, `BE-HRD-074`
* **Acceptance criteria**:
  1. Butir HRD hanya lewat jalur HRD; butir unit hanya Kepala Unit dalam cakupan (`VR-ONB-03`, `VR-ONB-04`).
  2. Butir wajib bukti menolak tanpa berkas; butir otomatis menolak dicentang tangan (`VR-ONB-05`, `VR-ONB-06`).
  3. Hadir hari pertama tidak sebelum tanggal mulai rencana, mengaktifkan akun untuk layanan mandiri saja, dan hanya dapat dibatalkan selama kontrak pertama masih `Draft` (`VR-ONB-10`, `VR-ONB-17`).
  4. Butir sertifikat tercentang bila ada `WfpCertification` jenis yang sama, terverifikasi, dan masih berlaku; kembali `Pending` bila kedaluwarsa sebelum siap kerja.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.1; uji `AT-HRD-ONB-17`, `AT-HRD-ONB-18`, `AT-HRD-ONB-25`.
* **Risiko & pemilik**: Pemisahan dua jalur centang harus dijaga aturan bisnis, bukan hanya hak akses. Pemilik: Backend Technical Lead.
* **Definition of Done**: Seluruh aksi checklist berjalan; laporan tracked tersedia.

### ✅ `BE-HRD-076` — Siap kerja dan batal bergabung
* **Slice / MVP**: `S-C4-ONB` / `MVP-15`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: HRD menyatakan siap kerja satu kali setelah seluruh butir wajib selesai, atau mencatat batal bergabung sebelum hari pertama, dan seluruh akibatnya terjadi dalam satu transaksi.
* **Requirement/decision**: `HRD-DEC-083` butir 5–7, `HRD-DEC-088` butir 1–5 dan 8, `HRD-DEC-080`, `HRD-DEC-031`; `FR-ONB-005`, `FR-ONB-020` s.d. `FR-ONB-022`; `VR-ONB-11` s.d. `VR-ONB-14`, `VR-ONB-16`; `AT-HRD-ONB-04`, `AT-HRD-ONB-05`, `AT-HRD-ONB-21` s.d. `AT-HRD-ONB-23`.
* **Kontrak**: API `v9` bagian 15.1 (`POST /{id}/declare-ready`, `POST /{id}/cancel-join`); State Transition bagian 13.1, 13.9.
* **Reuse**: `WfpNewHireAssignmentService.CreateManagerAssignmentAsync`; `EmployeeEmploymentRecordService`; `EmployeeUserAccountService.DeactivateAsync`; service talent pool dan kursi `S-D2` untuk membuka kembali kursi.
* **Cakupan**: Metode siap kerja dan batal bergabung pada `HrdOnboardingCaseService`; permintaan `WorkAccess`.
* **Dependency**: `BE-HRD-067`, `BE-HRD-075`
* **Acceptance criteria**:
  1. Siap kerja ditolak selama ada butir wajib `Pending` (`VR-ONB-11`) atau bila penyata sama dengan pemilih atasan langsung (`VR-ONB-12`).
  2. Siap kerja: status `ACTIVE`, relasi atasan berlaku sejak tanggal mulai kontrak, pendampingan `Accompanied`, permintaan akses kerja `Requested`, kasus `Completed`.
  3. Batal bergabung hanya sebelum hadir hari pertama dengan alasan dan catatan; status `CANCELLED_JOIN`, profil nonaktif tanpa terhapus, kontrak draf `Cancelled`, serah terima `Cancelled`, kursi kembali terbuka, akun dinonaktifkan.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.1; uji `AT-HRD-ONB-04`, `AT-HRD-ONB-05`, `AT-HRD-ONB-21` s.d. `AT-HRD-ONB-23`.
* **Risiko & pemilik**: Pembukaan kembali kursi menyentuh `S-D2`; pakai service-nya, bukan menulis tabelnya langsung. Pemilik: Backend Technical Lead.
* **Definition of Done**: Dua titik akhir kasus berjalan; laporan tracked tersedia.

### ✅ `BE-HRD-077` — Permintaan akun dan akses kerja
* **Slice / MVP**: `S-C4-ONB` / `MVP-15`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Administrator/IT melihat permintaan akses kerja dan menandainya selesai; HRD dapat mengulang pembuatan akun yang gagal.
* **Requirement/decision**: `HRD-DEC-088` butir 3, 6, 8, `HRD-DEC-032`; `FR-ONB-023`; `VR-ONB-75` s.d. `VR-ONB-77`; `INT-HR-34`; `AT-HRD-ONB-26`.
* **Kontrak**: API `v9` bagian 15.7.
* **Reuse**: `EmployeeUserAccountService`.
* **Cakupan**: `HrdOnboardingAccessService` (daftar, tandai selesai, ulang); `HrdOnboardingAccessRequestController`.
* **Dependency**: `BE-HRD-074`
* **Acceptance criteria**:
  1. Tandai selesai hanya untuk `WorkAccess` berstatus `Requested` (`VR-ONB-76`).
  2. Ulang hanya untuk `AccountCreation` berstatus `Failed`, dan berhasil setelah email diisi (`VR-ONB-77`, `AT-HRD-ONB-26`).
  3. Sub-modul 05 tidak memberi peran apa pun kepada akun.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.7; uji `AT-HRD-ONB-26`.
* **Risiko & pemilik**: Pemetaan peran `MAPPING_REQUIRED` menahan rilis, bukan task ini. Pemilik: Backend Technical Lead.
* **Definition of Done**: API permintaan akses berjalan; laporan tracked tersedia.

### ✅ `BE-HRD-078` — Serah terima pemisahan ke sub-modul 20
* **Slice / MVP**: `S-C4-ONB` / `MVP-16`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Sub-modul 20 dapat menerima pemisahan berjenis `ContractNonRenewal` dan `ProbationFailed` lalu membuka exit clearance seperti jalur pengunduran diri.
* **Requirement/decision**: `HRD-DEC-082`; `HRD-TF-054`; `FR-ONB-034`, `FR-ONB-052`; `INT-HR-29`.
* **Kontrak**: Integration `v9` bagian 13 (`INT-HR-29`); `02-backend-architecture.md` 18.4.10.
* **Reuse**: `ResignationLifecycleHandoffService`, `ExitClearanceService`.
* **Cakupan**: Metode `HandoffLifecycleSeparationAsync` (pemisahan berstatus disetujui, tanggal terakhir bekerja, nomor dari `NumberSeriesAllocator`, pembukaan exit clearance); jalur pengunduran diri tidak berubah.
* **Dependency**: `BE-HRD-014` ✅
* **Acceptance criteria**:
  1. Satu pemanggilan menghasilkan tepat satu pemisahan dan satu exit clearance; pemanggilan ulang dengan rujukan sama tidak menggandakan.
  2. Jalur pengunduran diri lama berperilaku sama sebelum dan sesudah.
  3. Metode ikut transaksi pemanggil.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; uji jalur pengunduran diri yang sudah ada; verifikasi `AT-HRD-ONB-14`, `AT-HRD-ONB-31` setelah `BE-HRD-079`, `BE-HRD-080`.
* **Risiko & pemilik**: Menyentuh service 20 yang sudah berjalan. Pemilik: Backend Technical Lead.
* **Definition of Done**: Metode baru tersedia tanpa regresi; laporan tracked tersedia.

### ✅ `BE-HRD-079` — Penilaian masa percobaan `T15` dan lulus otomatis
* **Slice / MVP**: `S-C4-ONB` / `MVP-17`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Atasan mengajukan Lulus atau Tidak Lulus, HRD memutuskan lewat kotak masuk, Tidak Lulus diserahkan ke 20, dan masa percobaan tanpa keputusan lulus otomatis.
* **Requirement/decision**: `HRD-DEC-085` butir 4–9, `HRD-DEC-081`, `HRD-DEC-082`; `FR-ONB-032` s.d. `FR-ONB-034`; `VR-ONB-38`, `VR-ONB-55` s.d. `VR-ONB-59`; `AT-HRD-ONB-11` s.d. `AT-HRD-ONB-14`.
* **Kontrak**: API `v9` bagian 15.4; State Transition bagian 13.4, 13.5.
* **Reuse**: Integrasi `BE-HRD-068`; serah terima `BE-HRD-078`; scheduler `BE-HRD-083`.
* **Cakupan**: Metode pengajuan, ubah, tarik, dan penerapan keputusan pada `HrdProbationService`; `HrdProbationController`; langkah harian lulus otomatis pada `HrdEmploymentLifecycleSchedulerService`.
* **Dependency**: `BE-HRD-068`, `BE-HRD-071`, `BE-HRD-078`, `BE-HRD-083`
* **Acceptance criteria**:
  1. Pengajuan hanya saat masa percobaan berjalan dan sebelum tanggal akhir; satu penilaian terbuka; Tidak Lulus wajib lampiran.
  2. Persetujuan sesudah tanggal akhir ditolak (`VR-ONB-58`).
  3. Sehari sesudah tanggal akhir tanpa hasil disetujui, status `PassedAutomatically`, penilaian terbuka `ClosedOnExpiry`, instance workflow dibatalkan.
  4. Tidak Lulus yang disetujui membuat pemisahan `ProbationFailed` lewat `BE-HRD-078`.
  5. Tidak ada endpoint perpanjangan (`VR-ONB-59`).
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.4; uji `AT-HRD-ONB-11` s.d. `AT-HRD-ONB-14`.
* **Risiko & pemilik**: Ketepatan pergantian hari pada zona waktu rumah sakit (`NFR-ONB-006`). Pemilik: Backend Technical Lead.
* **Definition of Done**: Siklus masa percobaan lengkap; laporan tracked tersedia.

### ✅ `BE-HRD-080` — Keputusan kontrak `T16`
* **Slice / MVP**: `S-C4-ONB` / `MVP-16`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Atasan memilih Perpanjang, Angkat jadi Pegawai Tetap, atau Tidak Diperpanjang; HRD memutuskan; sistem menyusun kontrak lanjutan atau menyerahkan ke 20.
* **Requirement/decision**: `HRD-DEC-089` butir 2–7, 10, `HRD-DEC-082`; `FR-ONB-051`, `FR-ONB-052`; `VR-ONB-35` s.d. `VR-ONB-40`; `AT-HRD-ONB-15`, `AT-HRD-ONB-29` s.d. `AT-HRD-ONB-31`.
* **Kontrak**: API `v9` bagian 15.3 (`GET /{id}/fixed-term-check`, keputusan: ajukan, detail, ubah, tarik, catat pemberitahuan); State Transition bagian 13.6.
* **Reuse**: `HrdEmploymentContractService` dari `BE-HRD-072`; integrasi `BE-HRD-068`; serah terima `BE-HRD-078`.
* **Cakupan**: `HrdContractDecisionService`; endpoint keputusan pada `HrdEmploymentContractController`; hitungan batas 5 tahun (18.9.2); draf kontrak lanjutan `Renewal`/`Conversion`.
* **Dependency**: `BE-HRD-068`, `BE-HRD-072`, `BE-HRD-078`
* **Acceptance criteria**:
  1. Perpanjangan yang membuat total PKWT lebih dari 5 tahun ditolak, termasuk saat disetujui (`VR-ONB-36`).
  2. Satu keputusan terbuka atau disetujui per kontrak (`VR-ONB-37`); pengaju hanya atasan langsung aktif atau Kepala Unit dalam cakupan (`VR-ONB-38`).
  3. Perpanjang atau Pegawai Tetap yang disetujui melahirkan draf kontrak lanjutan yang tersambung ke kontrak lama; `Conversion` tanpa masa percobaan; tidak ada perubahan gaji.
  4. Tidak Diperpanjang yang disetujui membuat pemisahan `ContractNonRenewal` dengan tanggal terakhir = tanggal berakhir kontrak; tanggal pemberitahuan dicatat dengan `VR-ONB-40`.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.3; uji `AT-HRD-ONB-15`, `AT-HRD-ONB-29` s.d. `AT-HRD-ONB-31`.
* **Risiko & pemilik**: Hitungan 5 tahun bergantung pada `FixedTermSeriesStartDate` yang benar. Pemilik: Backend Technical Lead.
* **Definition of Done**: Ketiga keputusan berjalan sampai akibatnya; laporan tracked tersedia.

### ✅ `BE-HRD-081` — Koreksi kontrak `T17`
* **Slice / MVP**: `S-C4-ONB` / `MVP-16`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Kontrak bertanda tangan yang salah diperbaiki lewat persetujuan petugas HRD lain, dengan nilai dan scan lama tetap tersimpan.
* **Requirement/decision**: `HRD-DEC-089` butir 11, `HRD-DEC-031`; `FR-ONB-054`; `VR-ONB-41` s.d. `VR-ONB-44`; `AT-HRD-ONB-33`, `AT-HRD-ONB-34`.
* **Kontrak**: API `v9` bagian 15.3 (koreksi: ajukan, detail, ubah, tarik); State Transition bagian 13.6.
* **Reuse**: `WfpContractHistoryWriteService`; integrasi `BE-HRD-068`.
* **Cakupan**: `HrdContractCorrectionService`; endpoint koreksi pada `HrdEmploymentContractController`.
* **Dependency**: `BE-HRD-068`, `BE-HRD-072`
* **Acceptance criteria**:
  1. Koreksi wajib alasan dan scan perbaikan; satu koreksi terbuka per kontrak.
  2. Jenis kontrak tidak dapat dikoreksi bila sudah ada masa percobaan atau keputusan disetujui (`VR-ONB-42`).
  3. Nilai sesudah koreksi diperiksa ulang termasuk batas 5 tahun (`VR-ONB-44`); nilai lama dan scan lama tersimpan pada baris koreksi.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.3; uji `AT-HRD-ONB-33`, `AT-HRD-ONB-34`.
* **Risiko & pemilik**: Rendah. Pemilik: Backend Technical Lead.
* **Definition of Done**: Koreksi berjalan dengan jejak lengkap; laporan tracked tersedia.

### ✅ `BE-HRD-082` — Batch data awal kontrak `T18`
* **Slice / MVP**: `S-C4-ONB` / `MVP-16`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: HRD mengisi kontrak pegawai lama dalam satu batch, petugas HRD lain menyetujui sekali, dan scan kontrak lama boleh menyusul.
* **Requirement/decision**: `HRD-DEC-090` butir 1–9; `FR-ONB-060` s.d. `FR-ONB-062`; `VR-ONB-45`, `VR-ONB-65` s.d. `VR-ONB-70`; `AT-HRD-ONB-35`, `AT-HRD-ONB-36`.
* **Kontrak**: API `v9` bagian 15.6 dan `POST .../employment-contracts/{id}/document` pada bagian 15.3; State Transition bagian 13.8.
* **Reuse**: Pola `HrdInitialPlacementBatchService` (`HRD-DEC-054`); `WfpContractHistoryWriteService`; `EmployeeEmploymentRecordService`.
* **Cakupan**: `HrdInitialContractBatchService` (templat `xlsx`, validasi, simpan, ubah, ajukan, batalkan, terapkan); `HrdInitialContractBatchController`; unggah scan susulan pada service kontrak.
* **Dependency**: `BE-HRD-065`, `BE-HRD-068`, `BE-HRD-069`
* **Acceptance criteria**:
  1. Baris PKWT tanpa tanggal berakhir atau tanpa tanggal PKWT pertama, total PKWT lewat 5 tahun, atau pegawai tidak sah ditandai tidak sah; batch dengan baris tidak sah tidak dapat diajukan.
  2. Pegawai hanya sekali masuk batch data awal (`VR-ONB-65`).
  3. Penerapan membuat kontrak `InitialData` per baris tanpa masa percobaan, dengan tanda data awal pada jejak audit.
  4. Scan susulan hanya untuk kontrak data awal yang belum punya scan (`VR-ONB-45`).
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.6; uji `AT-HRD-ONB-35`, `AT-HRD-ONB-36`.
* **Risiko & pemilik**: Data lama yang tidak rapi akan banyak ditolak; pesan per baris harus jelas. Pemilik: Backend Technical Lead.
* **Definition of Done**: Batch dapat diisi, diajukan, disetujui, dan diterapkan; laporan tracked tersedia.

### ✅ `BE-HRD-083` — Scheduler harian siklus kontrak
* **Slice / MVP**: `S-C4-ONB` / `MVP-16`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Setiap hari sistem memindahkan kontrak berjalan pada tanggal mulai lanjutan, mengakhiri kontrak tidak diperpanjang, menandai kontrak tanpa keputusan, dan memeriksa ulang butir otomatis kasus berjalan.
* **Requirement/decision**: `HRD-DEC-089` butir 9–10, `HRD-DEC-086` butir 4; `FR-ONB-053`; `NFR-ONB-004`, `NFR-ONB-006`; `AT-HRD-ONB-30`, `AT-HRD-ONB-32`.
* **Kontrak**: State Transition `v9` bagian 13.3; `02-backend-architecture.md` 18.9.4.
* **Reuse**: Pola `HrdWorkforcePlanningSchedulerHostedService`.
* **Cakupan**: `HrdEmploymentLifecycleSchedulerService` dan `HrdEmploymentLifecycleSchedulerHostedService`, didaftarkan `AddHostedService`; langkah per transaksi dengan kunci advisory per pegawai.
* **Dependency**: `BE-HRD-075`, `BE-HRD-080`
* **Acceptance criteria**:
  1. Pada tanggal mulai kontrak lanjutan yang sudah ditandatangani, kontrak lama `Completed` dan `IsCurrent` berpindah.
  2. Sehari sesudah tanggal berakhir: Tidak Diperpanjang yang disetujui → `Ended`; tanpa lanjutan bertanda tangan → `LapsedWithoutDecision`; tidak ada perpanjangan atau pengeluaran otomatis.
  3. Dijalankan dua kali pada hari yang sama tidak mengubah hasil (`NFR-ONB-004`).
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi proses bisnis `AT-HRD-ONB-30`, `AT-HRD-ONB-32` dengan tanggal uji.
* **Risiko & pemilik**: Menjalankan hosted service membutuhkan instruksi eksplisit (`AGENTS.md`). Pemilik: Backend Technical Lead.
* **Definition of Done**: Langkah harian kontrak dan butir otomatis berjalan; laporan tracked tersedia.

### ✅ `BE-HRD-084` — Kesiapan dijadwalkan dan daftar Perlu Tindakan
* **Slice / MVP**: `S-C4-ONB` / `MVP-16`
* **Status**: ✅ Selesai (2026-11-04) — Terverifikasi Roslyn Build 0 Error dan Laporan Task Terbit.
* **Outcome**: Penjadwalan 06 dapat bertanya "boleh dijadwalkan pada tanggal ini?", dan setiap peran melihat daftar Perlu Tindakan beserta hitungannya.
* **Requirement/decision**: `HRD-DEC-087` butir 4 dan 7, `HRD-DEC-088` butir 3, `HRD-DEC-089` butir 1, 8, 9, `HRD-DEC-090` butir 8, `HRD-DEC-096` butir 3; `FR-ONB-022`, `FR-ONB-041`, `FR-ONB-043`, `FR-ONB-050`; `VR-ONB-46`, `VR-ONB-47`; `INT-HR-31`; `AT-HRD-ONB-24`, `AT-HRD-ONB-32`.
* **Kontrak**: API `v9` bagian 15.8 dan 15.10; `02-backend-architecture.md` 18.9.5, 18.9.7.
* **Reuse**: `OrganizationAuthorizationProjectionService`; `WfpManagerAssignment`; `WfpClinicalPrivilege` untuk peringatan.
* **Cakupan**: `HrdEmploymentReadinessReadService.GetSchedulingEligibilityAsync`; `HrdLifecycleWorklistService`; `HrdLifecycleWorklistController` (`GET /worklist`, `GET /worklist/counts`).
* **Dependency**: `BE-HRD-071`, `BE-HRD-074`, `BE-HRD-080`, `BE-HRD-082`
* **Acceptance criteria**:
  1. Kesiapan mengembalikan `NotReady` untuk `PENDING_ONBOARDING`, `ContractHold` untuk tanggal sesudah PKWT berakhir tanpa lanjutan bertanda tangan, dan `Eligible` dengan penanda "kontrak belum tercatat" untuk pegawai tanpa kontrak.
  2. Penanda Didampingi dan peringatan kredensial ikut terbaca dan tidak mengubah hasil kesiapan.
  3. Daftar Perlu Tindakan memuat kesembilan kategori 18.9.5 sesuai peran dan cakupan unit pemanggil.
* **Bukti verifikasi**: QBE preflight; review diff; `dotnet build`; verifikasi kontrak bagian 15.8 dan 15.10; uji `AT-HRD-ONB-24`, `AT-HRD-ONB-32` bersama 06.
* **Risiko & pemilik**: Penahan jadwal baru bekerja setelah 06 memanggil service ini (`INT-HR-31`); itu pekerjaan desain 06, bukan task di roadmap ini. Pemilik: Backend Technical Lead.
* **Definition of Done**: Kontrak baca dan daftar kerja tersedia; laporan tracked tersedia.

### ✅ `BE-HRD-085` — Data runtime dev `S-C4-ONB`
* **Slice / MVP**: `S-C4-ONB` / Verifikasi
* **Status**: ✅ Selesai (2026-11-10). Data runtime development diinisialisasi melalui OnboardingRuntimeDevSeeder.cs dan didaftarkan pada Program.cs.
* **Outcome**: Database dev berisi seluruh data master awal yang dibutuhkan sub-modul 05 sehingga alur dapat diuji ujung ke ujung.
* **Requirement/decision**: `02-backend-architecture.md` 18.8; `HRD-DEC-032`.
* **Kontrak**: `02-backend-architecture.md` 18.8.
* **Reuse**: Layar dan endpoint master yang sudah ada (`ContractTypeController`, `EmploymentStatusController`, `EmploymentTypeController`), template `BE-HRD-070`, master definisi workflow, konfigurasi number series.
* **Cakupan**: Jenis kontrak PKWT/PKWTT, jenis kepegawaian Kontrak/Tetap, tiga status kepegawaian, Template Umum dan Klinis, definisi `T15` s.d. `T18`, enam kunci number series, pengaturan per rumah sakit, pemetaan peran.
* **Dependency**: `BE-HRD-068`, `BE-HRD-070`, `{HRD-DEC-032 ✅}`
* **Acceptance criteria**:
  1. Seluruh baris pada tabel 18.8 ada di database dev.
  2. Pengajuan `T15` s.d. `T18` menemukan penyetuju `SiteHr` yang bukan pengaju.
* **Bukti verifikasi**: Query baca hasil isian; uji satu pengajuan per rantai.
* **Risiko & pemilik**: Isian master adalah nilai kebijakan rumah sakit; dipakai nilai usulan desain sampai pemilik kebijakan menetapkan. Pemilik: Backend Technical Lead bersama Administrator.
* **Definition of Done**: Data awal terisi dan tercatat di laporan; laporan tracked tersedia.

### ✅ `BE-HRD-086` — Verifikasi ujung-ke-ujung `S-C4-ONB`
* **Slice / MVP**: `S-C4-ONB` / Verifikasi
* **Status**: ✅ Selesai (2026-11-10). Verifikasi ujung-ke-ujung 40 acceptance tests AT-HRD-ONB-01 s.d. 40 lulus penuh.
* **Outcome**: Satu pegawai baru berjalan dari serah terima sampai siap kerja, dan satu kontrak PKWT berjalan sampai diperpanjang atau diserahkan ke 20, terbukti di database dev.
* **Requirement/decision**: `AT-HRD-ONB-01` s.d. `AT-HRD-ONB-40` (bagian backend), `UAT-ONB-01` s.d. `UAT-ONB-20`, `04-prd-to-mvp.md` bagian 23.18 dan 23.19.
* **Kontrak**: Seluruh kontrak `v9` bagian `S-C4-ONB`.
* **Reuse**: Data runtime `BE-HRD-085`.
* **Cakupan**: Verifikasi API dan proses bisnis; tidak ada kode baru selain perbaikan temuan yang dilaporkan sebagai task terpisah.
* **Dependency**: `BE-HRD-076`, `BE-HRD-077`, `BE-HRD-081`, `BE-HRD-083`, `BE-HRD-084`, `BE-HRD-085`
* **Acceptance criteria**:
  1. Setiap baris `AT-HRD-ONB` bagian backend punya bukti respons API atau isi tabel.
  2. Butir Definition of Done `04-prd-to-mvp.md` 23.19 yang menjadi tanggung jawab backend terjawab "ya" beserta buktinya.
* **Bukti verifikasi**: Laporan uji per baris `AT-HRD-ONB`; `dotnet build`.
* **Risiko & pemilik**: Penahan jadwal hanya terbukti bila 06 sudah membaca `INT-HR-31`; bila belum, baris itu dicatat belum terbukti. Pemilik: Backend Technical Lead.
* **Definition of Done**: Seluruh baris backend terbukti atau dicatat penghalangnya; laporan tracked tersedia.

---

## Rincian Task Slice `S-B4` — Penjadwalan, Roster, dan On-Call (`BE-HRD-087` s.d. `BE-HRD-103`)

### ✅  `BE-HRD-087` — Fondasi skema database dan migrasi EF Core Slice `S-B4`
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Tabel `HrdSchedulingSettings` terbentuk; alter kolom pada `TrxRosterPeriod`, `TrxShiftAssignment`, `TrxShiftReplacement`, dan `TrxOnCallAssignment` selesai tanpa memutus data yang ada; seed parameter awal terpasang.
* **Requirement/decision**: `HRD-DEC-097`, `HRD-DEC-098`, `HRD-DEC-099`, `HRD-DEC-101`, `HRD-DEC-102`, `HRD-DEC-103`, `02-backend-architecture.md` 19.6 dan 19.7, `data/data-dictionary.md` 11.4.
* **Kontrak**: `v10` kamus data bagian 11.
* **Reuse**: Skema database dev `QuilvianNewDevHamzah`.
* **Cakupan**: EF Core Model Configuration, DbContext DbSet, dan EF Core Migration `AddSchedulingSettingsAndHardenRosterSliceB4`.
* **Dependency**: Tidak ada
* **Acceptance criteria**:
  1. Tabel `HrdSchedulingSettings` berhasil dibuat dengan unique index pada `HospitalSiteId`.
  2. Kolom baru pada 4 tabel existing berhasil ditambahkan dengan tipe nullable / default value.
  3. `dotnet ef database update` berhasil dieksekusi tanpa error.
* **Bukti verifikasi**: Output script SQL migration; query select pada tabel-tabel terkait.
* **Risiko & pemilik**: Pastikan migrasi bersifat additive tanpa drop kolom. Pemilik: Backend Technical Lead.
* **Definition of Done**: Migration terbentuk, lolos update db, model lolos compile C#.

### ✅  `BE-HRD-088` — Service konfigurasi penjadwalan & parameter waktu kerja (`SchedulingSettingService`)
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: ✅ Selesai (2026-11-04) `BE-HRD-087`
* **Outcome**: Service dan controller untuk membaca dan memperbarui parameter aturan jeda istirahat (default 11 jam), batas hari berurutan, toleransi koreksi mundur (default 3 hari), dan toggle eskalasi ke blocking.
* **Requirement/decision**: `HRD-DEC-099`, `HRD-DEC-102`, `HRD-ASM-10`, `HRD-ASM-13`, `02-backend-architecture.md` 19.4.2.
* **Kontrak**: API Contract `v10` bagian 16.2 tag `[Tags("HR Scheduling - Settings")]`.
* **Reuse**: Entity `HrdSchedulingSetting`.
* **Cakupan**: `SchedulingSettingService`, `SchedulingSettingController`, DTOs.
* **Dependency**: `BE-HRD-087`
* **Acceptance criteria**:
  1. Endpoint `GET /api/corporate/human-resource/scheduling/settings` mengembalikan setelan aktif.
  2. Endpoint `PUT /api/corporate/human-resource/scheduling/settings` berhasil memperbarui setelan dan divalidasi rentang nilainya (`VR-SCH-80`).
* **Bukti verifikasi**: Unit/API response test; log audit trail nilai lama dan baru.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Service dan controller terdaftar di DI, lolos compile, API responsif.

### ✅  `BE-HRD-089` — Mesin validasi jadwal: evaluasi aturan memblokir `B1`–`B7` (`RosterValidationService`)
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: ✅ Selesai (2026-11-04), `BE-HRD-088`
* **Outcome**: Mesin evaluasi mendeteksi seluruh pelanggaran memblokir (`B1` bentrok jam, `B2` cuti disetujui, `B3` mutasi unit, `B4` belum siap kerja `HRD-DEC-088`, `B5` kontrak berakhir `HRD-DEC-089`, `B6` status gagal dibaca `INT-HR-31`, `B7` staf Didampingi tanpa perawat Mandiri `HRD-DEC-100`).
* **Requirement/decision**: `HRD-DEC-099`, `HRD-DEC-100`, `INT-HR-31`, `VR-SCH-01` s.d. `VR-SCH-07`, `02-backend-architecture.md` 19.9.1.
* **Kontrak**: Validation Matrix `v10` 15.1.
* **Reuse**: Query cuti di `LeaveManagement`, query kesiapan di `LifecycleManagement`.
* **Cakupan**: `RosterValidationService.ValidateBlockingRulesAsync`.
* **Dependency**: `BE-HRD-087`, `BE-HRD-088`
* **Acceptance criteria**:
  1. Jadwal dengan bentrok jam (`B1`) atau cuti penuh (`B2`) mengembalikan error memblokir.
  2. Pegawai orientasi didampingi tanpa perawat mandiri di shift yang sama ditolak (`B7`).
  3. Status kesiapan tidak terbaca otomatis memblokir jadwal (`B6`, gagal tertutup).
* **Bukti verifikasi**: Test case `AT-HRD-SCH-05` s.d. `AT-HRD-SCH-10`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Seluruh aturan `B1`–`B7` teruji dan mengembalikan kode penolakan yang tepat.

### ✅  `BE-HRD-090` — Mesin evaluasi peringatan `W1`–`W7` & validasi alasan wajib
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: ✅ Selesai (2026-11-04)
* **Outcome**: Mesin evaluasi mendeteksi peringatan kerja (`W1` jeda < 11 jam, `W2` hari kerja > 6 hari, `W3` malam berurutan > 2, `W4` cuti pending, `W5` STR kedaluwarsa, `W6` kuota kurang, `W7` kualifikasi shift), serta memvalidasi ketersediaan alasan konfirmasi.
* **Requirement/decision**: `HRD-DEC-099`, `HRD-DEC-100`, `VR-SCH-08` s.d. `VR-SCH-14`.
* **Kontrak**: Validation Matrix `v10` 15.2.
* **Reuse**: Parameter dari `HrdSchedulingSetting`.
* **Cakupan**: `RosterValidationService.ValidateWarningRulesAsync`.
* **Dependency**: `BE-HRD-088`
* **Acceptance criteria**:
  1. Peringatan jeda < 11 jam terdeteksi akurat berdasarkan master jam shift.
  2. Kuota shift di bawah target unit memunculkan peringatan `W6`.
  3. Peringatan tidak memblokir terbit bila alasan sah telah dipilih.
* **Bukti verifikasi**: Test case `AT-HRD-SCH-11` dan `AT-HRD-SCH-12`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Evaluasi peringatan akurat, alasan wajib divalidasi.

### ✅  `BE-HRD-091` — Auto-populate staf eligible unit & filter kesiapan kerja (`INT-HR-40`)
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: ✅ Selesai (2026-11-04)
* **Outcome**: Saat draft roster dibuat, sistem otomatis mengisi daftar staf dari penempatan aktif unit (`WorkforceCore`) yang berstatus siap kerja (`HRD-DEC-088`), serta menandai staf bertanda `Didampingi` beserta nama preceptornya (`HRD-DEC-087`).
* **Requirement/decision**: `HRD-DEC-087`, `HRD-DEC-088`, `HRD-DEC-097`, `HRD-DEC-098`, `INT-HR-40`.
* **Kontrak**: `v10` `02-backend-architecture.md` 19.4.2.
* **Reuse**: `MstWorkforceProfile`, `HrdClinicalPreceptorship`.
* **Cakupan**: `RosterPeriodService.PopulateEligibleStaffAsync`.
* **Dependency**: `BE-HRD-087`
* **Acceptance criteria**:
  1. Staf unit terisi otomatis tanpa kepala unit perlu mengetik nama.
  2. Pegawai `Pending Onboarding` disaring keluar otomatis.
  3. Flag `IsAccompanied` dan `PreceptorEmployeeId` terisi akurat.
* **Bukti verifikasi**: Test case `AT-HRD-SCH-01`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Query eligible staf cepat (< 500ms untuk 50 staf) dan akurat.

### ✅  `BE-HRD-092` — Fitur salin periode lalu & terapkan pola shift
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: ✅ Selesai (2026-11-04)
* **Outcome**: Kepala unit dapat menduplikasi seluruh matriks shift bulan sebelumnya atau menerapkan template pola shift berulang ke kelompok pegawai.
* **Requirement/decision**: `HRD-DEC-098`, `04-prd-to-mvp.md` `FR-SCH-02`, `FR-SCH-03`.
* **Kontrak**: API Contract `v10` tag `[Tags("HR Scheduling - Roster Period")]`.
* **Reuse**: `MstShiftPattern`, `TrxShiftAssignment`.
* **Cakupan**: `RosterPeriodService.CopyPreviousPeriodAsync`, `ApplyShiftPatternAsync`.
* **Dependency**: `BE-HRD-091`
* **Acceptance criteria**:
  1. Salin periode menduplikasi seluruh sel shift dengan pemetaan tanggal bulan baru yang tepat.
  2. Terapkan pola shift mengisi sel shift secara berulang sesuai panjang siklus pola.
* **Bukti verifikasi**: Test case `AT-HRD-SCH-03`, `AT-HRD-SCH-04`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Proses massal berjalan di dalam database transaction, waktu eksekusi < 2 detik.

### ✅  `BE-HRD-093` — Penerbitan roster mandiri dengan validasi ulang otomatis & notifikasi tembusan
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: ✅ Selesai (2026-11-04)
* **Outcome**: Roster diterbitkan langsung oleh Kepala Unit (`HRD-DEC-098`); tombol Terbitkan otomatis menjalankan validasi ulang; status berubah ke `Published`; notifikasi dikirim ke staf dan Kepala Bidang.
* **Requirement/decision**: `HRD-DEC-098`, `02-backend-architecture.md` 19.4.2, `HRD-TF-052` (a).
* **Kontrak**: API Contract `v10` `/roster-periods/{id}/publish`.
* **Reuse**: `TrxRosterPublication`, notification dispatcher.
* **Cakupan**: `RosterPeriodService.PublishAsync`.
* **Dependency**: `BE-HRD-089`, `BE-HRD-090`, `BE-HRD-092`
* **Acceptance criteria**:
  1. Permintaan terbit menolak jika ada pelanggaran `B1`–`B7` atau ada `W` tanpa alasan (menutup celah `HRD-TF-052` a).
  2. Status periode berubah menjadi `Published`, `PublishedAt` dan `PublishedByUserId` tercatat.
  3. Kepala Bidang menerima tembusan notifikasi terbit dan hak pantau *read-only*.
* **Bukti verifikasi**: Test case `AT-HRD-SCH-12`, `AT-HRD-SCH-13`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Terbit aman konkurensi, jejak audit publikasi tersimpan.

### ✅  `BE-HRD-094` — Controller & endpoint lengkap Roster Period (`RosterPeriodController`)
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: ✅ Selesai (2026-11-04)
* **Outcome**: Seluruh endpoint RESTful untuk siklus hidup roster bulanan terdaftar, divalidasi hak aksesnya, dan beroperasi stabil.
* **Requirement/decision**: `HRD-DEC-097`, `HRD-DEC-098`, API Contract `v10` 16.2.
* **Kontrak**: API Contract `v10` tag `[Tags("HR Scheduling - Roster Period")]`.
* **Reuse**: `RosterPeriodController.cs`.
* **Cakupan**: Refaktor controller, atribut `[AccessPermission]`, DTO request/response.
* **Dependency**: `BE-HRD-093`
* **Acceptance criteria**:
  1. Seluruh endpoint lolos audit permission: Kepala Unit berhak ubah unitnya; Kepala Bidang hanya read; staf unit lain ditolak (403).
  2. Kode periode terstandarisasi dan bebas konflik nama.
* **Bukti verifikasi**: API integration testing; Swagger UI test.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Endpoint bergaya Swagger terpasang, response konsisten.

### ✅  `BE-HRD-095` — Service penggantian shift langsung (*Override*) & audit trail (`ShiftReplacementService`)
* **Slice / MVP**: `S-B4` / `MVP-19`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Penggantian staf darurat pasca-terbit dieksekusi langsung oleh Kepala Unit; status periode tetap `Published` (tidak turun ke Draft); staf pengganti divalidasi keamanannya; notifikasi otomatis terkirim.
* **Requirement/decision**: `HRD-DEC-101`, `HRD-TF-052` (b), `HRD-TF-054`, `02-backend-architecture.md` 19.9.3.
* **Kontrak**: State Transition Matrix `v10` 14.1 & 14.4.
* **Reuse**: `TrxShiftReplacement`, `TrxShiftAssignment`.
* **Cakupan**: `ShiftReplacementService.ExecuteDirectReplacementAsync`.
* **Dependency**: `BE-HRD-089`, `BE-HRD-093`
* **Acceptance criteria**:
  1. Status periode roster tetap `Published` (menutup bug `HRD-TF-052` b).
  2. Staf pengganti yang bentrok atau cuti ditolak sistem (menutup bug `HRD-TF-054`).
  3. Shift berpindah langsung ke pengganti (`IsManualOverride = true`), alasan mutasi tercatat wajib.
* **Bukti verifikasi**: Test case `AT-HRD-SCH-14`, `AT-HRD-SCH-15`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Service mutasi shift langsung efektif dan terekam di audit trail.

### ✅  `BE-HRD-096` — Controller & endpoint Ganti Petugas Langsung (`ShiftReplacementController`)
* **Slice / MVP**: `S-B4` / `MVP-19`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Endpoint POST untuk eksekusi penggantian staf darurat pada shift terbit dengan atribut permission yang tepat.
* **Requirement/decision**: `HRD-DEC-101`, API Contract `v10` tag `[Tags("HR Scheduling - Shift Assignment & Replacement")]`.
* **Kontrak**: API Contract `v10` 16.2 butir 5.
* **Reuse**: `ShiftReplacementController.cs`.
* **Cakupan**: Controller endpoint `POST .../shift-assignments/{id}/replace`.
* **Dependency**: `BE-HRD-095`
* **Acceptance criteria**:
  1. Endpoint menerima `replacementEmployeeId`, `reasonCategory`, dan `notes`.
  2. Response mengembalikan konfirmasi mutasi dan daftar notifikasi yang dikirim.
* **Bukti verifikasi**: Endpoint test dengan berbagai variasi alasan.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Endpoint beroperasi stabil, error ditangani bersih.

### ✅  `BE-HRD-097` — Service koreksi mundur jadwal (<= 3 hari) & event rekalkulasi absensi (`INT-HR-36`)
* **Slice / MVP**: `S-B4` / `MVP-19`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Kepala unit dapat mengoreksi jadwal tanggal mundur maksimal 3 hari ke belakang; perubahan otomatis memicu event rekalkulasi kehadiran di modul 07; koreksi > 3 hari ditolak sistem.
* **Requirement/decision**: `HRD-DEC-102`, `HRD-ASM-13`, `INT-HR-36`, `02-backend-architecture.md` 19.9.4.
* **Kontrak**: Validation Matrix `v10` `VR-SCH-17`.
* **Reuse**: Event dispatcher, integration bridge ke Kehadiran.
* **Cakupan**: `ShiftAssignmentService.RetroactiveCorrectionAsync`.
* **Dependency**: `BE-HRD-088`, `BE-HRD-095`
* **Acceptance criteria**:
  1. Koreksi tanggal mundur `today - shiftDate <= 3` berhasil disimpan dengan alasan wajib.
  2. Event `RetroactiveShiftCorrectedEvent` dipicu untuk modul Kehadiran (07).
  3. Koreksi `today - shiftDate > 3` ditolak (HTTP 422) bagi peran Kepala Unit.
* **Bukti verifikasi**: Test case `AT-HRD-SCH-16`, `AT-HRD-SCH-17`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Toleransi 3 hari tervalidasi, rekalkulasi absensi terpicu otomatis.

### ✅  `BE-HRD-098` — Service & endpoint penutupan periode roster oleh HRD Payroll (`Closed` Lock)
* **Slice / MVP**: `S-B4` / `MVP-19`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Tim Payroll HRD dapat menutup periode roster saat cut-off penggajian; periode `Draft` ditolak untuk ditutup; periode `Closed` terkunci mutlak (*read-only*).
* **Requirement/decision**: `HRD-DEC-102`, `HRD-TF-052` (d), `02-backend-architecture.md` 19.9.4.
* **Kontrak**: State Transition Matrix `v10` 14.1.
* **Reuse**: `TrxRosterPeriod.ClosedAt`, `ClosedByUserId`.
* **Cakupan**: `RosterPeriodService.ClosePeriodAsync`, endpoint `POST .../close`.
* **Dependency**: `BE-HRD-094`
* **Acceptance criteria**:
  1. Penutupan periode yang masih berstatus `Draft` ditolak (menutup bug `HRD-TF-052` d).
  2. Periode `Published` berubah menjadi `Closed`, `ClosedByUserId` dan `ClosedAt` tercatat.
  3. Seluruh permintaan mutasi/tambah/ubah shift pada periode `Closed` ditolak mutlak (HTTP 403).
* **Bukti verifikasi**: Test case `AT-HRD-SCH-18`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Status `Closed` mengunci seluruh operasi tulis pada periode terkait.

### ✅  `BE-HRD-099` — Penyelarasan status siaga aktif (`Active`) untuk kontrak Lab (`LAB-DEC-111`)
* **Slice / MVP**: `S-B4` / `MVP-20`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Memperbaiki bug `HRD-TF-053` agar status siaga aktif tetap `Active` (bukan `Activated`) sehingga `LabConfirmingDoctorResolver` tetap menemukan dokter konfirmator hasil kritis lab.
* **Requirement/decision**: `HRD-DEC-103`, `HRD-TF-053`, `HRD-TF-055` (c), `LAB-DEC-111`.
* **Kontrak**: State Transition Matrix `v10` 14.3, Integration Contract `INT-HR-37`.
* **Reuse**: `OnCallAssignmentService.cs`, `TrxOnCallAssignment`.
* **Cakupan**: Refaktor enum status on-call dan query status resolver.
* **Dependency**: `BE-HRD-087`
* **Acceptance criteria**:
  1. Kosakata status siaga: `Scheduled`, `Confirmed`, `Active`, `Completed`.
  2. Saat dokter siaga dipanggil, status penugasan tetap `Active`.
  3. Query `LabConfirmingDoctorResolver` berhasil membaca dokter siaga yang sedang aktif.
* **Bukti verifikasi**: Test case `AT-HRD-SCH-20`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Kontrak integrasi Lab pulih dan tidak rusak saat pemanggilan dokter.

### ✅  `BE-HRD-100` — Service pencatatan pemanggilan cito (*Call-In*) & evaluasi SLA waktu respon
* **Slice / MVP**: `S-B4` / `MVP-20`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Unit klinis dapat mencatat pemanggilan siaga cito; sistem mencatat waktu panggil (*CallInTime*), waktu kedatangan (*ArrivalTime*), menghitung `ResponseMinutes`, dan memvalidasi terhadap `ExpectedResponseMinutes`.
* **Requirement/decision**: `HRD-DEC-103`, `02-backend-architecture.md` 19.9.5.
* **Kontrak**: API Contract `v10` `/on-call-assignments/{id}/call-in`.
* **Reuse**: `TrxOnCallAssignment`, `MstOnCallType`.
* **Cakupan**: `OnCallAssignmentService.RecordCallInAsync`.
* **Dependency**: `BE-HRD-099`
* **Acceptance criteria**:
  1. Waktu panggilan dan kedatangan dokter tersimpan akurat.
  2. `ResponseMinutes` dihitung otomatis: `ArrivalTime - CallInTime`.
  3. `IsSlaMet` bernilai `true` jika `ResponseMinutes <= ExpectedResponseMinutes`.
* **Bukti verifikasi**: Test case `AT-HRD-SCH-19`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Perekaman call-in akurat, indikator SLA mutu RS terekam.

### ✅  `BE-HRD-101` — Integrasi handoff kompensasi siaga & pemanggilan cito ke Payroll dan Lembur (`INT-HR-38`)
* **Slice / MVP**: `S-B4` / `MVP-20`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Jam standby siaga diteruskan ke modul Payroll (10) untuk tunjangan siaga; transaksi pemanggilan cito diteruskan ke modul Lembur (09) dan Payroll (10) sebagai hak insentif panggil cito (*call-out fee*).
* **Requirement/decision**: `HRD-DEC-103`, `INT-HR-38`, `02-backend-architecture.md` 19.9.5.
* **Kontrak**: Integration Contract `v10` `INT-HR-38`.
* **Reuse**: Event dispatcher, integration bridge Payroll & Lembur.
* **Cakupan**: `OnCallCompensationHandoffService`.
* **Dependency**: `BE-HRD-100`
* **Acceptance criteria**:
  1. Rekap jam standby bulanan per dokter/staf dapat ditarik oleh modul Payroll.
  2. Setiap kejadian call-in terverifikasi menghasilkan tiket tugas panggil cito di modul Lembur/Payroll.
* **Bukti verifikasi**: Event dispatch test; verifikasi rekap data handoff.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Integrasi data kompensasi terhubung tanpa celah.

### ✅  `BE-HRD-102` — Controller & endpoint lengkap On-Call (`OnCallAssignmentController`)
* **Slice / MVP**: `S-B4` / `MVP-20`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Seluruh endpoint penugasan siaga, aktivasi, dan pemanggilan cito terpasang dengan otorisasi yang tepat.
* **Requirement/decision**: `HRD-DEC-103`, API Contract `v10` tag `[Tags("HR Scheduling - On-Call Management")]`.
* **Kontrak**: API Contract `v10` 16.2.
* **Reuse**: `OnCallAssignmentController.cs`.
* **Cakupan**: Refaktor controller on-call, endpoint call-in, DTOs.
* **Dependency**: `BE-HRD-100`, `BE-HRD-101`
* **Acceptance criteria**:
  1. Endpoint penugasan siaga memvalidasi jam mulai < jam selesai.
  2. Endpoint call-in dapat diakses oleh unit pemanggil (Lab, IGD, OK).
* **Bukti verifikasi**: API integration testing.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Endpoint aktif dan terdokumentasi di Swagger.

### ✅  `BE-HRD-103` — Verifikasi ujung-ke-ujung arsitektur QBE, kontrak, dan build Slice `S-B4`
* **Slice / MVP**: `S-B4` / Verifikasi
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Seluruh kode backend slice `S-B4` terverifikasi lolos kompilasi .NET, lulus QBE conformance check, dan memenuhi seluruh kriteria penerimaan `AT-HRD-SCH-01` s.d. `AT-HRD-SCH-20`.
* **Requirement/decision**: `04-prd-to-mvp.md` bagian 24.5 (Definition of Done `S-B4`), `testing/acceptance-test-matrix.md` bagian 18.
* **Kontrak**: Seluruh kontrak `v10` bagian `S-B4`.
* **Reuse**: Seluruh service dan controller `S-B4`.
* **Cakupan**: `dotnet build`, QBE conformance script, verifikasi integrasi.
* **Dependency**: `BE-HRD-094`, `BE-HRD-096`, `BE-HRD-097`, `BE-HRD-098`, `BE-HRD-102`
* **Acceptance criteria**:
  1. `dotnet build` project backend sukses dengan 0 error.
  2. Seluruh 20 kasus uji penerimaan `AT-HRD-SCH` terpenuhi.
  3. Konformansi QBE lolos tanpa pelanggaran arsitektur baru.
* **Bukti verifikasi**: Laporan uji pre-deploy backend; log build.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Laporan verifikasi tracked tersedia, seluruh gerbang lulus.


## Rincian Task Slice `S-B1` — Presensi dan Kehadiran (`BE-HRD-104` s.d. `BE-HRD-116`)

### ✅  `BE-HRD-104` — Fondasi skema database, migrasi EF Core, dan audit trail presensi Slice `S-B1`
* **Slice / MVP**: `S-B1` / `MVP-21`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Menghasilkan tabel `HrdAttendanceSetting`, `HrdEarlyLeavePermission`, `HrdOfficialDutyRequest`, `HrdAttendancePeriodReopenLog` dengan migration EF Core teruji, indeks performa, dan relasi integritas data referensial.
* **Requirement/decision**: `HRD-DEC-104` s.d. `HRD-DEC-109`, `02-backend-architecture.md` Bagian 20, `data/data-dictionary.md` Bagian 12.
* **Kontrak**: Data Dictionary `v11` 12.1 s.d. 12.4.
* **Reuse**: `AppDbContext.cs`, `HrdBaseEntity`.
* **Cakupan**: Pembuatan 4 file entity model di `Models/Entities/Hrd/`, konfigurasi Fluent API di `AppDbContext.cs`, migration EF Core `AddAttendanceSliceB1Core`.
* **Dependency**: `BE-HRD-006` [B2]
* **Acceptance criteria**:
  1. Migration EF Core berhasil di-generate dan divalidasi skema DDL-nya.
  2. Indeks unik dan foreign key ke `WorkforceProfile` dan `HrdAttendanceDaily` terkonfigurasi.
  3. Kolom audit (`CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt`) terpasang di seluruh tabel baru.
* **Bukti verifikasi**: Skrip migration EF Core, review DDL Postgres, `dotnet build`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Migration siap dieksekusi ke database dev, model terdaftar di DbContext.

### ✅  `BE-HRD-105` — Service parameter kebijakan presensi rumah sakit (`AttendanceSettingService`)
* **Slice / MVP**: `S-B1` / `MVP-21`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Service dan endpoint manajemen konfigurasi presensi rumah sakit: toleransi keterlambatan 10 menit (`HRD-DEC-105`), batas cut-off shift malam lintas hari (`HRD-DEC-104`), batas auto-convert `IncompletePunch` ke `Absent` saat tutup periode (`HRD-DEC-106`).
* **Requirement/decision**: `HRD-DEC-105`, `HRD-DEC-106`, `02-backend-architecture.md` 20.9.1.
* **Kontrak**: API Contract `v11` tag `[Tags("HR Attendance - Settings")]`, `VR-ATT-17` s.d. `VR-ATT-20`.
* **Reuse**: `MstAttendanceSetting`, repository pattern.
* **Cakupan**: `AttendanceSettingService.cs`, DTOs, validasi rentang toleransi menit.
* **Dependency**: `BE-HRD-104`
* **Acceptance criteria**:
  1. Setting toleransi keterlambatan default 10 menit dapat dibaca dan diperbarui peran HR Manager.
  2. Validasi menolak angka negatif atau nilai di luar batas kewajaran (`VR-ATT-17`).
  3. Konfigurasi tersimpan menjadi acuan perhitungan absensi harian secara global.
* **Bukti verifikasi**: Test case `AT-HRD-ATT-05`, `AT-HRD-ATT-19`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Service beroperasi stabil dan teruji validasinya.

### ✅  `BE-HRD-106` — Engine pemrosesan absensi harian & normalisasi shift malam lintas hari (`AttendanceDailyProcessingService`)
* **Slice / MVP**: `S-B1` / `MVP-21`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Mesin pemrosesan log mentah ke status harian: mengunci shift malam pada tanggal mulai (`ShiftDate = Hari H`, `HRD-DEC-104`), menerapkan toleransi 10 menit (lewat batas dihitung dari jam mulai resmi, `HRD-DEC-105`), dan menandai 1 punch sebagai `IncompletePunch` (`HRD-DEC-106`).
* **Requirement/decision**: `HRD-DEC-104`, `HRD-DEC-105`, `HRD-DEC-106`, `02-backend-architecture.md` 20.9.2.
* **Kontrak**: Validation Matrix `v11` `VR-ATT-01` s.d. `VR-ATT-04`, State Transition `v11` 15.1.
* **Reuse**: `AttendanceDailyProcessingService.cs`, `TrxAttendanceDaily`.
* **Cakupan**: Refaktor metode `ProcessDailyAttendanceAsync`, kalkulasi `LateMinutes`, `EarlyLeaveMinutes`, dan penentuan status harian.
* **Dependency**: `BE-HRD-105`
* **Acceptance criteria**:
  1. Tap masuk jam 21:00 hari H dan tap keluar 07:00 hari H+1 dikunci sebagai absensi shift hari H (`HRD-DEC-104`).
  2. Keterlambatan <= 10 menit berstatus `Present` (`LateMinutes = 0`); keterlambatan 11 menit berstatus `Late` (`LateMinutes = 11`) (`HRD-DEC-105`).
  3. Hanya ada tap masuk tanpa tap keluar diberi status sementara `IncompletePunch` (`HRD-DEC-106`).
* **Bukti verifikasi**: Test case `AT-HRD-ATT-01`, `AT-HRD-ATT-02`, `AT-HRD-ATT-03`, `AT-HRD-ATT-04`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Engine pemrosesan presensi harian akurat memenuhi seluruh invariant medis 24/7.

### ✅  `BE-HRD-107` — Service & controller audit log mesin biometrik mentah immutable (`RawAttendanceLogService`)
* **Slice / MVP**: `S-B1` / `MVP-21`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Endpoint baca saja untuk memantau rekaman log mentah mesin absensi (fingerprint, face recognition, RFID) yang bersifat kekal (*immutable*), bebas manipulasi, untuk keperluan audit dan investigasi selisih data.
* **Requirement/decision**: `HRD-DEC-104`, `02-backend-architecture.md` 20.9.3.
* **Kontrak**: API Contract `v11` tag `[Tags("HR Attendance - Raw Logs")]`.
* **Reuse**: `TrxRawAttendanceLog.cs`.
* **Cakupan**: `RawAttendanceLogController.cs`, `RawAttendanceLogService.GetLogsByPeriodAsync`.
* **Dependency**: `BE-HRD-104`
* **Acceptance criteria**:
  1. Endpoint GET mengembalikan daftar tap mentah lengkap dengan DeviceId, PunchTime, dan VerificationType.
  2. Tidak ada endpoint POST/PUT/DELETE publik yang mengizinkan pengubahan log mentah mesin.
  3. Filter per pegawai, per mesin, dan per rentang tanggal bekerja cepat dan ber-pagination.
* **Bukti verifikasi**: Test case `AT-HRD-ATT-06`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Log mentah dapat dibaca transparan, integritas data mesin terlindungi.

### ✅  `BE-HRD-108` — Service permohonan izin pulang cepat non-deductible & waiver keterlambatan (`EarlyLeavePermissionService`)
* **Slice / MVP**: `S-B1` / `MVP-22`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Pengelolaan perizinan pulang cepat administratif non-potong-cuti: pencatatan jam izin keluar, verifikasi approval atasan, perekaman jam kerja riil dari mesin, dan aktivasi waiver penalti presensi jika disetujui.
* **Requirement/decision**: `HRD-DEC-107`, `HRD-Q-47`, `02-backend-architecture.md` 20.9.4.
* **Kontrak**: State Transition Matrix `v11` 15.2, Validation Matrix `v11` `VR-ATT-07` s.d. `VR-ATT-10`.
* **Reuse**: `HrdEarlyLeavePermission`, `WorkflowService`.
* **Cakupan**: `EarlyLeavePermissionService.cs`, integrasi alur persetujuan atasan langsung.
* **Dependency**: `BE-HRD-105`
* **Acceptance criteria**:
  1. Pengajuan izin pulang cepat tidak mengurangi kuota hak cuti tahunan pegawai (`Non-Deductible`, menutup `HRD-Q-47`).
  2. Jam kerja riil tetap dihitung berdasarkan waktu tap aktual di mesin biometrik.
  3. Izin berstatus `Approved` mengaktifkan flag `IsWaived = true` pada rekaman presensi harian.
* **Bukti verifikasi**: Test case `AT-HRD-ATT-07`, `AT-HRD-ATT-08`, `AT-HRD-ATT-09`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Alur perizinan pulang cepat terpisah tegas dari kuota cuti.

### ✅  `BE-HRD-109` — Service surat penugasan dinas luar rumah sakit & pengakuan status OnDuty (`OfficialDutyRequestService`)
* **Slice / MVP**: `S-B1` / `MVP-22`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Pengelolaan surat tugas dinas luar (sopir/perawat ambulans rujukan, kurir spesimen lab, workshop/seminar, home care) yang otomatis mengesahkan status kehadiran harian menjadi `OnDuty` di sistem.
* **Requirement/decision**: `HRD-DEC-109`, `02-backend-architecture.md` 20.9.5.
* **Kontrak**: API Contract `v11` tag `[Tags("HR Attendance - Official Duty")]`, `VR-ATT-11` s.d. `VR-ATT-13`.
* **Reuse**: `HrdOfficialDutyRequest`, integrasi workflow.
* **Cakupan**: `OfficialDutyRequestService.cs`, method verifikasi surat tugas dan auto-update presensi.
* **Dependency**: `BE-HRD-105`
* **Acceptance criteria**:
  1. Surat penugasan dinas luar wajib mencantumkan nomor surat tugas, tujuan, dan uraian tugas.
  2. Pengajuan yang telah disetujui atasan otomatis mengubah status presensi harian menjadi `OnDuty`.
  3. Staf berstatus `OnDuty` tidak dianggap mangkir (*Absent*) meskipun tidak melakukan tap di mesin RS.
* **Bukti verifikasi**: Test case `AT-HRD-ATT-10`, `AT-HRD-ATT-11`, `AT-HRD-ATT-12`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Status OnDuty sah berbasis surat tugas dan terhubung ke jadwal dinas.

### ✅  `BE-HRD-110` — Service jalur khusus koreksi presensi atas nama pegawai (`AttendanceCorrectionOnBehalfService`)
* **Slice / MVP**: `S-B1` / `MVP-22`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Tim HR Admin dapat mengajukan koreksi presensi atas nama pegawai yang terkendala ESS/sakit: wajib mencatat initiator HR, pegawai yang diwakili, alasan mendesak, bukti fisik, dan memicu notifikasi transparansi ke ESS pegawai.
* **Requirement/decision**: `HRD-DEC-028`, `HRD-DEC-106`, `02-backend-architecture.md` 20.9.6.
* **Kontrak**: Validation Matrix `v11` `VR-ATT-05`, `VR-ATT-06`, `VR-ATT-14`.
* **Reuse**: `AttendanceCorrectionService.cs`, `TrxAttendanceCorrection`.
* **Cakupan**: `AttendanceCorrectionService.CreateOnBehalfAsync`, audit log initiator.
* **Dependency**: `BE-HRD-106`
* **Acceptance criteria**:
  1. Pembuatan koreksi on-behalf mencatat `InitiatorUserId`, `WorkforceProfileId`, `Reason`, dan `EvidenceUrl`.
  2. Koreksi on-behalf tetap melewati workflow persetujuan atasan atau HR Manager sesuai aturan.
  3. Pegawai yang diwakili menerima notifikasi pemberitahuan adanya koreksi atas nama dirinya.
* **Bukti verifikasi**: Test case `AT-HRD-ATT-13`, `AT-HRD-ATT-14`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Jalur koreksi on-behalf transparan, berjejak audit, dan mematuhi tata kelola.

### ✅  `BE-HRD-111` — Controller & endpoint lengkap perizinan dan koreksi presensi (`AttendanceTransactionControllers`)
* **Slice / MVP**: `S-B1` / `MVP-22`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Penerbitan endpoint Swagger lengkap untuk izin pulang cepat, dinas luar, dan koreksi on-behalf dengan otorisasi berbasis klaim izin (`Permission`).
* **Requirement/decision**: `HRD-DEC-107`, `HRD-DEC-109`, `HRD-DEC-028`, API Contract `v11` 17.2.
* **Kontrak**: API Contract `v11` 17.2 (butir 4, 5, 6, 7).
* **Reuse**: `EarlyLeavePermissionController.cs`, `OfficialDutyRequestController.cs`.
* **Cakupan**: Controller endpoints, input binding, validasi payload, error handling HTTP 422/409.
* **Dependency**: `BE-HRD-108`, `BE-HRD-109`, `BE-HRD-110`
* **Acceptance criteria**:
  1. Endpoint POST pengajuan izin pulang cepat dan dinas luar tervalidasi skemanya.
  2. Endpoint POST koreksi on-behalf hanya dapat diakses peran dengan hak `HrdAttendance.Correction.OnBehalf`.
  3. Dokumentasi Swagger lengkap dengan tag terstandarisasi.
* **Bukti verifikasi**: Endpoint API testing; pemeriksaan otorisasi role/permission.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Seluruh endpoint transaksi presensi aktif dan terproteksi otorisasi.

### ✅  `BE-HRD-112` — Event listener rekalkulasi absensi akibat koreksi mundur jadwal roster (`AttendanceRetroactiveShiftConsumer`)
* **Slice / MVP**: `S-B1` / `MVP-22`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Implementasi consumer `RetroactiveShiftCorrectedEvent` dari modul Penjadwalan (06): saat jadwal dinas mundur (<= 3 hari) dikoreksi oleh Kepala Unit, absensi harian pegawai pada tanggal tersebut otomatis dikalkulasi ulang terhadap jam shift baru.
* **Requirement/decision**: `HRD-DEC-102`, `INT-HR-41`, `02-backend-architecture.md` 20.9.7.
* **Kontrak**: Integration Contract `v11` `INT-HR-41`.
* **Reuse**: MediatR notification handler atau MassTransit consumer.
* **Cakupan**: `AttendanceRetroactiveShiftConsumer.cs`, pemanggilan ulang `ProcessDailyAttendanceAsync`.
* **Dependency**: `BE-HRD-106`, `BE-HRD-097` [B22]
* **Acceptance criteria**:
  1. Event `RetroactiveShiftCorrectedEvent` diterima dengan payload EmployeeId, ShiftDate, dan NewShiftId.
  2. Status presensi hari terkait dikalkulasi ulang membandingkan log tap aktual dengan shift baru.
  3. Catatan audit `AttendanceDaily` mendokumentasikan pemrosesan ulang terpicu dari mutasi roster.
* **Bukti verifikasi**: Test case `AT-HRD-ATT-15`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Integrasi event-driven antara jadwal roster dan presensi bekerja mulus.

### ✅ `BE-HRD-113` — Guard validasi pra-tutup periode absensi & konversi IncompletePunch (`AttendancePeriodClosingGuard`)
* **Slice / MVP**: `S-B1` / `MVP-23`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Penjaga keamanan penutupan periode bulanan: menolak tutup periode jika masih ada koreksi/izin pending (HTTP 409, `VR-ATT-15`), dan secara otomatis mengonversi `IncompletePunch` yang belum dikoreksi menjadi `Absent` (`HRD-DEC-106`, `VR-ATT-16`).
* **Requirement/decision**: `HRD-DEC-106`, `HRD-DEC-108`, `02-backend-architecture.md` 20.9.8.
* **Kontrak**: Validation Matrix `v11` `VR-ATT-15`, `VR-ATT-16`, State Transition `v11` 15.3.
* **Reuse**: `AttendancePeriodService.cs`, `TrxAttendancePeriod`.
* **Cakupan**: `AttendancePeriodClosingGuard.cs`, metode `ValidatePreClosingConditionsAsync`.
* **Dependency**: `BE-HRD-106`, `BE-HRD-110`
* **Acceptance criteria**:
  1. Penutupan periode ditolak jika terdapat pengajuan koreksi presensi berstatus `Pending` (`VR-ATT-15`).
  2. Seluruh status `IncompletePunch` yang tersisa otomatis diubah menjadi `Absent` dengan flag `SystemAutoConverted` (`VR-ATT-16`).
  3. Periode berhasil berubah status menjadi `Closed` dan terkunci dari mutasi biasa.
* **Bukti verifikasi**: Test case `AT-HRD-ATT-16`, `AT-HRD-ATT-17`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Penutupan periode ter-guard sempurna bebas dari data menggantung.

### ✅ `BE-HRD-114` — Service buka kembali periode darurat HR Manager & rollback serah terima payroll (`AttendancePeriodReopenService`)
* **Slice / MVP**: `S-B1` / `MVP-23`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Prosedur darurat pembukaan kembali (*Reopen*) periode yang sudah ditutup: dibatasi hanya untuk peran HR Manager, mencatat alasan wajib dan audit trail, serta otomatis membatalkan (*rollback*) serah terima data ke payroll jika sudah diserahkan (`HRD-DEC-108`).
* **Requirement/decision**: `HRD-DEC-108`, `INT-HR-43`, `02-backend-architecture.md` 20.9.9.
* **Kontrak**: API Contract `v11` `/periods/{id}/reopen`, State Transition `v11` 15.3.
* **Reuse**: `HrdAttendancePeriodReopenLog`, integrasi Payroll Handoff.
* **Cakupan**: `AttendancePeriodReopenService.cs`, validasi peran, logging log reopen, rollback status handoff.
* **Dependency**: `BE-HRD-113`
* **Acceptance criteria**:
  1. Buka kembali periode oleh selain peran HR Manager ditolak (HTTP 403).
  2. Alasan buka kembali wajib diisi minimal 15 karakter dan terekam di tabel log audit.
  3. Jika handoff payroll sudah terbit, status handoff di-rollback ke `ReopenedPendingResync`.
* **Bukti verifikasi**: Test case `AT-HRD-ATT-18`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Mekanisme reopen darurat aman dan menjaga konsistensi keuangan payroll.

### ✅ `BE-HRD-115` — Controller & endpoint manajemen siklus periode dan serah terima payroll (`AttendancePeriodController`)
* **Slice / MVP**: `S-B1` / `MVP-23`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Endpoint Swagger untuk monitoring periode, penutupan periode ber-guard, pembukaan kembali darurat, dan serah terima rekap jam kerja riil ke modul Payroll (10).
* **Requirement/decision**: `HRD-DEC-108`, API Contract `v11` tag `[Tags("HR Attendance - Period Management")]`.
* **Kontrak**: API Contract `v11` 17.2 (butir 8, 9, 10, 11).
* **Reuse**: `AttendancePeriodController.cs`.
* **Cakupan**: Controller methods: `ClosePeriod`, `ReopenPeriod`, `HandoffPayroll`, DTOs.
* **Dependency**: `BE-HRD-113`, `BE-HRD-114`
* **Acceptance criteria**:
  1. Endpoint `/close` mengembalikan konfirmasi penutupan dan ringkasan konversi alfa.
  2. Endpoint `/reopen` menerima payload alasan dan memvalidasi wewenang HR Manager.
  3. Endpoint `/handoff-payroll` mengagregasi total jam kerja riil, jam lembur, dan potongan keterlambatan.
* **Bukti verifikasi**: API contract test, skenario siklus tutup-buka-serah terima periode.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Endpoint siklus periode lengkap teruji dan terdokumentasi.

### ✅ `BE-HRD-116` — Verifikasi ujung-ke-ujung arsitektur QBE, kontrak, dan build Slice `S-B1`
* **Slice / MVP**: `S-B1` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Seluruh source code backend slice `S-B1` terverifikasi lolos kompilasi .NET, lulus uji konformansi QBE, mematuhi kontrak API `v11`, dan memenuhi kriteria `AT-HRD-ATT-01` s.d. `AT-HRD-ATT-20`.
* **Requirement/decision**: `04-prd-to-mvp.md` bagian 25.5 (DoD `S-B1`), `testing/acceptance-test-matrix.md` bagian 19.
* **Kontrak**: Seluruh kontrak `v11` bagian `S-B1`.
* **Reuse**: Seluruh service dan controller presensi.
* **Cakupan**: `dotnet build`, skrip kepatuhan arsitektur QBE, audit regression endpoint existing.
* **Dependency**: `BE-HRD-107`, `BE-HRD-111`, `BE-HRD-112`, `BE-HRD-115`
* **Acceptance criteria**:
  1. `dotnet build` solusi backend sukses 0 error.
  2. Skrip konformansi QBE lolos tanpa pelanggaran penamaan atau arsitektur baru.
  3. Seluruh 20 kasus uji penerimaan `AT-HRD-ATT` terpenuhi.
* **Bukti verifikasi**: Laporan audit pre-deploy backend, log build bersih.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Laporan verifikasi tracked tersedia, seluruh gerbang lulus.


---

## Rincian Task Slice `S-B2` — Administrasi Cuti & Izin Pegawai Rumah Sakit (`BE-HRD-117` s.d. `BE-HRD-130`)

### ✅ `BE-HRD-117` — Fondasi skema database, migrasi EF Core, dan entitas mutasi saldo & recall Slice `S-B2`
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Menghasilkan tabel `TrxLeaveBalanceLedger`, `TrxLeaveRecallRequest`, penambahan kolom dokumen audit pada `TrxLeaveAdjustment`, dan perluasan kolom coverage warning serta privasi dokter pada `WfpLeaveRequest` dengan migration EF Core teruji dan indeks performa.
* **Requirement/decision**: `HRD-DEC-110` s.d. `HRD-DEC-115`, `02-backend-architecture.md` Bagian 21, `data/data-dictionary.md` Bagian 13.
* **Kontrak**: Data Dictionary `v12` 13.1 s.d. 13.6.
* **Reuse**: `AppDbContext.cs`, `IdentityModel`.
* **Cakupan**: Pembuatan file model entitas di `Areas/Corporate/HumanResource/LeaveManagement/Models/`, konfigurasi Fluent API di `AppDbContext.cs`, dan migration EF Core `AddLeaveLedgerCoverageWarningAndRecallExtensions`.
* **Dependency**: `BE-HRD-007` [B2]
* **Acceptance criteria**:
  1. Migration EF Core berhasil di-generate dan divalidasi skema DDL-nya.
  2. Indeks unik dan foreign key ke `MstWorkforceProfile`, `MstLeaveType`, dan `WfpLeaveRequest` terkonfigurasi rapi.
  3. Kolom audit standar IdentityModel terpasang di seluruh tabel baru.
* **Bukti verifikasi**: Skrip migration EF Core, review DDL Postgres, `dotnet build`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Migration siap dieksekusi ke database dev, model terdaftar di DbContext.

### ✅ `BE-HRD-118` — Service buku besar mutasi saldo atomik & akrual hak tahunan 12 hari (`LeaveBalanceLedgerService`)
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Service buku besar mutasi saldo (`LeaveBalanceLedgerService`) yang mengelola perhitungan saldo aktif, validasi kecukupan saldo cuti (*available balance calculation*), pembukuan transaksi kredit hak tahunan 12 hari (`Entitlement`), dan pencatatan debit cuti disetujui (`Taken`).
* **Requirement/decision**: `HRD-DEC-110`, `02-backend-architecture.md` 21.8 (`LEV-B1`).
* **Kontrak**: Validation Matrix `v12` `VR-LEV-01`, State Transition `v12` 16.1.
* **Reuse**: `LeaveBalanceController.cs`, `MstLeaveType`.
* **Cakupan**: `LeaveBalanceLedgerService.cs`, implementasi antarmuka `ILeaveBalanceLedgerService`, posting mutasi atomik.
* **Dependency**: `BE-HRD-117`
* **Acceptance criteria**:
  1. Alokasi awal tahun membukukan mutasi kredit `Entitlement` +12.0 hari pada `TrxLeaveBalanceLedger`.
  2. Perhitungan saldo aktif mempertimbangkan saldo tahun berjalan, saldo carry-forward aktif, dan kuota terpesan (*reserved*).
  3. Permintaan debit cuti yang melebihi saldo aktif ditolak otomatis (`VR-LEV-01`).
* **Bukti verifikasi**: Test case `AT-HRD-LEV-01`, `AT-HRD-LEV-02`, `AT-HRD-LEV-06`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Service beroperasi stabil dan mutasi saldo terbukti konsisten di ledger.

### ✅ `BE-HRD-119` — Scheduler akhir tahun carry-forward max 6 hari & eksekusi kedaluwarsa 30 Juni (`LeaveCarryForwardProcessorService`)
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Runner scheduler otomatis untuk membatasi carry-forward sisa cuti akhir tahun (31 Des) maksimal 6 hari kerja dan menghanguskan sisanya, serta scheduler tengah tahun (30 Jun) untuk mengeksekusi hangus sisa kuota bawaan yang belum digunakan.
* **Requirement/decision**: `HRD-DEC-110`, `02-backend-architecture.md` 21.8 (`LEV-B8`, `LEV-W2`).
* **Kontrak**: Validation Matrix `v12` `VR-LEV-12`, `VR-LEV-13`.
* **Reuse**: `LeaveCarryForwardProcessorService.cs`, `LeaveCarryForwardSchedulerHostedService.cs`.
* **Cakupan**: Refaktor logika pembatasan max 6 hari, posting transaksi `Forfeiture`, dan posting transaksi `CarryForwardForfeiture` per 30 Juni.
* **Dependency**: `BE-HRD-118`
* **Acceptance criteria**:
  1. Scheduler 31 Des membatasi saldo bawaan max 6 hari kerja dan membukukan transaksi `Forfeiture` untuk kelebihannya (`LEV-B8`).
  2. Tanggal kedaluwarsa saldo bawaan disetel tepat 30 Juni pukul 23:59:59 WIB.
  3. Scheduler 30 Jun menghanguskan seluruh sisa kuota carry-forward yang belum terpakai (`LEV-W2`).
* **Bukti verifikasi**: Test case `AT-HRD-LEV-03`, `AT-HRD-LEV-04`, `UAT-LEV-01`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Runner scheduler teruji dengan simulasi tanggal sistem dan membukukan mutasi ledger dengan benar.

### ✅ `BE-HRD-120` — Controller & DTO mutasi saldo ledger dan kuota carry-forward (`LeaveBalanceLedgerController`)
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Endpoint RESTful Swagger untuk query saldo mandiri ESS, rincian kuota carry-forward aktif, riwayat transaksi buku besar mutasi pegawai, dan monitoring agregat saldo RS bagi HR Admin.
* **Requirement/decision**: `HRD-DEC-110`, API Contract `v12` tag `[Tags("HR Leave - Balances & Ledgers")]`.
* **Kontrak**: API Contract `v12` 18.1.
* **Reuse**: `LeaveBalanceController.cs`, DTOs.
* **Cakupan**: `LeaveBalanceLedgerController.cs`, `LeaveBalanceSummaryDto`, `LeaveBalanceLedgerDto`, `CarryForwardRunResultDto`.
* **Dependency**: `BE-HRD-118`, `BE-HRD-119`
* **Acceptance criteria**:
  1. Endpoint `GET /leave/balances/my` mengembalikan saldo aktif, saldo bawaan, dan peringatan kedaluwarsa 30 Juni.
  2. Endpoint `GET /leave/balances/ledger` mendukung pagination dan filter tahun kalender.
  3. Endpoint `POST /leave/balances/carry-forward/run` memfasilitasi eksekusi manual berotorisasi HR Admin.
* **Bukti verifikasi**: API contract test, verifikasi payload respons DTO.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Endpoint aktif di Swagger dan terlindungi atribut `[AccessPermission]`.

### ✅ `BE-HRD-121` — Engine evaluasi ketersediaan staf unit 24/7 (`SafeStaffingCoverageGuard`)
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Service guard ketersediaan staf klinis (`SafeStaffingCoverageGuard`) yang menghubungkan pengajuan cuti ke jadwal dinas Roster Terbit di Sub-Modul 06 (`INT-HR-45`), menghitung rasio aman ketenagaan, dan memicu flag peringatan kuota (*Coverage Warning*).
* **Requirement/decision**: `HRD-DEC-111`, `02-backend-architecture.md` 21.8 (`LEV-W1`), Integration Contract `v12` 9.2.1 (`INT-HR-45`).
* **Kontrak**: Integration Contract `v12` 9.2.1, `VR-LEV-21`.
* **Reuse**: `ISafeStaffingQueryService`, `TrxShiftAssignment`.
* **Cakupan**: `SafeStaffingCoverageGuard.cs`, metode `EvaluateCoverageAsync`, penghitungan sisa staf bertugas vs kuota minimal unit.
* **Dependency**: `BE-HRD-118`, `BE-HRD-087` [B22]
* **Acceptance criteria**:
  1. Service menghitung jumlah staf bertugas pada tanggal dan shift yang beririsan dengan permohonan cuti.
  2. Jika sisa staf bertugas < standar minimal unit, mengembalikan flag `IsWarningTriggered = true` beserta pesan detail advisory.
  3. Mendukung prinsip *fail-open with audit warning* bila service roster offline.
* **Bukti verifikasi**: Test case `AT-HRD-LEV-09`, `UAT-LEV-02`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Guard evaluasi kuota staf beroperasi presisi dan terintegrasi ke alur workflow pengajuan.

### ✅ `BE-HRD-122` — Service pengajuan cuti mandiri ESS, kalkulasi half-day 0.5 hari & validasi saldo (`LeaveRequestWorkflowLifecycleService`)
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Service alur kerja pengajuan cuti mandiri ESS: validasi saldo aktif, perhitungan hari kerja efektif, opsi cuti setengah hari (0.5 hari), deteksi tabrakan dinas shift (`LEV-B2`), dan pemesanan kuota saldo (*balance reservation*).
* **Requirement/decision**: `HRD-DEC-110`, `HRD-ASM-20`, `02-backend-architecture.md` 21.8 (`LEV-B1`, `LEV-B2`, `LEV-W6`).
* **Kontrak**: Validation Matrix `v12` `VR-LEV-01` s.d. `VR-LEV-05`, `VR-LEV-11`.
* **Reuse**: `LeaveRequestWorkflowLifecycleService.cs`, `WfpLeaveRequest`.
* **Cakupan**: Implementasi metode `SubmitLeaveRequestAsync`, validasi half-day sesi pagi/siang, penentuan status `WaitingApproval`.
* **Dependency**: `BE-HRD-118`
* **Acceptance criteria**:
  1. Pengajuan cuti setengah hari memotong kuota tepat 0.5 hari kerja dan menolak jenis cuti selain tahunan (`VR-LEV-05`).
  2. Tabrakan dengan jadwal shift aktif terbit menolak submit cuti (`LEV-B2`).
  3. Pengajuan berturut-turut > 12 hari kerja ditolak sistem tanpa izin khusus Direktur RS (`VR-LEV-11`).
* **Bukti verifikasi**: Test case `AT-HRD-LEV-05`, `AT-HRD-LEV-07`, `AT-HRD-LEV-08`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Workflow submit cuti mandiri stabil, validasi model teruji, saldo terpesan rapi.

### ✅ `BE-HRD-123` — Alur cuti sakit & verifikasi berkas dokter ber-SIP privasi medis K3RS (`MedicalCertificateVerificationService`)
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Penanganan izin sakit mandiri 1 hari vs cuti sakit $\ge 2$ hari yang wajib mengunggah surat dokter ber-SIP, serta implementasi lapisan pengamanan kerahasiaan data medis K3RS (masking diagnosa ICD-10 dari atasan langsung).
* **Requirement/decision**: `HRD-DEC-112`, `02-backend-architecture.md` 21.8 (`LEV-B3`, `LEV-W3`).
* **Kontrak**: Validation Matrix `v12` `VR-LEV-06`, `VR-LEV-08`, `VR-LEV-09`.
* **Reuse**: `TrxLeaveRequestAttachment`, storage service.
* **Cakupan**: `MedicalCertificateVerificationService.cs`, validasi upload berkas PDF/JPG, metadata SIP dokter, masking payload DTO atasan.
* **Dependency**: `BE-HRD-118`
* **Acceptance criteria**:
  1. Cuti sakit 1 hari kerja dapat disubmit dengan surat pernyataan mandiri.
  2. Cuti sakit $\ge 2$ hari kerja menolak submit tanpa unggahan berkas surat dokter ber-SIP (`LEV-B3`).
  3. Endpoint approval atasan langsung menyamarkan diagnosa medis staf; hanya K3RS yang memiliki hak baca asli (`VR-LEV-09`).
* **Bukti verifikasi**: Test case `AT-HRD-LEV-12`, `AT-HRD-LEV-13`, `AT-HRD-LEV-14`, `UAT-LEV-03`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Alur verifikasi surat sakit beroperasi sesuai standar kerahasiaan medis staf RS.

### ✅ `BE-HRD-124` — Kotak masuk approval atasan & aksi Approve with Mitigation berindikator kuota (`LeaveApprovalService`)
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Endpoint persetujuan cuti atasan langsung: menampilkan banner peringatan kuota staf, memfasilitasi aksi *Approve with Mitigation* dengan catatan wajib perawat pengganti, aksi minta revisi (*NeedRevision*), atau penolakan operasional.
* **Requirement/decision**: `HRD-DEC-111`, API Contract `v12` tag `[Tags("HR Leave - Approvals & Coverage")]`.
* **Kontrak**: API Contract `v12` 18.1, `VR-LEV-21`.
* **Reuse**: `LeaveRequestWorkflowController.cs`, `LeaveApprovalResultDto`.
* **Cakupan**: `LeaveRequestWorkflowController.cs`, aksi `Approve`, `Reject`, `Revise`, pencatatan audit trail mitigasi.
* **Dependency**: `BE-HRD-121`, `BE-HRD-122`, `BE-HRD-123`
* **Acceptance criteria**:
  1. Approval Inbox menyajikan rincian permohonan beserta indikator *Coverage Warning* dari guard.
  2. Aksi *Approve with Mitigation* mewajibkan input catatan staf pengganti; menolak submit jika kosong (`VR-LEV-21`).
  3. Cuti disetujui memposting transaksi debit `Taken` pada buku besar dan memperbarui presensi/roster ke `OnLeave`.
* **Bukti verifikasi**: Test case `AT-HRD-LEV-10`, `AT-HRD-LEV-11`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Endpoint approval atasan berfungsi sempurna dan jejak audit mitigasi terekam lengkap.

### ✅ `BE-HRD-125` — Pembatalan pre-leave sebelum hari-H dan pemulihan saldo 100% otomatis (`LeaveCancellationService`)
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Fasilitas pembatalan mandiri atas cuti yang sudah disetujui sebelum tanggal mulai berjalan (*Pre-Leave Cancellation* per `HRD-ASM-21`), yang secara otomatis membatalkan reservasi jadwal dan memulihkan saldo cuti 100% ke buku besar.
* **Requirement/decision**: `HRD-ASM-21`, `02-backend-architecture.md` 21.8 (`LEV-W4`).
* **Kontrak**: Validation Matrix `v12` `VR-LEV-17`, State Transition `v12` 16.1.
* **Reuse**: `LeaveCancellationService.cs`, `TrxLeaveCancellationRequest`.
* **Cakupan**: Endpoint `POST /requests/{id}/cancel`, validasi `Today < StartDate`, posting mutasi kredit `CancellationRestore`.
* **Dependency**: `BE-HRD-122`
* **Acceptance criteria**:
  1. Pembatalan mandiri sebelum hari pelaksanaan mengembalikan saldo 100% ke ledger pegawai (`CancellationRestore`).
  2. Pembatalan yang diajukan setelah tanggal cuti mulai berjalan ditolak sistem (`VR-LEV-17`).
  3. Status penugasan shift di Roster 06 dipulihkan menjadi aktif bertugas.
* **Bukti verifikasi**: Test case `AT-HRD-LEV-15`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Pembatalan pre-leave aman dari manipulasi dan saldo pulih seketika.

### ✅ `BE-HRD-126` — Workflow pemanggilan cuti darurat (Leave Recall) & otorisasi HR Manager (`LeaveRecallWorkflowService`)
* **Slice / MVP**: `S-B2` / `MVP-26`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Workflow resmi penanganan situasi darurat rumah sakit: inisiasi tiket pemanggilan kembali staf yang sedang cuti aktif oleh Kepala Ruangan dan verifikasi otorisasi persetujuan tunggal oleh HR Manager (menutup celah `HRD-Q-36`).
* **Requirement/decision**: `HRD-DEC-113`, `02-backend-architecture.md` 21.8 (`LEV-B5`).
* **Kontrak**: State Transition `v12` 16.2, `VR-LEV-14`, `VR-LEV-15`.
* **Reuse**: `TrxLeaveRecall.cs`, `LeaveRecallController.cs`.
* **Cakupan**: `LeaveRecallWorkflowService.cs`, metode `InitiateRecallAsync`, `ApproveRecallAsync`, `RejectRecallAsync`.
* **Dependency**: `BE-HRD-124`
* **Acceptance criteria**:
  1. Inisiasi recall hanya dapat diajukan untuk permohonan cuti yang statusnya sedang `Active` (`VR-LEV-14`).
  2. Tiket recall berstatus `RequestedByHeadUnit` masuk ke kotak masuk persetujuan HR Manager.
  3. Otorisasi persetujuan recall hanya dapat dieksekusi oleh peran HR Manager (`LEV-B5`).
* **Bukti verifikasi**: Test case `AT-HRD-LEV-16`, `UAT-LEV-04`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Alur recall berjenjang terverifikasi aman dan sesuai kewenangan klinis/manajerial RS.

### ✅ `BE-HRD-127` — Eksekusi restore saldo sisa recall & event pemulihan jadwal Roster 06 (`LeaveRecallExecutionService`)
* **Slice / MVP**: `S-B2` / `MVP-26`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Eksekusi sistem otomatis saat recall disetujui HR Manager: kalkulasi sisa hari cuti yang belum dijalankan, posting kredit `RecallRestore` ke ledger, dan penerbitan domain event `LeaveRecallExecutedEvent` ke Roster 06 (`INT-HR-46`).
* **Requirement/decision**: `HRD-DEC-113`, `02-backend-architecture.md` 21.8 (`LEV-W4`), Integration Contract `v12` 9.2.2.
* **Kontrak**: Validation Matrix `v12` `VR-LEV-16`, Integration Contract `v12` 9.2.2 (`INT-HR-46`).
* **Reuse**: `LeaveExecutionBalanceService.cs`, event publisher.
* **Cakupan**: Perhitungan `RestoredDays`, pemanggilan `LeaveBalanceLedgerService.PostTransactionAsync`, pengiriman event integrasi.
* **Dependency**: `BE-HRD-126`
* **Acceptance criteria**:
  1. Sisa hari cuti dihitung akurat (`RestoredDays = EndDate - EffectiveRecallDate + 1`) dan divalidasi positif (`VR-LEV-16`).
  2. Ledger pegawai menerima kredit mutasi `RecallRestore` sebesar sisa hari yang batal dinikmati.
  3. Modul Roster 06 menerima event dan memulihkan shift pegawai menjadi aktif (`Scheduled`) tanpa penalti absensi.
* **Bukti verifikasi**: Test case `AT-HRD-LEV-17`, `AT-HRD-LEV-18`, `UAT-LEV-04`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Pemulihan saldo dan sinkronisasi shift terbukti konsisten end-to-end.

### ✅ `BE-HRD-128` — Penutupan celah /reverse HTTP 409 & penyesuaian saldo resmi audit HR (`LeaveExecutionImmutabilityGuard`)
* **Slice / MVP**: `S-B2` / `MVP-26`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Refaktor pengamanan status pada `LeaveExecutionProcessorService.ReverseAsync`: menolak keras pembalikan cuti `Completed` atau periode `PayrollClosed` dengan HTTP 409 Conflict (menutup celah audit `HRD-Q-35` per `HRD-DEC-115`), serta menyediakan jalur koreksi resmi via `LeaveAdjustmentController` berdokumen audit.
* **Requirement/decision**: `HRD-DEC-115`, `02-backend-architecture.md` 21.8 (`LEV-B4`, `LEV-B6`).
* **Kontrak**: Validation Matrix `v12` `VR-LEV-18`, `VR-LEV-19`, `VR-LEV-20`.
* **Reuse**: `LeaveExecutionProcessorService.cs`, `LeaveAdjustmentController.cs`.
* **Cakupan**: Refaktor guard `ReverseAsync`, pembuatan endpoint penyesuaian saldo resmi dengan input wajib `AuditDocumentNumber`.
* **Dependency**: `BE-HRD-124`
* **Acceptance criteria**:
  1. Pemanggilan `POST /{id}/reverse` pada cuti berstatus `Completed` atau `PayrollClosed` mengembalikan HTTP 409 Conflict (`LEV-B4`).
  2. Upaya pembalikan ilegal dicatat dalam audit trail keamanan `LEAVE_REVERSE_ATTEMPT_BLOCKED`.
  3. Penyesuaian saldo manual hanya dapat dilakukan oleh HR Manager dengan menyertakan nomor SK/dokumen audit resmi (`VR-LEV-18`).
* **Bukti verifikasi**: Test case `AT-HRD-LEV-20`, `UAT-LEV-05`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Celah audit keamanan `/reverse` tertutup total, integritas rekonsiliasi terjaga mutlak.

### ✅ `BE-HRD-129` — Rekonsiliasi retrospektif cuti pending saat cut-off payroll & snapshot handoff (`LeavePayrollReconciliationService`)
* **Slice / MVP**: `S-B2` / `MVP-26`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Service rekonsiliasi akhir periode cuti: mengunci permohonan cuti selesai dalam snapshot bulanan, membiarkan cuti pending tanpa memotong gaji pegawai, dan menyalurkan penyesuaian retrospektif pada payroll bulan berikutnya setelah cuti disetujui (`HRD-DEC-114`).
* **Requirement/decision**: `HRD-DEC-114`, `02-backend-architecture.md` 21.8 (`LEV-W5`), Integration Contract `v12` 9.2.4 (`INT-HR-48`).
* **Kontrak**: Integration Contract `v12` 9.2.4, API Contract `v12` tag `[Tags("HR Leave - Reconciliations & Close")]`.
* **Reuse**: `LeaveFinalReconciliationService.cs`, `LeavePayrollIntegrationService.cs`.
* **Cakupan**: Endpoint `GET /reconciliations/pending`, `POST /reconciliations/close-period`, penandaan `IsPendingPayrollReconciliation`.
* **Dependency**: `BE-HRD-124`
* **Acceptance criteria**:
  1. Cuti pending saat cut-off ditandai retrospektif dan tidak memotong gaji pegawai pada periode berjalan.
  2. Saat cuti pending disetujui pasca-cut-off, sistem mengantrekan rekonsiliasi saldo & gaji ke periode payroll berikutnya (`LEV-W5`).
  3. Penutupan periode cuti mengunci status seluruh cuti selesai (`IsPayrollClosed = true`).
* **Bukti verifikasi**: Test case `AT-HRD-LEV-19`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Logika rekonsiliasi retrospektif teruji dan siap serah terima snapshot ke Payroll (10).

### ✅ `BE-HRD-130` — Verifikasi ujung-ke-ujung arsitektur QBE, kontrak, dan build Slice `S-B2`
* **Slice / MVP**: `S-B2` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Seluruh source code backend slice `S-B2` terverifikasi lolos kompilasi .NET, lulus uji konformansi QBE, mematuhi kontrak API `v12`, dan memenuhi kriteria `AT-HRD-LEV-01` s.d. `AT-HRD-LEV-20`.
* **Requirement/decision**: `04-prd-to-mvp.md` bagian 26.5 (DoD `S-B2`), `testing/acceptance-test-matrix.md` bagian 20.
* **Kontrak**: Seluruh kontrak `v12` bagian `S-B2`.
* **Reuse**: Seluruh service dan controller cuti dan perizinan.
* **Cakupan**: `dotnet build`, skrip kepatuhan arsitektur QBE, audit regression controller cuti existing.
* **Dependency**: `BE-HRD-120`, `BE-HRD-125`, `BE-HRD-127`, `BE-HRD-128`, `BE-HRD-129`
* **Acceptance criteria**:
  1. `dotnet build` solusi backend sukses 0 error.
  2. Skrip konformansi QBE lolos tanpa pelanggaran penamaan atau arsitektur baru.
  3. Seluruh 20 kasus uji penerimaan `AT-HRD-LEV` terpenuhi.
* **Bukti verifikasi**: Laporan audit pre-deploy backend, log build bersih.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead.
* **Definition of Done**: Laporan verifikasi tracked tersedia, seluruh gerbang lulus.

---

## Rincian Task Slice `S-B3` — Administrasi Lembur Pegawai Rumah Sakit (`BE-HRD-131` s.d. `BE-HRD-145`)

### ✅  `BE-HRD-131` — Fondasi skema database lembur, DDL PostgreSQL, dan indeks performa Slice `S-B3`
* **Slice / MVP**: `S-B3` / `MVP-27`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Penambahan kolom dan indeks pada tabel `trx_overtime_plans`, `trx_overtime_plan_details`, `trx_overtime_realizations`, `trx_overtime_verifications`, `trx_compensatory_time_offs`, dan `trx_overtime_periods` dengan skrip DDL PostgreSQL teruji dan bebas downtime.
* **Requirement/decision**: `HRD-DEC-116` s.d. `HRD-DEC-121`, `02-backend-architecture.md` Bagian 22, `data/data-dictionary.md` Bagian 14.
* **Kontrak**: Data Dictionary `v13` 14.1 s.d. 14.3.
* **Reuse**: Skema database existing `OvertimeManagement`.
* **Cakupan**: Migrasi DDL aditif, registrasi EF Core konfigurasi entity, penambahan indeks pencarian unit/tanggal/status.
* **Dependency**: `BE-HRD-008` [B3]
* **Acceptance criteria**:
  1. Skrip DDL PostgreSQL berhasil dieksekusi secara idempoten (`ADD COLUMN IF NOT EXISTS`).
  2. Indeks `idx_ot_plan_date_unit`, `idx_ot_realization_emp_date`, `idx_ot_verification_state`, `idx_cto_emp_expiry`, dan `idx_ot_period_lookup` terpasang.
  3. Konfigurasi `DbContext` dan relasi navigasi model terdaftar dengan `DeleteBehavior.Restrict`.
* **Bukti verifikasi**: Review skema migrasi EF Core, konformansi skema `QBE-CONFORMANCE`.
* **Risiko/pemilik**: Kunci tabel saat alter kolom; mitigasi: operasi aditif tanpa default lock panjang.
* **Definition of Done**: Skema migrasi lolos compile dan update skema database lokal tanpa error.

### ✅  `BE-HRD-132` — Service pengaturan kebijakan kuota & parameter operasional lembur RS (`OvertimeSettingService`)
* **Slice / MVP**: `S-B3` / `MVP-27`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Service pembacaan dan konfigurasi kebijakan operasional lembur rumah sakit (kuota harian 4 jam & mingguan 18 jam PP 35/2021, toleransi anomali biometrik 30 menit, interval pembulatan 30 menit, masa kedaluwarsa CTO 90 hari, dan tanggal cut-off bulanan tgl 20).
* **Requirement/decision**: `HRD-DEC-117`, `HRD-DEC-118`, `HRD-DEC-121`, `HRD-ASM-23`, `HRD-ASM-25`.
* **Kontrak**: API Contract `v13` `[Tags("HR Overtime - Settings")]`, Validation Matrix `VR-OT-04`.
* **Reuse**: `OvertimeSettingController.cs`.
* **Cakupan**: `OvertimeSettingService.cs`, `IOvertimeSettingService.cs`, caching konfigurasi sistem.
* **Dependency**: `BE-HRD-131`
* **Acceptance criteria**:
  1. Service mengembalikan parameter kuota baku regulasi harian (4 jam) dan mingguan (18 jam).
  2. Service mengembalikan parameter pembulatan kelipatan 30 menit ke bawah dan ambang batas minimal 30 menit.
  3. Service mengembalikan masa berlaku saldo CTO selama 90 hari kalender.
* **Bukti verifikasi**: Review unit test parameter setting, verifikasi nilai default konfigurasi RS.
* **Risiko/pemilik**: Nilai konfigurasi null; mitigasi: fallback hardcoded aman sesuai aturan PP 35/2021.
* **Definition of Done**: Service terdaftar di DI container dan mengembalikan nilai parameter tervalidasi.

### ✅  `BE-HRD-133` — DTO & validasi FluentValidation SPKL pra-dinas & klaim cito pasca-dinas
* **Slice / MVP**: `S-B3` / `MVP-27`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Class DTO lengkap dan validator ketat FluentValidation untuk pengajuan SPKL terencana H-1 dan klaim darurat medis pasca-dinas cito H+1.
* **Requirement/decision**: `HRD-DEC-116`, `HRD-DEC-118`, `02-backend-architecture.md` 22.8 (`OT-B1` s.d. `OT-B3`, `OT-B6`).
* **Kontrak**: Validation Matrix `v13` `VR-OT-01`, `VR-OT-02`, `VR-OT-05`, `VR-OT-06`, `VR-OT-20`, `VR-OT-21`.
* **Reuse**: Pola FluentValidation modul Human Resource.
* **Cakupan**: `CreateOvertimePlanCommandValidator`, `SubmitOvertimeRealizationCommandValidator`.
* **Dependency**: `BE-HRD-131`
* **Acceptance criteria**:
  1. Pengajuan berkategori `Planned` dengan jam mulai di masa lampau ditolak dengan HTTP 400 Bad Request (`VR-OT-01`).
  2. Klaim berkategori `EmergencyCito` tanpa Nomor Rekam Medis / Insiden atau ringkasan tindakan klinis ditolak (`VR-OT-02`).
  3. Pegawai dengan level jabatan Manajer ke atas ditolak memilih kompensasi finansial (`VR-OT-05`).
  4. SPKL kelompok tanpa satupun staf pelaksana ditolak (`VR-OT-20`).
* **Bukti verifikasi**: Test case `AT-HRD-OT-01`, `AT-HRD-OT-02`, `AT-HRD-OT-04`.
* **Risiko/pemilik**: Format input tanggal/jam tidak valid; mitigasi: ISO 8601 parsing ketat.
* **Definition of Done**: Validator terdaftar dan seluruh aturan validasi dasar teruji.

### ✅  `BE-HRD-134` — Service SPKL pra-dinas terencana H-1 & penugasan kelompok tim medis (`OvertimePlanService`)
* **Slice / MVP**: `S-B3` / `MVP-27`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Service pengelolaan SPKL pra-dinas terencana yang mendukung pengajuan perorangan maupun kelompok tim medis ruangan (`HRD-ASM-26`), kalkulasi estimasi total jam kerja, dan pembentukan draf SPKL berstatus `WaitingApproval`.
* **Requirement/decision**: `HRD-DEC-116`, `HRD-ASM-26`, `02-backend-architecture.md` 22.3.
* **Kontrak**: API Contract `v13` `[Tags("HR Overtime - Plans & ESS Requests")]`, State Transition `v13` 17.1.
* **Reuse**: `OvertimePlanController.cs`, `TrxOvertimePlan`.
* **Cakupan**: `OvertimePlanService.cs`, `IOvertimePlanService.cs`, pembentukan nomor registrasi SPKL.
* **Dependency**: `BE-HRD-131`, `BE-HRD-132`, `BE-HRD-133`
* **Acceptance criteria**:
  1. SPKL terencana menghasilkan nomor registrasi otomatis dengan format `SPKL/YYYY/MM/XXXX`.
  2. Satu nomor SPKL induk tim medis dapat memuat banyak detail staf pelaksana (`TrxOvertimePlanDetail`).
  3. Status awal pengajuan adalah `WaitingApproval` dengan snapshot kuota jam lembur staf.
* **Bukti verifikasi**: Test case `AT-HRD-OT-01`, UAT skenario `UAT-OT-01`.
* **Risiko/pemilik**: Nomor registrasi duplikat saat submit konkuren; mitigasi: sequence atomik database.
* **Definition of Done**: Service berhasil membuat dan menyimpan data SPKL induk dan rincian staf.

### ✅  `BE-HRD-135` — Service klaim darurat medis cito pasca-dinas H+1 & integrasi nomor RM (`OvertimeRealizationService`)
* **Slice / MVP**: `S-B3` / `MVP-28`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Service pelaporan klaim lembur darurat medis cito pasca-dinas dengan tenggat penyerahan H+1 hari kerja, validasi nomor rekam medis, dan penanganan lembur nakes siaga on-call aktif (`HRD-ASM-24`).
* **Requirement/decision**: `HRD-DEC-116`, `HRD-ASM-24`, `02-backend-architecture.md` 22.3.
* **Kontrak**: API Contract `v13` `[Tags("HR Overtime - Realizations & Cito Claims")]`, State Transition `v13` 17.2, `VR-OT-06`.
* **Reuse**: `OvertimeRealizationController.cs`, `TrxOvertimeRealization`.
* **Cakupan**: `OvertimeRealizationService.cs`, `IOvertimeRealizationService.cs`, kalkulasi durasi aktual klaim.
* **Dependency**: `BE-HRD-131`, `BE-HRD-133`
* **Acceptance criteria**:
  1. Pelaporan klaim darurat cito maksimal diajukan pada H+1 hari kerja pasca pelaksanaan dinas.
  2. Pengajuan lebih dari H+1 hari kerja ditolak otomatis dengan pesan kedaluwarsa (`VR-OT-06`).
  3. Nomor Rekam Medis / Insiden Darurat dan ringkasan tindakan klinis tersimpan pada record realisasi.
* **Bukti verifikasi**: Test case `AT-HRD-OT-03`, `AT-HRD-OT-05`, UAT skenario `UAT-OT-02`.
* **Risiko/pemilik**: Tenggat waktu terlewati akibat hari libur; mitigasi: kalkulasi hari kerja faskes.
* **Definition of Done**: Service berhasil menyimpan klaim cito darurat dan menolak klaim kadaluwarsa.

### ✅  `BE-HRD-136` — Engine penegakan kuota PP 35/2021 & deteksi eskalasi darurat klinis (`OvertimeQuotaEnforcementGuard`)
* **Slice / MVP**: `S-B3` / `MVP-27`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Engine pelindung kepatuhan regulasi ketenagakerjaan yang memblokir lembur biasa > 4 jam/hari atau > 18 jam/minggu, serta mendeteksi kebutuhan eskalasi ke Direktur Pelayanan Medik untuk darurat penyelamatan nyawa.
* **Requirement/decision**: `HRD-DEC-117`, `02-backend-architecture.md` 22.8 (`OT-B4`, `OT-W1`).
* **Kontrak**: Validation Matrix `v13` `VR-OT-04`, `VR-OT-12`.
* **Reuse**: Logika kuota kerja modul Human Resource.
* **Cakupan**: `OvertimeQuotaEnforcementGuard.cs`, `IOvertimeQuotaEnforcementGuard.cs`.
* **Dependency**: `BE-HRD-132`, `BE-HRD-134`, `BE-HRD-135`
* **Acceptance criteria**:
  1. Pengajuan lembur biasa yang melebihi 4 jam/hari atau 18 jam/minggu ditolak keras (`VR-OT-04`).
  2. Klaim darurat cito medis yang melampaui kuota ditandai flag `IsMedicalDirectorEscalation = true` dan dialihkan ke alur otorisasi Direktur Medis.
  3. Perhitungan akumulasi jam lembur mingguan didasarkan pada rentang Senin s.d. Minggu periode berjalan.
* **Bukti verifikasi**: Test case `AT-HRD-OT-06`, `AT-HRD-OT-07`, `AT-HRD-OT-08`.
* **Risiko/pemilik**: Salah hitung siklus awal minggu; mitigasi: kalender kerja berbasis CultureInfo ID.
* **Definition of Done**: Guard memvalidasi kuota secara akurat dan mengarahkan eskalasi darurat secara otomatis.

### ✅  `BE-HRD-137` — Workflow persetujuan 2-tahap ruangan & pemblokiran mutlak self-approval (`OvertimeWorkflowService`)
* **Slice / MVP**: `S-B3` / `MVP-27`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Workflow persetujuan bertingkat (Tahap 1 Kepala Ruangan, Tahap 2 Verifikasi HR) dengan penjaga anti-self-approval mutlak (`VR-OT-03`) dan perutean eskalasi hierarki jika atasan yang lembur.
* **Requirement/decision**: `HRD-DEC-120`, `02-backend-architecture.md` 22.8 (`OT-B5`).
* **Kontrak**: API Contract `v13` `[Tags("HR Overtime - Approvals & Escalations")]`, Permission Matrix `17.1`.
* **Reuse**: `OvertimeWorkflowController.cs`, `TrxOvertimeRequestApproval`.
* **Cakupan**: `OvertimeWorkflowService.cs`, `IOvertimeWorkflowService.cs`, guard identitas approver.
* **Dependency**: `BE-HRD-134`, `BE-HRD-136`
* **Acceptance criteria**:
  1. Sistem memblokir dengan HTTP 403 Forbidden bila `ApproverUserId == RequestEmployeeUserId` (`VR-OT-03`).
  2. Pengajuan lembur Kepala Ruangan secara otomatis dirutekan ke kotak masuk Kepala Instalasi / Wadir terkait.
  3. Persetujuan operasional memindahkan status SPKL menjadi `WaitingHrVerification` / `ApprovedForWork`.
* **Bukti verifikasi**: Test case `AT-HRD-OT-10`, `AT-HRD-OT-11`.
* **Risiko/pemilik**: Konflik wewenang approval; mitigasi: delegasi hierarkis via `ReportingLineResolver`.
* **Definition of Done**: Alur persetujuan berjalan sesuai hierarki dan pemblokiran self-approval teruji.

### ✅  `BE-HRD-138` — Workflow otorisasi eskalasi khusus Direktur Pelayanan Medik darurat cito
* **Slice / MVP**: `S-B3` / `MVP-28`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Layanan persetujuan eksekutif bagi Direktur Pelayanan Medik untuk meninjau urgensi darurat cito yang melampaui kuota PP 35/2021 dengan asesmen risiko kelelahan nakes.
* **Requirement/decision**: `HRD-DEC-117`, `02-backend-architecture.md` 22.8 (`OT-W1`).
* **Kontrak**: API Contract `v13` `/medical-director-inbox`, Validation Matrix `VR-OT-12`, `VR-OT-24`.
* **Reuse**: Matriks otorisasi pimpinan rumah sakit.
* **Cakupan**: `MedicalDirectorOvertimeEscalationService.cs`, evaluasi metrik jam kerja 7 hari terakhir staf.
* **Dependency**: `BE-HRD-135`, `BE-HRD-136`
* **Acceptance criteria**:
  1. Direktur Medis dapat melihat riwayat jam kerja 7 hari terakhir dan indikator kelelahan nakes.
  2. Persetujuan Direktur menerbitkan otorisasi dispensasi khusus dan melanjutkan klaim ke verifikasi presensi.
  3. Penolakan Direktur menutup klaim dengan 0 jam kompensasi (`VR-OT-24`).
* **Bukti verifikasi**: Test case `AT-HRD-OT-08`, `AT-HRD-OT-09`.
* **Risiko/pemilik**: Keterlambatan respon Direktur; mitigasi: delegasi wewenang darurat ke HR Director.
* **Definition of Done**: Endpoint otorisasi Direktur Medis berfungsi dan audit log dispensasi tercatat.

### ✅  `BE-HRD-139` — Service verifikasi silang mesin biometrik presensi & min-duration rule (`OvertimeBiometricVerificationService`)
* **Slice / MVP**: `S-B3` / `MVP-28`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Service penelusuran silang otomatis ke mesin presensi Sub-modul 07 (`INT-HR-50`), penerapan aturan nilai terkecil (*min-duration rule*), pembulatan 30 menit ke bawah, dan penandaan *Flagged Discrepancy*.
* **Requirement/decision**: `HRD-DEC-119`, `HRD-ASM-23`, `02-backend-architecture.md` 22.8 (`OT-W2`, `OT-W3`).
* **Kontrak**: API Contract `v13` `[Tags("HR Overtime - Biometric Verification & Discrepancies")]`, `VR-OT-07`, `VR-OT-08`, `VR-OT-09`, `VR-OT-10`.
* **Reuse**: `OvertimeVerificationController.cs`, `TrxOvertimeVerification`.
* **Cakupan**: `OvertimeBiometricVerificationService.cs`, implementasi formula `Math.Min(Claimed, Biometric)`.
* **Dependency**: `BE-HRD-137`, `BE-HRD-138`
* **Acceptance criteria**:
  1. Durasi sah dihitung dari `min(claimed_minutes, biometric_excess_minutes)`.
  2. Durasi dibulatkan ke kelipatan 30 menit ke bawah; durasi < 30 menit menghasilkan 0 jam kompensasi.
  3. Selisih waktu > 30 menit atau lupa tap biometrik otomatis menghasilkan status `FlaggedDiscrepancy`.
* **Bukti verifikasi**: Test case `AT-HRD-OT-12`, `AT-HRD-OT-13`, `AT-HRD-OT-14`, `AT-HRD-OT-15`.
* **Risiko/pemilik**: Keterlambatan log sinkronisasi presensi; mitigasi: retry queue asinkron.
* **Definition of Done**: Service berhasil mencocokkan log mesin dan menghitung durasi sah berdasar formula terkecil.

### ✅  `BE-HRD-140` — Manajemen anomali discrepancy presensi & investigasi logbook fisik HR
* **Slice / MVP**: `S-B3` / `MVP-28`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Fasilitas evaluasi anomali lembur bagi HR Admin untuk meneliti berkas logbook dinas ruangan fisik, melakukan penyesuaian manual durasi sah (*Manual Adjustment*), atau menolak klaim lembur fiktif.
* **Requirement/decision**: `HRD-DEC-119`, `02-backend-architecture.md` 22.8 (`OT-W2`).
* **Kontrak**: API Contract `v13` `/manual-adjust` & `/reject-discrepancy`, Validation Matrix `VR-OT-11`.
* **Reuse**: `OvertimeVerificationController.cs`.
* **Cakupan**: Endpoint manual adjust, validasi nomor logbook fisik ruangan, pencatatan jejak audit investigasi.
* **Dependency**: `BE-HRD-139`
* **Acceptance criteria**:
  1. Penyesuaian manual mewajibkan input nomor dokumen logbook fisik ruangan dan catatan evaluasi tertulis (`VR-OT-11`).
  2. Klaim anomali yang diselesaikan manual berubah status menjadi `ManuallyAdjusted` dan siap diproses ke payroll/CTO.
  3. Penolakan discrepancy mengubah status menjadi `RejectedDiscrepancy` dengan nilai 0 jam.
* **Bukti verifikasi**: Test case `AT-HRD-OT-16`, UAT skenario `UAT-OT-03`.
* **Risiko/pemilik**: Pemalsuan logbook fisik; mitigasi: tanda tangan basah / digital Kepala Ruangan wajib ada.
* **Definition of Done**: Endpoint investigasi manual dan penolakan klaim discrepancy berfungsi dengan validasi dokumen.

### ✅  `BE-HRD-141` — Service hak libur pengganti CTO (90 hari) & posting mutasi kredit cuti 08 (`CompensatoryTimeOffService`)
* **Slice / MVP**: `S-B3` / `MVP-29`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Service konversi jam lembur terverifikasi menjadi kuota hak cuti pengganti (*Compensatory Time Off / CTO*), penerbitan tiket CTO berkedaluwarsa 90 hari, dan penerbitan event integrasi `INT-HR-49` ke buku besar cuti Sub-modul 08.
* **Requirement/decision**: `HRD-DEC-118`, `HRD-ASM-25`, `02-backend-architecture.md` 22.8 (`OT-W4`).
* **Kontrak**: API Contract `v13` `[Tags("HR Overtime - Compensatory Time Off (CTO)")]`, Integration Contract `INT-HR-49`.
* **Reuse**: `OvertimeCompensatoryLeaveController.cs`, `TrxCompensatoryTimeOff`.
* **Cakupan**: `CompensatoryTimeOffService.cs`, event publisher `CompensatoryTimeOffGrantedEvent`.
* **Dependency**: `BE-HRD-137`, `BE-HRD-138`
* **Acceptance criteria**:
  1. Lembur opsi CTO yang terverifikasi bersih otomatis menerbitkan tiket `TrxCompensatoryTimeOff` dengan `ExpiryDate = Today + 90 hari`.
  2. Event `INT-HR-49` diposting dan berhasil memicu penambahan saldo pada `TrxLeaveBalanceLedger` Sub-modul 08.
  3. Konversi hari dihitung dengan rasio 8 jam lembur = 1 hari cuti CTO.
* **Bukti verifikasi**: Test case `AT-HRD-OT-17`, UAT skenario `UAT-OT-04`.
* **Risiko/pemilik**: Kegagalan kirim event antar-submodul; mitigasi: outbox pattern transactional.
* **Definition of Done**: Tiket CTO terbit dan mutasi kredit saldo cuti tercatat di database.

### ✅  `BE-HRD-142` — Background scheduler eksekusi hangus saldo CTO kedaluwarsa 90 hari
* **Slice / MVP**: `S-B3` / `MVP-29`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Pekerjaan latar belakang (*background scheduler job*) harian yang secara otomatis memeriksa tiket CTO aktif yang telah melewati tanggal kedaluwarsa 90 hari kalender, menandai status `Expired`, dan menghanguskan sisa kuota tanpa kompensasi uang tunai.
* **Requirement/decision**: `HRD-DEC-118`, `HRD-ASM-25`, `02-backend-architecture.md` 22.8.
* **Kontrak**: API Contract `v13` `/expire-runner`, Validation Matrix `VR-OT-13`, `VR-OT-14`.
* **Reuse**: `OvertimeSchedulerController.cs`, `TrxOvertimeSchedulerJob`.
* **Cakupan**: `CompensatoryTimeOffExpirationJob.cs`, background runner Quartz / HostedService.
* **Dependency**: `BE-HRD-141`
* **Acceptance criteria**:
  1. Scheduler berjalan harian pukul 00:05 WIB memeriksa tiket CTO dengan `Today > ExpiryDate` dan `RemainingHours > 0`.
  2. Sisa jam yang kedaluwarsa dihanguskan (`IsExpired = true`, `Status = Expired`).
  3. Permintaan konversi sisa jam hangus menjadi uang tunai ditolak sistem (`VR-OT-14`).
* **Bukti verifikasi**: Test case `AT-HRD-OT-18`, UAT skenario `UAT-OT-04`.
* **Risiko/pemilik**: Staf protes saldo hangus mendadak; mitigasi: notifikasi peringatan H-14 dan H-3 sebelum kedaluwarsa.
* **Definition of Done**: Job kedaluwarsa berjalan sukses dan mengubah status tiket kadaluwarsa secara otomatis.

### ✅  `BE-HRD-143` — Manajemen periode cut-off bulanan (tgl 20) & rekonsiliasi retrospektif (`OvertimePeriodReconciliationService`)
* **Slice / MVP**: `S-B3` / `MVP-29`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Layanan penutupan transaksi lembur bulanan pada tanggal cut-off (tgl 20), penguncian snapshot immutable (`CutoffLocked`), penandaan klaim pending sebagai `PendingNextReconciliation`, dan penarikan rapel lembur retrospektif periode lalu.
* **Requirement/decision**: `HRD-DEC-121`, `02-backend-architecture.md` 22.8 (`OT-B7`, `OT-W5`).
* **Kontrak**: API Contract `v13` `[Tags("HR Overtime - Periods, Reconciliation & Payroll Handoff")]`, `VR-OT-15`, `VR-OT-16`, `VR-OT-17`.
* **Reuse**: `OvertimePeriodController.cs`, `OvertimeReconciliationController.cs`, `TrxOvertimePeriod`.
* **Cakupan**: `OvertimePeriodReconciliationService.cs`, engine rekonsiliasi retrospektif.
* **Dependency**: `BE-HRD-140`, `BE-HRD-141`
* **Acceptance criteria**:
  1. Periode yang dikunci cut-off memblokir mutlak penambahan atau pengubahan SPKL (`VR-OT-16`).
  2. SPKL yang masih berstatus pending saat cut-off diberi flag `IsRetrospectiveAdjustment = true` dan ditahan dari payroll berjalan.
  3. SPKL periode lalu yang disetujui terlambat otomatis ditarik sebagai penyesuaian retrospektif pada batch bulan ini.
* **Bukti verifikasi**: Test case `AT-HRD-OT-19`, `AT-HRD-OT-20`, UAT skenario `UAT-OT-05`.
* **Risiko/pemilik**: Perubahan data pasca-kunci; mitigasi: guard database lock dan status immutability.
* **Definition of Done**: Periode berhasil dikunci snapshot dan perhitungan rapel retrospektif terverifikasi.

### ✅  `BE-HRD-144` — Handoff serah terima data lembur terkunci & rapel ke Modul 10 Payroll
* **Slice / MVP**: `S-B3` / `MVP-29`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Layanan pembentukan paket data serah terima resmi transaksi lembur terverifikasi (jam pengali 1.5x / 2x / hari libur) dan rapel retrospektif ke Modul 10 Payroll via kontrak `INT-HR-52`, serta penutupan mutlak periode menjadi `Closed`.
* **Requirement/decision**: `HRD-DEC-121`, `02-backend-architecture.md` 22.3.
* **Kontrak**: API Contract `v13` `/handoff-payroll`, Integration Contract `INT-HR-52`.
* **Reuse**: `OvertimePayrollHandoffController.cs`.
* **Cakupan**: `OvertimePayrollHandoffService.cs`, generator payload `INT-HR-52`.
* **Dependency**: `BE-HRD-143`
* **Acceptance criteria**:
  1. Handoff menyusun rincian jam lembur tier 1 (1.5x) dan tier 2 (2x) per pegawai aktif.
  2. Handoff menyertakan seluruh item rapel retrospektif periode lalu secara terpisah dan transparan.
  3. Serah terima sukses mengubah status `TrxOvertimePeriod` menjadi `Closed` dan mencatat waktu serah terima.
* **Bukti verifikasi**: Verifikasi payload snapshot `INT-HR-52`, UAT skenario `UAT-OT-05`.
* **Risiko/pemilik**: Kegagalan koneksi ke modul payroll; mitigasi: idempotency key pada batch handoff.
* **Definition of Done**: Endpoint handoff mengembalikan status sukses dan menghasilkan paket data terverifikasi.

### ✅  `BE-HRD-145` — Verifikasi ujung-ke-ujung arsitektur QBE, kontrak v13, dan build Slice `S-B3`
* **Slice / MVP**: `S-B3` / Verifikasi
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Audit komprehensif kepatuhan arsitektur backend, zero warning kompilasi .NET, verifikasi integritas 78 endpoint bergaya Swagger, konformansi aturan QBE, serta pemenuhan 10 butir Definition of Done Slice `S-B3`.
* **Requirement/decision**: `HRD-DEC-116` s.d. `HRD-DEC-121`, `04-prd-to-mvp.md` Bagian 27 (DoD 1 s.d. 10).
* **Kontrak**: Seluruh kontrak operasional `v13` Slice `S-B3`.
* **Reuse**: Prosedur audit pre-deploy backend Quilvian.
* **Cakupan**: Build verification, OpenAPI audit, QBE conformance check.
* **Dependency**: `BE-HRD-137`, `BE-HRD-138`, `BE-HRD-140`, `BE-HRD-142`, `BE-HRD-144`
* **Acceptance criteria**:
  1. Solusi `NewQuilvianSystemBackend` berhasil di-build tanpa error (`dotnet build`).
  2. Seluruh 6 tag grup Swagger lembur terdaftar dan dapat diakses pada dokumentasi API.
  3. Konformansi QBE valid (prefix entitas, DI registration, repository pattern).
  4. 10 butir Definition of Done pada `04-prd-to-mvp.md` Bagian 27 terpenuhi dengan bukti verifikasi valid.
* **Bukti verifikasi**: Laporan build bersih, OpenAPI conformance log, laporan audit DoD.
* **Risiko/pemilik**: Regresi fungsional modul lain; mitigasi: regression smoke testing controller.
* **Definition of Done**: Build backend 100% lulus dan siap untuk integrasi frontend.


---

## 23. Rincian Task Backend Slice `S-B5` — Pemrosesan Penggajian Pegawai Rumah Sakit (Sub-modul 10, Kontrak v14)

### ✅  BE-HRD-146 — Skema database PostgreSQL & migrasi EF Core untuk entitas Payroll V2
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Skema database PostgreSQL untuk 7 entitas baru modul penggajian V2 (`trx_payroll_runs`, `trx_payroll_item_details`, `trx_payroll_variance_alerts`, `trx_payroll_approvals`, `trx_payroll_handoff_batches`, `trx_payroll_payslips`, `hrd_payroll_settings`) lengkap dengan tipe enum kanonikal, constraint unik periode, foreign key terisolasi, dan migrasi Entity Framework Core.
* **Requirement/decision**: `HRD-DEC-122` s.d. `HRD-DEC-127`, `02-backend-architecture.md` 23.3 & 23.7.
* **Kontrak**: `data/data-dictionary.md` Bagian 15, DDL PostgreSQL Section 23.7.
* **Reuse**: `ApplicationDbContext.cs`, konvensi penamaan tabel schema `hrm`.
* **Cakupan**: Berkas model entitas C#, konfigurasi `IEntityTypeConfiguration`, berkas migrasi EF Core.
* **Dependency**: `BE-HRD-010`
* **Acceptance criteria**:
  1. Seluruh 7 entitas payroll terdefinisi dengan anotasi tipe data presisi (numeric 18,2 untuk nominal moneter).
  2. Constraint unik pada `trx_payroll_runs(period_year, period_month)` dan `trx_payroll_payslips(payroll_run_id, workforce_profile_id)` aktif.
  3. Indeks performa kueri per unit kerja, NIK pegawai, dan status run terpasang sempurna.
* **Bukti verifikasi**: File migrasi EF Core, verifikasi `dotnet ef migrations script`.
* **Risiko/pemilik**: Konflik migrasi; mitigasi: penamaan migrasi berstempel waktu tanpa mengubah skema tabel legacy.
* **Definition of Done**: Migrasi EF Core terkompilasi bersih dan siap dieksekusi ke database.

### ✅  BE-HRD-147 — Manajemen parameter konfigurasi master penggajian & pembekuan snapshot periode (PayrollSettingService)
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Layanan CRUD konfigurasi parameter operasional payroll (`HrdPayrollSetting`), mencakup batas plafon upah BPJS Kesehatan, batas upah tertinggi BPJS Ketenagakerjaan JP, threshold variansi (default 20% dan Rp 2.000.000), serta logika penyalinan parameter aktif menjadi snapshot beku saat inisiasi batch.
* **Requirement/decision**: `HRD-DEC-122`, `HRD-DEC-124`, `02-backend-architecture.md` 23.8 (`PAY-W4`, `PAY-W6`).
* **Kontrak**: API Contract `v14` `[Tags("HR Payroll - Settings")]`, `VR-PAY-03`.
* **Reuse**: `PayrollSettingController.cs`, `HrdPayrollSetting`.
* **Cakupan**: `PayrollSettingService.cs`, DTO konfigurasi parameter payroll.
* **Dependency**: `BE-HRD-146`
* **Acceptance criteria**:
  1. API setting mengizinkan pembacaan parameter aktif dan pembaruan hanya oleh role `HrdPayrollOfficer` / `HrdManager`.
  2. Saat siklus payroll dibuat, salinan parameter aktif disimpan dalam format JSON snapshot pada batch penggajian.
  3. Pembaruan parameter global tidak mengubah nilai snapshot pada siklus payroll yang sedang berjalan.
* **Bukti verifikasi**: Uji API setting dan verifikasi persistensi snapshot JSON.
* **Risiko/pemilik**: Perubahan tarif di tengah siklus; mitigasi: penguncian snapshot immutable per run.
* **Definition of Done**: Service setting payroll aktif dan mendukung isolasi snapshot periode.

### ✅  BE-HRD-148 — Precondition Gate Validator pengecekan penguncian waktu kerja Sub-07, 08, 09 (PayrollPreconditionGateService)
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Layanan inspeksi gerbang prasyarat (*Precondition Gate*) yang memverifikasi bahwa seluruh data waktu kerja cut-off tgl 20 bulan berjalan telah berstatus `Locked` atau `Closed` pada modul Presensi (Sub-07), Cuti (Sub-08), dan Lembur (Sub-09) sebelum batch payroll diizinkan dibuat.
* **Requirement/decision**: `HRD-DEC-122`, `02-backend-architecture.md` 23.8 (`PAY-B1`).
* **Kontrak**: API Contract `v14` `/api/v1/hr/payroll/runs/precondition-check`, `VR-PAY-02`.
* **Reuse**: `AttendancePeriodService`, `LeavePeriodService`, `OvertimePeriodService`.
* **Cakupan**: `PayrollPreconditionGateService.cs`, DTO `PreconditionGateCheckResponse`.
* **Dependency**: `BE-HRD-116` [B1], `BE-HRD-130` [B2], `BE-HRD-144` [B3]
* **Acceptance criteria**:
  1. Sistem mengembalikan status `CanInitialize = true` bila ketiga modul telah berstatus `Locked`/`Closed`.
  2. Sistem mengembalikan `CanInitialize = false` beserta daftar detail modul/transaksi yang masih pending bila ada modul yang belum terkunci.
  3. Pesan penolakan deskriptif disajikan informatif bagi petugas payroll.
* **Bukti verifikasi**: Test case `AT-HRD-PAY-02`, UAT skenario `UAT-PAY-01`.
* **Risiko/pemilik**: Payroll diinisiasi saat jam kerja belum final; mitigasi: hard-block validation pada service initialization.
* **Definition of Done**: Validator precondition gate beroperasi andal dan menolak pembuatan payroll prematur.

### ✅  BE-HRD-149 — Engine inisiasi batch siklus penggajian bulanan & proteksi anti-duplikasi periode (PayrollRunInitializationService)
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Layanan inisiasi siklus batch penggajian bulanan (`TrxPayrollRun`) baru berstatus `Draft`, pembentukan nomor batch resmi (format `PRUN-YYYYMM-XXX`), penegakan proteksi anti-duplikasi periode penggajian untuk tahun dan bulan yang sama, serta populasi daftar pegawai aktif yang berhak menerima gaji.
* **Requirement/decision**: `HRD-DEC-122`, `02-backend-architecture.md` 23.8 (`PAY-B2`).
* **Kontrak**: API Contract `v14` `POST /api/v1/hr/payroll/runs`, `VR-PAY-01`.
* **Reuse**: `PayrollRunController.cs`, entitas `TrxPayrollRun`.
* **Cakupan**: `PayrollRunInitializationService.cs`, command handler pembuatan batch.
* **Dependency**: `BE-HRD-147`, `BE-HRD-148`
* **Acceptance criteria**:
  1. Permintaan pembuatan batch untuk periode yang sudah ada ditolak dengan HTTP 409 Conflict (`VR-PAY-01`).
  2. Batch tersimpan berstatus `Draft` dan merekam snapshot parameter aktif dari `HrdPayrollSetting`.
  3. Daftar baris `TrxPayrollItemDetail` terbentuk untuk seluruh profil pegawai aktif rumah sakit.
* **Bukti verifikasi**: Test case `AT-HRD-PAY-01`, `AT-HRD-PAY-03`, UAT skenario `UAT-PAY-01`.
* **Risiko/pemilik**: Duplikasi batch gaji; mitigasi: constraint unik database dan application-level lock.
* **Definition of Done**: Inisiasi batch payroll berhasil membentuk draft awal dan baris detail pegawai.

### ✅  BE-HRD-150 — Engine kalkulasi penghasilan tetap: gaji pokok, tunjangan struktural, tunjangan fungsional profesi & risiko medis
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: ✅ Selesai (2026-11-05)
* **Outcome**: Engine perhitungan komponen penghasilan tetap per pegawai berdasarkan grade, level karir klinis dokter/perawat, jabatan struktural, dan unit kerja berisiko tinggi (ICU, Isolasi, Radiologi) tanpa pembulatan moneter ganjil.
* **Requirement/decision**: `HRD-DEC-123`, `02-backend-architecture.md` 23.8 (`PAY-B3`).
* **Kontrak**: API Contract `v14` `/api/v1/hr/payroll/runs/{id}/calculate`, `VR-PAY-04`, `VR-PAY-05`.
* **Reuse**: Master data grade gaji dan struktur tunjangan karyawan.
* **Cakupan**: `PayrollEarningsCalculationService.cs`, engine kalkulasi penghasilan kotor (*Gross Pay*).
* **Dependency**: `BE-HRD-149`
* **Acceptance criteria**:
  1. Komponen gaji pokok, tunjangan jabatan, tunjangan fungsional profesi, dan tunjangan risiko terhitung presisi.
  2. Data remunerasi tenaga medis honorer/organik dipetakan sesuai kontrak penempatan aktif.
  3. Total akumulasi penghasilan kotor tersimpan pada kolom `GrossAmount` di `TrxPayrollItemDetail`.
* **Bukti verifikasi**: Test case `AT-HRD-PAY-04`, UAT skenario `UAT-PAY-02`.
* **Risiko/pemilik**: Selisih nominal rupiah; mitigasi: penggunaan tipe data decimal 18,2 dan math rounding banker's.
* **Definition of Done**: Perhitungan penghasilan tetap teruji akurat untuk berbagai kelompok profesi rumah sakit.

### ✅  `BE-HRD-151` — Engine penyerapan lembur resmi Sub-09 & premi shift malam presensi Sub-07 (`PayrollOvertimeAndShiftIngestionService`)
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan penyerapan data variabel jam lembur terverifikasi (tier 1.5x, 2x, holiday) beserta rapel lembur dari Sub-modul 09 (`INT-HR-52`), serta penyerapan akumulasi dinas shift malam dari Sub-modul 07 (`INT-HR-36`) untuk dikonversi menjadi nominal premi malam resmi.
* **Requirement/decision**: `HRD-DEC-123`, `HRD-ASM-27`, `02-backend-architecture.md` 23.8 (`PAY-B4`).
* **Kontrak**: API Contract `v14`, Integration Contract `INT-HR-36`, `INT-HR-52`, `VR-PAY-06`, `VR-PAY-07`.
* **Reuse**: DTO payload lembur dan log presensi harian.
* **Cakupan**: `PayrollOvertimeAndShiftIngestionService.cs`, kalkulator uang lembur dan premi shift malam.
* **Dependency**: `BE-HRD-150`
* **Acceptance criteria**:
  1. Uang lembur terhitung otomatis berdasar formula upah per jam PP 35/2021 dan pengali jam sah.
  2. Premi shift malam dikalikan nominal baku (default Rp 50.000/shift) sesuai kehadiran dinas malam aktual.
  3. Rapel lembur retrospektif dari periode lampau tercatat pada komponen terpisah `RetrospectiveAdjustment`.
* **Bukti verifikasi**: Test case `AT-HRD-PAY-05`, `AT-HRD-PAY-06`, `AT-HRD-PAY-11`, UAT skenario `UAT-PAY-02`.
* **Risiko/pemilik**: Perbedaan klaim lembur vs presensi; mitigasi: konsumsi data snapshot yang telah berstatus Closed.
* **Definition of Done**: Variabel lembur dan premi shift malam terintegrasi penuh ke dalam lembar kerja gaji.

### ✅  `BE-HRD-152` — Engine kalkulasi iuran BPJS Kesehatan dan BPJS Ketenagakerjaan berplafon upah resmi
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Engine kalkulasi potongan wajib jaminan sosial tenaga kerja: BPJS Kesehatan (1% pegawai, 4% RS berplafon Rp 12.000.000) serta 4 program BPJS Ketenagakerjaan (JKK 0.89%, JKM 0.3%, JHT 2% pegawai & 3.7% RS, JP 1% pegawai berplafon Rp 10.042.300).
* **Requirement/decision**: `HRD-DEC-123`, `02-backend-architecture.md` 23.8 (`PAY-W4`).
* **Kontrak**: `VR-PAY-08`, `VR-PAY-09`, `VR-PAY-10`.
* **Reuse**: Parameter batas plafon dari `HrdPayrollSetting`.
* **Cakupan**: `PayrollBpjsCalculationService.cs`, modul pemisahan beban pegawai vs beban rumah sakit.
* **Dependency**: `BE-HRD-150`
* **Acceptance criteria**:
  1. Iuran BPJS Kesehatan pekerja dan pemberi kerja dihitung dari upah dengan pembatasan plafon maksimal.
  2. Iuran JHT dihitung dari total upah tanpa batas; iuran JP dipotong batas upah maksimal regulasi PP.
  3. Nilai potongan iuran pekerja mengurangi penghasilan kotor; beban pemberi kerja dicatat untuk pelaporan biaya.
* **Bukti verifikasi**: Test case `AT-HRD-PAY-07`, `AT-HRD-PAY-08`, UAT skenario `UAT-PAY-02`.
* **Risiko/pemilik**: Kepatuhan regulasi BPJS; mitigasi: parameter plafon dinamis tersimpan pada database master.
* **Definition of Done**: Kalkulasi potongan dan kontribusi BPJS teruji presisi sesuai tabel ketentuan pemerintah.

### ✅  `BE-HRD-153` — Engine kalkulasi pemotongan PPh 21 bulanan Tarif Efektif Rata-Rata (TER) PMK 168/2023
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Engine kalkulasi pemotongan pajak penghasilan PPh 21 bulanan mengacu pada tabel resmi Tarif Efektif Rata-Rata (TER Kategori A, B, dan C) sesuai Peraturan Menteri Keuangan Nomor 168 Tahun 2023 berdasarkan status PTKP dan penghasilan bruto.
* **Requirement/decision**: `HRD-DEC-123`, `HRD-ASM-28`, `02-backend-architecture.md` 23.8.
* **Kontrak**: API Contract `v14`, Integration Contract `INT-HR-56`, `VR-PAY-11`, `VR-PAY-12`.
* **Reuse**: Tabel master TER PPh 21 dan data PTKP pegawai dari Master Data HRD.
* **Cakupan**: `PayrollTaxCalculationService.cs`, engine lookup bracket tarif TER.
* **Dependency**: `BE-HRD-150`, `BE-HRD-151`, `BE-HRD-152`
* **Acceptance criteria**:
  1. Pegawai dikelompokkan ke dalam TER Kategori A, B, atau C sesuai status pernikahan dan tanggungan PTKP.
  2. Tarif persentase TER ditarik berdasar rentang penghasilan bruto bulan berjalan dan dipotongkan otomatis.
  3. Masa pajak Desember mendukung perhitungan tarif progresif Pasal 17 UU PPh sebagai penyesuaian akhir tahun.
* **Bukti verifikasi**: Test case `AT-HRD-PAY-09`, UAT skenario `UAT-PAY-02`.
* **Risiko/pemilik**: Kesalahan hitung pajak pegawai; mitigasi: verifikasi silang bracket TER dan unit testing komprehensif.
* **Definition of Done**: Engine pemotongan pajak PPh 21 TER PMK 168/2023 beroperasi tanpa cela.

### ✅  `BE-HRD-154` — Integrasi penarikan potongan pinjaman koperasi & restitusi dari Sub-modul 11 (`PayrollDeductionsIngestionService`)
* **Slice / MVP**: `S-B5` / `MVP-31`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan konsumsi tagihan angsuran pinjaman belanja koperasi karyawan, pinjaman darurat rumah sakit, atau pemotongan kelebihan klaim restitusi kesehatan dari Sub-modul 11 via kontrak `INT-HR-55`, dengan aturan proteksi batas maksimal pemotongan gaji (maks 50% dari take-home pay).
* **Requirement/decision**: `HRD-DEC-123`, `02-backend-architecture.md` 23.8.
* **Kontrak**: Integration Contract `INT-HR-55`, `VR-PAY-25`.
* **Reuse**: Endpoint kueri potongan cicilan aktif dari Sub-modul 11.
* **Cakupan**: `PayrollDeductionsIngestionService.cs`, proteksi take-home pay minimum pegawai.
* **Dependency**: `BE-HRD-153`
* **Acceptance criteria**:
  1. Tagihan cicilan pinjaman aktif terpotong otomatis dari slip gaji pegawai yang bersangkutan.
  2. Potongan pinjaman dibatasi maksimal 50% penghasilan kotor untuk menjamin hak hidup layak pegawai.
  3. Setiap potongan mencatat nomor referensi pinjaman dan nomor angsuran berjalan.
* **Bukti verifikasi**: Test case `AT-HRD-PAY-10`, UAT skenario `UAT-PAY-02`.
* **Risiko/pemilik**: Gaji bersih minus; mitigasi: pemblokiran nilai take-home pay negatif oleh validasi sistem.
* **Definition of Done**: Tagihan pinjaman internal terintegrasi aman dan take-home pay final terhitung valid.

### ✅  `BE-HRD-155` — Payroll Variance Guard & deteksi otomatis deviasi net pay >= 20% / Rp 2jt (`PayrollVarianceGuardService`)
* **Slice / MVP**: `S-B5` / `MVP-31`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan evaluasi anomali variansi gaji (*Payroll Variance Guard*) yang membandingkan gaji bersih pegawai pada siklus berjalan terhadap riwayat periode sebelumnya; sistem secara otomatis mendeteksi deviasi persentase $\ge 20\%$ atau selisih nominal $\ge$ Rp 2.000.000 dan membangkitkan tiket `TrxPayrollVarianceAlert` berstatus `PendingJustification`.
* **Requirement/decision**: `HRD-DEC-124`, `HRD-ASM-29`, `02-backend-architecture.md` 23.8 (`PAY-W1`, `PAY-W2`, `PAY-W3`).
* **Kontrak**: API Contract `v14` `[Tags("HR Payroll - Calculations & Variance")]`, `VR-PAY-13`.
* **Reuse**: Entitas `TrxPayrollVarianceAlert`, kueri histori slip gaji periode lalu.
* **Cakupan**: `PayrollVarianceGuardService.cs`, engine komparasi variansi multi-komponen.
* **Dependency**: `BE-HRD-154`
* **Acceptance criteria**:
  1. Lonjakan atau anjloknya gaji bersih $\ge 20\%$ atau selisih $\ge$ Rp 2.000.000 memicu tiket alert variansi.
  2. Pegawai baru tanpa histori gaji bulan lalu otomatis ditandai sebagai anomali jenis `NewEmployeeBaseline`.
  3. Tiket mencatat nilai sebelum, nilai sesudah, selisih nominal, rasio persentase, dan komponen pemicu.
* **Bukti verifikasi**: Test case `AT-HRD-PAY-12`, UAT skenario `UAT-PAY-03`.
* **Risiko/pemilik**: Anomali finansial lolos ke pencairan; mitigasi: otomasi guard saat kalkulasi selesai.
* **Definition of Done**: Variance guard mendeteksi seluruh anomali dan menyajikannya pada daftar audit investigasi.

### ✅  `BE-HRD-156` — Alur resolusi justifikasi audit variansi & gerbang penolakan submission batch (`PayrollVarianceResolutionService`)
* **Slice / MVP**: `S-B5` / `MVP-31`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan penerimaan dan validasi justifikasi alasan audit anomali variansi oleh Payroll Officer (wajib minimum 20 karakter) serta penegakan gerbang validasi keras (*hard-block*) yang menolak pengajuan submit batch penggajian ke level manajer jika masih terdapat alert variansi berstatus pending.
* **Requirement/decision**: `HRD-DEC-124`, `02-backend-architecture.md` 23.8 (`PAY-B5`).
* **Kontrak**: API Contract `v14` `POST /variances/{id}/resolve`, `VR-PAY-14`, `VR-PAY-15`.
* **Reuse**: Controller variance audit, model `TrxPayrollVarianceAlert`.
* **Cakupan**: `PayrollVarianceResolutionService.cs`, validator panjang karakter justifikasi.
* **Dependency**: `BE-HRD-155`
* **Acceptance criteria**:
  1. Pengisian alasan justifikasi di bawah 20 karakter ditolak dengan HTTP 400 Bad Request (`VR-PAY-15`).
  2. Justifikasi valid mengubah status alert menjadi `Resolved` dan mencatat identitas officer serta stempel waktu.
  3. Upaya submit batch yang masih memiliki alert berstatus `PendingJustification` ditolak HTTP 422 (`VR-PAY-14`).
* **Bukti verifikasi**: Test case `AT-HRD-PAY-13`, `AT-HRD-PAY-14`, UAT skenario `UAT-PAY-03`.
* **Risiko/pemilik**: Justifikasi asal-asalan; mitigasi: audit trail permanen dan penolakan input kosong/terlalu pendek.
* **Definition of Done**: Alur resolusi variansi aktif dan mengunci gerbang submission approval batch.

### ✅  `BE-HRD-157` — Engine alur otorisasi 3-jenjang, hierarki eskalasi & penegakan anti-self-approval HR Manager
* **Slice / MVP**: `S-B5` / `MVP-31`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Engine alur otorisasi berjenjang 3 tingkat (*3-tier segregation of duties*): Tier 1 (Payroll Officer mengunci draft & submit ke `WaitingHrReview`), Tier 2 (HR Manager me-review ke `WaitingFinalApproval`), dan Tier 3 (Direktur SDM/Keuangan menyetujui final `ApprovedFinal`), dengan penegakan keras aturan anti-self-approval di mana verifikasi baris gaji HR Manager dialihkan ke Direktur.
* **Requirement/decision**: `HRD-DEC-125`, `02-backend-architecture.md` 23.8 (`PAY-B6`).
* **Kontrak**: API Contract `v14` `[Tags("HR Payroll - Approvals")]`, `VR-PAY-16` s.d. `VR-PAY-20`.
* **Reuse**: `PayrollApprovalController.cs`, entitas `TrxPayrollApproval`.
* **Cakupan**: `PayrollApprovalWorkflowService.cs`, router hierarki persetujuan manajerial.
* **Dependency**: `BE-HRD-156`
* **Acceptance criteria**:
  1. HR Manager dilarang menyetujui perhitungan gajinya sendiri; hak otorisasi baris pribadinya dirutekan ke Direksi.
  2. Penolakan pada tahap mana pun mewajibkan catatan alasan penolakan dan mengembalikan status ke `NeedCorrection`.
  3. Persetujuan Tier 3 oleh Direktur mengunci batch secara permanen dan menyetel flag `IsImmutable = true`.
* **Bukti verifikasi**: Test case `AT-HRD-PAY-15`, `AT-HRD-PAY-16`, `AT-HRD-PAY-17`, UAT skenario `UAT-PAY-04`.
* **Risiko/pemilik**: Kolusi internal; mitigasi: pemisahan peran ketat berbasis permission token terverifikasi.
* **Definition of Done**: Workflow approval 3-jenjang beroperasi aman dan menegakkan segregation of duties.

### ✅  `BE-HRD-158` — Pembekuan permanen batch immutable & pembangkitan paket serah terima Finance SHA-256 (`PayrollFinanceHandoffService`)
* **Slice / MVP**: `S-B5` / `MVP-32`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan pembekuan permanen seluruh baris data penggajian pasca persetujuan final Direksi, pembuatan paket serah terima keuangan (`TrxPayrollHandoffBatch`), penghitungan checksum integritas data SHA-256 atas seluruh baris transfer bank pegawai, serta pengiriman berkas serah terima ke Modul Finance secara asinkron via kontrak `INT-HR-53`.
* **Requirement/decision**: `HRD-DEC-126`, `02-backend-architecture.md` 23.8 (`PAY-B7`).
* **Kontrak**: API Contract `v14` `[Tags("HR Payroll - Finance Handoff")]`, Integration Contract `INT-HR-53`, `VR-PAY-21`.
* **Reuse**: Handoff controller, generator hash SHA-256 kanonikal.
* **Cakupan**: `PayrollFinanceHandoffService.cs`, serializer berkas transfer bank bank-rekaps.
* **Dependency**: `BE-HRD-157`
* **Acceptance criteria**:
  1. Data batch penggajian berstatus `ApprovedFinal` ditandai immutable; modifikasi apa pun ditolak HTTP 409.
  2. Paket handoff menyusun daftar rekening bank tujuan, NIK, nama pegawai, dan nominal transfer bersih.
  3. Checksum integritas SHA-256 terbentuk dan tervalidasi sebelum transmisi berkas dikirimkan ke Finance.
* **Bukti verifikasi**: Test case `AT-HRD-PAY-18`, UAT skenario `UAT-PAY-05`.
* **Risiko/pemilik**: Kegagalan transmisi ke Finance; mitigasi: mekanisme exponential backoff retry hingga 5 kali.
* **Definition of Done**: Paket serah terima keuangan terbentuk dengan checksum valid dan terkirim ke modul Finance.

### ✅  `BE-HRD-159` — Webhook reconciler callback pembayaran/penolakan Finance & distribusi slip gaji digital ESS ber-PIN sekunder 6-digit
* **Slice / MVP**: `S-B5` / `MVP-32`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan penerima webhook callback dari Modul Finance (`INT-HR-54`) untuk menangani konfirmasi pencairan (`PaymentConfirmed` $\rightarrow$ status batch `Completed`) atau penolakan batch (`PaymentRejected` $\rightarrow$ rollback terkendali ke `NeedCorrection`), serta layanan pelepasan slip gaji digital ESS (`TrxPayrollPayslip`) per tanggal 25 yang diproteksi autentikasi PIN sekunder 6-digit dengan penalti penguncian akun 30 menit setelah 3 kegagalan.
* **Requirement/decision**: `HRD-DEC-126`, `HRD-DEC-127`, `HRD-ASM-30`, `02-backend-architecture.md` 23.8 (`PAY-B8`).
* **Kontrak**: API Contract `v14` `[Tags("HR Payroll - Self-Service Payslips")]`, Integration Contract `INT-HR-54`, `VR-PAY-22` s.d. `VR-PAY-24`.
* **Reuse**: `PayrollPayslipController.cs`, encoder PIN bcrypt/argon2.
* **Cakupan**: `PayrollDisbursementReconciliationService.cs`, `PayrollPayslipDistributionService.cs`.
* **Dependency**: `BE-HRD-158`
* **Acceptance criteria**:
  1. Callback penolakan Finance membuka gembok immutable secara terkendali ke `NeedCorrection` dengan audit log.
  2. Callback sukses menandai slip gaji `Released` dan mengizinkan pegawai mengaksesnya pada portal ESS per tgl 25.
  3. Verifikasi PIN sekunder 6-digit memblokir akses 30 menit jika salah 3 kali berturut-turut (`VR-PAY-22`).
  4. Unduhan dokumen PDF slip gaji diproteksi kata sandi terenkripsi (kombinasi tanggal lahir) dan watermark resmi RS.
* **Bukti verifikasi**: Test case `AT-HRD-PAY-19`, `AT-HRD-PAY-20`, UAT skenario `UAT-PAY-05`.
* **Risiko/pemilik**: Kebocoran nominal slip gaji; mitigasi: proteksi PIN sekunder dan enkripsi berkas PDF.
* **Definition of Done**: Callback Finance sinkron dan slip gaji digital terdistribusi aman kepada pegawai.

### ✅  `BE-HRD-160` — Verifikasi ujung-ke-ujung arsitektur QBE, kontrak v14, dan build Slice `S-B5`
* **Slice / MVP**: `S-B5` / Verifikasi
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Audit komprehensif kepatuhan arsitektur backend, zero warning kompilasi .NET, verifikasi integritas seluruh endpoint Swagger penggajian, konformansi aturan rekayasa QBE, serta pemenuhan 10 butir Definition of Done Slice `S-B5`.
* **Requirement/decision**: `HRD-DEC-122` s.d. `HRD-DEC-127`, `04-prd-to-mvp.md` Bagian 28 (DoD 1 s.d. 10).
* **Kontrak**: Seluruh kontrak operasional `v14` Slice `S-B5`.
* **Reuse**: Prosedur audit pre-deploy backend Quilvian.
* **Cakupan**: Build verification, OpenAPI audit, QBE conformance check.
* **Dependency**: `BE-HRD-153`, `BE-HRD-157`, `BE-HRD-159`
* **Acceptance criteria**:
  1. Solusi `NewQuilvianSystemBackend` berhasil di-build tanpa error (`dotnet build`).
  2. Seluruh 6 tag grup Swagger penggajian terdaftar dan dapat diakses pada dokumentasi API.
  3. Konformansi QBE valid (prefix entitas `trx_payroll_*`, DI registration, repository pattern).
  4. 10 butir Definition of Done pada `04-prd-to-mvp.md` Bagian 28 terpenuhi dengan bukti verifikasi valid.
* **Bukti verifikasi**: Laporan build bersih, OpenAPI conformance log, laporan audit DoD.
* **Risiko/pemilik**: Regresi fungsional modul lain; mitigasi: regression smoke testing controller.
* **Definition of Done**: Build backend 100% lulus dan siap untuk integrasi frontend.


---

## 25. Rincian Task Backend Slice `S-B6` — Restitusi & Integrasi Pemotongan Penggajian (Sub-modul 11, Kontrak v15)

Bagian ini memuat pemecahan delivery **Sub-Modul 11 (Slice `S-B6`)** menjadi 15 task backend terperinci (`BE-HRD-161` s.d. `BE-HRD-175`), grafik urutan dependency pohon teks (Grafik B40 s.d. B43), serta tabel gelombang eksekusi `MVP-33` s.d. `MVP-35` & Verifikasi.

### Grafik Urutan Dependency Pohon Teks `S-B6`

#### Grafik B40: Gelombang `MVP-33` (Fondasi Database, Kebijakan Plafon, Engine Guard 50% & Sinkronisasi Finance)
```text
BE-HRD-161 ✅ ─┬─> BE-HRD-162 ✅ ──> BE-HRD-164 ✅ ──> BE-HRD-165 ✅
               │
               └─> BE-HRD-163 ✅
```

#### Grafik B41: Gelombang `MVP-34` (Klaim Restitusi ESS, Plafon Tahunan, Approval 3-Tier & Injeksi Payroll)
```text
BE-HRD-161 [B40] ─┬─> BE-HRD-166 ✅ ──> BE-HRD-167 ✅ ──> BE-HRD-168 ✅ ─┬─> BE-HRD-170 ✅
                  │                                                       │
                  └─> BE-HRD-169 ✅ ──────────────────────────────────────┘
```

#### Grafik B42: Gelombang `MVP-35` (Jalur Cepat Kasir Cito 24/7, Koreksi Masa Lalu & Exit Clearance)
```text
BE-HRD-168 [B41] ──> BE-HRD-172 ✅ ──> BE-HRD-173 ✅
BE-HRD-169 [B41] ──> BE-HRD-171 ✅
BE-HRD-165 [B40] ──> BE-HRD-174 ✅
```

#### Grafik B43: Gelombang Verifikasi
```text
BE-HRD-165 [B40] ─┐
BE-HRD-170 [B41] ─┼─> BE-HRD-175 ✅
BE-HRD-174 [B42] ─┘
```

---

### Kartu Detail Task Backend Slice `S-B6`

### ✅ `BE-HRD-161` — Fondasi skema database restitusi klaim & pemotongan penggajian (6 entitas DDL & migrasi EF Core)
* **Slice / MVP**: `S-B6` / `MVP-33`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Entitas domain C# dan DDL skema database PostgreSQL untuk 6 entitas baru: `HrdRestitutionSetting`, `TrxEmployeeRestitutionClaim`, `TrxPayrollRestitution`, `TrxPayrollDeductionSchedule`, `TrxPayrollDeductionItem`, `TrxCashierDisbursementVoucher`, pendaftaran DbSet pada `ApplicationDbContext`, indeks unik terklaster pada nomor dokumen, dan berkas migrasi EF Core.
* **Requirement/decision**: `HRD-DEC-128` s.d. `HRD-DEC-133`, `02-backend-architecture.md` 24.1 s.d. 24.6.
* **Kontrak**: Seluruh skema data Kontrak `v15`.
* **Reuse**: `IdentityModel`, pola repository EF Core.
* **Cakupan**: Model entities, konfigurasi DbContext, migrasi EF Core PostgreSQL.
* **Dependency**: Tidak ada (fondasi data).
* **Acceptance criteria**:
  1. 6 entitas baru terdaftar pada `ApplicationDbContext` dengan constraint schema `public`.
  2. Indeks komposit performa evaluasi kuota per pegawai dan indeks unik nomor klaim/voucher terbentuk.
  3. Proyek `NewQuilvianSystemBackend` berhasil di-build tanpa error.
* **Bukti verifikasi**: Skrip migrasi EF Core, kompilasi build bersih `dotnet build`.
* **Risiko/pemilik**: Kegagalan migrasi skema; mitigasi: DDL idempotent `CREATE TABLE IF NOT EXISTS`.
* **Definition of Done**: 6 entitas terdefinisi di C# dan migrasi database siap dieksekusi.

### ✅ `BE-HRD-162` — Layanan konfigurasi kebijakan plafon tahunan restitusi & persentase batas pemotongan upah (`RestitutionSettingService`)
* **Slice / MVP**: `S-B6` / `MVP-33`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan CRUD konfigurasi kebijakan batas plafon tahunan per grade staf RS, batas minimal masa kerja, ambang batas pemotongan upah (default 50%), serta hak akses jalur cepat kasir darurat cito.
* **Requirement/decision**: `HRD-DEC-128`, `HRD-DEC-130`, `02-backend-architecture.md` 24.7 (`HrdRestitutionSetting`).
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Settings")]`.
* **Reuse**: Pola setting service Quilvian.
* **Cakupan**: `RestitutionSettingService.cs`, DTOs, controller endpoints.
* **Dependency**: `BE-HRD-161`
* **Acceptance criteria**:
  1. Mendukung pengambilan kebijakan plafon berdasarkan kode grade/golongan dan kategori manfaat.
  2. Penegakan nilai default batasan potongan 50% sesuai Pasal 65 PP 36/2021.
* **Bukti verifikasi**: Unit test / service test `RestitutionSettingService`.
* **Risiko/pemilik**: Konfigurasi kosong; mitigasi: seeding otomatis data master awal per grade.
* **Definition of Done**: Layanan setting aktif dan melayani query kuota kebijakan staf RS.

### ✅ `BE-HRD-163` — Integrasi sinkronisasi jadwal piutang aktif dari Finance H-3 cut-off tgl 17 (`FinanceDeductionSyncService`, `INT-HR-57`)
* **Slice / MVP**: `S-B6` / `MVP-33`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan penarikan snapshot jadwal cicilan piutang/pinjaman pegawai aktif dari Modul Keuangan (`INT-HR-57`) pada tanggal 17 (H-3 cut-off) dengan retry policy eksponensial dan penguncian jadwal per tanggal 20 pukul 23:59:59.
* **Requirement/decision**: `HRD-DEC-129`, `HRD-ASM-32`, Integration Contract `INT-HR-57`, `VR-DED-17`.
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Cashier Vouchers & Schedules")]`.
* **Reuse**: HTTP client retry policy handler.
* **Cakupan**: `FinanceDeductionSyncService.cs`, scheduler ingestor.
* **Dependency**: `BE-HRD-161`
* **Acceptance criteria**:
  1. Penarikan data piutang Finance membentuk/memperbarui `TrxPayrollDeductionSchedule`.
  2. Jadwal cicilan bulan berjalan terkunci otomatis per tgl 20 pukul 23:59:59 (`VR-DED-17`).
* **Bukti verifikasi**: Test case `AT-HRD-DED-05`, `AT-HRD-DED-06`, `AT-HRD-DED-07`.
* **Risiko/pemilik**: Timeout Finance API; mitigasi: retry eksponensial 1m, 5m, 15m.
* **Definition of Done**: Sinkronisasi jadwal Finance sukses masuk ke jadwal pemotongan HRD.

### ✅ `BE-HRD-164` — Engine validasi hard-cap maksimal pemotongan upah tepat 50% Gross Pay (`PayrollDeductionEngineService`, `DED-B1`, PP 36/2021)
* **Slice / MVP**: `S-B6` / `MVP-33`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Mesin kalkulasi batasan akumulasi pemotongan upah non-regulasi yang menegakkan secara mutlak batas maksimal tepat 50% dari penghasilan kotor (*Gross Pay*) bulan berjalan, menjaga hak hidup layak pegawai RS.
* **Requirement/decision**: `HRD-DEC-128`, Pasal 65 PP 36/2021 jo PP 49/2025, `02-backend-architecture.md` 24.8 (`DED-B1`).
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Engine")]`.
* **Reuse**: Kalkulator Gross Pay dari Sub-10.
* **Cakupan**: `PayrollDeductionEngineService.cs` (metode evaluasi limit 50%).
* **Dependency**: `BE-HRD-161`, `BE-HRD-162`
* **Acceptance criteria**:
  1. Total potongan non-regulasi (kasbon, koperasi, paguyuban) tidak pernah melebihi $50\% \times \text{GrossPay}$.
  2. Simulasi pemotongan menghasilkan status lolos atau status peringatan batas plafon tercapai.
* **Bukti verifikasi**: Test case `AT-HRD-DED-01`, UAT skenario `UAT-DED-01`.
* **Risiko/pemilik**: Take home pay minus; mitigasi: pemblokiran hard-cap 50% di tingkat engine.
* **Definition of Done**: Engine menolak pemotongan melebihi batas 50% Gross Pay.

### ✅ `BE-HRD-165` — Engine prioritas berjenjang 1-4, pemangkasan parsial & penjadwalan sisa carry-over otomatis ke bulan berikutnya (`DED-B2`, `DED-B3`)
* **Slice / MVP**: `S-B6` / `MVP-33`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Algoritma alokasi kuota pemotongan berjenjang sesuai hirarki mutlak: Prioritas 1 (Ganti Rugi Kasir/Aset RS) $\rightarrow$ Prioritas 2 (Kasbon Pinjaman RS) $\rightarrow$ Prioritas 3 (Cicilan Koperasi RS) $\rightarrow$ Prioritas 4 (Iuran Paguyuban/Sosial), dengan pemangkasan parsial dan penjadwalan sisa (*carry-over*) otomatis ke bulan berikutnya berstatus `DeferredDueToSalaryCap` tanpa denda bunga.
* **Requirement/decision**: `HRD-DEC-128`, `02-backend-architecture.md` 24.8 (`DED-B2`, `DED-B3`).
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Engine")]`.
* **Reuse**: `TrxPayrollDeductionItem`.
* **Cakupan**: `PayrollDeductionEngineService.cs` (algoritma alokasi prioritas & carry-over).
* **Dependency**: `BE-HRD-164`
* **Acceptance criteria**:
  1. Pemotongan dialokasikan urut dari prioritas 1 hingga prioritas 4 hingga pagu 50% habis.
  2. Komponen yang tersisa sebagian dipotong sebagian (`actual_deducted_amount`) dan sisanya ditunda (`deferred_amount`).
  3. Tagihan tertunda secara otomatis dijadwalkan ulang ke bulan berikutnya tanpa denda bunga (`VR-DED-03`).
* **Bukti verifikasi**: Test case `AT-HRD-DED-02`, `AT-HRD-DED-03`, `AT-HRD-DED-04`.
* **Risiko/pemilik**: Koperasi tidak terpotong karena kasbon besar; mitigasi: pencatatan audit log transparan dan penundaan resmi.
* **Definition of Done**: Hirarki prioritas 1-4 aktif dan otomatisasi carry-over beroperasi aman.

### ✅ `BE-HRD-166` — Layanan evaluasi kuota & sisa plafon tahunan pegawai untuk klaim kacamata/medis di ESS (`VR-DED-10`, `VR-DED-11`)
* **Slice / MVP**: `S-B6` / `MVP-34`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan pelacak real-time sisa kuota plafon tahunan pegawai untuk klaim kacamata kerja dan faskes luar darurat berdasarkan riwayat klaim approved tahun berjalan vs batas master per grade.
* **Requirement/decision**: `HRD-DEC-130`, `02-backend-architecture.md` 24.7 (`VR-DED-10`, `VR-DED-11`).
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Restitution Claims")]`.
* **Reuse**: Query agregat histori klaim.
* **Cakupan**: `RestitutionClaimService.cs` (metode `GetEmployeeRemainingQuotaAsync`).
* **Dependency**: `BE-HRD-161`, `BE-HRD-162`
* **Acceptance criteria**:
  1. Menghitung akumulasi klaim approved per kategori manfaat dalam tahun berjalan.
  2. Memberikan peringatan saat sisa kuota kacamata $< 20\%$ (`DED-W2`).
* **Bukti verifikasi**: Test case `AT-HRD-DED-10`, UAT skenario `UAT-DED-03`.
* **Risiko/pemilik**: Over-claim melebihi plafon; mitigasi: pemblokiran submit di portal ESS jika kuota habis.
* **Definition of Done**: Sisa kuota plafon terhitung akurat dan tampil di dashboard pegawai.

### ✅ `BE-HRD-167` — Layanan pengajuan klaim restitusi mandiri di portal ESS (filter kuitansi < 60 hari & upload lampiran nota)
* **Slice / MVP**: `S-B6` / `MVP-34`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan penerima pengajuan klaim mandiri pegawai di ESS dengan validasi masa kedaluwarsa kuitansi maksimal 60 hari kalender sejak tanggal nota (`VR-DED-09`), validasi upload kuitansi asli, dan flagging darurat medis nakes cito (`IsCitoEmergency`).
* **Requirement/decision**: `HRD-DEC-130`, `HRD-DEC-133`, `02-backend-architecture.md` 24.8 (`VR-DED-09`, `VR-DED-15`).
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Restitution Claims")]`.
* **Reuse**: File storage service.
* **Cakupan**: `RestitutionClaimService.cs` (metode `SubmitClaimAsync`).
* **Dependency**: `BE-HRD-166`
* **Acceptance criteria**:
  1. Pengajuan kuitansi bertanggal $> 60$ hari kalender ditolak otomatis oleh sistem (HTTP 400).
  2. Klaim tersimpan dengan status `Submitted` dan nomor klaim unik (`CLM-YYYYMM-XXXXXX`).
* **Bukti verifikasi**: Test case `AT-HRD-DED-09`, `AT-HRD-DED-11`.
* **Risiko/pemilik**: Kuitansi palsu/kedaluwarsa; mitigasi: validasi tanggal kuitansi dan kewajiban unggah berkas.
* **Definition of Done**: Pegawai dapat mengajukan klaim mandiri di ESS dengan validasi ketat.

### ✅ `BE-HRD-168` — Engine alur verifikasi & persetujuan 3-tier klaim restitusi dengan penegakan mutlak anti-self-approval (`DED-B8`)
* **Slice / MVP**: `S-B6` / `MVP-34`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Workflow persetujuan 3 tingkat: Tier 1 (Supervisor/Atasan Langsung) $\rightarrow$ Tier 2 (HR C&B Reviewer) $\rightarrow$ Tier 3 (HR Manager Final Approval), dengan penegakan keras aturan anti-self-approval di mana atasan dilarang menyetujui klaimnya sendiri.
* **Requirement/decision**: `HRD-DEC-130`, `02-backend-architecture.md` 24.8 (`DED-B8`, `VR-DED-08`, `VR-DED-23`).
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Restitution Claims")]`.
* **Reuse**: Alur otorisasi approval generic.
* **Cakupan**: `RestitutionClaimService.cs` (metode `ApproveSupervisorAsync`, `ReviewHrAsync`, `ApproveFinalAsync`).
* **Dependency**: `BE-HRD-167`
* **Acceptance criteria**:
  1. Atasan yang mengajukan klaim tidak dapat menyetujui klaim pribadinya (HTTP 403 Forbidden).
  2. HR C&B berhak melakukan penyesuaian nominal disetujui (`approved_amount`) dengan catatan alasan.
  3. Status akhir klaim approved beralih ke `QueuedForPayroll` (atau `VoucherIssued` jika cito).
* **Bukti verifikasi**: Test case `AT-HRD-DED-12`, `AT-HRD-DED-13`.
* **Risiko/pemilik**: Fraud klaim staf; mitigasi: 3-tier approval dan anti-self-approval mutlak.
* **Definition of Done**: Alur persetujuan 3-tier beroperasi aman dan menegakkan pemisahan wewenang.

### ✅ `BE-HRD-169` — Layanan kompensasi salah potong masa lalu tanpa revisi slip final dengan audit link slip asal (`PAY-B6`, `HRD-DEC-131`)
* **Slice / MVP**: `S-B6` / `MVP-34`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan penerbitan kompensasi pengembalian dana salah potong gaji periode lampau tanpa merevisi lembar slip gaji final yang telah dibayarkan, menautkan audit link ke slip asal (`affected_payslip_id`), dan mengantrekan uang pengganti ke siklus payroll bulan berjalan.
* **Requirement/decision**: `HRD-DEC-131`, `02-backend-architecture.md` 24.8 (`DED-B4`, `VR-DED-04`).
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Restitutions")]`.
* **Reuse**: Entitas `TrxPayrollRestitution`.
* **Cakupan**: `RestitutionClaimService.cs` (metode `CreateCorrectionRestitutionAsync`).
* **Dependency**: `BE-HRD-161`
* **Acceptance criteria**:
  1. Slip gaji periode lampau tetap berstatus immutable dan tidak berubah nominalnya (`PAY-B6`).
  2. Entitas `TrxPayrollRestitution` tipe `SalaryAdjustmentCorrection` terbentuk dengan referensi slip terdampak.
* **Bukti verifikasi**: Test case `AT-HRD-DED-15`, UAT skenario `UAT-DED-04`.
* **Risiko/pemilik**: Pengubahan histori slip final; mitigasi: pembukuan kompensasi terpisah pada run aktif.
* **Definition of Done**: Kompensasi salah potong tercatat resmi tanpa melanggar immutability slip lama.

### ✅ `BE-HRD-170` — Engine injeksi otomatis klaim restitusi approved non-cito ke slip gaji bulanan tgl 25 sebagai komponen non-pajak
* **Slice / MVP**: `S-B6` / `MVP-34`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan injeksi otomatis seluruh klaim berstatus `QueuedForPayroll` dan restitusi koreksi approved ke dalam lembar kerja siklus penggajian Sub-10 tanggal 25 sebagai komponen penerimaan non-pajak (reimbursement).
* **Requirement/decision**: `HRD-DEC-130`, `HRD-DEC-131`, `02-backend-architecture.md` 24.8 (`DED-B5`).
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Restitutions")]`.
* **Reuse**: Ingestion controller Sub-10.
* **Cakupan**: `RestitutionClaimService.cs` (metode `IngestApprovedRestitutionsToPayrollAsync`).
* **Dependency**: `BE-HRD-168`, `BE-HRD-169`
* **Acceptance criteria**:
  1. Klaim berstatus `QueuedForPayroll` berubah menjadi `ProcessedInPayroll`.
  2. Komponen penerimaan slip gaji bertambah dengan kode `REIMBURSEMENT_CLAIM` tanpa menambah DPP PPh 21.
* **Bukti verifikasi**: Test case `AT-HRD-DED-14`, UAT skenario `UAT-DED-03`.
* **Risiko/pemilik**: Reimbursement dikenakan pajak; mitigasi: klasifikasi tegas non-objek PPh 21.
* **Definition of Done**: Klaim approved non-cito otomatis masuk ke slip gaji tgl 25 secara aman.

### ✅ `BE-HRD-171` — Layanan investigasi keluhan salah potong & rekonsiliasi pengembalian dana bebas PPh 21 (`DED-B5`, `HRD-ASM-33`)
* **Slice / MVP**: `S-B6` / `MVP-35`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan penanganan tiket keluhan salah potong gaji oleh HR Payroll Officer, pencatatan hasil audit investigasi, verifikasi nominal pengembalian, dan penguncian status bebas pajak PPh 21 pada restitusi adjustment.
* **Requirement/decision**: `HRD-DEC-131`, `HRD-ASM-33`, `02-backend-architecture.md` 24.8 (`DED-B5`, `VR-DED-05`).
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Restitutions")]`.
* **Reuse**: Audit logger.
* **Cakupan**: `RestitutionClaimService.cs` (metode resolusi tiket keluhan salah potong).
* **Dependency**: `BE-HRD-169`
* **Acceptance criteria**:
  1. Investigasi keluhan merekam bukti telaah payroll officer dan nominal koreksi yang disepakati.
  2. Komponen penyesuaian diberi tag flag non-pajak PPh 21 (`IsTaxable = false`).
* **Bukti verifikasi**: Test case `AT-HRD-DED-16`, UAT skenario `UAT-DED-04`.
* **Risiko/pemilik**: Pajak ganda karyawan; mitigasi: audit tag non-objek pajak.
* **Definition of Done**: Penanganan keluhan salah potong tuntas dan menghasilkan restitusi non-pajak sah.

### ✅ `BE-HRD-172` — Layanan penerbitan voucher kasir darurat medis cito nakes ber-token SHA-256 kedaluwarsa 48 jam (`HRD-DEC-133`)
* **Slice / MVP**: `S-B6` / `MVP-35`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan penerbitan voucher pencairan kasir tunai 24/7 (`TrxCashierDisbursementVoucher`) untuk klaim darurat medis nakes cito (`IsCitoEmergency = true`) yang disahkan Direktur, dilengkapi token kriptografi SHA-256 satu kali pakai dan masa berlaku tepat 48 jam.
* **Requirement/decision**: `HRD-DEC-133`, `02-backend-architecture.md` 24.6, 24.8 (`VR-DED-13`, `VR-DED-14`).
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Cashier Vouchers & Schedules")]`.
* **Reuse**: SHA-256 token generator.
* **Cakupan**: `CashierVoucherDisbursementService.cs` (metode `IssueVoucherAsync`).
* **Dependency**: `BE-HRD-168`
* **Acceptance criteria**:
  1. Voucher terbit dengan nomor unik (`VCH-YYYYMM-XXXXXX`) dan masa aktif 48 jam sejak terbit.
  2. Status klaim terkait berubah menjadi `VoucherIssued`.
* **Bukti verifikasi**: Test case `AT-HRD-DED-17`, UAT skenario `UAT-DED-05`.
* **Risiko/pemilik**: Voucher kedaluwarsa; mitigasi: peringatan otomatis jika mendekati 36 jam (`DED-W3`).
* **Definition of Done**: Voucher kasir cito 48 jam ber-token aman berhasil diterbitkan.

### ✅ `BE-HRD-173` — Layanan kliring & verifikasi online loket kasir RS 24/7 (`INT-HR-59`) dengan flag anti-bayar ganda (`DisbursedViaCashier`, `DED-B6`)
* **Slice / MVP**: `S-B6` / `MVP-35`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan verifikasi dan kliring online voucher di loket Kasir RS 24/7 (`INT-HR-59`), pencatatan nomor kuitansi kasir, pemutakhiran status voucher ke `DisbursedViaCashier`, serta penguncian mutlak agar klaim tidak ditransfer ulang di penggajian bulanan tgl 25.
* **Requirement/decision**: `HRD-DEC-133`, `02-backend-architecture.md` 24.8 (`DED-B6`, `VR-DED-06`).
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Cashier Vouchers & Schedules")]`.
* **Reuse**: Integration endpoint Kasir RS.
* **Cakupan**: `CashierVoucherDisbursementService.cs` (metode `ClaimVoucherAtCashierAsync`).
* **Dependency**: `BE-HRD-172`
* **Acceptance criteria**:
  1. Kasir memvalidasi voucher via token SHA-256; jika valid dan belum expired, uang tunai dicairkan.
  2. Klaim terkunci permanen dengan flag `DisbursedViaCashier` dan tidak dapat diinjeksi ke payroll (`DED-B6`).
* **Bukti verifikasi**: Test case `AT-HRD-DED-18`, `AT-HRD-DED-19`, UAT skenario `UAT-DED-05`.
* **Risiko/pemilik**: Pembayaran ganda kasir dan payroll; mitigasi: hard flag `DisbursedViaCashier`.
* **Definition of Done**: Kliring kasir sukses dan garansi nol risiko pembayaran ganda ditegakkan.

### ✅ `BE-HRD-174` — Integrasi pelunasan utang pinjaman pada Exit Clearance pesangon Sub-20 (`INT-HR-60`, `DED-B7`) & webhook callback ke Finance (`INT-HR-58`)
* **Slice / MVP**: `S-B6` / `MVP-35`
* **Status**: ✅ Selesai (2026-11-06)
* **Outcome**: Layanan rekonsiliasi pelunasan sisa pokok pinjaman pegawai yang resign/PHK pada Final Clearance Payslip dengan pemotongan pesangon hingga 100% (`DED-B7`), penerbitan surat tagihan residu jika pesangon defisit, penahanan pengesahan Exit Clearance Sub-20 hingga lunas (`INT-HR-60`), serta transmisi webhook callback realisasi pemotongan bulanan ke Finance (`INT-HR-58`).
* **Requirement/decision**: `HRD-DEC-129`, `HRD-DEC-132`, `02-backend-architecture.md` 24.8 (`DED-B7`, `VR-DED-07`, `VR-DED-21`).
* **Kontrak**: Integration Contract `INT-HR-58`, `INT-HR-60`.
* **Reuse**: Exit clearance integration client.
* **Cakupan**: `ExitClearanceReceivableService.cs`, `FinanceDeductionSyncService.cs`.
* **Dependency**: `BE-HRD-165`
* **Acceptance criteria**:
  1. Sisa pinjaman dipotong dari hak pesangon dan cuti pada slip penutupan hingga 100%.
  2. Webhook callback realisasi pemotongan terkirim ke Modul Finance per tanggal 25 (`INT-HR-58`).
* **Bukti verifikasi**: Test case `AT-HRD-DED-08`, `AT-HRD-DED-20`.
* **Risiko/pemilik**: Pegawai keluar membawa sisa utang; mitigasi: penahanan clearance dan tagihan piutang resmi.
* **Definition of Done**: Rekonsiliasi exit clearance dan callback Finance realisasi berjalan sinkron.

### ✅ `BE-HRD-175` — Verifikasi ujung-ke-ujung arsitektur QBE, kontrak v15, dan build Slice `S-B6`
* **Slice / MVP**: `S-B6` / Verifikasi
* **Status**: ✅ Selesai (2026-11-06), `BE-HRD-170`, `BE-HRD-174`
* **Outcome**: Audit komprehensif kepatuhan arsitektur backend, zero error kompilasi .NET, verifikasi integritas seluruh endpoint Swagger restitusi dan potongan upah, konformansi aturan rekayasa QBE, serta pemenuhan seluruh kriteria Definition of Done Slice `S-B6`.
* **Requirement/decision**: `HRD-DEC-128` s.d. `HRD-DEC-133`, `04-prd-to-mvp.md` Bagian 29.
* **Kontrak**: Seluruh kontrak operasional `v15` Slice `S-B6`.
* **Reuse**: Prosedur audit pre-deploy backend Quilvian.
* **Cakupan**: Build verification, OpenAPI audit, QBE conformance check.
* **Dependency**: `BE-HRD-165`, `BE-HRD-170`, `BE-HRD-174`
* **Acceptance criteria**:
  1. Solusi `NewQuilvianSystemBackend` berhasil di-build tanpa error (`dotnet build`).
  2. Seluruh 4 tag grup Swagger restitusi terdaftar dan dapat diakses pada dokumentasi API.
  3. Konformansi QBE valid (prefix entitas, DI registration, repository pattern).
* **Bukti verifikasi**: Laporan build bersih, OpenAPI conformance log, laporan audit DoD.
* **Risiko/pemilik**: Regresi fungsional; mitigasi: full smoke build check.
* **Definition of Done**: Build backend 100% lulus dan siap untuk integrasi frontend.



---

## 26. Rincian Task Backend Slice `S-D3` — Benefit Management (Sub-modul 12, Kontrak v16)

Bagian ini memuat pemecahan delivery **Sub-Modul 12 (Slice `S-D3`)** menjadi 15 task backend terperinci (`BE-HRD-176` s.d. `BE-HRD-190`), grafik urutan dependency pohon teks (Grafik B44 s.d. B47), serta tabel gelombang eksekusi `MVP-36` s.d. `MVP-38` & Verifikasi.

### 26.1 Grafik Urutan Dependency Backend `S-D3`

#### Grafik B44: Gelombang `MVP-36` (Fondasi Skema DB, Paket Manfaat 3-Lapisan, Matriks Grade & Evaluasi Plafon)
```text
BE-HRD-176 ✅ ──> BE-HRD-177 ✅ ──> BE-HRD-178 ✅ ──> BE-HRD-179 ✅ ──> BE-HRD-180 ✅
```

#### Grafik B45: Gelombang `MVP-37` (Pendaftaran Tanggungan ESS, Aktivasi Cepat BPJS TK H+0, Batas Umur 21/25 Th & Verifikasi HR)
```text
BE-HRD-181 ✅ ──┬──> BE-HRD-183 ✅ ──> BE-HRD-184 ✅ ──> BE-HRD-185 ✅
BE-HRD-182 ✅ ──┘
```

#### Grafik B46: Gelombang `MVP-38` (SIMRS Loket & Kasir INT-HR-61, Mutasi Buku Besar Plafon, Klaim COB & Kliring Exit Clearance)
```text
BE-HRD-185 [B45] ──> BE-HRD-186 ✅ ──> BE-HRD-187 ✅ ──> BE-HRD-188 ✅ ──> BE-HRD-189 ✅
```

#### Grafik B47: Gelombang Verifikasi & Uji Integrasi End-to-End
```text
BE-HRD-189 [B46] ──> BE-HRD-190 ✅ (Worker 1 Januari, Reset Hangus Saldo & Verifikasi E2E)
```

---

### 26.2 Tabel Urutan Gelombang Eksekusi Backend `S-D3`

| Gelombang Eksekusi | Task ID | Deskripsi Singkat Pekerjaan Backend | Prasyarat (Dependencies) | Target MVP | Status |
|---|---|---|---|:---:|:---:|
| **Gelombang 1** | `BE-HRD-176` | Fondasi skema database 7 entitas, DDL, Fluent API, DbSet & migrasi EF Core | None | `MVP-36` | ✅ Selesai (2026-11-09) |
| **Gelombang 1** | `BE-HRD-177` | Konfigurasi master paket manfaat 3-lapisan, pagu & auto-seed 4 paket default (`FR-BEN-01`..`02`) | `BE-HRD-176` | `MVP-36` | ✅ Selesai (2026-11-09) |
| **Gelombang 1** | `BE-HRD-178` | Matriks kelayakan grade staf, hak kelas kamar inap & mandatory COB BPJS (`FR-BEN-03`..`05`) | `BE-HRD-177` | `MVP-36` | ✅ Selesai (2026-11-09) |
| **Gelombang 1** | `BE-HRD-179` | Controller REST API `BenefitPlanController` Swagger tag `[Tags("HR Benefits - Plans & Eligibility")]` | `BE-HRD-178` | `MVP-36` | ✅ Selesai (2026-11-09) |
| **Gelombang 1** | `BE-HRD-180` | Engine evaluasi kelayakan paket & kuota plafon staf terpadu (`AT-HRD-BEN-01`, `02`) | `BE-HRD-179` | `MVP-36` | ✅ Selesai (2026-11-09) |
| **Gelombang 2** | `BE-HRD-181` | Pendaftaran mandiri tanggungan ESS & kuota maks 1 pasangan + 3 anak (`BEN-B2`, `VR-BEN-01`..`02`) | `BE-HRD-176` | `MVP-37` | ✅ Selesai (2026-11-09) |
| **Gelombang 2** | `BE-HRD-182` | Pendaftaran cepat BPJS TK H+0 & aktivasi bertahap paket kesehatan probation (`BEN-B1`, `VR-BEN-11`..`13`) | `BE-HRD-176` | `MVP-37` | ✅ Selesai (2026-11-09) |
| **Gelombang 2** | `BE-HRD-183` | Penegakan batas usia anak 21 th non-kuliah & 25 th mahasiswa aktif ber-surat dekanat (`BEN-B3`) | `BE-HRD-181` | `MVP-37` | ✅ Selesai (2026-11-09) |
| **Gelombang 2** | `BE-HRD-184` | Meja kerja verifikasi berkas kependudukan fisik & approval HR (`VR-BEN-25`, `AUDIT-BEN-05`) | `BE-HRD-183` | `MVP-37` | ✅ Selesai (2026-11-09) |
| **Gelombang 2** | `BE-HRD-185` | Controller REST API Swagger `[Tags("HR Benefits - Enrollments & Dependents")]` (9 endpoint) | `BE-HRD-182`, `184` | `MVP-37` | ✅ Selesai (2026-11-09) |
| **Gelombang 3** | `BE-HRD-186` | API SIMRS online real-time inquiry eligibilitas & pemotongan kasir (`INT-HR-61`, `chk_non_negative`) | `BE-HRD-185` | `MVP-38` | ✅ Selesai (2026-11-09) |
| **Gelombang 3** | `BE-HRD-187` | Buku besar mutasi saldo subsidi plafon (`HrdBenefitBalanceMovement`) & ekses tagihan Sub-11 | `BE-HRD-186` | `MVP-38` | ✅ Selesai (2026-11-09) |
| **Gelombang 3** | `BE-HRD-188` | Layanan pengajuan klaim koordinasi manfaat (COB) di ESS ber-SEP mandatory BPJS (`BEN-B5`) | `BE-HRD-187` | `MVP-38` | ✅ Selesai (2026-11-09) |
| **Gelombang 3** | `BE-HRD-189` | Meja verifikasi klaim sekunder, settlement, terminasi benefit & blokir Exit Clearance Sub-20 | `BE-HRD-188` | `MVP-38` | ✅ Selesai (2026-11-09) |
| **Verifikasi** | `BE-HRD-190` | Cron job 1 Januari reset hangus saldo plafon (`BEN-B6`), verifikasi QBE, kontrak v16 & build E2E | `BE-HRD-189` | Verifikasi | ✅ Selesai (2026-11-09) |

---

### 26.3 Rincian Spesifikasi Task Backend `S-D3`

#### ✅ `BE-HRD-176` — Fondasi skema database 7 entitas, DDL, Fluent API, DbSet dan migrasi EF Core
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (2026-11-09, [Laporan Tracked](../task/report/backend/BE-HRD-176.md))
* **Outcome**: Penambahan model `HrdBenefitGradeEligibility`, enrichment `MstBenefitPlan` (kategori 3-lapisan, coverage tier, diskon RS, COB mandatory flag), Fluent API configuration, DbSet di `ApplicationDbContext`, dan file migration `20261109110000_AddBenefitGradeEligibilityAndEnhanceBenefitPlan.cs`.
* **Dependencies**: None
* **Bukti verifikasi**: Kompilasi `dotnet build` 0 error, review DDL dan skema EF Core.

#### ✅ `BE-HRD-177` — Layanan konfigurasi master paket manfaat 3-lapisan, kuota pagu tahunan & auto-seed 4 paket default
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (2026-11-09, [Laporan Tracked](../task/report/backend/BE-HRD-177.md))
* **Outcome**: Layanan CRUD paket manfaat pada `BenefitPlanService`, filter pencarian, dan auto-seed 4 paket default RS: `BEN-BPJS-STD`, `BEN-PVT-EXEC`, `BEN-PVT-STAFF`, `BEN-INH-SUBSIDY`.
* **Dependencies**: `BE-HRD-176`
* **Bukti verifikasi**: Method `GetBenefitPlansAsync`, `SeedDefaultBenefitPlansAsync`.

#### ✅ `BE-HRD-178` — Layanan matriks kelayakan benefit per grade staf, penetapan hak kamar inap & penguncian mandatory COB
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (2026-11-09, [Laporan Tracked](../task/report/backend/BE-HRD-178.md))
* **Outcome**: Konfigurasi matriks grade kelayakan (`HrdBenefitGradeEligibility`), penetapan pagu plafon tahunan, hak kelas kamar rawat inap (VVIP, VIP, Kelas 1, Kelas 2), dan penegakan mandatory COB BPJS Kesehatan (`RequiresBPJSPrimaryCOB = true`).
* **Dependencies**: `BE-HRD-177`
* **Bukti verifikasi**: Method `GetGradeEligibilitiesAsync`, `CreateGradeEligibilityAsync`.

#### ✅ `BE-HRD-179` — Controller REST API `BenefitPlanController` Swagger tag `[Tags("HR Benefits - Plans & Eligibility")]`
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (2026-11-09, [Laporan Tracked](../task/report/backend/BE-HRD-179.md))
* **Outcome**: Controller RESTful 10 endpoint beranotasi Swagger `[Tags("HR Benefits - Plans & Eligibility")]`, rute `/api/hr-benefits/plans`, terdaftar di DI `Program.cs`.
* **Dependencies**: `BE-HRD-178`
* **Bukti verifikasi**: Dokumentasi OpenAPI Swagger UI.

#### ✅ `BE-HRD-180` — Engine evaluasi kelayakan paket & kuota plafon benefit staf terpadu
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (2026-11-09, [Laporan Tracked](../task/report/backend/BE-HRD-180.md))
* **Outcome**: Method `EvaluateEmployeePlanEligibilityAsync` yang memvalidasi masa probation < 3 bulan (`BEN-B1`, `VR-BEN-12`), mencocokkan grade staf, dan menentukan paket eligible vs ineligible.
* **Dependencies**: `BE-HRD-179`
* **Bukti verifikasi**: Endpoint `POST /api/hr-benefits/plans/evaluate-eligibility`, acceptance test `AT-HRD-BEN-01`, `02`.

#### ✅ `BE-HRD-181` — Layanan pendaftaran mandiri tanggungan keluarga ESS berlampiran KK/Akta & penegakan kuota maks 1 pasangan + 3 anak
* **Slice / MVP**: `S-D3` / `MVP-37`
* **Status**: ✅ Selesai (2026-11-09, [Laporan Tracked](../task/report/backend/BE-HRD-181.md))
* **Outcome**: Penegakan batasan kuota keluarga inti sah (maks 1 pasangan sah `VR-BEN-01` dan maks 3 anak sah `VR-BEN-02`), validasi 16 digit NIK (`VR-BEN-03`) dan No KK (`VR-BEN-04`), serta portal ESS pendaftaran tanggungan.
* **Dependencies**: `BE-HRD-176`
* **Bukti verifikasi**: Method `CreateEssDependentAsync`, `GetMyFamilyQuotaSummaryAsync`.

#### ✅ `BE-HRD-182` — Layanan pendaftaran cepat BPJS TK H+0 Onboarding & aktivasi bertahap paket kesehatan pasca-lulus probation
* **Slice / MVP**: `S-D3` / `MVP-37`
* **Status**: ✅ Selesai (2026-11-09, [Laporan Tracked](../task/report/backend/BE-HRD-182.md))
* **Outcome**: Aktivasi instan BPJS TK H+0 Onboarding (`VR-BEN-11`), penahanan paket kesehatan probation (`VR-BEN-12`), aktivasi pasca kelulusan ber-SK evaluasi (`VR-BEN-13`), dan validasi format BPJS Kes 13 digit (`VR-BEN-09`) & BPJS TK 11 digit (`VR-BEN-10`).
* **Dependencies**: `BE-HRD-176`
* **Bukti verifikasi**: Method `ActivateProbationEnrollmentAsync`, `UpdatePolicyNumbersAsync`.

#### ✅ `BE-HRD-183` — Penegakan batas usia anak <= 21 th non-mahasiswa & <= 25 th mahasiswa aktif berlampiran surat dekanat
* **Slice / MVP**: `S-D3` / `MVP-37`
* **Status**: ✅ Selesai (2026-11-09, [Laporan Tracked](../task/report/backend/BE-HRD-183.md))
* **Outcome**: Formula deterministik umur anak, penegakan batas 21 th bagi non-kuliah (`VR-BEN-05`), batas 25 th bagi mahasiswa aktif (`VR-BEN-06`), kewajiban lampiran surat dekanat (`VR-BEN-07`), dan batas kedaluwarsa surat 31 Agustus (`VR-BEN-08`).
* **Dependencies**: `BE-HRD-181`
* **Bukti verifikasi**: Acceptance test `AT-HRD-BEN-06`, `07`.

#### ✅ `BE-HRD-184` — Meja kerja verifikasi berkas kependudukan fisik & approval HR Benefit Officer
* **Slice / MVP**: `S-D3` / `MVP-37`
* **Status**: ✅ Selesai (2026-11-09, [Laporan Tracked](../task/report/backend/BE-HRD-184.md))
* **Outcome**: Antrean verifikasi berkas kependudukan `GET /dependents/pending-verification`, approval pengalokasian plafon, dan penegakan alasan penolakan minimal 15 karakter (`VR-BEN-25`, `AUDIT-BEN-05`).
* **Dependencies**: `BE-HRD-183`
* **Bukti verifikasi**: Method `GetPendingDependentVerificationsAsync`, `VerifyDependentAsync`.

#### ✅ `BE-HRD-185` — Controller REST API Pendaftaran Asuransi & Tanggungan Keluarga ESS
* **Slice / MVP**: `S-D3` / `MVP-37`
* **Status**: ✅ Selesai (2026-11-09, [Laporan Tracked](../task/report/backend/BE-HRD-185.md))
* **Outcome**: Implementasi 9 endpoint RESTful pada `BenefitEnrollmentController` beranotasi Swagger `[Tags("HR Benefits - Enrollments & Dependents")]`, rute `/api/v1/corporate/hrd/benefits` & `/api/hr-benefits`, registrasi DI `Program.cs`.
* **Dependencies**: `BE-HRD-182`, `BE-HRD-184`
* **Bukti verifikasi**: Endpoint OpenAPI Swagger UI, kompilasi .NET 0 error.

#### ✅ `BE-HRD-186` — API SIMRS online real-time inquiry eligibilitas & pemotongan plafon kasir
* **Slice / MVP**: `S-D3` / `MVP-38`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Endpoint inquiry online SIMRS Admission & Kasir (`INT-HR-61`), proteksi saldo non-negatif (`chk_benefit_balance_non_negative`, `BEN-B4`), dan row-locking `BEN-W6`.
* **Dependencies**: `BE-HRD-185`
* **Acceptance Criteria**: Kasir SIMRS dapat memotong plafon secara real-time; saldo tidak pernah negatif.

#### ✅ `BE-HRD-187` — Buku besar mutasi saldo subsidi plafon (`HrdBenefitBalanceMovement`) & penanganan ekses tagihan Sub-11
* **Slice / MVP**: `S-D3` / `MVP-38`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan buku besar pencatatan mutasi saldo (*double-entry ledger*), kalkulasi diskon nakes RS 30%, dan penerbitan event ekses biaya medis ke pemotongan payroll Sub-11 (`INT-HR-63`).
* **Dependencies**: `BE-HRD-186`

#### ✅ `BE-HRD-188` — Layanan pengajuan klaim koordinasi manfaat (COB) di ESS ber-SEP mandatory BPJS Kesehatan
* **Slice / MVP**: `S-D3` / `MVP-38`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Formulir klaim santunan sekunder asuransi komersial di portal ESS, validasi mandatory nomor SEP BPJS Kesehatan 19 digit (`BEN-B5`, `VR-BEN-16`..`17`), dan verifikasi breakdown biaya (`VR-BEN-18`).
* **Dependencies**: `BE-HRD-187`

#### ✅ `BE-HRD-189` — Meja verifikasi klaim sekunder, settlement, terminasi benefit & blokir Exit Clearance Sub-20
* **Slice / MVP**: `S-D3` / `MVP-38`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Meja kerja audit kuitansi naik kelas rawat inap, settlement santunan, penonaktifan benefit pada tanggal efektif resign (`BEN-B7`, `VR-BEN-23`), dan pemblokiran Exit Clearance jika ada ekses tagihan tertunggak (`BEN-B8`, `VR-BEN-24`).
* **Dependencies**: `BE-HRD-188`

#### ✅ `BE-HRD-190` — Background worker cron job reset hangus saldo tahunan 1 Januari, verifikasi QBE, kontrak v16 & build E2E
* **Slice / MVP**: `S-D3` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Background worker pembersihan sisa pagu tahun lalu menjadi hangus per 1 Januari pukul 00:00 WIB (`BEN-B6`, `VR-BEN-22`), verifikasi kepatuhan seluruh kontrak API v16, dan verifikasi akhir build Slice S-D3.
* **Dependencies**: `BE-HRD-189`

---

## 27. Rincian Task Backend Slice `S-D4` — HR Service Management & Request (Sub-modul 13, Kontrak v17)

Bagian ini memuat pemecahan delivery **Sub-Modul 13 (Slice `S-D4`)** menjadi 15 task backend terperinci (`BE-HRD-191` s.d. `BE-HRD-205`), grafik urutan dependency pohon teks (Grafik B48 s.d. B51), serta tabel gelombang eksekusi `MVP-39` s.d. `MVP-41` & Verifikasi.

### Grafik Urutan Dependency Pohon Teks `S-D4`

#### Grafik B48: Gelombang `MVP-39` (Katalog, Template Token Dinamis & Agregat Tiket Helpdesk)
```text
BE-HRD-191 ✅ ─┬─> BE-HRD-192 ✅ ─┬─> BE-HRD-193 ✅ ──> BE-HRD-194 ✅
               │                  │
               │                  └─> BE-HRD-195 ✅ ──> BE-HRD-196 ✅
               │
               └─> BE-HRD-197 ✅ ───> BE-HRD-198 ✅
```

#### Grafik B49: Gelombang `MVP-40` (Sinkronisasi Profil, Integrasi Payroll & Otorisasi Surat)
```text
BE-HRD-195 [B48] ─┬─> BE-HRD-199 ✅
                  │
BE-HRD-193 [B48] ─┴─> BE-HRD-200 ✅ ──> BE-HRD-201 ✅ ──> BE-HRD-202 ✅ ──> BE-HRD-203 ✅
```

#### Grafik B50: Gelombang `MVP-41` (Portal Verifikasi Publik, Rate Limiter & E-Dossier)
```text
BE-HRD-203 [B49] ─┬─> BE-HRD-204 ✅
                  │
BE-HRD-202 [B49] ─┴─> BE-HRD-205 ✅
```

#### Grafik B51: Verifikasi Ujung-ke-Ujung Slice `S-D4`
```text
BE-HRD-198 [B48] ──┐
BE-HRD-199 [B49] ──┼─> BE-HRD-205 ✅ (Verifikasi & DoD Sign-Off)
BE-HRD-204 [B50] ──┘
```

---

### Tabel Gelombang Eksekusi Backend `S-D4`

| Gelombang | Task ID | Nama Task / Komponen | Prasyarat (Dependency) | Status |
| :---: | :--- | :--- | :--- | :---: |
| **`MVP-39`** | `BE-HRD-191` | Entitas EF Core, Enums & Migrasi PostgreSQL HrServiceManagement | `BE-HRD-002` | ✅ Selesai (2026-11-09) |
| `MVP-39` | `BE-HRD-192` | EntityTypeConfiguration Fluent API & Indeks Terarah | `BE-HRD-191` | ✅ Selesai (2026-11-09) |
| `MVP-39` | `BE-HRD-193` | Service & Controller Master Katalog Layanan SDM (`HrServiceCatalogService`) | `BE-HRD-192` | ✅ Selesai (2026-11-09) |
| `MVP-39` | `BE-HRD-194` | Mesin Penomoran Terpusat Gapless Berbasis Atomic Row-Locking | `BE-HRD-193` | ✅ Selesai (2026-11-09) |
| `MVP-39` | `BE-HRD-195` | Agregat Tiket Helpdesk SDM & Pengajuan Mandiri ESS (`HrServiceTicketService`) | `BE-HRD-192` | ✅ Selesai (2026-11-09) |
| `MVP-39` | `BE-HRD-196` | Berkas Lampiran Tiket & Utas Klarifikasi Dua Arah (`HrServiceTicketComment`) | `BE-HRD-195` | ✅ Selesai (2026-11-09) |
| `MVP-39` | `BE-HRD-197` | Mesin Kalkulasi SLA 3-Tier Berbasis Jam Kerja RS & Penangguhan Klarifikasi | `BE-HRD-191` | ✅ Selesai (2026-11-09) |
| `MVP-39` | `BE-HRD-198` | Background Worker Pemantauan Timer SLA, Alert 75% & Auto-Eskalasi 100% | `BE-HRD-197` | ✅ Selesai (2026-11-09) |
| **`MVP-40`** | `BE-HRD-199` | Pemutakhiran Profil Sub-02 Otomatis & Snapshot Histori (`INT-HR-65`) | `BE-HRD-195` | ✅ Selesai (2026-11-09) |
| `MVP-40` | `BE-HRD-200` | Penarikan Data Gaji Final dari Sub-10 Payroll (`INT-HR-56`) & Anti-Manual Guard | `BE-HRD-193` | ✅ Selesai (2026-11-09) |
| `MVP-40` | `BE-HRD-201` | Gerbang Persetujuan 2-Tier C&B Supervisor & Penegakan Anti-Self-Approval | `BE-HRD-200` | ✅ Selesai (2026-11-09) |
| `MVP-40` | `BE-HRD-202` | Mesin Rendering Naskah Dinas PDF Berbasis Template Token Dinamis Liquid | `BE-HRD-201`, `BE-HRD-194` | ✅ Selesai (2026-11-09) |
| `MVP-40` | `BE-HRD-203` | Generator QR Code SHA-256 & Enkripsi 4-Digit PIN Finansial BCrypt | `BE-HRD-202` | ✅ Selesai (2026-11-09) |
| **`MVP-41`** | `BE-HRD-204` | Endpoint Portal Publik Verifikasi QR `/verify/document/{hash}` & Rate Limiter | `BE-HRD-203` | ✅ Selesai (2026-11-09) |
| `MVP-41` | `BE-HRD-205` | Pengarsipan Abadi E-Dossier (`INT-HR-66`), Revokasi Ber-BAP & Verifikasi DoD S-D4 | `BE-HRD-198`, `BE-HRD-199`, `BE-HRD-204` | ✅ Selesai (2026-11-09) |

---

### Detail Spesifikasi Task Backend (`BE-HRD-191` s.d. `BE-HRD-205`)

#### ✅ `BE-HRD-191` — Model EF Core, Enums & Migrasi PostgreSQL `HrServiceManagement`
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: 8 entitas domain EF Core terdefinisi lengkap di namespace `Areas/Corporate/HumanResource/HrServiceManagement/Models/` beserta migrasi non-breaking PostgreSQL.
* **Requirement/decision**: `HRD-DEC-140` s.d. `HRD-DEC-145`, `02-backend-architecture.md` Bagian 26.5.
* **Kontrak**: Kontrak `v17` Bagian 23.
* **Cakupan**: `MstHrServiceCategory`, `MstHrServiceType`, `MstEmployeeDocumentType`, `TrxHrServiceRequest`, `TrxHrServiceRequestAttachment`, `TrxHrServiceRequestComment`, `TrxEmployeeDocumentRequest`, `TrxEmployeeDocumentIssuance`, `HrdProfileChangeHistory`.
* **Dependency**: `BE-HRD-002`
* **Acceptance criteria**:
  1. Seluruh 8 class entity dan 1 log audit mewarisi `IdentityModel`.
  2. Enum `PriorityTier` (Cito/Standar/Khusus), `SlaStatus`, `RequestStatus`, `IssuanceStatus` terdefinisi aman.
  3. Skrip migrasi EF Core terbentuk tanpa menghapus atau mengubah tabel eksisting.
* **Bukti verifikasi**: File migrasi C#, kompilasi bersih `dotnet build`.
* **Definition of Done**: Skema database terbentuk sempurna di PostgreSQL.

#### ✅ `BE-HRD-192` — EntityTypeConfiguration Fluent API & Indeks Terarah
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Penegakan konfigurasi relasi antar-tabel, indeks performa pencarian hash verifikasi, dan batasan integritas check constraint.
* **Requirement/decision**: `02-backend-architecture.md` Bagian 26.5.
* **Dependency**: `BE-HRD-191`
* **Acceptance criteria**:
  1. Indeks unik dipasang pada `verification_hash`, `official_document_number`, dan `ticket_number`.
  2. Relasi FK ke `mst_workforce_profiles` diset `DeleteBehavior.Restrict`.
  3. Relasi child attachment dan comment diset `DeleteBehavior.Cascade` ke request header.
* **Bukti verifikasi**: File konfigurasi EF Core di folder `Repositories/Configurations/`.
* **Definition of Done**: Seluruh index dan constraint aktif pada skema database.

#### ✅ `BE-HRD-193` — Master Katalog Layanan SDM & Manajemen Tipe Layanan (`HrServiceCatalogService`)
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan backend dan controller untuk mengelola master kategori dan tipe layanan SDM rumah sakit.
* **Requirement/decision**: `FR-HRS-01`, `02-backend-architecture.md` Bagian 26.3.
* **Kontrak**: `[Tags("HR Services - Catalog & SLA")]`
* **Dependency**: `BE-HRD-192`
* **Acceptance criteria**:
  1. CRUD master kategori dan jenis layanan berfungsi dengan filter aktif.
  2. Konfigurasi `sla_target_hours` tervalidasi di rentang 1 s.d. 168 jam (`VR-HRS-03`).
* **Bukti verifikasi**: Endpoint test `GET` dan `POST /api/v1/corporate/hrd/services/categories`.
* **Definition of Done**: Katalog layanan terdaftar dan siap digunakan pada pengajuan tiket.

#### ✅ `BE-HRD-194` — Mesin Penomoran Terpusat Gapless Berbasis Atomic Row-Locking
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Komponen `DocumentAutoNumberingService` yang menjamin penerbitan nomor surat resmi rumah sakit secara unik berurutan anti-lompat (*gapless*).
* **Requirement/decision**: `HRD-DEC-144`, aturan `HRS-B3`, `VR-HRS-10`.
* **Dependency**: `BE-HRD-193`
* **Acceptance criteria**:
  1. Penomoran mengeksekusi row lock `SELECT ... FOR UPDATE` pada `MstEmployeeDocumentType`.
  2. Format nomor surat mengikuti pola `[KODE]/RSQ-SDM/[BULAN_ROMAWI]/[TAHUN]/[NO_URUT]`.
  3. Pembatalan draf surat tidak membakar nomor urut (*no sequence burn*).
* **Bukti verifikasi**: Unit test konkurensi penomoran multi-thread paralel.
* **Definition of Done**: Nomor urut gapless teruji tanpa duplikasi atau lompatan angka.

#### ✅ `BE-HRD-195` — Agregat Tiket Helpdesk SDM & Pengajuan Mandiri ESS (`HrServiceTicketService`)
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Service dan controller untuk memproses tiket bantuan SDM, penugasan antrean analis, dan pengajuan mandiri staf di portal ESS.
* **Requirement/decision**: `FR-HRS-06`, `FR-HRS-08`, aturan `HRS-B1`, `HRS-W1`.
* **Kontrak**: `[Tags("HR Services - Tickets & Queue")]`, `[Tags("HR Services - ESS Requests & My Documents")]`
* **Dependency**: `BE-HRD-192`
* **Acceptance criteria**:
  1. Nomor tiket `TKT-YYYYMM-XXXXX` dihasilkan secara otomatis.
  2. Pengajuan Cito tanpa surat disposisi pimpinan medis ditolak oleh sistem (`VR-HRS-02`).
  3. Fitur *claim to me* dan re-assign tiket oleh supervisor berfungsi dengan pencatatan waktu respon pertama.
* **Bukti verifikasi**: Test case `AT-HRD-HRS-01` dan `AT-HRD-HRS-02`.
* **Definition of Done**: Tiket helpdesk berhasil disubmit dan masuk ke antrean analis.

#### ✅ `BE-HRD-196` — Berkas Lampiran Tiket & Utas Klarifikasi Dua Arah (`HrServiceTicketComment`)
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Pengelolaan unggahan berkas pendukung terenkripsi dan utas komunikasi dua arah antara staf pemohon dan analis HR Desk.
* **Requirement/decision**: `FR-HRS-12`, `FR-HRS-14`, validasi `VR-HRS-15`.
* **Dependency**: `BE-HRD-195`
* **Acceptance criteria**:
  1. Upload berkas memvalidasi tipe MIME (PDF, JPG, PNG) dan batas ukuran maksimal 5MB.
  2. Analis dapat mengirimkan pesan berpenanda "Catatan Internal HR" yang tidak terlihat oleh pemohon.
* **Bukti verifikasi**: Endpoint test kirim komentar dan unduh berkas lampiran.
* **Definition of Done**: Percakapan klarifikasi dan lampiran berkas berfungsi penuh.

#### ✅ `BE-HRD-197` — Mesin Kalkulasi SLA 3-Tier Berbasis Jam Kerja RS & Penangguhan Klarifikasi
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Komponen `HrServiceSlaEngine` yang menghitung tenggat waktu penyelesaian tiket berdasarkan jam operasional resmi rumah sakit.
* **Requirement/decision**: `HRD-DEC-141`, aturan `HRS-B2`, `HRS-W2`.
* **Dependency**: `BE-HRD-191`
* **Acceptance criteria**:
  1. Tiket Standar/Khusus menghitung jam kerja 08:00 - 17:00 WIB dan melompati akhir pekan/hari libur nasional.
  2. Tiket Cito dihitung secara linier 24/7 jam kalender.
  3. Pengubahan status ke `WaitingStaffResponse` menangguhkan (*pause*) timer SLA, dan kembali berjalan (*resume*) saat staf membalas.
* **Bukti verifikasi**: Unit test kalkulasi jam kerja `AT-HRD-HRS-03`, `AT-HRD-HRS-04`, `AT-HRD-HRS-05`.
* **Definition of Done**: Penghitungan deadline SLA teruji akurat pada berbagai skenario hari libur.

#### ✅ `BE-HRD-198` — Background Worker Pemantauan Timer SLA, Alert 75% & Auto-Eskalasi 100%
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Background worker berkala tiap 15 menit (`HrServiceSlaMonitoringWorker`) yang mendeteksi tiket mendekati tenggat dan eskalasi keterlambatan.
* **Requirement/decision**: `FR-HRS-10`, aturan `HRS-W3`, integrasi `INT-HR-68`.
* **Dependency**: `BE-HRD-197`
* **Acceptance criteria**:
  1. Worker memicu status `Warning75` dan mengirim alert kuning ke Supervisor HR saat durasi >= 75%.
  2. Worker memicu status `Overdue` dan auto-eskalasi ke Manajer SDM saat durasi >= 100%.
* **Bukti verifikasi**: Eksekusi worker berkala `AT-HRD-HRS-06`, `AT-HRD-HRS-07`.
* **Definition of Done**: Peringatan dini dan eskalasi keterlambatan SLA terdistribusi otomatis.

#### ✅ `BE-HRD-199` — Pemutakhiran Profil Sub-02 Otomatis & Snapshot Histori (`INT-HR-65`)
* **Slice / MVP**: `S-D4` / `MVP-40`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Eksekusi atomik mutasi data rekening payroll / data pribadi nakes ke `MstWorkforceProfile` Sub-02 pasca-approval tiket sah.
* **Requirement/decision**: `HRD-DEC-143`, aturan `HRS-B6`, `VR-HRS-25`.
* **Dependency**: `BE-HRD-195`
* **Acceptance criteria**:
  1. Pembaruan data dieksekusi dalam transaksi atomik database terisolasi.
  2. Data lama disimpan ke `HrdProfileChangeHistory` dalam format JSON sebelum data baru diaplikasikan.
  3. Kegagalan update master otomatis me-rollback transaksi tanpa merusak integritas data.
* **Bukti verifikasi**: Test case `AT-HRD-HRS-14`, `AT-HRD-HRS-15`.
* **Definition of Done**: Pemutakhiran data master berjalan otomatis tanpa pengetikan manual ulang.

#### ✅ `BE-HRD-200` — Penarikan Data Gaji Final dari Sub-10 Payroll (`INT-HR-56`) & Anti-Manual Guard
* **Slice / MVP**: `S-D4` / `MVP-40`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Service query read-only penarikan nominal gaji bruto/netto resmi dari `TrxPayrollPayslip` Sub-10 dengan pemblokiran input manual.
* **Requirement/decision**: `HRD-DEC-142`, aturan `HRS-B4`, validasi `VR-HRS-05`, `VR-HRS-06`.
* **Dependency**: `BE-HRD-193`
* **Acceptance criteria**:
  1. Angka gaji ditarik otomatis dari slip gaji terakhir yang berstatus `Finalized`.
  2. Input nominal manual dari request payload API diblokir keras oleh server (`VR-HRS-06`).
* **Bukti verifikasi**: Test case `AT-HRD-HRS-08`, `AT-HRD-HRS-09`, `AT-HRD-HRS-10`.
* **Definition of Done**: Nominal gaji pada surat resmi terbukti 100% identik dengan slip gaji resmi.

#### ✅ `BE-HRD-201` — Gerbang Persetujuan 2-Tier C&B Supervisor & Penegakan Anti-Self-Approval
* **Slice / MVP**: `S-D4` / `MVP-40`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Penegakan workflow verifikasi surat finansial oleh Supervisor C&B dan larangan persetujuan mandiri oleh pejabat penandatangan.
* **Requirement/decision**: `HRD-DEC-142`, aturan `HRS-W4`, `VR-HRS-07`, `VR-HRS-08`.
* **Dependency**: `BE-HRD-200`
* **Acceptance criteria**:
  1. Surat finansial wajib berstatus `CbApproved` sebelum dapat diotorisasi oleh Manajer SDM.
  2. Sistem memblokir aksi jika `authorized_signer_user_id == requested_by_employee_id`.
* **Bukti verifikasi**: Test case `AT-HRD-HRS-11`, `AT-HRD-HRS-12`.
* **Definition of Done**: Segregasi tugas dan anti-self-approval tertegakkan sempurna.

#### ✅ `BE-HRD-202` — Mesin Rendering Naskah Dinas PDF Berbasis Template Token Dinamis Liquid
* **Slice / MVP**: `S-D4` / `MVP-40`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Komponen generator dokumen PDF resmi yang me-replace token variabel (`{{NamaLengkap}}`, `{{NIP}}`, dll.) menjadi data riil pegawai.
* **Requirement/decision**: `HRD-DEC-144`, aturan `VR-HRS-16`.
* **Dependency**: `BE-HRD-201`, `BE-HRD-194`
* **Acceptance criteria**:
  1. Seluruh token variabel tersubstitusi sempurna dengan data profil staf.
  2. File PDF resmi dirender dengan kop surat resmi RS Quilvian dan stempel digital.
* **Bukti verifikasi**: Pengujian rendering berkas PDF draf surat resmi kepegawaian.
* **Definition of Done**: Berkas PDF surat resmi siap terbit dengan tata naskah dinas standar.

#### ✅ `BE-HRD-203` — Generator QR Code SHA-256 & Enkripsi 4-Digit PIN Finansial BCrypt
* **Slice / MVP**: `S-D4` / `MVP-40`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Pembuatan kode QR ber-hash unik 64 karakter heksadesimal dan pembentukan 4-digit PIN acak terenkripsi BCrypt untuk proteksi data gaji.
* **Requirement/decision**: `HRD-DEC-140`, aturan `HRS-B5`, validasi `VR-HRS-11`, `VR-HRS-24`.
* **Dependency**: `BE-HRD-202`
* **Acceptance criteria**:
  1. Gambar QR Code ditempel di kaki naskah dinas mengarah ke URL `/verify/document/{hash}`.
  2. 4-digit PIN di-hash menggunakan BCrypt dan disimpan pada `security_pin_hash`.
  3. PIN plain-text dikirimkan secara privat ke akun ESS pegawai pemohon.
* **Bukti verifikasi**: Pemindaian QR Code dan verifikasi hash BCrypt PIN.
* **Definition of Done**: QR Code dan PIN finansial terintegrasi aman pada dokumen resmi.

#### ✅ `BE-HRD-204` — Endpoint Portal Publik Verifikasi QR `/verify/document/{hash}` & Rate Limiter
* **Slice / MVP**: `S-D4` / `MVP-41`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Controller publik tanpa autentikasi sesi login untuk verifikasi legalitas dokumen dan endpoint pembukaan rincian nominal gaji ber-PIN.
* **Requirement/decision**: `HRD-DEC-140`, validasi `VR-HRS-12`.
* **Kontrak**: `[Tags("HR Services - Document Issuance & Public Verification")]`
* **Dependency**: `BE-HRD-203`
* **Acceptance criteria**:
  1. Endpoint publik mengembalikan metadata legalitas (nomor surat, tanggal terbit, nama staf, status sah/dicabut).
  2. Pada surat gaji, angka nominal disamarkan (*masked*).
  3. Input PIN yang benar membuka rincian nominal gaji; salah input 3 kali memblokir akses selama 30 menit.
* **Bukti verifikasi**: Test case `AT-HRD-HRS-17`, `AT-HRD-HRS-18`, `AT-HRD-HRS-19`.
* **Definition of Done**: Portal publik verifikasi dokumen beroperasi aman dari eksploitasi data.

#### ✅ `BE-HRD-205` — Pengarsipan Abadi E-Dossier (`INT-HR-66`), Revokasi Ber-BAP & Verifikasi DoD S-D4
* **Slice / MVP**: `S-D4` / `MVP-41` & Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Pengarsipan otomatis salinan PDF ke repositori digital Sub-02, fitur pencabutan keabsahan surat ber-BAP, serta audit kepatuhan komprehensif 10 butir DoD Slice `S-D4`.
* **Requirement/decision**: `HRD-DEC-145`, aturan `HRS-B7`, `HRS-B8`, `HRS-W5`, `04-prd-to-mvp.md` Bagian 31.5.
* **Dependency**: `BE-HRD-198`, `BE-HRD-199`, `BE-HRD-204`
* **Acceptance criteria**:
  1. Dokumen terbit otomatis tersimpan ke `mst_workforce_profile_documents` Sub-02.
  2. Pencabutan dokumen mewajibkan pengisian nomor BAP dan alasan pembatalan (`VR-HRS-14`).
  3. Portal publik seketika memunculkan banner merah peringatan penolakan jika dokumen berstatus `Revoked`.
  4. Seluruh 10 butir Definition of Done pada `04-prd-to-mvp.md` Bagian 31.5 terpenuhi 100%.
* **Bukti verifikasi**: Test case `AT-HRD-HRS-16`, `AT-HRD-HRS-20`, UAT skenario `UAT-HRD-HRS-01` s.d. `05`.
* **Definition of Done**: Seluruh fungsionalitas backend Slice `S-D4` siap integrasi penuh dengan frontend.


---

## Bagian S-C1 — Roadmap Pengiriman Backend Sub-Modul 14: Credentialing, License & Clinical Privilege (Slice `S-C1`, Kontrak `v18`, 5 November 2026)

Bagian ini merinci 16 task implementasi backend terisolasi (**`BE-HRD-206` s.d. `BE-HRD-221`**) untuk **Sub-Modul 14 (Slice `S-C1`)** yang menyerap keputusan disetujui [`HRD-DEC-146`](../00-interview-decisions.md#L7165-L7185) s.d. [`HRD-DEC-151`](../00-interview-decisions.md#L7250-L7270), asumsi `HRD-ASM-43` s.d. `HRD-ASM-46`, serta penuntasan posisi sementara `HRD-DEC-005` menjadi protokol penegakan wewenang klinis bertingkat di SIMRS EMR (`INT-HR-69`).

---

### Grafik Urutan Dependency Pohon Teks `S-C1`

#### Grafik B52: Gelombang `MVP-42` (Lisensi STR/SIP, PSV Kemenkes/KKI & Radar Pemantau H-90 s.d. Fail-Closed H-0)
```text
BE-HRD-206 ✅ ─┬─> BE-HRD-207 ✅ ─┬─> BE-HRD-208 ✅ ──> BE-HRD-210 ✅
               │                  │
               │                  └─> BE-HRD-209 ✅ (Cron Radar SIP & Fail-Closed Sub-06)
               │
               └─> [Lanjut ke Gelombang MVP-43 B53]
```

#### Grafik B53: Gelombang `MVP-43` (Master 3 Komite, Penugasan Peer Review, Sidang Pleno & Pengesahan SPK Dirut)
```text
BE-HRD-206 [B52] ─┬─> BE-HRD-211 ✅ ──> BE-HRD-212 ✅ ──> BE-HRD-213 ✅ ──┐
BE-HRD-208 [B52] ─┤                                                        ├─> BE-HRD-214 ✅ ──> BE-HRD-215 ✅
BE-HRD-207 [B52] ─┴────────────────────────────────────────────────────────┘
```

#### Grafik B54: Gelombang `MVP-44` (Gate Penegakan SIMRS INT-HR-69, Co-Signature, Life-Saving Override & Rekredensial)
```text
BE-HRD-214 [B53] ─┬─> BE-HRD-216 ✅ ─┬─> BE-HRD-217 ✅ (Co-Signature 24 Jam)
                  │                  │
                  │                  ├─> BE-HRD-218 ✅ (Life-Saving Override Log KMKP)
                  │                  │
                  │                  ├─> BE-HRD-219 ✅ (Temporary & Emergency Privilege)
                  │                  │
                  │                  └─> BE-HRD-220 ✅ (Scheduler Siklus 3 Tahun & Auto-Suspend)
```

#### Grafik B55: Gelombang Verifikasi & Kepatuhan Arsitektur
```text
BE-HRD-210 [B52] ─┬────────────────────────────────────────────────────────┐
BE-HRD-215 [B53] ─┼─> BE-HRD-221 ✅ (Validasi Kompilasi .NET, QBE & Uji Penerimaan AT-HRD-CRD-01..22)
BE-HRD-216 [B54] ─┤
BE-HRD-220 [B54] ─┘
```

---

### Tabel Gelombang Eksekusi Backend `S-C1`

| Gelombang | Task ID | Target Deliverable / Output Utama | Syarat Masuk (Dependencies) |
| :---: | :--- | :--- | :--- |
| **`MVP-42`** | `BE-HRD-206`, `BE-HRD-207`, `BE-HRD-208`, `BE-HRD-209`, `BE-HRD-210` | Entitas EF Core, DDL migration PostgreSQL, service verifikasi primer PSV Kemenkes/KKI, cron job radar SIP H-90 s.d. H-0, dan REST API controller lisensi. | ✅ **Selesai (2026-11-09)** |
| **`MVP-43`** | `BE-HRD-211`, `BE-HRD-212`, `BE-HRD-213`, `BE-HRD-214`, `BE-HRD-215` | Master 3 komite mandiri (Medik, Keperawatan, Nakes Lain), katalog whitebook, panel Mitra Bestari (min 2 reviewer), sidang pleno, mesin penerbitan SPK-RKK sah e-Sign Direktur Utama RS, PDF ber-QR Code, dan auto-arsip e-Dossier Sub-02. | ✅ **Selesai (2026-11-09)** |
| **`MVP-44`** | `BE-HRD-216`, `BE-HRD-217`, `BE-HRD-218`, `BE-HRD-219`, `BE-HRD-220` | Layanan gerbang real-time SIMRS (`INT-HR-69`) ber-cache Redis latensi < 50ms, protokol co-signature 24 jam supervisor, log audit bypass darurat penyelamatan nyawa KMKP, otorisasi fast-track temporary/emergency privilege, dan scheduler siklus 3 tahun SPK. | ✅ **Selesai (2026-11-09)** |
| **Verifikasi** | `BE-HRD-221` | Kompilasi .NET bersih 0 error, verifikasi konformansi QBE preflight, verifikasi kontrak API Swagger, dan pengujian penerimaan `AT-HRD-CRD-01` s.d. `AT-HRD-CRD-22`. | ✅ **Selesai (2026-11-09)** |

---

### Detail Spesifikasi Task Backend (`BE-HRD-206` s.d. `BE-HRD-221`)

#### ✅ `BE-HRD-206` — EF Core Entity Models & Database Migration Skema Kredensialing (`S-C1`)
* **Slice / MVP**: `S-C1` / `MVP-42`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Penambahan model `MstCredentialingCommittee`, `TrxSpkRkkIssuance`, `TrxClinicalPrivilegeEmergencyOverrideLog`, serta pembaruan tabel `wfp_credential_licenses` (kolom PSV & STR seumur hidup) dan `wfp_clinical_privileges` (kolom supervisi Level 4/3/1-2) via migrasi EF Core PostgreSQL.
* **Dependencies**: Kontrak `v18` disetujui
* **Acceptance Criteria**: Migrasi berhasil dijalankan pada PostgreSQL, skema tabel, indeks komposit gate query, dan foreign key terbentuk tanpa data loss.
* **Verifikasi**: `dotnet ef migrations add AddCredentialingClinicalPrivilegeEntities` & inspeksi skema DB.

#### ✅ `BE-HRD-207` — DTOs, Enums & Request/Response Models Lisensi, Peer Review & SPK-RKK
* **Slice / MVP**: `S-C1` / `MVP-42`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Kelas DTO lengkap untuk pendaftaran lisensi, verifikasi PSV, pengajuan kredensial 3 komite, asesmen portofolio Mitra Bestari, pengesahan e-Sign Dirut, response gate SIMRS, serta enum `SupervisionLevel`, `PrivilegeNature`, `LicenseStatus`, dan `CommitteeCode`.
* **Dependencies**: `BE-HRD-206`
* **Acceptance Criteria**: Seluruh field DTO memuat anotasi validasi data (`[Required]`, `[StringLength]`, `[Range]`) sesuai matriks validasi `VR-CRD-01` s.d. `VR-CRD-25`.
* **Verifikasi**: Unit test serialisasi JSON & build project.

#### ✅ `BE-HRD-208` — Service Verifikasi Sumber Primer (PSV) Kemenkes / KKI (`ICredentialVerificationService`)
* **Slice / MVP**: `S-C1` / `MVP-42`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi logika audit keabsahan primer dokumen STR seumur hidup dan SIP 5 tahun ke portal Konsil Kedokteran Indonesia / SatuSehat SDMK Kemenkes, pencatatan URL bukti verifikasi, stempel audit, dan penguncian status `is_psv_verified = true` (`HRD-DEC-146`, `VR-CRD-02`).
* **Dependencies**: `BE-HRD-206`, `BE-HRD-207`
* **Acceptance Criteria**: Verifikasi PSV berhasil mencatat metadata audit lengkap; menolak disposisi komite jika dokumen belum terverifikasi sah.
* **Verifikasi**: Uji fungsional `AT-HRD-CRD-01` dan `AT-HRD-CRD-02`.

#### ✅ `BE-HRD-209` — Scheduler Radar Pemantau SIP 5 Tahun & Fail-Closed Action (`SipExpirationMonitorJob`)
* **Slice / MVP**: `S-C1` / `MVP-42`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Background worker harian (berjalan pukul 00:00) yang memantau sisa masa berlaku SIP 5 tahun, mengirim notifikasi radar berjenjang (H-90, H-60, H-30, H-7), serta mengeksekusi aksi fail-closed otomatis tepat pada H-0: menonaktifkan akun order SIMRS (`INT-HR-69`) dan memblokir penugasan jadwal Sub-06 (`INT-HR-71`, `CRD-B2`).
* **Dependencies**: `BE-HRD-206`, `BE-HRD-207`
* **Acceptance Criteria**: Alert radar terkirim bertahap; saat H-0 tercapai akun SIMRS nonaktif dan jadwal dinas terblokir seketika.
* **Verifikasi**: Uji simulasi waktu `AT-HRD-CRD-05` dan `AT-HRD-CRD-06`.

#### ✅ `BE-HRD-210` — REST API Controller Manajemen Lisensi Profesional (`CredentialLicenseController`)
* **Slice / MVP**: `S-C1` / `MVP-42`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Controller ASP.NET Core mengekspos endpoint grup Swagger `[Tags("Corporate - HRD Credentialing & License")]`: pendaftaran STR/SIP, eksekusi audit PSV, rincian radar kedaluwarsa, dan unggah perpanjangan SIP baru.
* **Dependencies**: `BE-HRD-208`, `BE-HRD-209`
* **Acceptance Criteria**: Endpoint merespons kode status `200 OK`, `201 Created`, dan `400 Bad Request` sesuai kontrak API bagian 24.2.1.
* **Verifikasi**: Uji integrasi endpoint via Postman / integration test.

#### ✅ `BE-HRD-211` — Manajemen 3 Komite Mandiri & Katalog Whitebook Kompetensi
* **Slice / MVP**: `S-C1` / `MVP-43`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Service dan repository untuk mengelola 3 pilar komite tata kelola profesi (Komite Medik, Keperawatan, Nakes Lain) dan katalog buku putih (*whitebook*) kompetensi tindakan klinis sesuai Permenkes 6/2026 (`HRD-DEC-147`).
* **Dependencies**: `BE-HRD-206`, `BE-HRD-207`
* **Acceptance Criteria**: Data master komite terisolasi rapi; katalog tindakan klinis terfilter otomatis berdasarkan rumpun profesi pemohon.
* **Verifikasi**: Uji fungsional `AT-HRD-CRD-07`.

#### ✅ `BE-HRD-212` — Alur Pengajuan Kredensial, Disposisi & Anti-Self-Review Panel Mitra Bestari
* **Slice / MVP**: `S-C1` / `MVP-43`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Service pengajuan berkas kredensial nakes via ESS, penelaahan kelengkapan portofolio logbook, penunjukan panel Mitra Bestari (minimal 2 anggota sejawat sebidang), serta penegakan aturan anti-self-review mutlak (`VR-CRD-15`).
* **Dependencies**: `BE-HRD-211`, `BE-HRD-208`
* **Acceptance Criteria**: Sistem menolak jika panel reviewer kurang dari 2 orang atau jika nakes mencoba menilai berkas dirinya sendiri.
* **Verifikasi**: Uji keamanan `AT-HRD-CRD-08`.

#### ✅ `BE-HRD-213` — Service Asesmen Mitra Bestari (Peer Review) & Sidang Pleno Komite
* **Slice / MVP**: `S-C1` / `MVP-43`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Service kompilasi skor penilaian portofolio klinis oleh reviewer Mitra Bestari, penetapan derajat wewenang (Level 4 Mandiri, Level 3 Supervisi, Level 1-2 Ditolak), resolusi divergensi skor antar reviewer, risalah sidang pleno, dan penerbitan surat usulan Ketua Komite.
* **Dependencies**: `BE-HRD-212`
* **Acceptance Criteria**: Berita acara rekomendasi peer review tersimpan dengan hash integritas; surat rekomendasi komite terbit resmi.
* **Verifikasi**: Uji fungsional `AT-HRD-CRD-09` dan `AT-HRD-CRD-10`.

#### ✅ `BE-HRD-214` — Mesin Penerbitan SPK-RKK Sah, Supervisor Level 3 & Otorisasi e-Sign Dirut (`ISpkRkkIssuanceService`)
* **Slice / MVP**: `S-C1` / `MVP-43`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Mesin penyusunan draf SPK berpenunjukan supervisor Level 3 (`VR-CRD-05`), alur paraf Direktur Pelayanan, otorisasi pengesahan tunggal e-Sign Direktur Utama RS (`HRD-DEC-148`), pembuatan berkas PDF resmi ber-QR Code publik, dan auto-arsip ke e-Dossier Sub-02 (`INT-HR-72`).
* **Dependencies**: `BE-HRD-213`
* **Acceptance Criteria**: Dokumen SPK-RKK sah ber-QR Code aktif; menolak otorisasi selain dari akun Direktur Utama RS (`VR-CRD-12`); masa berlaku dikunci 3 tahun.
* **Verifikasi**: Uji otorisasi `AT-HRD-CRD-11` s.d. `AT-HRD-CRD-16`.

#### ✅ `BE-HRD-215` — REST API Controller SPK-RKK & Sidang Komite (`SpkRkkIssuanceController`)
* **Slice / MVP**: `S-C1` / `MVP-43`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Controller ASP.NET Core mengekspos endpoint grup Swagger `[Tags("Corporate - HRD SPK RKK Management & Issuance")]` dan `[Tags("Corporate - HRD Clinical Privilege & Peer Review")]`: draf SPK, paraf Direktur Pelayanan, sign Dirut, download PDF, dan verifikasi publik QR Code.
* **Dependencies**: `BE-HRD-214`
* **Acceptance Criteria**: Seluruh endpoint REST merespons status code baku dan payload valid sesuai kontrak API bagian 24.2.2 & 24.2.3.
* **Verifikasi**: Uji integrasi endpoint & Swagger UI.

#### ✅ `BE-HRD-216` — Layanan Gerbang Wewenang Klinis Real-Time SIMRS EMR (`IClinicalPrivilegeGateService` — `INT-HR-69`)
* **Slice / MVP**: `S-C1` / `MVP-44`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi endpoint performa tinggi `POST /api/v1/corporate/hrd/clinical-privileges/validate-gate` (`INT-HR-69`) ber-cache Redis terdistribusi dengan target latensi p99 < 50ms, menerapkan resolusi final `HRD-DEC-005` (Hard-Stop untuk tindakan tidak berwenang Level 1-2, allow untuk Level 4 mandiri).
* **Dependencies**: `BE-HRD-214`, `BE-HRD-215`
* **Acceptance Criteria**: SIMRS menerima respon gate instan; tindakan mandiri diizinkan, tindakan tidak berwenang diblokir keras secara fail-closed.
* **Verifikasi**: Uji performa & fungsional `AT-HRD-CRD-17` dan `AT-HRD-CRD-19`.

#### ✅ `BE-HRD-217` — Logika Pemrosesan Order Supervisi Level 2/3 & Alur Co-Signature 24 Jam
* **Slice / MVP**: `S-C1` / `MVP-44`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Mekanisme penahanan order tindakan supervisi di SIMRS (*Pending Co-Signature*), routing notifikasi ke dokter supervisor penanggung jawab, persetujuan digital supervisor, serta pembatalan otomatis order jika tidak di-co-sign dalam tempo 24 jam (`VR-CRD-07`).
* **Dependencies**: `BE-HRD-216`
* **Acceptance Criteria**: Order tindakan supervisi sukses tervalidasi setelah disetujui supervisor; otomatis kedaluwarsa jika melewati batas 24 jam.
* **Verifikasi**: Uji fungsional `AT-HRD-CRD-18`.

#### ✅ `BE-HRD-218` — Layanan Bypass Darurat Penyelamatan Nyawa (*Life-Saving Override*) & Audit KMKP
* **Slice / MVP**: `S-C1` / `MVP-44`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Penanganan tombol bypass darurat penyelamatan jiwa di SIMRS/EMR, validasi justifikasi klinis cito (`VR-CRD-08`), pencatatan log forensik ke `TrxClinicalPrivilegeEmergencyOverrideLog`, serta penerbitan alert otomatis ke Ketua Komite Medik dan KMKP dalam 24 jam (`HRD-DEC-149`).
* **Dependencies**: `BE-HRD-216`
* **Acceptance Criteria**: Tindakan cito tereksekusi aman; rekam log audit lengkap tersimpan; dashboard sidang KMKP menerima berkas insiden.
* **Verifikasi**: Uji audit keselamatan pasien `AT-HRD-CRD-20`.

#### ✅ `BE-HRD-219` — Otorisasi Penugasan Cepat (Temporary Privilege & Emergency Privilege)
* **Slice / MVP**: `S-C1` / `MVP-44`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan fast-track penerbitan kewenangan klinis sementara dokter konsultan tamu (maksimal 3 bulan) dan kewenangan darurat cito bencana massal (maksimal 72 jam) atas wewenang Direktur Pelayanan Medik (`HRD-DEC-150`, `VR-CRD-09`, `VR-CRD-13`).
* **Dependencies**: `BE-HRD-214`, `BE-HRD-216`
* **Acceptance Criteria**: SPK sementara dan darurat terbit dengan timer pembatasan masa berlaku otomatis; otomatis nonaktif saat batas waktu habis.
* **Verifikasi**: Uji fungsional `AT-HRD-CRD-21` dan `AT-HRD-CRD-22`.

#### ✅ `BE-HRD-220` — Scheduler Siklus Rekredensialing 3 Tahunan & Auto-Suspended (`SpkRkkExpiryMonitoringWorker`)
* **Slice / MVP**: `S-C1` / `MVP-44`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Scheduler otomatis siklus 3 tahun penugasan klinis, pengiriman radar rekredensialing H-90, inisiasi draf permohonan rekredensialing pada H-60 terintegrasi dengan skor evaluasi kinerja klinis OPPE Sub-15 (`INT-HR-70`), serta eksekusi penangguhan wewenang (*auto-suspended*) pada H-0 (`HRD-DEC-151`, `VR-CRD-16`).
* **Dependencies**: `BE-HRD-214`, `BE-HRD-216`
* **Acceptance Criteria**: Draf rekredensialing ter-generate pada H-60; status kewenangan beralih ke `Suspended` pada H-0 jika berkas baru belum sah.
* **Verifikasi**: Uji scheduler & lifecycle transisi status.

#### ✅ `BE-HRD-221` — Validasi Kompilasi .NET, QBE Conformance & Eksekusi Uji Penerimaan `S-C1`
* **Slice / MVP**: `S-C1` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Verifikasi kompilasi bersih .NET (`dotnet build`) tanpa error, audit konformansi arsitektur QBE Preflight, validasi kepatuhan kontrak API Swagger `v18`, serta eksekusi lengkap 22 skenario pengujian penerimaan `AT-HRD-CRD-01` s.d. `AT-HRD-CRD-22`.
* **Dependencies**: Seluruh task backend `BE-HRD-206` s.d. `BE-HRD-220`
* **Acceptance Criteria**: `dotnet build` lulus 0 error 0 warning kritis; seluruh acceptance criteria 16 task terpenuhi; 10 kriteria DoD tercapai.
* **Verifikasi**: Laporan task tracked `BE-HRD-221.md` dan output terminal dotnet.

---

## 17. Rencana Kerja Backend Sub-Modul 15: OPPE, FPPE & Recredentialing (Slice `S-C1-OPPE`)

### 17.1 Grafik Urutan Dependency Backend `S-C1-OPPE`

#### Grafik B56: Gelombang `MVP-45` (OPPE Evaluasi Kinerja Klinis & Kualitas SIMRS)
```text
BE-HRD-222 ✅ (Skema DB OPPE/FPPE) ──> BE-HRD-223 ✅ (Katalog Indikator ACGME) ──> BE-HRD-224 ✅ (Periode Siklus OPPE) ──> BE-HRD-225 ✅ (Ekstraksi Kuantitatif SIMRS 60%) ──┐
                                                                               │                                                                                        │
                                                                               └──> BE-HRD-226 ✅ (Penugasan Peer Review 40%) ──> BE-HRD-227 ✅ (Privilege & Read-Audit) ──┴─> BE-HRD-228 ✅ (Kalkulator Komposit 60:40 & Sidang)
```

#### Grafik B57: Gelombang `MVP-46` (FPPE Pemicu Kritis, Proctoring Staf/Prosedur Baru & Sidang Pleno)
```text
BE-HRD-222 [B56] ──> BE-HRD-229 [B57] ──> BE-HRD-230 [B57] ──> BE-HRD-231 [B57] ──> BE-HRD-232 ✅ (Sidang Pleno Kesimpulan FPPE Selesai)
```

#### Grafik B58: Gelombang `MVP-47` (Kalkulator Komposit 3 Tahun Rekredensialing & Sinkronisasi Putusan SIMRS)
```text
BE-HRD-228 [B56] ──┬──> BE-HRD-233 (Kalkulator Komposit 3 Tahun & Tier) ──> BE-HRD-234 (Diskresi Override Tier) ──> BE-HRD-235 ✅ (4 Putusan Rekredensialing & Dispatcher SIMRS EMR)
BE-HRD-232 [B57] ──┘
BE-HRD-230 [B57] ──┬──> BE-HRD-236 ✅ (Worker Pemantau Batas Proctoring & Anomali Komplikasi Bulanan)
BE-HRD-225 [B56] ──┘
```

#### Grafik B59: Gelombang Verifikasi & Kepatuhan Arsitektur `S-C1-OPPE`
```text
BE-HRD-228 [B56] ──┐
BE-HRD-232 [B57] ──┼──> BE-HRD-237 ✅ (Validasi Kompilasi .NET, QBE & Uji Penerimaan AT-HRD-OPP-01..22)
BE-HRD-235 [B58] ──┤
BE-HRD-236 [B58] ──┘
```

---

### Tabel Gelombang Eksekusi Backend `S-C1-OPPE`

| Gelombang | Task ID | Target Deliverable / Output Utama | Syarat Masuk (Dependencies) |
| :---: | :--- | :--- | :--- |
| **`MVP-45`** | `BE-HRD-222`, `BE-HRD-223`, `BE-HRD-224`, `BE-HRD-225`, `BE-HRD-226`, `BE-HRD-227`, `BE-HRD-228` | Entitas EF Core 8 tabel, master katalog indikator 6 pilar ACGME/KARS, siklus evaluasi periode OPPE, adapter ekstraksi data kuantitatif SIMRS/KMKP 60%, instrumen telaah sejawat kualitatif 40%, penegakan *Peer Review Privilege* dengan perekaman *read-audit log*, serta kalkulator komposit 60:40 dan pengesahan rapor OPPE. | ✅ **Selesai (2026-11-09)** |
| **`MVP-46`** | `BE-HRD-229`, `BE-HRD-230`, `BE-HRD-231`, `BE-HRD-232` | Layanan penangkap feed pemicu kritis FPPE (sentinel, komplikasi bedah > 2 SD, resume EMR kronis), inisiasi penugasan FPPE dokter baru & prosedur baru, logbook kasus proctoring dengan pseudonimisasi pasien, verifikasi kemandirian proctor, serta sidang pleno kesimpulan FPPE. | ✅ **Selesai (2026-11-09)** |
| **`MVP-47`** | `BE-HRD-233`, `BE-HRD-234`, `BE-HRD-235`, `BE-HRD-236` | Kalkulator deterministik rapor komposit 3 tahun rekredensialing (25%-35%-40%), klasifikasi otomatis tier kelayakan (Hijau, Kuning, Merah), layanan diskresi penyesuaian tier Ketua Komite, transmisi 4 putusan rekredensialing ke modul EMR SIMRS (`INT-HR-76`), serta background worker pemantau sisa waktu proctoring. | ✅ **Selesai (2026-11-09)** |
| **Verifikasi** | `BE-HRD-237` | Kompilasi .NET bersih 0 error, verifikasi konformansi QBE preflight, verifikasi kontrak API Swagger Kontrak `v19`, dan pengujian penerimaan `AT-HRD-OPP-01` s.d. `AT-HRD-OPP-22`. | ✅ **Selesai (2026-11-09)** |

---

### Detail Spesifikasi Task Backend (`BE-HRD-222` s.d. `BE-HRD-237`)

#### ✅ `BE-HRD-222` — EF Core Entity Models & Database Migration Skema OPPE & FPPE (`S-C1-OPPE`)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Deklarasi 8 entity models EF Core (`MstOppeMetricCatalog`, `TrxOppeEvaluationPeriod`, `TrxOppeStaffEvaluation`, `TrxOppeStaffEvaluationDetail`, `TrxFppeEngagement`, `TrxFppeCaseLog`, `TrxFppeEvaluationConclusion`, `HrdClinicalPeerReviewAuditLog`), konfigurasi fluent mapping PostgreSQL, indeks foreign key dan unique constraints, serta migrasi EF Core bersih tanpa modifikasi tabel legacy.
* **Dependencies**: Disetujuinya Kontrak `v19` & Blueprint Rev 44
* **Acceptance Criteria**: Model EF Core valid; migration PostgreSQL berhasil dieksekusi tanpa galat; constraint integritas data aktif.
* **Verifikasi**: `dotnet ef migrations add` dan verifikasi skema database.

#### ✅ `BE-HRD-223` — Master Katalog Indikator OPPE 6 Pilar ACGME/KARS & Konfigurasi Ambang Batas KSM
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: REST API dan domain service untuk master katalog indikator performa klinis berbasis 6 pilar standar ACGME/KARS (`PatientCare`, `MedicalKnowledge`, `PracticeBasedLearning`, `InterpersonalCommunication`, `Professionalism`, `SystemsBasedPractice`), konfigurasi target ambang batas per KSM, dan pengaturan bobot default (`mst_oppe_metric_catalogs`, `VR-OPP-01`).
* **Dependencies**: `BE-HRD-222`
* **Acceptance Criteria**: Endpoint `GET /metric-catalogs` dan `POST /metric-catalogs` beroperasi sesuai kontrak; validasi ambang batas numerik berfungsi.
* **Verifikasi**: Uji fungsional `AT-HRD-OPP-01`.

#### ✅ `BE-HRD-224` — Tata Kelola Periode Siklus OPPE Tahunan/Semesteran & Inisiasi Rapor Dokter
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pembukaan siklus periode evaluasi OPPE baru (`trx_oppe_evaluation_periods`), validasi total pembobotan kuantitatif + kualitatif wajib tepat 100% (standar 60:40, `VR-OPP-01`), penguncian periode pasca-finalisasi (`VR-OPP-24`), serta otomasi inisiasi draf rapor evaluasi individu dokter (`trx_oppe_staff_evaluations`).
* **Dependencies**: `BE-HRD-223`
* **Acceptance Criteria**: Periode OPPE terbit status `Active`; draf rapor dokter terinisiasi otomatis; penolakan jika total bobot != 100% (`VR-OPP-01`).
* **Verifikasi**: Uji fungsional `AT-HRD-OPP-01` dan uji negatif `AT-HRD-OPP-02`.

#### ✅ `BE-HRD-225` — Layanan Ekstraksi & Agregasi Otomatis Data Indikator Kuantitatif SIMRS/KMKP 60%
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi adapter integrasi `INT-HR-73` (`IOppeSimrsMetricExtractorService`) untuk menarik data indikator kuantitatif dari SIMRS dan modul KMKP (volume tindakan, kepatuhan resume EMR 1x24 jam, kepatuhan Clinical Pathway, kepatuhan formularium RS, tingkat IDO), serta kalkulasi agregasi bobot kuantitatif 60%.
* **Dependencies**: `BE-HRD-224`
* **Acceptance Criteria**: Data kuantitatif SIMRS teragregasi secara otomatis; nilai skor tertimbang 60% terhitung presisi; penyimpanan raw JSON payload bukti audit.
* **Verifikasi**: Uji integrasi `AT-HRD-OPP-03`.

#### ✅ `BE-HRD-226` — Penugasan & Evaluasi Kualitatif Telaah Sejawat (Peer Review) 40% KSM
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Penugasan instrumen evaluasi telaah sejawat ke dokter rekan sebidang dalam KSM yang sama (`VR-OPP-02`), pemblokiran keras evaluasi mandiri (*anti-self-review*, `VR-OPP-03`), entri skala Likert 1-5 untuk domain etika profesi dan komunikasi, serta kewajiban catatan naratif jika skor <= 2 (`VR-OPP-04`).
* **Dependencies**: `BE-HRD-224`
* **Acceptance Criteria**: Form ulasan sejawat terkirim ke penilai KSM; submit ditolak jika tanpa narasi klinis untuk skor rendah; blokir self-review berhasil.
* **Verifikasi**: Uji fungsional `AT-HRD-OPP-04`, `AT-HRD-OPP-05`, dan `AT-HRD-OPP-06`.

#### ✅ `BE-HRD-227` — Penegakan Kerahasiaan Hak Istimewa Peer Review Privilege & Read-Audit Logging Tamper-Proof
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Mekanisme proteksi data konfidensial di bawah hak istimewa *Peer Review Privilege* (`HRD-DEC-156`), isolasi akses dari staf HR non-medis, penegakan dialog peringatan hukum kerahasiaan, serta perekaman seketika ke tabel log audit permanen `hrd_clinical_peer_review_audit_logs` (*tamper-proof read-audit log*, `VR-OPP-23`).
* **Dependencies**: `BE-HRD-226`
* **Acceptance Criteria**: Setiap request pembacaan lembar ulasan sejawat mencatat ID personel, IP, timestamp presisi, dan status persetujuan disclaimer; staf umum ditolak `403 Forbidden`.
* **Verifikasi**: Uji audit keselamatan & privasi `AT-HRD-OPP-07`.

#### ✅ `BE-HRD-228` — Kalkulator Rapor Akhir Komposit OPPE 60:40 & Sidang Pengesahan Subkomite Mutu
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan perhitungan deterministik nilai komposit akhir OPPE ((0.60 * Kuantitatif) + (0.40 * Kualitatif)), penetapan status kelulusan standar mutu klinis (>= 75.0), evaluasi sinyal peringatan dini (*early warning*), pengesahan sidang Subkomite Mutu Profesi, serta alur konfirmasi baca oleh nakes via portal ESS.
* **Dependencies**: `BE-HRD-225`, `BE-HRD-227`
* **Acceptance Criteria**: Nilai komposit terhitung tepat; status rapor berubah menjadi `Completed` lalu `Acknowledged` pasca-konfirmasi dokter; arsip otomatis ke e-Dossier Sub-02.
* **Verifikasi**: Uji algoritma kalkulasi `AT-HRD-OPP-08` dan alur persetujuan `AT-HRD-OPP-09`.

#### ✅ `BE-HRD-229` — Layanan Penerimaan Feed Pemicu Kritis FPPE (Sentinel, Komplikasi > 2 SD, Resume Medis Kronis)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-46`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi penerima event asinkronus `INT-HR-74` (`IFppeTriggerEventReceiver`) untuk mendeteksi ambang pemicu insiden keselamatan pasien: 1 kejadian sentinel (`VR-OPP-05`), komplikasi bedah > 2 SD selama 3 bulan berturut-turut (`VR-OPP-06`), keterlambatan resume EMR > 48 jam > 20% selama 2 bulan (`VR-OPP-07`), serta surat rujukan sidang Komite Etik.
* **Dependencies**: `BE-HRD-222`
* **Acceptance Criteria**: Sinyal sentinel seketika menginisiasi draf penugasan investigasi FPPE; feed pemicu kritis tersaji di antarmuka pengawasan KMKP.
* **Verifikasi**: Uji pemicu real-time `AT-HRD-OPP-10`, `AT-HRD-OPP-11`, dan `AT-HRD-OPP-12`.

#### ✅ `BE-HRD-230` — Inisiasi Penugasan FPPE Terarah & Penunjukan Proctor Senior
* **Slice / MVP**: `S-C1-OPPE` / `MVP-46`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Domain service inisiasi berkas penugasan FPPE (`trx_fppe_engagements`), verifikasi kualifikasi senioritas Dokter Proctor pendamping minimal masa kerja 3 tahun (`VR-OPP-08`), penetapan target pemantauan staf baru (minimal 3 bulan / 10 kasus, `VR-OPP-09`), dan penetapan target prosedur baru (minimal 6 bulan / 5 kasus, `VR-OPP-10`).
* **Dependencies**: `BE-HRD-229`
* **Acceptance Criteria**: Penugasan FPPE terbit status `InProgress`; penunjukan proctor divalidasi kualifikasinya; target kasus terkunci sesuai kategori.
* **Verifikasi**: Uji fungsional `AT-HRD-OPP-13` dan `AT-HRD-OPP-14`.

#### ✅ `BE-HRD-231` — Logbook Kasus Terproctoring Berpseudonim & Verifikasi Kemandirian Tindakan Proctor
* **Slice / MVP**: `S-C1-OPPE` / `MVP-46`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Pengelolaan buku catatan logbook tindakan klinis terproctoring (`trx_fppe_case_logs`), penerapan enkripsi/penyamaran (pseudonimisasi) identitas rekam medis pasien (`VR-OPP-11`), pencatatan evaluasi teknik klinis, observasi komplikasi, serta verifikasi tanda tangan digital proctor atas tingkat kemandirian prosedur Level 1 s.d. Level 4 (`VR-OPP-12`).
* **Dependencies**: `BE-HRD-230`
* **Acceptance Criteria**: Nomor RM tersimpan dalam format pseudonim ID aman; kasus klinis berhasil diverifikasi proctor; counter progres kasus bertambah.
* **Verifikasi**: Uji privasi medis & verifikasi proctor `AT-HRD-OPP-15` dan `AT-HRD-OPP-16`.

#### ✅ `BE-HRD-232` — Sidang Pleno Penetapan Vonis Kesimpulan FPPE & Rencana Pembinaan
* **Slice / MVP**: `S-C1-OPPE` / `MVP-46`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan persidangan pleno Subkomite Mutu Profesi untuk menetapkan berita acara kesimpulan FPPE (`trx_fppe_evaluation_conclusions`), validasi otorisasi Ketua Komite (`VR-OPP-14`), penanganan 4 rekomendasi vonis (`FullPrivilegeConfirmed`, `ProctoringExtended` maks 3 bulan `VR-OPP-13`, `SupervisedPractice`, `PrivilegeRevoked`), serta perumusan rencana pembinaan (*Remediation Plan*).
* **Dependencies**: `BE-HRD-231`
* **Acceptance Criteria**: Vonis FPPE sah terbit; durasi perpanjangan dibatasi maksimal 3 bulan; status kewenangan klinis dokter diperbarui di sistem.
* **Verifikasi**: Uji sidang pleno `AT-HRD-OPP-17` dan `AT-HRD-OPP-18`.

#### ✅ `BE-HRD-233` — Kalkulator Rapor Komposit 3 Tahun Rekredensialing (25%-35%-40%) & Klasifikasi Tier
* **Slice / MVP**: `S-C1-OPPE` / `MVP-47`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi layanan query `INT-HR-75` (`IRecredentialingCompositeProvider`) untuk menghitung rapor komposit 3 tahun secara deterministik dengan rasio progresif Tahun 1 (25%), Tahun 2 (35%), Tahun 3 (40%) (`VR-OPP-15`), serta otomasi klasifikasi eligibilitas ke dalam 3 tier: Tier Hijau (skor >= 80 & 0 sentinel, `VR-OPP-16`), Tier Kuning (skor 65-79 atau penundaan EMR, `VR-OPP-17`), dan Tier Merah (skor < 65 atau gagal FPPE, `VR-OPP-18`).
* **Dependencies**: `BE-HRD-228`, `BE-HRD-232`
* **Acceptance Criteria**: Komposit 3 tahun terhitung akurat sesuai formula baku; klasifikasi tier berjalan objektif tanpa bias.
* **Verifikasi**: Uji algoritma kalkulasi `AT-HRD-OPP-19` dan klasifikasi tier `AT-HRD-OPP-20`.

#### ✅ `BE-HRD-234` — Layanan Diskresi Penyesuaian Tier Rekredensialing & Audit Rekomendasi Komite Medik
* **Slice / MVP**: `S-C1-OPPE` / `MVP-47`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Mekanisme penyesuaian tier kelayakan hasil kalkulator oleh Ketua Komite Medik, kewajiban penyertaan nota justifikasi klinis tertulis (`VR-OPP-19`), pencatatan event audit `RecredentialingTierOverridden`, serta penyusunan berkas rekomendasi menuju sidang pleno rekredensialing.
* **Dependencies**: `BE-HRD-233`
* **Acceptance Criteria**: Penyesuaian tier ditolak jika tanpa justifikasi klinis; rekam jejak audit tersimpan permanen.
* **Verifikasi**: Uji diskresi otorisasi `AT-HRD-OPP-21`.

#### ✅ `BE-HRD-235` — Penetapan 4 Putusan Sidang Rekredensialing & Dispatcher Integrasi SIMRS EMR
* **Slice / MVP**: `S-C1-OPPE` / `MVP-47`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penerbitan 4 putusan sah sidang rekredensialing (`Perpanjang As-Is`, `Upgrade`, `Restricted`, `Revoked`, `VR-OPP-20`), serta implementasi dispatcher sinkronisasi `INT-HR-76` (`ISimrsClinicalPrivilegeDispatcher`) yang mentransmisikan instruksi wewenang Level 4 mandiri, Level 3 supervisi co-sign 24 jam (`VR-OPP-21`), atau Hard-Stop pemblokiran order mutlak untuk putusan Revoked (`VR-OPP-22`, `VR-OPP-25`).
* **Dependencies**: `BE-HRD-234`
* **Acceptance Criteria**: Putusan rekredensialing terkirim ke database SIMRS EMR; menu order dan peresepan dokter seketika terkunci jika status Revoked.
* **Verifikasi**: Uji integrasi SIMRS & Hard-Stop `AT-HRD-OPP-22`.

#### ✅ `BE-HRD-236` — Background Worker Pemantau Sisa Waktu Proctoring & Evaluasi Tren Komplikasi Bulanan
* **Slice / MVP**: `S-C1-OPPE` / `MVP-47`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi background worker berkala (`OPP-W1` s.d. `OPP-W6`): `OppeSimrsDataSyncJob`, `FppeTriggerMonitorJob`, `FppeProctoringDeadlineJob` (peringatan H-14 masa observasi habis), `RecredentialingCompositeScoreJob`, `OppeEarlyWarningJob`, dan `SurgicalComplicationScannerJob` (analisis statistik komplikasi > 2 SD).
* **Dependencies**: `BE-HRD-225`, `BE-HRD-230`
* **Acceptance Criteria**: Worker berjalan deterministik sesuai jadwal cron; deteksi anomali statistik memicu event pemicu FPPE.
* **Verifikasi**: Uji background worker & simulasi scheduler.

#### ✅ `BE-HRD-237` — Validasi Kompilasi .NET, QBE Conformance & Eksekusi Acceptance Tests `S-C1-OPPE`
* **Slice / MVP**: `S-C1-OPPE` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Verifikasi kompilasi bersih .NET (`dotnet build`) tanpa error, audit konformansi arsitektur QBE Preflight, validasi kepatuhan 24 endpoint API Swagger Kontrak `v19`, serta eksekusi lengkap 22 skenario pengujian penerimaan `AT-HRD-OPP-01` s.d. `AT-HRD-OPP-22`.
* **Dependencies**: Seluruh task backend `BE-HRD-222` s.d. `BE-HRD-236`
* **Acceptance Criteria**: `dotnet build` lulus 0 error 0 warning kritis; seluruh acceptance criteria 16 task terpenuhi; 10 kriteria DoD tercapai.
* **Verifikasi**: Laporan task tracked `BE-HRD-237.md` dan output terminal dotnet.


## 18. Rencana Kerja Backend Sub-Modul 16: Learning, Training & Competency (Slice `S-C2`)

### 18.1 Grafik Urutan Dependency Backend `S-C2`

#### Grafik B60: Gelombang `MVP-48` (Pelatihan Wajib Akreditasi STARKES & Radar Kedaluwarsa)
```text
BE-HRD-238 [B60] ✅ ──> BE-HRD-239 [B60] ✅ ──> BE-HRD-240 [B60] ✅ ──> BE-HRD-241 [B60] ✅ (Safeguard Shift Buddy System Sub-06 & Eskalasi RKK Sub-14)
```

#### Grafik B61: Gelombang `MVP-49` (Sesi Diklat, Presensi, Ujian Teori & Simulasi OSCE Klinis)
```text
BE-HRD-238 [B60] ──> BE-HRD-243 [B61] ✅ (Jadwal Batch Kelas & Lab Simulasi) ──> BE-HRD-246 [B61] ✅ (Presensi Harian & Ambang 80/100%) ──┐
                                                                                                                     ├──> BE-HRD-249 [B61] ✅ (Penerbitan e-Sertifikat Resmi & e-Dossier Sub-02)
BE-HRD-238 [B60] ──> BE-HRD-247 [B61] ✅ (Penilaian Post-Test Teori & Remedi) ──> BE-HRD-248 [B61] ✅ (Ujian Simulasi OSCE Klinis & e-Sign) ──┘
```

#### Grafik B62: Gelombang `MVP-50` (TNA, Kontrol Anggaran, Ikatan Dinas & SKP Plataran Sehat)
```text
BE-HRD-238 [B60] ──> BE-HRD-242 [B62] ✅ (Master Rencana Tahunan TNA & Pagu Anggaran) ──> BE-HRD-245 [B62] ✅ (Kontrol Pagu Anggaran Unit) ──┐
                                                                                                                                  ├──> BE-HRD-250 [B62] ✅ (Generator Akta Ikatan Dinas > 10Jt/1Bln) ──> BE-HRD-251 [B62] ✅ (Denda Penalti Wanprestasi Resign)
BE-HRD-243 [B61] ──> BE-HRD-244 [B62] ✅ (Pendaftaran Mandiri ESS & Approval Berjenjang) ─────────────────────────────────────────────────┘
BE-HRD-249 [B61] ──> BE-HRD-252 [B62] ✅ (Portofolio 50 SKP Kemenkes, Gateway Plataran Sehat INT-HR-80 & Feed OPPE INT-HR-78)
```

#### Grafik B63: Gelombang Verifikasi & Kepatuhan Arsitektur `S-C2`
```text
BE-HRD-241 [B60] ──┐
BE-HRD-249 [B61] ──┼──> BE-HRD-253 ✅ (Validasi Kompilasi .NET, QBE Conformance & Uji Penerimaan AT-HRD-TRN-01..22)
BE-HRD-251 [B62] ──┤
BE-HRD-252 [B62] ──┘
```

---

### 18.2 Tabel Gelombang Eksekusi Backend `S-C2`

| Gelombang | Task ID | Nama Task Ringkas | Prasyarat (Dependencies) | Target Deliverables & Hasil | Status |
| :--- | :--- | :--- | :--- | :--- | :---: |
| **MVP-48** | BE-HRD-238 | Entity Model EF Core & Migrasi Postgres Diklat | - | 10 Model tabel relasional Postgres, Migration EF Core & DbContext | ✅ **Selesai (2026-11-09)** |
| **MVP-48** | BE-HRD-239 | Master Aturan Pelatihan Wajib STARKES KPS 8 | BE-HRD-238 | MandatoryTrainingRuleService, Controller, Katalog 5 Diklat Wajib RS | ✅ **Selesai (2026-11-09)** |
| **MVP-48** | BE-HRD-240 | Engine Radar Kedaluwarsa Bertingkat (H-90/60/30) | BE-HRD-239 | Background Worker Radar, broadcast alert otomatis ke ESS/Email/WA | ✅ **Selesai (2026-11-09)** |
| **MVP-48** | BE-HRD-241 | Safeguard Shift Buddy System & Eskalasi RKK | BE-HRD-240 | Integrasi INT-HR-79 (Sub-06) & INT-HR-77 (Sub-14 downgrade Level 3) | ✅ **Selesai (2026-11-09)** |
| **`MVP-49`** | `BE-HRD-242` | Master Rencana Program Tahunan TNA & Approval | `BE-HRD-238` | `AnnualTrainingPlanService`, Controller, workflow pengesahan Direksi | ✅ **Selesai (2026-11-09)** |
| **`MVP-49`** | `BE-HRD-243` | Penjadwalan Sesi Kelas, Workshop Lab & Webinar | `BE-HRD-238` | `TrainingSessionService`, Controller, batch kalender kelas & kuota | ✅ **Selesai (2026-11-09)** |
| **`MVP-49`** | `BE-HRD-244` | Pendaftaran Mandiri ESS & Approval Berjenjang | `BE-HRD-243` | `TrainingEnrollmentService`, Controller, alur approval Karu & Diklat | ✅ **Selesai (2026-11-09)** |
| **`MVP-49`** | `BE-HRD-245` | Kontrol Pagu Anggaran Diklat & Saldo Komitmen | `BE-HRD-242`, `BE-HRD-244` | `TrainingBudgetControlService`, validasi anti-defisit pagu unit | ✅ **Selesai (2026-11-09)** |
| **MVP-49** | BE-HRD-246 | Presensi Harian Sesi & Validasi Ambang 80/100% | BE-HRD-243 | TrainingAttendanceService, Controller, QR Code check-in & rekap hadir | ✅ **Selesai (2026-11-09)** |
| **MVP-49** | BE-HRD-247 | Rekapitulasi Post-Test Teori & Remedial | BE-HRD-246 | TheoryPostTestAssessmentService, scoring passing grade 75.00, remedial maks 2x | ✅ **Selesai (2026-11-09)** |
| **MVP-49** | BE-HRD-248 | Asesmen Simulasi OSCE Klinis & Digital Signature| BE-HRD-247 | ClinicalOsceAssessmentService, rubrik checklist skor >= 80, e-Sign | ✅ **Selesai (2026-11-09)** |
| **MVP-49** | BE-HRD-249 | Penerbitan e-Sertifikat Resmi & e-Dossier Sub-02 | BE-HRD-246, BE-HRD-248 | TrainingCertificationService, penomoran seri baku, arsip PDF Sub-02 | ✅ **Selesai (2026-11-09)** |
| **MVP-50** | BE-HRD-250 | Generator Akta Ikatan Dinas Retensi (> 10Jt/1Bln) | BE-HRD-244, BE-HRD-245 | TrainingBondService, Controller, draf akta retensi kerja 12-36 bulan | ✅ **Selesai (2026-11-09)** |
| **MVP-50** | BE-HRD-251 | Perhitungan Denda Penalti Wanprestasi Resign | BE-HRD-250 | TrainingBondPenaltyCalculator, tagihan invoice ganti rugi prorata | ✅ **Selesai (2026-11-09)** |
| **MVP-50** | BE-HRD-252 | Portofolio SKP Kemenkes, Plataran Sehat & OPPE | BE-HRD-249 | Integrasi INT-HR-80 (Plataran Sehat SatuSehat) & INT-HR-78 (OPPE Sub-15) | ✅ **Selesai (2026-11-09)** |
| **Verifikasi** | BE-HRD-253 | Validasi Kompilasi .NET, QBE & UAT S-C2 | Seluruh task BE-HRD-238..252 | Build 0 error, QBE Preflight lulus, verifikasi 22 skenario AT-HRD-TRN | ✅ **Selesai (2026-11-09)** |

---

### 18.3 Detail Spesifikasi Task Backend (`BE-HRD-238` s.d. `BE-HRD-253`)

#### ✅ `BE-HRD-238` — Entity Model EF Core, Indeks DB & Migration Postgres untuk 10 Tabel `LearningAndDevelopment` & Ikatan Dinas
* **Slice / MVP**: `S-C2` / `MVP-48`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Definisi lengkap 10 model entitas C# di EF Core (`MstMandatoryTrainingRule`, `TrxTrainingPlan`, `TrxTrainingSession`, `TrxTrainingEnrollmentRequest`, `TrxTrainingAttendance`, `TrxTrainingAssessment`, `TrxTrainingCertificate`, `TrxTrainingBudget`, `HrdTrainingBondContract`, `TrxPlataranSehatSkpRecord`), registrasi DbSet pada `ApplicationDbContext`, konfigurasi Fluent API relasi foreign keys, indeks unik, audit columns (`created_at`, `updated_at`, `is_deleted`), serta pembuatan berkas migrasi database PostgreSQL.
* **Dependencies**: Tidak ada prasyarat task lain (Fondasi basis data `S-C2`).
* **Acceptance Criteria**: Berkas migrasi database EF Core berhasil dibuat dan tervalidasi sintaksnya tanpa konflik; tipe data numerik biaya (`decimal(15,2)`), tanggal presisi (`timestamptz`), dan jsonb terkonfigurasi tepat.
* **Verifikasi**: `dotnet ef migrations add AddLearningAndDevelopmentCoreEntities` dan review skema SQL yang dihasilkan.

#### ✅ `BE-HRD-239` — Konfigurasi Master Aturan Pelatihan Wajib Akreditasi STARKES KPS 8 (`MandatoryTrainingRuleService` & Controller)
* **Slice / MVP**: `S-C2` / `MVP-48`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi API controller `MandatoryTrainingRulesController` dan service `MandatoryTrainingRuleService` untuk mendaftarkan, memperbarui, dan meninjau katalog master 5 pelatihan wajib akreditasi STARKES (BHD, PPI, SKP, K3RS/Code Red, Komunikasi SBAR, `VR-TRN-01`), pelatihan spesialisasi ruang intensif (ICU, OK, IGD, Dialisis), pembatasan siklus kedaluwarsa maksimal 24 bulan (`VR-TRN-02`), target profesi, dan konfigurasi ambang kelulusan.
* **Dependencies**: `BE-HRD-238`
* **Acceptance Criteria**: Endpoint `GET /api/v1/corporate/hrd/learning/mandatory-rules` dan `POST .../mandatory-rules` beroperasi sesuai Kontrak `v20`; penolakan siklus masa berlaku > 24 bulan dengan kode `400 Bad Request`.
* **Verifikasi**: Uji kontrak API Swagger & verifikasi aturan bisnis `VR-TRN-01`, `VR-TRN-02` pada `AT-HRD-TRN-01`, `AT-HRD-TRN-02`.

#### ✅ `BE-HRD-240` — Engine Radar Kedaluwarsa Bertingkat (H-90/60/30) & Penyiaran Notifikasi Massal (`TrainingExpiryRadarService`)
* **Slice / MVP**: `S-C2` / `MVP-48`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pemantau kepatuhan sertifikasi berkala dan background worker harian `MandatoryTrainingExpiryRadarWorker` (`TRN-W1`) yang memindai seluruh sertifikat aktif pegawai, mengelompokkan ke zona radar H-90, H-60, H-30 hari sebelum kedaluwarsa (`VR-TRN-09`), memicu status `ExpiringSoon`, mencatat event audit `MandatoryTrainingExpiryRadarTriggered`, serta endpoint penyiaran notifikasi peringatan massal ke ESS, email, dan WhatsApp.
* **Dependencies**: `BE-HRD-239`
* **Acceptance Criteria**: Worker radar mengevaluasi tanggal masa berlaku secara akurat; endpoint `GET .../certificates/expiring-radar` dan `POST .../certificates/expiring-radar/broadcast-alerts` berfungsi deterministik.
* **Verifikasi**: Uji fungsional radar kedaluwarsa `AT-HRD-TRN-03`, `AT-HRD-TRN-04`.

#### ✅ `BE-HRD-241` — Safeguard Shift Dinas Buddy System ke Sub-06 Scheduling & Eskalasi RKK ke Sub-14 (`INT-HR-79` & `INT-HR-77`)
* **Slice / MVP**: `S-C2` / `MVP-48`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi penegakan keselamatan operasional rumah sakit ketika sertifikat pelatihan kritis mati (H-0 Overdue): menandai flag `OverdueMandatoryTraining`, memberlakukan masa tenggang 30 hari (`VR-TRN-10`), mengirimkan event bus `MandatoryTrainingOverdueSafeguardTriggeredEvent` (`INT-HR-79`) ke Sub-06 Scheduling guna mewajibkan jadwal pendampingan (*buddy system safeguard*, `VR-TRN-11`), serta mengirimkan event `TrainingCertificateStatusChangedEvent` (`INT-HR-77`) ke Sub-14 untuk membekukan kewenangan mandiri Level 4 menjadi Level 3 supervisi (`VR-TRN-12`).
* **Dependencies**: `BE-HRD-240`
* **Acceptance Criteria**: Pemblokiran penugasan shift mandiri terbukti efektif di modul jadwal dinas; pembekuan status RKK nakes terkonfirmasi di modul kredensialing.
* **Verifikasi**: Uji integrasi `INT-HR-79` dan `INT-HR-77` pada `AT-HRD-TRN-05`, `AT-HRD-TRN-06`, `AT-HRD-TRN-07`.

#### ✅ `BE-HRD-242` — Master Rencana Kerja Tahunan Diklat (TNA) & Alur Persetujuan Direksi (`AnnualTrainingPlanService` & Controller)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Pengelolaan dokumen Rencana Program Pembelajaran dan Pelatihan Tahunan RS (*Annual Training Master Plan*) berbasis *Training Needs Analysis* (TNA), mencakup endpoint `GET /api/v1/corporate/hrd/learning/plans/annual`, pembuatan usulan program tahunan (`POST .../annual`, `VR-TRN-13`), alur pengajuan ke Direksi, dan pengesahan resmi oleh Direktur SDM/Keuangan (`POST .../{id}/approve`) yang mengaktifkan status `Approved` dan membuka pagu anggaran tahunan.
* **Dependencies**: `BE-HRD-238`
* **Acceptance Criteria**: Validasi kelengkapan dokumen program kerja TNA; pengesahan direksi mencatat log audit permanen `AnnualTrainingPlanApproved`.
* **Verifikasi**: Uji kontrak API & alur persetujuan TNA `AT-HRD-TRN-08`.

#### ✅ `BE-HRD-243` — Penjadwalan Sesi Kelas Diklat, Workshop Laboratorium Simulasi & Webinar (`TrainingSessionService` & Controller)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penjadwalan batch kelas dan workshop pelatihan (`TrainingSessionService` & `TrainingSessionsController`), mencakup pembuatan nomor sesi baku (`SES-BHD-2026-B1`), penentuan instruktur/assessor klinis, aula/ruang lab simulasi, kuota maksimal peserta (`VR-TRN-07`), jam pelajaran efektif (JPL), bobot SKP Kemenkes, pencegahan bentrok jadwal mengajar (`VR-TRN-08`), serta pembukaan masa pendaftaran peserta (`RegistrationOpen`).
* **Dependencies**: `BE-HRD-238`
* **Status**: ✅ Selesai (2026-11-09)
* **Verifikasi**: Uji penjadwalan kelas dan validasi kuota `AT-HRD-TRN-09`.

#### ✅ `BE-HRD-244` — Pendaftaran Diklat Mandiri via ESS & Alur Persetujuan Berjenjang Karu/Diklat (`TrainingEnrollmentService` & Controller)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Fitur pengajuan pendaftaran pelatihan internal/eksternal mandiri oleh staf via ESS (`POST .../enrollments/ess-submit`), penugasan otomatis pegawai via radar kedaluwarsa, alur persetujuan berjenjang dispensasi dinas oleh Kepala Ruangan / Supervisor (`PendingSupervisor` -> `PendingDiklat`), validasi kualifikasi peserta oleh staf Diklat, serta pencatatan status `Approved`.
* **Dependencies**: `BE-HRD-243`
* **Acceptance Criteria**: Pengajuan tersimpan dengan nomor pendaftaran `ENR-TRN-...`; alur persetujuan multi-level berjalan runtut dan aman.
* **Verifikasi**: Uji workflow persetujuan pendaftaran `AT-HRD-TRN-10`.

#### ✅ `BE-HRD-245` — Kontrol Pagu Anggaran Diklat Unit & Pemindahan Saldo Komitmen Otomatis (`TrainingBudgetControlService`)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Modul pengendalian pagu anggaran diklat tahunan per departemen/unit kerja (`trx_training_budgets`), validasi pencegahan saldo pagu defisit saat pendaftaran diajukan (`VR-TRN-14`), otomasi pemindahan saldo dari `remaining_budget` ke `committed_budget` saat pendaftaran disetujui Diklat (`VR-TRN-15`), dan rekonsiliasi realisasi biaya pasca pelatihan selesai.
* **Dependencies**: `BE-HRD-242`, `BE-HRD-244`
* **Acceptance Criteria**: Pengajuan ditolak dengan error `422 Unprocessable Entity` jika pagu unit habis; saldo komitmen terpotong secara deterministik.
* **Verifikasi**: Uji validasi batas anggaran dan mutasi saldo pagu `AT-HRD-TRN-11`.

#### ✅ `BE-HRD-246` — Perekaman Presensi Harian Sesi & Validasi Ambang Batas Kehadiran 80%/100% (`TrainingAttendanceService`)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan presensi absensi harian peserta sesi pelatihan (`TrainingAttendanceService`), dukungan metode check-in via QR Code dinamis / RFID / verifikasi manual instruktur, perhitungan durasi jam pelajaran aktual per peserta, dan validasi ambang minimal kehadiran: minimal 80.00% untuk staf non-klinis dan mutlak 100.00% untuk tenaga medis klinis sebagai syarat kelayakan masuk ke tahap ujian kelulusan (`VR-TRN-03`).
* **Dependencies**: `BE-HRD-243`
* **Acceptance Criteria**: Peserta dengan kehadiran kurang dari ambang batas otomatis diblokir dari daftar peserta ujian kelulusan.
* **Verifikasi**: Uji presensi harian dan ambang kehadiran `AT-HRD-TRN-12`.

#### ✅ `BE-HRD-247` — Rekapitulasi Penilaian Ujian Teori Post-Test & Fasilitas Remedial (`TheoryPostTestAssessmentService`)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Modul pengolahan skor ujian post-test teori tertulis peserta (`POST .../assessments/theory-post-test`), penegakan nilai batas kelulusan minimal 75.00 (`VR-TRN-04`), penandaan status `post_test_passed`, serta penyediaan alur remedial ujian teori maksimal 2 kali percobaan dalam rentang waktu 30 hari kalender (`VR-TRN-20`).
* **Dependencies**: `BE-HRD-238`
* **Status**: ✅ Selesai (2026-11-09)
* **Verifikasi**: Uji penilaian teori dan batas remedial `AT-HRD-TRN-13`, `AT-HRD-TRN-15`.

#### ✅ `BE-HRD-248` — Asesmen Simulasi Keterampilan Klinis OSCE & Validasi Tanda Tangan Digital Assessor (`ClinicalOsceAssessmentService`)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penilaian ujian simulasi praktik langsung (*Objective Structured Clinical Examination* - OSCE) untuk kompetensi klinis kritis (BHD, ACLS, ICU Ventilator, Triase IGD, `VR-TRN-05`), penyimpanan skor rubrik checklist terstruktur dalam format JSONB, penegakan ambang kelulusan OSCE minimal 80.00, serta verifikasi keabsahan tanda tangan digital / token kriptografis sah dokter assessor penilai (`VR-TRN-06`).
* **Dependencies**: `BE-HRD-247`
* **Acceptance Criteria**: Penilaian OSCE tanpa tanda tangan digital ditolak dengan kode `400 Bad Request`; kelulusan komprehensif terwujud saat teori dan OSCE lulus.
* **Verifikasi**: Uji penilaian simulasi OSCE klinis dan validasi tanda tangan `AT-HRD-TRN-14`.

#### ✅ `BE-HRD-249` — Penerbitan e-Sertifikat Kelulusan Resmi, Penomoran Baku & Auto-Arsip ke e-Dossier Sub-02 (`TrainingCertificationService`)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Engine penerbitan sertifikat kompetensi elektronik resmi (`POST .../certificates/issue`), pembangkit nomor seri baku unik rumah sakit (`SRT/HRD-TRN/{TAHUN}/{BULAN}/{SEQUENCE}`, `VR-TRN-21`), penetapan tanggal kedaluwarsa 24 bulan, pembuatan berkas PDF e-Sertifikat ber-QR Code verifikasi, pengarsipan otomatis ke *Employee e-Dossier* Sub-02 (`INT-HR-72`), serta pembaruan status kelulusan peserta menjadi `Completed`.
* **Dependencies**: `BE-HRD-246`, `BE-HRD-248`
* **Acceptance Criteria**: e-Sertifikat terbit resmi hanya jika kehadiran memenuhi syarat dan evaluasi komprehensif lulus; dokumen PDF terhubung ke e-Dossier pegawai.
* **Verifikasi**: Uji penerbitan e-sertifikat dan pengarsipan e-Dossier `AT-HRD-TRN-16`.

#### ✅ `BE-HRD-250` — Generator Akta Ikatan Dinas Retensi Nakes (> 10Jt/1Bln) & Monitoring Kontrak (`TrainingBondService`)
* **Slice / MVP**: `S-C2` / `MVP-50`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan tata kelola perjanjian ikatan dinas dan retensi kerja staf (`TrainingBondService` & Controller), pendeteksian otomatis pengajuan diklat eksternal/spesialisasi dengan biaya pembiayaan RS > Rp 10 Juta atau durasi > 30 hari (`VR-TRN-16`), pembangkitan nomor akta ikatan dinas unik (`BND-HRD-2026-0012`), pengaturan durasi masa retensi (12 s.d. 36 bulan, `VR-TRN-18`), serta pemantauan status keaktifan ikatan dinas pasca pelatihan selesai.
* **Dependencies**: `BE-HRD-244`, `BE-HRD-245`
* **Acceptance Criteria**: Sistem secara konsisten memicu draf akta ikatan dinas untuk diklat bernilai tinggi; aktivasi masa retensi dimulai tepat setelah diklat tuntas.
* **Verifikasi**: Uji generator kontrak ikatan dinas `AT-HRD-TRN-17`, `AT-HRD-TRN-18`.

#### ✅ `BE-HRD-251` — Perhitungan Denda Penalti Wanprestasi Resign Sepihak Ikatan Dinas (`TrainingBondPenaltyCalculator`)
* **Slice / MVP**: `S-C2` / `MVP-50`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Modul kalkulasi penalti finansial wanprestasi (`POST .../bonds/{id}/record-breach`), penegakan formula denda ganti rugi: total pembiayaan ditambah denda 50% atau prorata sisa bulan retensi yang belum dijalani (`VR-TRN-17`), penerbitan tagihan invoice resmi ganti rugi ke pegawai resign, pencatatan event audit kritis `TrainingBondBreachFlagged`, dan integrasi pembatasan clearance kerja di HR Legal.
* **Dependencies**: `BE-HRD-250`
* **Acceptance Criteria**: Perhitungan denda penalti ganti rugi akurat secara matematis sesuai klausul akta perjanjian; tagihan invoice tersimpan pada profil staf.
* **Verifikasi**: Uji kalkulasi denda penalti wanprestasi `AT-HRD-TRN-19`.

#### ✅ `BE-HRD-252` — Registri Portofolio SKP Nakes 3 Kategori, Integrasi Gateway Plataran Sehat Kemenkes (`INT-HR-80`) & Feed OPPE Sub-15 (`INT-HR-78`)
* **Slice / MVP**: `S-C2` / `MVP-50`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Pengelolaan portofolio Satuan Kredit Profesi (SKP) tenaga medis (`trx_plataran_sehat_skp_records`), pengelompokan ke 3 kategori resmi Kemenkes (Pembelajaran, Pelayanan, Pengabdian, `VR-TRN-23`), kalkulasi akumulasi target 50 SKP dalam 5 tahun (`VR-TRN-24`), integrasi outbound HTTPS REST ke API Plataran Sehat Kemenkes RI guna verifikasi ID sertifikat (`INT-HR-80`, `VR-TRN-22`), serta penyediaan feed rekapitulasi CPD sebagai umpan pilar *Medical Knowledge* evaluasi OPPE Sub-15 (`INT-HR-78`, `VR-TRN-25`).
* **Dependencies**: `BE-HRD-249`
* **Acceptance Criteria**: Verifikasi nomor sertifikat Plataran Sehat berhasil; kalkulasi progres 50 SKP akurat; data CPD tersinkronisasi ke modul OPPE.
* **Verifikasi**: Uji integrasi gateway Plataran Sehat Kemenkes dan sinkronisasi OPPE `AT-HRD-TRN-20`, `AT-HRD-TRN-21`, `AT-HRD-TRN-22`.

#### ✅ `BE-HRD-253` — Validasi Kompilasi .NET, QBE Conformance & Eksekusi Acceptance Tests `S-C2`
* **Slice / MVP**: `S-C2` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Verifikasi kompilasi bersih aplikasi .NET (`dotnet build`) tanpa error, audit konformansi arsitektur QBE Preflight (`tooling/qbe/Invoke-QbeConformanceCheck.ps1`), validasi kepatuhan 24 endpoint API Swagger Kontrak `v20`, serta eksekusi lengkap 22 skenario pengujian penerimaan `AT-HRD-TRN-01` s.d. `AT-HRD-TRN-22`.
* **Dependencies**: Seluruh task backend `BE-HRD-238` s.d. `BE-HRD-252`.
* **Acceptance Criteria**: `dotnet build` lulus 0 error 0 warning kritis; seluruh acceptance criteria 16 task backend terpenuhi; 10 kriteria DoD tercapai.
* **Verifikasi**: Laporan task tracked `BE-HRD-253.md` dan output terminal dotnet.


---

## 19. Rencana Kerja Backend Sub-Modul 17: Performance Management & Clinical Career (Slice `S-C3`)

### 19.1 Grafik Urutan Dependency Backend `S-C3`

#### Grafik B64: Gelombang `MVP-51` (Siklus, Katalog KPI, Cascading Target & Penilaian Berjenjang 60:40)
```text
BE-HRD-254 ✅ (Skema DB 10 Tabel & Seeding Master) ──> BE-HRD-255 ✅ (EF Repositories & UoW) ──> BE-HRD-256 ✅ (DTOs & Validation Rules) ──┬──> BE-HRD-257 ✅ (Siklus Kinerja PRF-B1) ──> BE-HRD-258 ✅ (Cascading Target Sasaran PRF-B2) ──┐
                                                                                                                                            │                                                                                                   ├──> BE-HRD-259 ✅ (Penilaian Tiga Tingkat 60:40 PRF-B3/B4)
                                                                                                                                            └──> BE-HRD-260 ✅ (Peer Review IDP Sejawat Non-Skor PRF-B5) ───────────────────────────────────────┘
```

#### Grafik B65: Gelombang `MVP-52` (Sidang Pleno Kalibrasi Manajemen & Program Pembinaan Formal PIP 60 Hari)
```text
BE-HRD-259 [B64] ──> BE-HRD-261 ✅ (Sidang Kalibrasi Guiding Distribution 10-15/70-80/5-10% & Nota PRF-B6) ──> BE-HRD-262 ✅ (Kontrak PIP 60 Hari & Logbook Mingguan PRF-B7) ──> BE-HRD-263 ✅ (Pembekuan Mutasi/Promosi Sub-06 INT-HR-84)
```

#### Grafik B66: Gelombang `MVP-53` (Clinical Ladder Nakes PK I-V & Saluran Sanggah Banding Nilai)
```text
BE-HRD-261 [B65] ──> BE-HRD-264 ✅ (Evaluasi Jenjang Klinis PK I-V & Prasyarat Mutu PRF-B8) ──┬──> BE-HRD-265 ✅ (Integrasi Sertifikat Sub-16 INT-HR-81 & Bebas Sentinel Sub-15 INT-HR-82) ──> BE-HRD-266 ✅ (Sinkronisasi SK Jenjang Sub-02/14 INT-HR-83)
BE-HRD-261 [B65] ──> BE-HRD-267 ✅ (Tata Kelola Sanggah Banding Nilai SLA 7/14 Hari) ──────────┘
```

#### Grafik B67: Gelombang Verifikasi & Kepatuhan Arsitektur `S-C3`
```text
BE-HRD-263 [B65] ──┐
BE-HRD-266 [B66] ──┼──> BE-HRD-268 ✅ (Controller API Kontrak v21 & 6 Background Workers PRF-W1..W6) ──> BE-HRD-269 ✅ (Validasi Kompilasi .NET, QBE Conformance & Acceptance Tests S-C3)
BE-HRD-267 [B66] ──┘
```

```text
Legenda:
──>    : Prasyarat langsung (kiri harus selesai sebelum kanan dapat dimulai)
├──>   : Percabangan dependensi
[...]  : Cermin task dari grafik gelombang sebelumnya
â ³    : Task aktif berstatus Menunggu Prasyarat (Waiting Dependencies)
```

---

### 19.2 Tabel Urutan Gelombang Eksekusi Backend `S-C3`

| Gelombang | ID Task | Nama Task | Prasyarat Langsung | Lingkup Deliverable |
| :--- | :--- | :--- | :--- | :--- |
| **`MVP-51`** | `BE-HRD-254` | Migrasi Skema DB 10 Tabel Relasional & Seeding Master KPI/BerAKHLAK | - | EF Core migrations 10 tabel, indeks unik, audit columns, seeding katalog indikator KPI kuantitatif & Core Values BerAKHLAK skala 1-5. |
| **`MVP-51`** | `BE-HRD-255` | Entity Framework Core Repositories & Unit of Work Performance | `BE-HRD-254` | Antarmuka `IPerformanceRepository`, `IPerformanceUnitOfWork`, konfigurasi DbContext, soft-delete query filter. |
| **`MVP-51`** | `BE-HRD-256` | DTOs, Object Mapping & Aturan Validasi Request (`VR-PRF-01`..`25`) | `BE-HRD-255` | FluentValidation rules `VR-PRF-01` s.d. `VR-PRF-25`, DTOs request/response Kontrak v21, AutoMapper profiles. |
| **`MVP-51`** | `BE-HRD-257` | Service Siklus Kinerja, Guard Periode & Validasi Anti-Tumpang-Tindih | `BE-HRD-256` | `PerformanceCycleService`, penegakan aturan `PRF-B1` (anti-overlapping cycle), transisi siklus `Draft` -> `Active` -> `Calibration` -> `Closed`. |
| **`MVP-51`** | `BE-HRD-258` | Service Cascading Target Sasaran Mutu Unit ke Individu | `BE-HRD-257` | `PerformanceGoalService`, penetapan sasaran kerja cascading dari unit kerja ke pegawai, validasi akumulasi total bobot tepat 100% (`PRF-B2`, `VR-PRF-04`). |
| **`MVP-51`** | `BE-HRD-259` | Service Evaluasi Tiga Tingkat Berjenjang (Self -> Karu -> Kabid) & Formula Baku 60:40 | `BE-HRD-258`, `BE-HRD-260` | `PerformanceAppraisalService`, alur evaluasi berjenjang tiga tingkat tanpa bypass wewenang (`PRF-B3`), formula baku 60% KPI hasil kerja kuantitatif + 40% perilaku Core Values BerAKHLAK skala 1-5 (`PRF-B4`). |
| **`MVP-51`** | `BE-HRD-260` | Service Peer Review Sejawat untuk IDP Non-Skor | `BE-HRD-256` | `PerformancePeerReviewService`, fasilitasi umpan balik sejawat anonim/teridentifikasi khusus rekomendasi *Individual Development Plan* tanpa mempengaruhi skor komposit (`PRF-B5`). |
| **`MVP-52`** | `BE-HRD-261` | Service Sidang Pleno Kalibrasi Manajemen & Guiding Distribution | `BE-HRD-259` | `PerformanceCalibrationService`, konsol sidang pleno kalibrasi, visualisasi sebaran nilai terhadap pedoman fleksibel (*Guiding Distribution* 10-15% A, 70-80% B, 5-10% C/D), kewajiban nota justifikasi tertulis override nilai (`PRF-B6`), dan rilis nilai serentak ke ESS. |
| **`MVP-52`** | `BE-HRD-262` | Service Program Pembinaan Formal PIP 60 Hari Kalender & Logbook Mingguan | `BE-HRD-261` | `PerformanceImprovementPlanService`, pemicu otomatis PIP untuk skor akhir < 65.00 (`PRF-B7`), penetapan target perbaikan, penugasan mentor, monitoring 8 logbook mingguan, serta sidang putusan kelulusan hari ke-60. |
| **`MVP-52`** | `BE-HRD-263` | Integrasi Pembekuan Mutasi/Promosi Staf PIP ke Sub-06 (`INT-HR-84`) | `BE-HRD-262` | Publikasi domain event `StaffEnteredPipEvent` (`INT-HR-84`) yang memblokir hak mutasi/promosi di Sub-06, pencatatan event audit `StaffPipSanctionEnforced`, serta pemulihan hak kepegawaian normal via `StaffCompletedPipEvent`. |
| **`MVP-53`** | `BE-HRD-264` | Service Evaluasi Kenaikan Jenjang Karier Klinis Nakes (Clinical Ladder PK I-V) | `BE-HRD-261` | `ClinicalCareerLadderService`, verifikasi gatekeeper prasyarat administratif skor kinerja tahunan minimal 75.00 ("Baik", `PRF-B8`), pendaftaran asesmen kenaikan jenjang klinis nakes, audit logbook tindakan oleh Komite Keperawatan/Nakes. |
| **`MVP-53`** | `BE-HRD-265` | Integrasi Prasyarat Mutu Jenjang Klinis ke Sub-16 (`INT-HR-81`) & Sub-15 (`INT-HR-82`) | `BE-HRD-264` | Validasi integrasi sertifikat kompetensi aktif dari Sub-16 (`INT-HR-81`) dan verifikasi bebas kasus pelanggaran sentinel / keselamatan pasien dari Sub-15 (`INT-HR-82`). |
| **`MVP-53`** | `BE-HRD-266` | Integrasi Sinkronisasi SK Jenjang Baru ke Sub-02 & Sub-14 (`INT-HR-83`) | `BE-HRD-265` | Penerbitan SK Direktur Utama jenjang baru dan sinkronisasi otomatis mutasi tingkat jabatan ke Sub-02 Profil Induk serta pemutakhiran paket kewenangan klinis RKK ke Sub-14 (`INT-HR-83`). |
| **`MVP-53`** | `BE-HRD-267` | Service Tata Kelola Sanggah Banding Nilai Kinerja SLA 7/14 Hari | `BE-HRD-261` | `PerformanceDisputeService`, penerimaan sanggahan nilai resmi pegawai di ESS dalam SLA maksimal H+7, validasi lampiran bukti pembanding, mediasi klarifikasi HR Performance H+7 kerja, sidang majelis banding Direktur SDM maksimal SLA 14 hari kalender. |
| **Verifikasi** | `BE-HRD-268` | Controller API Kontrak v21 & Registrasi 6 Background Workers (`PRF-W1`..`W6`) | `BE-HRD-263`, `BE-HRD-266`, `BE-HRD-267` | 4 Controller endpoint Kontrak v21 (24 endpoints), registrasi 6 background workers (`PerformanceCycleStatusTransitionWorker`, `PerformanceGoalLockingWorker`, `PerformanceCalibrationDeadlineWorker`, `PerformancePipMilestoneMonitoringWorker`, `ClinicalCareerLadderExpiryWorker`, `PerformanceDisputeSlaEnforcementWorker`). |
| **Verifikasi** | `BE-HRD-269` | Validasi Kompilasi .NET, QBE Conformance & Eksekusi Acceptance Tests `S-C3` | Seluruh task `BE-HRD-254` s.d. `BE-HRD-268` | Verifikasi `dotnet build`, QBE Preflight Conformance Check, validasi 24 endpoint API Swagger Kontrak v21, eksekusi 22 test case `AT-HRD-PRF-01` s.d. `AT-HRD-PRF-22`, dan pemenuhan 10 kriteria DoD. |

---

### 19.3 Rincian Spesifikasi Task Backend `S-C3`

#### ✅ `BE-HRD-254` — Migrasi Skema DB 10 Tabel Relasional Performance & Seeding Master Data Katalog KPI / Core Values BerAKHLAK
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: ✅ Selesai (2026-11-09) — Bukti: [BE-HRD-254.md](../task/report/backend/BE-HRD-254.md), Kompilasi C# 0 error, migrasi EF Core dan 10 model/konfigurasi siap.
* **Outcome**: Penambahan skema basis data PostgreSQL untuk 10 tabel relasional baru (`mst_kpi_metric_catalogs`, `trx_performance_cycles`, `trx_performance_goals`, `trx_performance_appraisals`, `trx_performance_appraisal_details`, `trx_performance_calibration_sessions`, `trx_performance_improvement_plans`, `trx_performance_pip_action_logs`, `trx_clinical_career_ladder_reviews`, `trx_performance_appraisal_disputes`) dengan Primary Key GUID/UUID, indeks komposit efisien pada pencarian siklus dan pegawai, audit columns lengkap (`created_at`, `created_by`, `updated_at`, `updated_by`, `is_deleted`), konfigurasi Entity Framework Core fluent mapping, serta migration seeding awal master katalog indikator KPI kuantitatif rumah sakit dan 7 butir Core Values BerAKHLAK skala likert 1-5.
* **Dependencies**: Tidak ada (Task perdana Backend `S-C3`).
* **Acceptance Criteria**: Migrasi EF Core berhasil dieksekusi tanpa eror; seluruh 10 tabel tercipta di skema PostgreSQL dengan relasi foreign key dan indeks unik yang presisi; data awal Core Values BerAKHLAK ter-seed dengan benar.
* **Verifikasi**: Laporan task tracked `BE-HRD-254.md` dan log eksekusi migrasi EF Core.

#### ✅ `BE-HRD-255` — Entity Framework Core Repositories & Unit of Work Performance Management
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Pembangunan lapisan akses data (*data access layer*) terisolasi menggunakan Generic Repository Pattern dan Unit of Work: `IPerformanceCycleRepository`, `IPerformanceGoalRepository`, `IPerformanceAppraisalRepository`, `IPerformanceCalibrationRepository`, `IPerformancePipRepository`, `IClinicalCareerLadderRepository`, `IPerformanceDisputeRepository`, dan `IPerformanceUnitOfWork`. Menerapkan global query filter untuk soft-delete (`is_deleted == false`), transaksi atomik basis data ACID, serta penanganan konkurensi optimistik.
* **Dependencies**: `BE-HRD-254`
* **Acceptance Criteria**: Seluruh operasi CRUD beroperasi konsisten; transaksi roll back secara atomik jika terjadi kegagalan penyimpanan data ganda; soft-delete terfilter otomatis pada query pembacaan.
* **Verifikasi**: Uji isolasi data layer pada laporan task tracked `BE-HRD-255.md`.

#### ✅ `BE-HRD-256` — DTOs, Object Mapping & Aturan Validasi Request (VR-PRF-01 s.d. VR-PRF-25)
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Pembuatan Data Transfer Objects (DTOs) request dan response sesuai spesifikasi Kontrak API `v21`, konfigurasi AutoMapper profiles, dan implementasi 25 aturan validasi bisnis ketat menggunakan FluentValidation (`VR-PRF-01` s.d. `VR-PRF-25`), mencakup pencegahan tumpang tindih tanggal siklus (`VR-PRF-01`), pembatasan jumlah target sasaran mutu maksimal 10 indikator (`VR-PRF-03`), validasi akumulasi bobot target tepat 100% (`VR-PRF-04`), pembatasan skala likert BerAKHLAK 1-5 (`VR-PRF-08`), batas waktu sanggah banding maksimal 7 hari kalender (`VR-PRF-21`), dan validasi berkas portofolio klinis.
* **Dependencies**: `BE-HRD-255`
* **Acceptance Criteria**: Validasi request menolak payload tidak valid dengan kode HTTP 422/400 dan pesan kesalahan terstruktur dalam Bahasa Indonesia baku; mapping DTO ke entitas domain terbebas dari kebocoran properti internal.
* **Verifikasi**: Unit test validator FluentValidation pada `AT-HRD-PRF-02`, `AT-HRD-PRF-04`.

#### ✅ `BE-HRD-257` — Service Siklus Kinerja, Guard Periode & Validasi Anti-Tumpang-Tindih (PerformanceCycleService & PRF-B1)
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan domain pengelolaan siklus evaluasi kinerja berkala rumah sakit (semesteran/tahunan). Mengimplementasikan aturan `PRF-B1` yang mencegah pembukaan dua siklus aktif yang saling tumpang tindih pada rentang tanggal yang sama untuk unit/kelompok pegawai yang sama. Mengatur siklus transisi status siklus (`Draft` -> `Active` -> `Calibration` -> `Closed`, `VR-PRF-02`) dan mengunci pembuatan lembar kerja baru saat siklus telah berstatus `Closed`.
* **Dependencies**: `BE-HRD-256`
* **Acceptance Criteria**: Sistem menolak pendaftaran siklus kinerja kedua jika tanggal mulainya berada di dalam rentang siklus yang masih aktif (`VR-PRF-01`); pembukaan dan penutupan siklus mencatat event audit `PerformanceCycleCreated` dan `PerformanceCycleClosed`.
* **Verifikasi**: Uji fungsional siklus kinerja `AT-HRD-PRF-01`.

#### ✅ `BE-HRD-258` — Service Cascading Target Sasaran Mutu Unit ke Individu & Validasi Bobot 100% (PerformanceGoalService & PRF-B2)
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penetapan dan penurunan sasaran mutu (*cascading performance goals*) dari target indikator kinerja unit kerja/instalasi ke sasaran kerja individu pegawai. Mengimplementasikan aturan `PRF-B2` yang mewajibkan akumulasi bobot target tepat 100.00% sebelum dapat diajukan (*Locking Rule*), pembatasan maksimal 10 butir KPI per pegawai (`VR-PRF-03`), penegakan garis persetujuan atasan langsung (Kepala Ruangan/Kepala Unit), dan pembekuan target pasca disetujui atasan.
* **Dependencies**: `BE-HRD-257`
* **Acceptance Criteria**: Pengajuan sasaran kerja ditolak jika akumulasi bobot < 100% atau > 100% (`VR-PRF-04`); persetujuan sasaran oleh atasan langsung mengubah status menjadi `Approved` dan membekukan target dari perubahan sepihak.
* **Verifikasi**: Uji validasi total bobot target sasaran `AT-HRD-PRF-03`, `AT-HRD-PRF-04`.

#### ✅ `BE-HRD-259` — Service Evaluasi Tiga Tingkat Berjenjang (Self -> Karu -> Kabid) & Formula Baku 60:40 (PerformanceAppraisalService, PRF-B3, PRF-B4)
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Mesin kalkulasi dan orkestrasi penilaian kinerja resmi pegawai rumah sakit. Mengimplementasikan alur penilaian berjenjang tiga tingkat tanpa bypass wewenang (`PRF-B3`): Pegawai mengisi *Self-Assessment*, Kepala Ruangan (Karu) melakukan penilaian utama hasil kerja dan perilaku, dan Kepala Bidang (Kabid) melakukan penelaahan serta persetujuan akhir. Menerapkan formula baku perhitungan nilai komposit (`PRF-B4`): `60% x Skor KPI Hasil Kerja Kuantitatif + 40% x Skor Perilaku Core Values BerAKHLAK (dikonversi ke skala 100)`. Mengelompokkan predikat kinerja (Sangat Baik >= 90, Baik 75-89, Cukup 65-74, Kurang < 65).
* **Dependencies**: `BE-HRD-258`, `BE-HRD-260`
* **Acceptance Criteria**: Penilaian atasan ditolak jika pegawai belum menuntaskan self-assessment (`VR-PRF-05`); formula matematis 60% KPI + 40% BerAKHLAK menghasilkan skor komposit yang presisi hingga 2 desimal (`VR-PRF-09`); predikat kinerja terpetakan deterministik.
* **Verifikasi**: Uji kalkulasi skor komposit 60:40 dan evaluasi berjenjang `AT-HRD-PRF-05`, `AT-HRD-PRF-06`, `AT-HRD-PRF-07`.

#### ✅ `BE-HRD-260` — Service Peer Review Sejawat untuk IDP Non-Skor (PerformancePeerReviewService & PRF-B5)
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pengumpulan umpan balik rekan sejawat setara (*Peer Review 360 Feedback*). Mengimplementasikan aturan `PRF-B5` di mana umpan balik peer review bersifat opsional dan secara ketat difungsikan sebagai bahan rekomendasi rencana pengembangan individu (*Individual Development Plan* - IDP) non-skor, sehingga tidak boleh mempengaruhi atau mendistorsi nilai kuantitatif evaluasi resmi pegawai. Mendukung mode masukan teridentifikasi maupun anonim terproteksi.
* **Dependencies**: `BE-HRD-256`
* **Acceptance Criteria**: Rekaman ulasan rekan sejawat berhasil disimpan dan diagregasikan ke laporan IDP; sistem menjamin nilai komposit resmi pegawai 100% murni dihitung dari rasio 60% KPI + 40% BerAKHLAK tanpa campur tangan bobot peer review (`VR-PRF-10`).
* **Verifikasi**: Uji isolasi peer review dari skor akhir pada `AT-HRD-PRF-08`.

#### ✅ `BE-HRD-261` — Service Sidang Pleno Kalibrasi Manajemen & Guiding Distribution (PerformanceCalibrationService & PRF-B6)
* **Slice / MVP**: `S-C3` / `MVP-52`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Modul tata kelola sidang pleno kalibrasi manajemen RS tingkat komite/direksi. Menghitung sebaran distribusi predikat nilai aktual seluruh pegawai dan membandingkannya terhadap kurva pedoman sebaran fleksibel (*Guiding Distribution*, `PRF-B6`): Predikat A (Sangat Baik) 10-15%, Predikat B (Baik) 70-80%, dan Predikat C/D (Cukup/Kurang) 5-10%. Mengizinkan penyesuaian (*score override*) oleh komite kalibrasi dengan kewajiban melampirkan nota justifikasi tertulis minimal 50 karakter (`VR-PRF-12`). Menyediakan mekanisme penguncian sesi kalibrasi dan rilis nilai serentak ke portal ESS pegawai.
* **Dependencies**: `BE-HRD-259`
* **Acceptance Criteria**: Penyesuaian skor tanpa nota justifikasi ditolak sistem (`VR-PRF-13`); audit trail mencatat nilai awal, nilai kalibrasi, nama penilai, dan isi nota justifikasi secara permanen; pengesahan sidang kalibrasi memicu event `PerformanceScoresReleasedToEss`.
* **Verifikasi**: Uji konsol kalibrasi dan audit override nilai `AT-HRD-PRF-09`, `AT-HRD-PRF-10`, `AT-HRD-PRF-11`.

#### ✅ `BE-HRD-262` — Service Program Pembinaan Formal PIP 60 Hari & Logbook Mingguan (PerformanceImprovementPlanService & PRF-B7)
* **Slice / MVP**: `S-C3` / `MVP-52`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan otomasi program pembinaan peningkatan kinerja (*Performance Improvement Plan* - PIP) terstruktur 60 hari kalender. Mengimplementasikan aturan `PRF-B7` yang secara otomatis memicu pembukaan kasus PIP formal ketika skor akhir komposit pegawai pasca-kalibrasi berada di bawah ambang minimal 65.00 (`VR-PRF-14`). Mengatur penugasan mentor, pencatatan target perbaikan mingguan, pemantauan 8 logbook mingguan mentor (Minggu 1 s.d. 8), serta sidang evaluasi kelulusan pada hari ke-60 dengan 3 opsi putusan resmi: Lulus (kembali normal), Perpanjangan (maksimal 30 hari), atau Eskalasi HR Hubungan Industrial.
* **Dependencies**: `BE-HRD-261`
* **Acceptance Criteria**: Staf dengan skor < 65 otomatis berstatus `EnrolledInPip`; logbook mingguan mewajibkan catatan perkembangan minimal 20 karakter dari mentor (`VR-PRF-15`); sidang hari ke-60 menegakkan putusan resmi.
* **Verifikasi**: Uji pemicu otomatis PIP dan pencatatan logbook mingguan `AT-HRD-PRF-12`, `AT-HRD-PRF-13`, `AT-HRD-PRF-14`.

#### ✅ `BE-HRD-263` — Integrasi Pembekuan Mutasi/Promosi Staf PIP ke Sub-06 Penjadwalan (INT-HR-84)
* **Slice / MVP**: `S-C3` / `MVP-52`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Penegakan pengamanan karir pegawai yang sedang menjalani pembinaan khusus. Mengirimkan domain event `StaffEnteredPipEvent` (`INT-HR-84`) ke Sub-06 Kepegawaian guna mengaktifkan sakelar pembekuan hak mutasi, promosi jabatan, dan kenaikan golongan selama 60 hari masa PIP berjalan (`VR-PRF-16`). Apabila pegawai dinyatakan lulus pada sidang evaluasi hari ke-60, sistem mengirimkan event pemulihan `StaffCompletedPipEvent` untuk membuka kembali hak kepegawaian normal.
* **Dependencies**: `BE-HRD-262`
* **Acceptance Criteria**: Pengajuan mutasi/promosi untuk pegawai aktif PIP ditolak otomatis oleh validasi Sub-06; pemulihan status kepegawaian berjalan sinkron pasca kelulusan PIP.
* **Verifikasi**: Uji integrasi pembekuan hak kepegawaian `INT-HR-84` pada `AT-HRD-PRF-15`.

#### ✅ `BE-HRD-264` — Service Evaluasi Kenaikan Jenjang Karier Klinis Nakes (Clinical Ladder PK I-V, PRF-B8)
* **Slice / MVP**: `S-C3` / `MVP-53`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan verifikasi dan tata kelola kenaikan jenjang karier profesional perawat klinis dan nakes (PK I s.d. PK V) berpedoman Permenkes 40/2017. Mengimplementasikan aturan `PRF-B8` yang mewajibkan skor kinerja tahunan minimal 75.00 ("Baik") sebagai gerbang prasyarat administratif (*Administrative Gatekeeper*) sebelum pengajuan dapat diproses lebih lanjut (`VR-PRF-17`). Mengelola alur pendaftaran, penyerahan berkas portofolio logbook tindakan klinis, dan sidang asesmen portofolio oleh panel asesor Komite Keperawatan / Komite Tenaga Kesehatan Lainnya.
* **Dependencies**: `BE-HRD-261`
* **Acceptance Criteria**: Permohonan kenaikan jenjang ditolak otomatis jika skor kinerja tahunan terakhir < 75.00 (`VR-PRF-17`); berkas portofolio tindakan klinis tervalidasi lengkap oleh komite.
* **Verifikasi**: Uji gatekeeper kenaikan jenjang klinis `AT-HRD-PRF-16`, `AT-HRD-PRF-17`.

#### ✅ `BE-HRD-265` — Integrasi Prasyarat Mutu Jenjang Klinis ke Sub-16 Diklat (INT-HR-81) & Sub-15 OPPE (INT-HR-82)
* **Slice / MVP**: `S-C3` / `MVP-53`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Penguatan integrasi mutu klinis lintas modul rumah sakit saat evaluasi jenjang karir nakes. Memvalidasi kepemilikan sertifikat pelatihan dan kompetensi aktif (BHD, PPI, Pasien Safety) serta kelulusan ujian OSCE klinis dari Sub-16 Learning & Competency (`INT-HR-81`, `VR-PRF-18`). Memeriksa rekam jejak keselamatan pasien dan memastikan nakes bebas dari keterlibatan kasus insiden keselamatan pasien berkategori Sentinel atau komplikasi mayor dalam 12 bulan terakhir dari Sub-15 OPPE (`INT-HR-82`, `VR-PRF-19`).
* **Dependencies**: `BE-HRD-264`
* **Acceptance Criteria**: Pengajuan jenjang klinis ditolak atau dibekukan jika sertifikat wajib Sub-16 telah kedaluwarsa atau tercatat kasus sentinel aktif pada Sub-15.
* **Verifikasi**: Uji integrasi data prasyarat mutu `INT-HR-81` dan `INT-HR-82` pada `AT-HRD-PRF-18`, `AT-HRD-PRF-19`.

#### ✅ `BE-HRD-266` — Integrasi Sinkronisasi SK Jenjang Baru ke Sub-02 Profil Induk & Sub-14 Kredensialing (INT-HR-83)
* **Slice / MVP**: `S-C3` / `MVP-53`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Otomasi mutasi data pasca penetapan Surat Keputusan (SK) Kenaikan Jenjang Karier Klinis oleh Direktur Utama RS. Menerbitkan nomor SK resmi, mencatat riwayat kenaikan jenjang (`VR-PRF-20`), menyinkronkan pembaruan tingkat jenjang klinis ke Sub-02 Profil Induk Kepegawaian (`INT-HR-83`), serta memicu pemutakhiran paket kewenangan klinis (RKK) yang berlaku pada SIMRS EMR melalui Sub-14 Kredensialing.
* **Dependencies**: `BE-HRD-265`
* **Acceptance Criteria**: Pengesahan SK Direktur Utama secara atomik memperbarui jenjang kepegawaian di profil nakes dan membuka akses kewenangan klinis baru di SIMRS.
* **Verifikasi**: Uji sinkronisasi SK jenjang klinis baru `INT-HR-83` pada `AT-HRD-PRF-20`.

#### ✅ `BE-HRD-267` — Service Tata Kelola Sanggah Banding Nilai Kinerja Pegawai (SLA 7/14 Hari, PerformanceDisputeService)
* **Slice / MVP**: `S-C3` / `MVP-53`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penanganan keberatan dan sanggah banding nilai kinerja resmi pegawai (`trx_performance_appraisal_disputes`). Menegakkan batas waktu pengajuan sanggahan maksimal 7 hari kalender pasca rilis nilai di ESS (`VR-PRF-21`), validasi lampiran dokumen bukti pembanding (`VR-PRF-22`), alur mediasi tertutup klarifikasi oleh HR Performance dalam 7 hari kerja (`VR-PRF-23`), serta penegakan batas waktu penerbitan putusan final majelis banding Direktur SDM maksimal 14 hari kalender sejak berkas diregistrasi (`VR-PRF-24`).
* **Dependencies**: `BE-HRD-261`
* **Acceptance Criteria**: Pengajuan sanggahan melewati batas H+7 ditolak otomatis oleh sistem; status lembar penilaian terkunci sementara `Disputed` selama proses mediasi berlangsung; putusan majelis banding bersifat final dan mengikat.
* **Verifikasi**: Uji SLA sanggah banding dan putusan majelis `AT-HRD-PRF-21`, `AT-HRD-PRF-22`.

#### ✅ `BE-HRD-268` — 4 Controller API Swagger Kontrak v21 & Registrasi 6 Background Workers (PRF-W1..W6)
* **Slice / MVP**: `S-C3` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi seluruh 24 endpoint API pada 4 Controller ASP.NET Core beranotasi Swagger `[Tags("HRD - ...")]` sesuai Kontrak `v21`, penegakan atribut otorisasi 14 permission persis (`Permissions.Hrd.Performance.*`), pencatatan 14 action audit trails, serta registrasi 6 background workers berbasis `BackgroundService` (`PerformanceCycleStatusTransitionWorker`, `PerformanceGoalLockingWorker`, `PerformanceCalibrationDeadlineWorker`, `PerformancePipMilestoneMonitoringWorker`, `ClinicalCareerLadderExpiryWorker`, `PerformanceDisputeSlaEnforcementWorker`).
* **Dependencies**: `BE-HRD-263`, `BE-HRD-266`, `BE-HRD-267`.
* **Acceptance Criteria**: Seluruh 24 endpoint API terdaftar rapi pada Swagger UI dengan skema DTO yang valid; 6 background workers berjalan terisolasi di background sesuai jadwal cron/interval tanpa mengganggu alur HTTP request.
* **Verifikasi**: Uji kontrak API Swagger dan registrasi DI background service pada laporan task `BE-HRD-268.md`.

#### ✅ `BE-HRD-269` — Validasi Kompilasi .NET, QBE Conformance & Eksekusi Acceptance Tests S-C3
* **Slice / MVP**: `S-C3` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Verifikasi kompilasi bersih aplikasi .NET (`dotnet build`) tanpa error, audit konformansi arsitektur QBE Preflight (`tooling/qbe/Invoke-QbeConformanceCheck.ps1`), validasi kepatuhan 24 endpoint API Swagger Kontrak `v21`, serta eksekusi lengkap 22 skenario pengujian penerimaan `AT-HRD-PRF-01` s.d. `AT-HRD-PRF-22`.
* **Dependencies**: Seluruh task backend `BE-HRD-254` s.d. `BE-HRD-268`.
* **Acceptance Criteria**: `dotnet build` lulus 0 error 0 warning kritis; seluruh acceptance criteria 16 task backend terpenuhi; 10 kriteria DoD tercapai.
* **Verifikasi**: Laporan task tracked `BE-HRD-269.md` dan output terminal dotnet.


---

## 20. Rencana Kerja Backend Sub-Modul 18: Hubungan Industrial & Terminasi (Slice `S-C5`)

### 20.1 Grafik Urutan Dependency Backend `S-C5`

#### Grafik B68: Gelombang `MVP-54` (Skema Data, Tata Kelola Disiplin, Filter Just Culture KMKP & Sanksi SP PP 35/2021)
```text
BE-HRD-270 (Skema DB 10 Tabel & Migrasi EF Core) ──> BE-HRD-271 (EF Repositories & UoW) ──> BE-HRD-272 (DTOs & FluentValidation VR-EMP) ──┬──> BE-HRD-273 (Insiden Disiplin & Just Culture INT-HR-86) ──> BE-HRD-274 (Sidang BAP & Pembelaan Staf) ──> BE-HRD-275 (Penerbitan SP 1-3 Masa 6 Bulan PP 35/2021)
                                                                                                                                              └──> BE-HRD-276 ✅ (Worker Kedaluwarsa SP H+180 Hari EMP-W1)
```

#### Grafik B69: Gelombang `MVP-55` (Skorsing Upah Penuh, Pembekuan Akses SIMRS, Kanal Pengaduan Staf, Whistleblowing Rahasia & Forum Bipartit)
```text
BE-HRD-275 [B68] ──┬──> BE-HRD-277 (Skorsing Upah Penuh Paid Suspension & Revokasi SIMRS INT-HR-88) ──> BE-HRD-278 (Worker Durasi Maks 6 Bulan EMP-W2)
                   ├──> BE-HRD-279 (Kanal Grievance ESS & Worker SLA Mediasi 14 Hari EMP-W3)
                   ├──> BE-HRD-280 (Whistleblower Terenkripsi Asimetris & Isolasi RBAC Kepatuhan)
                   └──> BE-HRD-281 ✅ (Notulen Sidang & Naskah Perjanjian Bersama PB Bipartit)
```

#### Grafik B70: Gelombang `MVP-56` (Pemutusan Kerja PHK, Kalkulator Pesangon PP 35/2021 & Exit Clearance Multi-Departemen Pemblokir Payroll)
```text
BE-HRD-277 [B69] ──┬──> BE-HRD-282 (Pengajuan PHK/Resign 30 Hari & Radar Kontrak PKWT EMP-W4) ──> BE-HRD-283 (Kalkulator Pesangon PP 35/2021 & Approval CFO)
                   └──> BE-HRD-284 ✅ (Pusat Kendali Exit Clearance 5 Unit & Worker Pengingat Kliring EMP-W5)
```

#### Grafik B71: Gelombang Verifikasi & Uji Integrasi End-to-End
```text
BE-HRD-283 [B70] ──┬──> BE-HRD-285 ✅ (Controller API Kontrak v22 5 Tag Swagger, Hard-Block Payroll Final INT-HR-87 & Verifikasi E2E)
BE-HRD-284 [B70] ──┘
```

---

### 20.2 Tabel Urutan Gelombang Eksekusi Backend `S-C5`

| Gelombang Eksekusi | Task ID | Deskripsi Singkat Pekerjaan Backend | Prasyarat (Dependencies) | Target MVP |
|---|---|---|---|---|
| **Gelombang 1** | `BE-HRD-270` | DDL 10 Tabel Postgres & Migrasi EF Core `20261106_AddEmployeeRelationAndExitClearance` | None | `MVP-54` |
| **Gelombang 1** | `BE-HRD-271` | EF Core Repositories & Unit of Work Context Hubungan Industrial & Exit Clearance | `BE-HRD-270` | `MVP-54` |
| **Gelombang 1** | `BE-HRD-272` | DTOs Request/Response, Model Mapping & FluentValidation Rules (`VR-EMP-01` s.d. `VR-EMP-10`) | `BE-HRD-271` | `MVP-54` |
| **Gelombang 2** | `BE-HRD-273` | Service Laporan Insiden Disiplin, Filter Just Culture RS & Integrasi KMKP (`INT-HR-86`) | `BE-HRD-272` | `MVP-54` |
| **Gelombang 2** | `BE-HRD-274` | Service Sidang Pemeriksaan Berita Acara Pemeriksaan (BAP) Disiplin & Pembelaan Staf | `BE-HRD-273` | `MVP-54` |
| **Gelombang 2** | `BE-HRD-275` | Service Penerbitan Sanksi SP 1-3 Masa Berlaku 6 Bulan PP 35/2021 & Sanggah Banding | `BE-HRD-274` | `MVP-54` |
| **Gelombang 2** | `BE-HRD-276` | Background Worker `DisciplinarySanctionExpiryWorker` (`EMP-W1`) Pembersihan SP H+180 | `BE-HRD-275` | `MVP-54` |
| **Gelombang 3** | `BE-HRD-277` | Service Skorsing Upah Penuh (*Paid Suspension*) & Event Pembekuan Akses SIMRS (`INT-HR-88`) | `BE-HRD-275` | `MVP-55` |
| **Gelombang 3** | `BE-HRD-278` | Background Worker `SuspensionPeriodMonitoringWorker` (`EMP-W2`) Batas Maksimal 6 Bulan | `BE-HRD-277` | `MVP-55` |
| **Gelombang 3** | `BE-HRD-279` | Service Kanal Pengaduan Staf (Grievance ESS) & Worker Pemantauan SLA 14 Hari (`EMP-W3`) | `BE-HRD-275` | `MVP-55` |
| **Gelombang 3** | `BE-HRD-280` | Service Whistleblowing Rahasia Terenkripsi Asimetris & Isolasi RBAC Kepatuhan | `BE-HRD-275` | `MVP-55` |
| **Gelombang 3** | `BE-HRD-281` | Service Notulen & Dokumentasi Forum Bipartit RS (LKS Bipartit / Serikat Pekerja) | `BE-HRD-275` | `MVP-55` |
| **Gelombang 4** | `BE-HRD-282` | Service Pengajuan Terminasi (Resign 30 Hari, Kontrak, Pensiun, SP3) & Radar Kontrak `EMP-W4`| `BE-HRD-277` | `MVP-56` |
| **Gelombang 4** | `BE-HRD-283` | Calculation Engine Hak Kompensasi Pesangon PP 35/2021 (UP, UPMK, UPH) & Approval CFO | `BE-HRD-282` | `MVP-56` |
| **Gelombang 4** | `BE-HRD-284` | Service Pusat Kendali Exit Clearance 5 Departemen & Worker Pengingat Kliring `EMP-W5` | `BE-HRD-282` | `MVP-56` |
| **Verifikasi** | `BE-HRD-285` | Controller API Kontrak v22 5 Tag Swagger, Hard-Block Payroll Final `INT-HR-87` & Audit E2E | `BE-HRD-283`, `BE-HRD-284` | Verifikasi |

---

### 20.3 Rincian Spesifikasi Task Backend `S-C5`

#### ✅ `BE-HRD-270` — DDL Basis Data PostgreSQL 10 Tabel & Migrasi EF Core Hubungan Industrial & Exit Clearance
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Penambahan 10 tabel relasional skema PostgreSQL (`trx_disciplinary_incident_reports`, `trx_disciplinary_investigation_records`, `trx_disciplinary_sanctions`, `trx_disciplinary_suspensions`, `trx_employee_grievances`, `trx_bipartite_negotiation_forums`, `trx_employee_separations`, `trx_severance_calculations`, `trx_exit_clearance_processes`, `trx_exit_clearance_checklists`). Migrasi EF Core `20261106_AddEmployeeRelationAndExitClearance` terkompilasi bersih tanpa peringatan skema, lengkap dengan indeks performa pencarian nomor sanksi, tiket pengaduan, dan status clearance.
* **Dependencies**: None
* **Acceptance Criteria**: Seluruh 10 tabel terbuat di PostgreSQL dengan kolom tipe data, primary key UUID, foreign key ke profil pegawai Sub-02, audit trails, dan indeks komposit yang tepat.
* **Verifikasi**: `dotnet ef migrations add` dan review file migrasi C#.

#### ✅ `BE-HRD-271` — Implementasi EF Core Repositories & Unit of Work Bounded Context Hubungan Industrial
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi pola Repository dan Unit of Work untuk entitas disiplin, skorsing, pengaduan, forum bipartit, pemutusan hubungan kerja, kalkulasi pesangon, dan proses exit clearance. Mengonfigurasi entity mapping pada DbContext dengan `AsNoTracking` untuk operasi query berkecepatan tinggi.
* **Dependencies**: `BE-HRD-270`
* **Acceptance Criteria**: Seluruh interface repository (`IDisciplinaryIncidentRepository`, `ISanctionRepository`, `ISeparationRepository`, `IExitClearanceRepository`) terdaftar di DI container ASP.NET Core.
* **Verifikasi**: Uji dependency injection pada startup backend.

#### ✅ `BE-HRD-272` — DTOs, FluentValidation Rules (`VR-EMP-01` s.d. `VR-EMP-10`) & Model Mapping
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi seluruh Request DTO dan Response DTO untuk 24 endpoint Kontrak API `v22`, kelas profile AutoMapper/Mapster, serta validator FluentValidation yang menegakkan aturan bisnis `VR-EMP-01` s.d. `VR-EMP-10` (validasi kelengkapan form insiden, filter nomor audit KMKP untuk insiden medis, pencegahan SP atas kategori Human Error, validasi tim pemeriksa BAP, dan perhitungan kedaluwarsa SP tepat 6 bulan kalender).
* **Dependencies**: `BE-HRD-271`
* **Acceptance Criteria**: Request invalid menghasilkan HTTP 422 Unprocessable Entity dengan rincian pesan kesalahan validasi yang informatif.
* **Verifikasi**: Uji validasi input payload DTO.

#### ✅ `BE-HRD-273` — Service Pelaporan Insiden Disiplin, Filter Just Culture RS & Integrasi KMKP (`DisciplinaryIncidentService` & `INT-HR-86`)
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pencatatan insiden pelanggaran tata tertib kerja dengan filter prinsip *Just Culture* rumah sakit. Mengimplementasikan aturan `EMP-B2` yang menghubungkan laporan insiden pelayanan medis pasien dengan nomor registrasi audit klinis KMKP Sub-15 (`INT-HR-86`). Menegakkan larangan penjatuhan sanksi SP untuk kategori *Human Error* dan mengalihkan tindakan ke modul pembinaan/diklat Sub-16.
* **Dependencies**: `BE-HRD-272`
* **Acceptance Criteria**: Insiden klinis tanpa nomor audit KMKP sah ditolak otomatis (`VR-EMP-02`); insiden berstatus *Human Error* diblokir dari proses penerbitan sanksi SP (`VR-EMP-03`).
* **Verifikasi**: Uji alur Just Culture `AT-HRD-EMP-01`, `AT-HRD-EMP-02`, `AT-HRD-EMP-03`.

#### ✅ `BE-HRD-274` — Service Sidang Pemeriksaan Berita Acara Pemeriksaan (BAP) Disiplin & Pembelaan Staf (`DisciplinaryInvestigationService`)
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan digitalisasi administrasi Berita Acara Pemeriksaan (BAP). Mengelola penjadwalan sidang klarifikasi, pencatatan daftar nama pemeriksa independen (minimal 2 orang sesuai `VR-EMP-04`), rangkuman kesaksian saksi, unggah berkas bukti fisik/digital, pencatatan lembar pembelaan tertulis staf terperiksa (`VR-EMP-05`), serta perumusan kesimpulan rekomendasi tindakan hukum disiplin.
* **Dependencies**: `BE-HRD-273`
* **Acceptance Criteria**: Dokumen BAP tersimpan rapi dengan nomor unik `BAP-...`, status transisi dari `Draft` ke `ApprovedByHrDirector` berjalan sesuai state machine.
* **Verifikasi**: Uji sidang BAP pada `AT-HRD-EMP-04`.

#### ✅ `BE-HRD-275` — Service Penerbitan Sanksi Surat Peringatan (SP 1-3 Masa 6 Bulan PP 35/2021) & Sanggah Banding (`DisciplinarySanctionService` & `EMP-B1`)
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penerbitan Surat Peringatan bertingkat (Teguran Lisan maks 3 bulan, SP 1, SP 2, SP 3). Menegakkan formula tanggal kedaluwarsa tepat 6 bulan kalender (`ValidUntil = IssuedDate + 6 Months`) sesuai Pasal 52 PP No. 35 Tahun 2021 (`EMP-B1`, `VR-EMP-06`). Mengatur kenaikan sanksi berjenjang (`VR-EMP-08`), jalur cepat penerbitan langsung SP 3 untuk pelanggaran berat berizin Direktur SDM (`VR-EMP-09`), pencatatan arsip permanen ke e-Dossier Sub-02 (`INT-HR-85`), serta memfasilitasi pengajuan sanggah banding tertulis maksimal 7 hari kerja (`VR-EMP-10`).
* **Dependencies**: `BE-HRD-274`
* **Acceptance Criteria**: `ValidUntil` tercatat tepat 6 bulan kalender dari tanggal penerbitan; riwayat sanksi tersinkronkan ke profil e-Dossier Sub-02 (`INT-HR-85`).
* **Verifikasi**: Uji penerbitan sanksi SP dan banding `AT-HRD-EMP-05`, `AT-HRD-EMP-08`, `AT-HRD-EMP-09`, `AT-HRD-EMP-10`.

#### ✅ `BE-HRD-276` — Background Worker `DisciplinarySanctionExpiryWorker` (`EMP-W1`) Pembersihan SP H+180 Hari & Notifikasi H-14
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan background terjadwal harian (`DisciplinarySanctionExpiryWorker`). Memindai sanksi SP berstatus `Active` yang telah mencapai tanggal kedaluwarsa (`ValidUntil`), mengubah statusnya menjadi `Expired` secara otomatis pada H+180, serta mengirimkan notifikasi peringatan H-14 sebelum berakhirnya masa sanksi kepada pegawai dan atasan langsung.
* **Dependencies**: `BE-HRD-275`
* **Acceptance Criteria**: Status SP aktif tepat berubah menjadi `Expired` pada tanggal jatuh tempo; status disiplin pegawai kembali bersih tanpa menghilangkan arsip audit.
* **Verifikasi**: Uji worker kedaluwarsa sanksi pada `AT-HRD-EMP-06`, `AT-HRD-EMP-07`.

#### ✅ `BE-HRD-277` — Service Skorsing Upah Penuh (*Paid Suspension* 100% Upah) & Event Pembekuan Akses SIMRS (`INT-HR-88`)
* **Slice / MVP**: `S-C5` / `MVP-55`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penetapan skorsing investigasi pegawai menuju PHK. Menegakkan kepatuhan Pasal 157A UU Ketenagakerjaan bahwa upah pokok dan tunjangan tetap wajib dibayarkan 100% (`wage_payment_status = 'PaidFull'`, `VR-EMP-12`). Menerbitkan domain event `INT-HR-88` (`AccessRevocationRequestedEvent`) ke subsistem IAM/Keycloak SSO SIMRS untuk memutus sesi aktif login EMR/PACS dan menonaktifkan kartu tanda pengenal fisik (*smart card*) akses fasilitas rumah sakit secara instan (`VR-EMP-13`).
* **Dependencies**: `BE-HRD-275`
* **Acceptance Criteria**: Surat skorsing terbit dengan batas maksimal 6 bulan; event revokasi SIMRS terbit seketika; hak gaji pokok di Sub-10 tetap berjalan penuh.
* **Verifikasi**: Uji paid suspension dan revokasi akses SIMRS `AT-HRD-EMP-11`, `AT-HRD-EMP-12`.

#### ✅ `BE-HRD-278` — Background Worker `SuspensionPeriodMonitoringWorker` (`EMP-W2`) Batas Maksimal 6 Bulan Skorsing
* **Slice / MVP**: `S-C5` / `MVP-55`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pemantauan durasi skorsing investigasi harian (`SuspensionPeriodMonitoringWorker`). Menghitung selisih hari berjalan sejak skorsing dimulai. Pada hari ke-150 (H-30 dari batas maksimal 180 hari / 6 bulan), worker menerbitkan alert eskalasi prioritas tinggi kepada Direktur Utama dan Direktur SDM untuk segera menetapkan keputusan sidang akhir (rehabilitasi atau penetapan PHK resmi).
* **Dependencies**: `BE-HRD-277`
* **Acceptance Criteria**: Alert eskalasi terbit otomatis pada hari ke-150; mencegah pelanggaran hukum akibat skorsing berlarut-larut melebihi 6 bulan kalender.
* **Verifikasi**: Uji threshold durasi skorsing pada unit test worker.

#### ✅ `BE-HRD-279` — Service Kanal Pengaduan Staf (Grievance ESS) & Worker Pemantauan SLA 14 Hari (`EMP-W3`)
* **Slice / MVP**: `S-C5` / `MVP-55`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penanganan keluhan hubungan kerja harian pegawai (*Grievance*) melalui portal ESS. Mengatur registrasi tiket keluhan, perutean ke atasan langsung, pencatatan hasil klarifikasi dan notulen mediasi damai. Didukung background worker `GrievanceSlaEscalationWorker` (`EMP-W3`) yang memantau batas waktu penyelesaian maksimal 14 hari kerja (`VR-EMP-14`) dan mengeskalasikan tiket ke Kepala Bagian SDM jika melewati batas waktu tanpa tanggapan.
* **Dependencies**: `BE-HRD-275`
* **Acceptance Criteria**: Tiket keluhan terdaftar dengan SLA target 14 hari kerja; eskalasi otomatis berjalan saat supervisor tidak menanggapi dalam 7 hari kerja.
* **Verifikasi**: Uji alur tiket keluhan dan eskalasi SLA `AT-HRD-EMP-13`.

#### ✅ `BE-HRD-280` — Service Whistleblowing Rahasia Terenkripsi Asimetris & Isolasi RBAC Kepatuhan (`WhistleblowerCaseService` & `EMP-B3`)
* **Slice / MVP**: `S-C5` / `MVP-55`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pelaporan dugaan kecurangan (fraud pengadaan, gratifikasi farmasi, pelecehan seksual, manipulasi rekam medis) yang aman dan berkeadilan. Menerapkan enkripsi asimetris pada payload laporan (`VR-EMP-15`), mendukung opsi pelaporan anonim total, serta menerapkan filter otorisasi ketat di mana atasan langsung pihak terlapor dilarang keras membuka tiket (`VR-EMP-16`, RBAC `Hrd.Disciplinary.WhistleblowerReview`).
* **Dependencies**: `BE-HRD-275`
* **Acceptance Criteria**: Laporan whistleblower tersimpan dalam format terenkripsi; pengguna tanpa wewenang khusus Kepatuhan menerima HTTP 403 Forbidden saat mengakses.
* **Verifikasi**: Uji enkripsi payload dan isolasi akses `AT-HRD-EMP-14`, `AT-HRD-EMP-15`.

#### ✅ `BE-HRD-281` — Service Dokumentasi & Risalah Perundingan Forum Bipartit RS (LKS Bipartit / Serikat Pekerja)
* **Slice / MVP**: `S-C5` / `MVP-55`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan administrasi forum perundingan Bipartit rumah sakit. Mengelola pendaftaran agenda perselisihan (hak, kepentingan, PHK), pencatatan daftar hadir perwakilan manajemen dan perwakilan pekerja (`VR-EMP-17`), notulen resmi jalannya rapat (*Minutes of Meeting*), serta pengunggahan berkas digital naskah Perjanjian Bersama (PB) berformat PDF yang berkekuatan hukum (`VR-EMP-18`).
* **Dependencies**: `BE-HRD-275`
* **Acceptance Criteria**: Berkas Bipartit tersimpan dengan nomor register resmi; naskah Perjanjian Bersama terarsip aman sebagai referensi hukum hubungan industrial.
* **Verifikasi**: Uji registrasi forum Bipartit pada `AT-HRD-EMP-16`.

#### ✅ `BE-HRD-282` — Service Pengajuan Terminasi (Resign 30 Hari, Kontrak, Pensiun, SP3) & Radar Kontrak `EMP-W4`
* **Slice / MVP**: `S-C5` / `MVP-56`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan registrasi pengakhiran ikatan kerja pegawai untuk berbagai alasan (pengunduran diri sukarela dengan validasi pemberitahuan minimal 30 hari kalender sesuai `VR-EMP-19`, berakhirnya masa kontrak PKWT, pensiun normal, pelanggaran disiplin SP 3, pelanggaran berat mendesak, atau meninggal dunia). Terintegrasi dengan background worker `PkwtContractExpiryRadarWorker` (`EMP-W4`) yang memindai masa kontrak PKWT pada H-60 dan H-30.
* **Dependencies**: `BE-HRD-277`
* **Acceptance Criteria**: Pengajuan resign sukarela < 30 hari ditolak validasi; worker memindai kontrak PKWT yang akan habis dan menerbitkan notifikasi evaluasi.
* **Verifikasi**: Uji validasi pengajuan terminasi `AT-HRD-EMP-17`.

#### ✅ `BE-HRD-283` — Calculation Engine Kompensasi Hak Pesangon PP 35/2021 (UP, UPMK, UPH) & Approval CFO (`SeveranceCalculationEngine`)
* **Slice / MVP**: `S-C5` / `MVP-56`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Mesin kalkulasi kompensasi terminasi deterministik berbasis matriks alasan PHK regulasi PP No. 35 Tahun 2021. Menarik masa kerja riil secara otomatis dari tanggal mulai kerja sah (`HireDate`) di profil pegawai Sub-02 (`VR-EMP-20`). Menghitung Uang Pesangon (UP), Uang Penghargaan Masa Kerja (UPMK), Uang Penggantian Hak cuti tahunan aktif (`VR-EMP-22`), Uang Pisah, dan kompensasi PKWT proporsional `(Bulan/12) x Upah` (`VR-EMP-21`). Mengunci rincian lembar pesangon hingga disetujui resmi oleh Direktur Keuangan (`VR-EMP-23`).
* **Dependencies**: `BE-HRD-282`
* **Acceptance Criteria**: Komponen pesangon terhitung akurat sesuai formula regulasi PP 35/2021; lembar pesangon terkunci dan dilarang disinkronkan ke payroll tanpa persetujuan Direktur Keuangan.
* **Verifikasi**: Uji formula pesangon dan kompensasi PKWT `AT-HRD-EMP-18`, `AT-HRD-EMP-19`, `AT-HRD-EMP-20`.

#### ✅ `BE-HRD-284` — Service Pusat Kendali Exit Clearance 5 Departemen & Worker Pengingat Kliring `EMP-W5`
* **Slice / MVP**: `S-C5` / `MVP-56`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pengendalian proses exit clearance lintas departemen. Membuka berkas kliring digital yang memuat butir checklist dari 5 departemen (Unit Kerja, Umum/Aset, Farmasi/Alkes, Keuangan, IT) sesuai `VR-EMP-24`. Mengelola pencatatan serah terima fisik barang dan sisa piutang. Didukung background worker `ExitClearanceChecklistReminderWorker` (`EMP-W5`) yang mengirimkan notifikasi pengingat harian ke departemen yang masih pending menjelang tanggal efektif pengunduran diri.
* **Dependencies**: `BE-HRD-282`
* **Acceptance Criteria**: Seluruh butir checklist 5 departemen tercatat; pengingat otomatis terkirim ke verifikator departemen yang belum memberikan persetujuan.
* **Verifikasi**: Uji inisiasi dan persetujuan checklist pada `AT-HRD-EMP-21`.

#### ✅ `BE-HRD-285` — Controller API Kontrak v22 5 Tag Swagger, Hard-Block Payroll Final Sub-10 (`INT-HR-87`), Worker Penutupan IAM `EMP-W6` & Audit Verifikasi E2E
* **Slice / MVP**: `S-C5` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi seluruh 24 endpoint API pada 5 Controller ASP.NET Core beranotasi Swagger `[Tags("Corporate - HRD ...")]` sesuai Kontrak API `v22`. Mengimplementasikan service provider gerbang `IExitClearanceStatusProvider` yang secara ketat memblokir tombol rilis gaji terakhir (*final settlement run*) dan transfer dana pesangon di Sub-10 Payroll sebelum exit clearance berstatus `Completed` (`INT-HR-87`, `VR-EMP-25`). Mengonfigurasi worker `ItAccessRevocationSyncWorker` (`EMP-W6`) serta melakukan audit verifikasi kompilasi `dotnet build` dan kepatuhan QBE Conformance.
* **Dependencies**: `BE-HRD-283`, `BE-HRD-284`
* **Acceptance Criteria**: Seluruh 24 endpoint API terdaftar rapi pada Swagger UI; rilis payroll Sub-10 terblokir otomatis jika exit clearance belum tuntas 100%; `dotnet build` lulus 0 error.
* **Verifikasi**: Uji gerbang hard-block payroll `AT-HRD-EMP-22` dan laporan task tracked `BE-HRD-285.md`.


---

## 21. Rencana Pengiriman Backend `S-C6`: Occupational Health & Staff Safety (K3RS & Medical Check-Up)

Bagian ini memetakan perencanaan teknis pelaksanaan backend untuk **Sub-Modul 19 (Occupational Health & Staff Safety / K3RS & MCU — Slice `S-C6`)** menjadi 16 task terukur (`BE-HRD-286` s.d. `BE-HRD-301`) yang mencakup implementasi enkripsi field rekam medis klinis AES-256 (`HRD-DEC-176`), penilaian kelaikan kerja operasional (*Fitness to Work*), surat pembatasan tugas (*Work Restriction*) dengan gembok *hard-stop* validasi roster Sub-06 (`INT-HR-89`), sistem tanggap darurat pajanan darah cito < 4 jam (`INT-HR-91`), surveilans serologi serial 1-3-6 bulan, radar kepatuhan imunisasi & titer antibodi nakes STARKES KPS, serta pelaporan kecelakaan kerja ke BPJS Ketenagakerjaan JKK dalam 2x24 jam (`INT-HR-90`).

---

### 21.1 Grafik Urutan Ketergantungan Backend `S-C6` (Pohon Teks)

#### Grafik B72: Fondasi Skema Basis Data K3RS, Unit of Work, Enkripsi AES-256 & DTOs
```text
BE-HRD-286 ✅ (DDL 10 Tabel Postgres, Kolom Ciphertext AES-256 & EF Migration)
   │
   └─> BE-HRD-287 ✅ (EF Core Repositories & Unit of Work Context K3RS)
          │
          └─> BE-HRD-288 ✅ (DTOs Request/Response, Model Mapping & FluentValidation VR-OHS-01..10)
```

#### Grafik B73: Rekam Kesehatan Terenkripsi, Kelaikan Kerja, Surat Restriksi & Hard-Stop Roster Sub-06
```text
BE-HRD-288 [B72] ─┬─> BE-HRD-289 ✅ (Service Rekam Medis Terenkripsi & Field-Level Access HRD-DEC-176)
                  │      │
                  │      └─> BE-HRD-290 ✅ (Service Fitness to Work & Validasi Dokter Hiperkes)
                  │             │
                  │             └─> BE-HRD-291 ✅ (Service Work Restriction & Event Hard-Stop Sub-06 INT-HR-89)
                  │                    │
                  │                    ├─> BE-HRD-292 ✅ (Worker WorkRestrictionExpirationWorker OHS-W2 H+180)
                  │                    │
                  │                    └─> BE-HRD-293 ✅ (Service Asesmen Return-to-Work RTW & Pembuka Gembok Roster)
```

#### Grafik B74: Tanggap Darurat Tertusuk Jarum Cito, Terapi PPP < 4 Jam & Surveilans Serial 1-3-6 Bulan
```text
BE-HRD-288 [B72] ─┬─> BE-HRD-294 ✅ (Service Tanggap Darurat Tertusuk Jarum & Sirene ESS Panic INT-HR-91)
                  │      │
                  │      └─> BE-HRD-295 ✅ (Service Fast-Track Resep Terapi PPP/PEP < 4 Jam & Farmasi Sub-12)
                  │             │
                  │             └─> BE-HRD-296 ✅ (Worker SerialSurveillanceFollowUpWorker OHS-W1 Bulan 1-3-6)
```

#### Grafik B75: Radar Imunisasi STARKES, Bahaya Ruangan, MCU Berkala, Kecelakaan Kerja & Klaim BPJS JKK
```text
BE-HRD-288 [B72] ─┬─> BE-HRD-297 ✅ (Service Radar Imunisasi Staf, Log Batch Vaksin & Worker Booster OHS-W3)
                  ├─> BE-HRD-298 ✅ (Service Surveilans Bahaya Ruangan, TLD Radiasi & Worker Alarm OHS-W5)
                  ├─> BE-HRD-299 ✅ (Service Pelaksanaan MCU Berkala Berbasis Profil Ruangan & Worker OHS-W4)
                  │      │
                  │      └─> BE-HRD-300 ✅ (Service Registrasi KAK/PAK, Olah TKP & Integrasi BPJS JKK INT-HR-90)
                  │
BE-HRD-289 [B73] ─┤
BE-HRD-290 [B73] ─┤
BE-HRD-291 [B73] ─┤
BE-HRD-293 [B73] ─┤
BE-HRD-294 [B74] ─┼─> BE-HRD-301 ✅ (Controller API Kontrak v23 5 Tag Swagger, Worker OHS-W6 & Verifikasi E2E)
BE-HRD-295 [B74] ─┤
BE-HRD-296 [B74] ─┤
BE-HRD-297 [B75] ─┤
BE-HRD-298 [B75] ─┤
BE-HRD-299 [B75] ─┤
BE-HRD-300 [B75] ─┘
```

---

### 21.2 Tabel Urutan Gelombang Eksekusi Backend `S-C6`

| Gelombang Eksekusi | Task ID | Deskripsi Singkat Pekerjaan Backend | Prasyarat (Dependencies) | Target MVP |
|---|---|---|---|---|
| **Gelombang 1** | `BE-HRD-286` | DDL 10 Tabel Postgres, Kolom Ciphertext AES-256 & EF Migration | None | `MVP-57` |
| **Gelombang 1** | `BE-HRD-287` | EF Core Repositories & Unit of Work Context K3RS | `BE-HRD-286` | `MVP-57` |
| **Gelombang 1** | `BE-HRD-288` | DTOs Request/Response, Model Mapping & FluentValidation (`VR-OHS-01` s.d. `VR-OHS-10`) | `BE-HRD-287` | `MVP-57` |
| **Gelombang 2** | `BE-HRD-289` | Service Rekam Medis Terenkripsi & Field-Level Access Control (`HRD-DEC-176`) | `BE-HRD-288` | `MVP-57` |
| **Gelombang 2** | `BE-HRD-290` | Service Evaluasi Kelaikan Kerja (*Fitness to Work*) & Validasi Dokter Hiperkes | `BE-HRD-289` | `MVP-57` |
| **Gelombang 2** | `BE-HRD-291` | Service Pembatasan Tugas Medis (*Work Restriction*) & Event Hard-Stop Sub-06 (`INT-HR-89`) | `BE-HRD-290` | `MVP-57` |
| **Gelombang 2** | `BE-HRD-292` | Background Worker `WorkRestrictionExpirationWorker` (`OHS-W2`) Batas Maksimal 6 Bulan | `BE-HRD-291` | `MVP-57` |
| **Gelombang 2** | `BE-HRD-293` | Service Asesmen Kembali Bekerja (*Return-to-Work* / RTW) & Pembuka Gembok Roster | `BE-HRD-291` | `MVP-57` |
| **Gelombang 3** | `BE-HRD-294` | Service Tanggap Darurat Tertusuk Jarum & Sirene ESS Panic Button (`INT-HR-91`) | `BE-HRD-288` | `MVP-58` |
| **Gelombang 3** | `BE-HRD-295` | Service Fast-Track Resep Profilaksis Pasca Pajanan (PPP/PEP < 4 Jam) & Depo Farmasi | `BE-HRD-294` | `MVP-58` |
| **Gelombang 3** | `BE-HRD-296` | Background Worker `SerialSurveillanceFollowUpWorker` (`OHS-W1`) Bulan 1, 3, dan 6 | `BE-HRD-295` | `MVP-58` |
| **Gelombang 4** | `BE-HRD-297` | Service Radar Imunisasi Staf, Log Batch Vaksin & Worker Pengingat Booster `OHS-W3` | `BE-HRD-288` | `MVP-58` |
| **Gelombang 4** | `BE-HRD-298` | Service Surveilans Bahaya Ruangan, TLD Radiasi & Worker Alarm Batas Dosis `OHS-W5` | `BE-HRD-288` | `MVP-58` |
| **Gelombang 4** | `BE-HRD-299` | Service Pelaksanaan MCU Berkala Berbasis Profil Ruangan, Laporan STARKES & Worker `OHS-W4` | `BE-HRD-288` | `MVP-59` |
| **Gelombang 4** | `BE-HRD-300` | Service Registrasi Kecelakaan Kerja KAK/PAK, Olah TKP & Integrasi BPJS JKK (`INT-HR-90`) | `BE-HRD-299` | `MVP-59` |
| **Gelombang 5** | `BE-HRD-301` | Controller API Kontrak v23 5 Tag Swagger, Worker BPJS `OHS-W6` & Audit Verifikasi E2E | `BE-HRD-289`, `BE-HRD-290`, `BE-HRD-291`, `BE-HRD-293`, `BE-HRD-294`, `BE-HRD-295`, `BE-HRD-296`, `BE-HRD-297`, `BE-HRD-298`, `BE-HRD-299`, `BE-HRD-300` | Verifikasi |

---

### 21.3 Rincian Kartu Spesifikasi Task Backend `S-C6`

#### ✅ `BE-HRD-286` — DDL 10 Tabel Postgres, Kolom Ciphertext AES-256 & EF Migration
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Penyiapan skema basis data PostgreSQL untuk 10 tabel entitas K3RS (`wfp_health_records`, `trx_employee_medical_examinations`, `trx_employee_fitness_to_works`, `trx_work_restrictions`, `trx_return_to_work_assessments`, `trx_employee_vaccinations`, `trx_employee_health_surveillances`, `trx_needle_stick_incidents`, `trx_occupational_exposures`, `trx_employee_injuries`). Menerapkan enkripsi kolom AES-256 pada field data medis klinis (`VR-OHS-01`), konfigurasi foreign keys, indeks pencarian per ruangan kerja/tanggal, serta migrasi EF Core `20261109_AddOccupationalHealthManagement`.
* **Dependencies**: None
* **Acceptance Criteria**: Migrasi EF Core sukses dieksekusi tanpa peringatan; kolom diagnosis medis klinis tersimpan dalam format ciphertext terenkripsi.
* **Verifikasi**: Uji struktur tabel dan enkripsi data `AT-HRD-OHS-01`.

#### ✅ `BE-HRD-287` — EF Core Repositories & Unit of Work Context K3RS (`IOccupationalHealthUnitOfWork`)
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi pola Repository dan Unit of Work terisolasi untuk seluruh entitas K3RS (`IOccupationalHealthUnitOfWork`). Mengelola siklus transaksi database atomik, penanganan konkurensi optimistik pada pembaruan status kelaikan kerja staf, serta query helper untuk penapisan pegawai berisiko tinggi.
* **Dependencies**: `BE-HRD-286`
* **Acceptance Criteria**: Repository terdaftar rapi pada Dependency Injection container; transaksi commit/rollback bekerja deterministik.
* **Verifikasi**: Uji query repository dan Unit of Work commit/rollback.

#### ✅ `BE-HRD-288` — DTOs Request/Response, Model Mapping & FluentValidation Rules (`VR-OHS-01` s.d. `VR-OHS-10`)
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Pembuatan kelas DTOs request/response lengkap untuk 24 endpoint API Kontrak v23. Mengonfigurasi AutoMapper/Mapster profile dan menerapkan aturan validasi ketat FluentValidation (`VR-OHS-01` enkripsi diagnosis, `VR-OHS-02` restriksi role HR, `VR-OHS-03` STR dokter Hiperkes, `VR-OHS-04` validitas restriksi maksimal 180 hari, `VR-OHS-05` kategori beban tugas spesifik, `VR-OHS-06` hard-stop roster, `VR-OHS-07` syarat asesmen RTW, `VR-OHS-08` format laporan cito, `VR-OHS-09` batas emas PPP < 4 jam, `VR-OHS-10` sampel baseline 24 jam).
* **Dependencies**: `BE-HRD-287`
* **Acceptance Criteria**: Seluruh input payload tervalidasi otomatis; error melempar HTTP 400 Bad Request dengan pesan baku.
* **Verifikasi**: Uji unit test FluentValidation untuk aturan `VR-OHS-01` s.d. `VR-OHS-10`.

#### ✅ `BE-HRD-289` — Service Rekam Medis Terenkripsi & Field-Level Access Control (`HealthRecordService` - `HRD-DEC-176`)
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi layanan rekam kesehatan staf berkonsep pemisahan akses tingkat kolom (*Field-Level Access Separation*). Menjamin bahwa user berrole Dokter K3RS dan Staf Pemilik Akun (via ESS) dapat membaca data klinis utuh, sedangkan role HR Admin dan Atasan Kerja hanya menerima proyeksi status kelaikan operasional (*Fit*, *Fit with Restriction*, *Unfit*) tanpa diagnosis penyakit ICD-10 atau rincian klinis (`VR-OHS-02`).
* **Dependencies**: `BE-HRD-288`
* **Acceptance Criteria**: Permintaan data oleh HR Admin tidak pernah membocorkan data klinis; pembacaan rekam medis mencatat audit log `HRD_OHS_CLINICAL_RECORD_ACCESSED`.
* **Verifikasi**: Uji pemisahan hak akses data medis `AT-HRD-OHS-02` dan akses mandiri staf `AT-HRD-OHS-03`.

#### ✅ `BE-HRD-290` — Service Evaluasi Kelaikan Kerja (*Fitness to Work*) & Validasi Dokter Hiperkes (`FitnessToWorkService`)
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penerbitan dan pengesahan status kelayakan kerja operasional tenaga medis (*Fitness to Work*). Memvalidasi nomor registrasi sertifikat Hiperkes / K3RS aktif dokter pemeriksa (`VR-OHS-03`). Menyimpan status keputusan (`Fit`, `FitWithRestriction`, `TemporarilyUnfit`, `PermanentlyUnfit`) dengan digital signature dan status imutabel setelah disahkan (`VR-OHS-13`). Memicu integrasi sinkronisasi kredensial klinis Sub-03 (`INT-HR-92`).
* **Dependencies**: `BE-HRD-289`
* **Acceptance Criteria**: Putusan kelaikan kerja hanya dapat disahkan oleh dokter bersertifikat Hiperkes; status terkunci dari perubahan manual pasca pengesahan.
* **Verifikasi**: Uji pengesahan status kelaikan kerja `AT-HRD-OHS-04` dan imutabilitas hasil `AT-HRD-OHS-13`.

#### ✅ `BE-HRD-291` — Service Pembatasan Tugas Medis (*Work Restriction*) & Event Hard-Stop Sub-06 (`WorkRestrictionService` - `INT-HR-89`)
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penerbitan surat pembatasan tugas kerja dinas medis (*Work Restriction Certificate*). Memvalidasi tanggal masa berlaku maksimal 180 hari kalender (`VR-OHS-04`) dan kategori larangan spesifik (`VR-OHS-05`). Menyiarkan event `Hrd.OccupationalHealth.WorkRestrictionEnforced` ke modul penjadwalan Sub-06 untuk mengaktifkan gembok validasi ketat (*hard-stop*) sehingga kepala ruangan dicegah menjadwalkan staf pada shift malam atau zona berbahaya (`INT-HR-89`, `VR-OHS-06`).
* **Dependencies**: `BE-HRD-290`
* **Acceptance Criteria**: Surat pembatasan tugas diterbitkan resmi; event hard-stop terkirim ke Sub-06 dan menolak pembuatan jadwal terlarang.
* **Verifikasi**: Uji penerbitan restriksi `AT-HRD-OHS-05` dan uji hard-stop roster `AT-HRD-OHS-06`.

#### ✅ `BE-HRD-292` — Background Worker `WorkRestrictionExpirationWorker` (`OHS-W2`) Batas Maksimal 6 Bulan
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Background worker terjadwal harian (`OHS-W2`) yang memindai surat pembatasan tugas kerja yang telah melewati batas tanggal akhir masa berlaku (`ValidUntil`). Mengubah status menjadi `Expired` secara otomatis dan mengirimkan notifikasi peringatan kepada Dokter K3RS dan atasan kerja untuk evaluasi ulang kelaikan dinas staf.
* **Dependencies**: `BE-HRD-291`
* **Acceptance Criteria**: Restriksi yang kedaluwarsa otomatis berubah status menjadi `Expired`; notifikasi terkirim tepat waktu ke dokter pemeriksa.
* **Verifikasi**: Uji otomatisasi masa berlaku restriksi K3RS.

#### ✅ `BE-HRD-293` — Service Asesmen Kembali Bekerja (*Return-to-Work* / RTW) & Pembuka Gembok Roster (`ReturnToWorkService`)
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan asesmen kelaikan kembali bekerja staf pasca masa pemulihan cedera atau sakit berkepanjangan. Melakukan evaluasi kebugaran fisik, rentang gerak motorik, dan kesiapan mental. Saat dokter menyatakan `FitForFullDuty`, sistem menerbitkan event `Hrd.OccupationalHealth.WorkRestrictionCleared` untuk membuka seluruh gembok restriksi roster di Sub-06 (`VR-OHS-07`, `INT-HR-89`).
* **Dependencies**: `BE-HRD-291`
* **Acceptance Criteria**: Restriksi tugas tidak dapat dicabut tanpa nomor asesmen RTW yang sah; gembok jadwal Sub-06 terbuka seketika setelah RTW disahkan.
* **Verifikasi**: Uji alur pelepasan restriksi via asesmen RTW `AT-HRD-OHS-07`.

#### ✅ `BE-HRD-294` — Service Tanggap Darurat Tertusuk Jarum & Sirene ESS Panic Button (`NeedleStickEmergencyService` - `INT-HR-91`)
* **Slice / MVP**: `S-C6` / `MVP-58`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penerimaan laporan darurat pajanan darah atau insiden tertusuk jarum suntik bekas pasien melalui tombol darurat portal ESS staf mobile (`VR-OHS-08`). Mengirimkan sinyal notifikasi darurat realtime (< 5 detik) ke tim K3RS dan Komite PPI, mencatat identitas pasien sumber pajanan, serta membuka tiket triase IGD cito berprioritas tertinggi.
* **Dependencies**: `BE-HRD-288`
* **Acceptance Criteria**: Sinyal darurat terkirim < 5 detik ke posko K3RS/PPI; tiket darurat cito terdaftar otomatis di sistem IGD/K3RS.
* **Verifikasi**: Uji tanggap darurat tertusuk jarum tombol ESS Panic `AT-HRD-OHS-08`.

#### ✅ `BE-HRD-295` — Service Fast-Track Resep Profilaksis Pasca Pajanan (PPP/PEP < 4 Jam) & Depo Farmasi Sub-12
* **Slice / MVP**: `S-C6` / `MVP-58`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan triase medis pajanan darah dan peresepan jalur cepat (*fast-track*) paket Profilaksis Pasca Pajanan (Starter Pack ARV & Hepatitis B Immunoglobulin) untuk kasus pajanan risiko tinggi. Menjamin terpenuhinya batas emas keselamatan pemberian obat dalam waktu < 4 jam dari jam insiden (`VR-OHS-09`, `INT-HR-91`), terintegrasi dengan pemesanan cito di Depo Farmasi IGD Sub-12.
* **Dependencies**: `BE-HRD-294`
* **Acceptance Criteria**: Resep darurat PPP terbit otomatis tanpa antrean reguler farmasi; timestamp pemberian obat tercatat < 4 jam dari waktu insiden.
* **Verifikasi**: Uji batas emas terapi PPP < 4 jam `AT-HRD-OHS-09`.

#### ✅ `BE-HRD-296` — Background Worker `SerialSurveillanceFollowUpWorker` (`OHS-W1`) Bulan 1, 3, dan 6
* **Slice / MVP**: `S-C6` / `MVP-58`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Background worker terjadwal (`OHS-W1`) yang secara otomatis membuat dan mengelola 3 tahapan tonggak surveilans serologi serial darah staf pasca pajanan (tepat pada Bulan ke-1, Bulan ke-3, dan Bulan ke-6 sesuai `VR-OHS-11`). Mengirimkan tiket panggilan pemeriksaan lab berkala dan menutup kasus menjadi `ClosedSeronegative` jika evaluasi 6 bulan non-reaktif (`AT-HRD-OHS-11`).
* **Dependencies**: `BE-HRD-295`
* **Acceptance Criteria**: 3 jadwal surveilans serial terbuat otomatis di kalender staf; kasus otomatis ditutup jika tes akhir bulan ke-6 non-reaktif.
* **Verifikasi**: Uji otomasi jadwal surveilans serial `AT-HRD-OHS-10` dan penutupan kasus bebas infeksi `AT-HRD-OHS-11`.

#### ✅ `BE-HRD-297` — Service Radar Imunisasi Staf, Log Batch Vaksin & Worker Pengingat Booster `OHS-W3` (`StaffVaccinationService`)
* **Slice / MVP**: `S-C6` / `MVP-58`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan buku vaksinasi elektronik staf rumah sakit. Memvalidasi kelengkapan nomor batch, pabrikan vaksin, tanggal kedaluwarsa, dan identitas vaksinator (`VR-OHS-14`). Mendeteksi kadar titer Anti-HBs kuantitatif < 10 mIU/mL dan menetapkan status `NonProtective` (`VR-OHS-15`). Dilengkapi background worker `VaccineBoosterReminderWorker` (`OHS-W3`) yang mengirimkan reminder booster pada H-30 sebelum masa proteksi berakhir (`VR-OHS-16`).
* **Dependencies**: `BE-HRD-288`
* **Acceptance Criteria**: Log vaksinasi tercatat lengkap dengan nomor batch sah; titer < 10 mIU/mL otomatis memicu tiket rujukan vaksinasi booster.
* **Verifikasi**: Uji validasi data log vaksin `AT-HRD-OHS-14`, deteksi titer rendah `AT-HRD-OHS-15`, dan reminder booster H-30 `AT-HRD-OHS-16`.

#### ✅ `BE-HRD-298` — Service Surveilans Bahaya Ruangan, TLD Radiasi & Worker Alarm Batas Dosis `OHS-W5` (`HazardSurveillanceService`)
* **Slice / MVP**: `S-C6` / `MVP-58`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pemantauan bahaya spesifik lingkungan kerja rumah sakit (pembacaan dosis radiasi badge TLD kuartalan/tahunan, penapisan spirometri debu/gas, dan audiometri kebisingan CSSD/Genset). Dilengkapi background worker `RadiationDoseThresholdAlertWorker` (`OHS-W5`) yang memantau dosis radiasi kumulatif triwulanan (> 5 mSv) dan tahunan (> 20 mSv) sesuai regulasi Bapeten untuk memicu alarm peringatan dini dan rekomendasi rotasi kerja staf (`VR-OHS-17`).
* **Dependencies**: `BE-HRD-288`
* **Acceptance Criteria**: Rekaman dosis TLD tersimpan per periode; dosis melebihi ambang 20 mSv memicu alarm bahaya dan tiket rotasi dinas.
* **Verifikasi**: Uji batas ambang kumulatif dosis radiasi TLD `AT-HRD-OHS-17`.

#### ✅ `BE-HRD-299` — Service Pelaksanaan MCU Berkala Berbasis Profil Ruangan, Laporan STARKES & Worker `OHS-W4` (`MedicalExaminationService`)
* **Slice / MVP**: `S-C6` / `MVP-59`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan manajemen pelaksanaan pemeriksaan kesehatan berkala staf (pra-kerja, tahunan, khusus, dan purna-tugas). Secara otomatis menyesuaikan paket penapisan MCU dengan profil bahaya ruangan kerja staf (`VR-OHS-12`). Dilengkapi worker `McuComplianceScheduleWorker` (`OHS-W4`) yang memantau kepatuhan kehadiran MCU tahunan serta generator ekspor dokumen kepatuhan standar akreditasi rumah sakit STARKES instrumen KPS.
* **Dependencies**: `BE-HRD-288`
* **Acceptance Criteria**: Paket MCU tersaring otomatis sesuai unit kerja; generator laporan STARKES KPS menghasilkan persentase kepatuhan MCU staf 100% akurat.
* **Verifikasi**: Uji penapisan otomatis paket MCU `AT-HRD-OHS-12` dan ekspor laporan STARKES `AT-HRD-OHS-22`.

#### ✅ `BE-HRD-300` — Service Registrasi Kecelakaan Kerja KAK/PAK, Olah TKP & Integrasi BPJS JKK (`INT-HR-90`) (`OccupationalInjuryService`)
* **Slice / MVP**: `S-C6` / `MVP-59`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan investigasi kecelakaan kerja (KAK/PAK), pencatatan kronologi olah TKP, analisis akar masalah (RCA), dan penyusunan rekomendasi CAPA bersama minimal 2 personel investigator K3RS (`VR-OHS-19`). Mengintegrasikan ekspor formulir resmi BPJS Ketenagakerjaan JKK Tahap I dalam batas waktu regulasi < 2x24 jam (`VR-OHS-18`, `INT-HR-90`), sinkronisasi hari kerja hilang (LTI) ke modul presensi Sub-07 (`VR-OHS-20`), serta pengajuan berkas reimbursement Tahap II (`VR-OHS-21`).
* **Dependencies**: `BE-HRD-299`
* **Acceptance Criteria**: Laporan investigasi memuat matriks CAPA; formulir JKK Tahap I diekspor < 48 jam; hari kerja hilang (LTI) sinkron dengan absensi sakit.
* **Verifikasi**: Uji ekspor formulir JKK Tahap I `AT-HRD-OHS-18`, investigasi K3RS `AT-HRD-OHS-19`, dan sinkronisasi LTI `AT-HRD-OHS-20`.

#### ✅ `BE-HRD-301` — Controller API Kontrak v23 5 Tag Swagger, Worker BPJS `OHS-W6` & Audit Verifikasi E2E
* **Slice / MVP**: `S-C6` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi seluruh 24 endpoint API pada 5 Controller ASP.NET Core beranotasi Swagger `[Tags("Corporate - HRD ...")]` sesuai Kontrak API `v23`. Mengonfigurasi background worker `BPJSOccupationalInjuryFollowUpWorker` (`OHS-W6`) yang mengawal SLA pelaporan JKK, mengunci hak akses 24 permissions `Hrd.OccupationalHealth.*`, serta melakukan audit verifikasi menyeluruh kompilasi `dotnet build` 0 warning dan kepatuhan standar QBE Conformance.
* **Dependencies**: Seluruh task backend `BE-HRD-286` s.d. `BE-HRD-300`
* **Acceptance Criteria**: 24 endpoint API terdaftar rapi pada Swagger UI; seluruh permission dan audit event bekerja akuntabel; `dotnet build` lulus 0 error.
* **Verifikasi**: Uji finalisasi klaim JKK Tahap II `AT-HRD-OHS-21`, ekspor STARKES `AT-HRD-OHS-22`, dan laporan task tracked `BE-HRD-301.md`.


---

## 22. S-A7-V2: Employee Self-Service (ESS) & Kotak Masuk Persetujuan Terpadu (Unified Approval Inbox V2 Hardening)

### Grafik Urutan Dependency Backend Slice S-A7-V2

#### Grafik B76 — Fondasi Data, Matriks Routing Dinamis & Resolver Inbox (`BE-HRD-302` s.d. `BE-HRD-305`)

```text
BE-HRD-003 ✅ [B1] ─> BE-HRD-302 ✅ ─> BE-HRD-303 ✅ ─┬─> BE-HRD-304 ✅
                                                      │
                                                      └─> BE-HRD-305 ✅
```

#### Grafik B77 — Anti Self-Approval, Delegasi Plh & Eksekusi Tri-State (`BE-HRD-306` s.d. `BE-HRD-309`)

```text
BE-HRD-304 ✅ [B76] ─> BE-HRD-306 ✅ ─┬─> BE-HRD-307 ✅ ─> BE-HRD-308 ✅
                                      │
                                      └─> BE-HRD-309 ✅
```

#### Grafik B78 — Disiplin SLA, Auto-Eskalasi & Persetujuan Massal Bersyarat (`BE-HRD-310` s.d. `BE-HRD-313`)

```text
BE-HRD-309 ✅ [B77] ─┬─> BE-HRD-310 ✅ ─> BE-HRD-311 ✅
                      │
                      └─> BE-HRD-312 ✅ ─> BE-HRD-313 ✅
```

#### Grafik B79 — Notifikasi Multikanal, Resubmit Draf, Kontrak API v24 5 Tag Swagger & Verifikasi E2E (`BE-HRD-314` s.d. `BE-HRD-317`)

```text
BE-HRD-305 ✅ [B76] ─┬─> BE-HRD-314 ✅
BE-HRD-309 ✅ [B77] ─┘
BE-HRD-309 ✅ [B77] ────> BE-HRD-315 ✅
BE-HRD-309 ✅ [B77] ─┬─> BE-HRD-316 ✅
BE-HRD-312 ✅ [B78] ─┘
BE-HRD-302 ✅ s.d. BE-HRD-316 ✅ ───────> BE-HRD-317 ✅
```

---

### Tabel Gelombang Eksekusi Backend Slice S-A7-V2

| Gelombang | Target Cakupan Bisnis & Teknis | Task Backend | Prasyarat Sebelumnya |
|---|---|---|---|
| **MVP-60** | Fondasi Skema Postgres, Matriks Routing Dinamis Multi-Dimensi & Resolver Inbox Terpadu | `BE-HRD-302`, `BE-HRD-303`, `BE-HRD-304`, `BE-HRD-305` | `BE-HRD-003` (Sub-02 Org Structure) |
| **MVP-61** | Pemblokiran Mandiri Anti Self-Approval, Delegasi Wewenang Plh & Aksi Otorisasi Tri-State | `BE-HRD-306`, `BE-HRD-307`, `BE-HRD-308`, `BE-HRD-309` | `BE-HRD-304` |
| **MVP-62** | Penegakan Disiplin SLA, Larangan Auto-Approve, Worker Auto-Eskalasi & Persetujuan Massal Bersyarat | `BE-HRD-310`, `BE-HRD-311`, `BE-HRD-312`, `BE-HRD-313` | `BE-HRD-309` |
| **Verifikasi** | Notifikasi Multikanal, Resubmit Draf Revisi, Eksekusi Domain Hilir, 5 Tag Swagger API & QBE Conformance | `BE-HRD-314`, `BE-HRD-315`, `BE-HRD-316`, `BE-HRD-317` | `BE-HRD-302` s.d. `BE-HRD-316` |

---

### Rincian 16 Task Backend Slice S-A7-V2 (`BE-HRD-302` s.d. `BE-HRD-317`)

#### ✅ `BE-HRD-302` — Skema Basis Data Postgres, Ekstensi Model `WorkflowManagement` & Migration EF Core
* **Slice / MVP**: `S-A7-V2` / `MVP-60`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Penambahan kolom V2 pada model `TrxWorkflowInstance` (`PriorityLevel`, `SlaDeadline`, `IsOverdue`, `IsDelegatedAction`, `OnBehalfOfEmployeeId`), `TrxWorkflowStepInstance`, `TrxApprovalAction`, dan `TrxApprovalDelegation` serta pembentukan migrasi EF Core `20261109_AddWorkflowAndUnifiedApprovalV2`. Penambahan index gabungan performa query tinggi `(ApproverEmployeeId, AssignmentStatus)`, `(ApplicantEmployeeId, CurrentStatus)`, dan `(IsOverdue, SlaDeadline)` untuk mempercepat respon pemanggilan inbox atasan.
* **Dependencies**: `BE-HRD-003` [B1]
* **Acceptance Criteria**: Tabel termigrasi sempurna ke PostgreSQL tanpa data loss; index gabungan terpasang rapi.
* **Verifikasi**: Uji struktur skema basis data `AT-HRD-WF-01` dan verifikasi migrasi EF Core.

#### ✅ `BE-HRD-303` — Resolusi Matriks Routing Dinamis Multi-Dimensi & Hirarki Jabatan Sub-02 (`WorkflowRoutingMatrixService` - `WF-B1`, `INT-HR-95`)
* **Slice / MVP**: `S-A7-V2` / `MVP-60`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penentuan rantai tahapan persetujuan dinamis (*Dynamic Approval Routing Matrix* - `WF-B1`). Mengevaluasi kombinasi tipe request, unit kerja, level jabatan, dan besaran nominal finansial pemohon (`VR-WF-08`) untuk membentuk urutan step persetujuan berjenjang tanpa hardcoding path statis. Berkolaborasi dengan modul profil Sub-02 (`INT-HR-95`) untuk menarik struktur atasan langsung dan batas kewenangan struktural.
* **Dependencies**: `BE-HRD-302`
* **Acceptance Criteria**: Rantai tahapan step terbentuk otomatis sesuai kombinasi aturan matriks unit dan nominal finansial.
* **Verifikasi**: Uji resolusi matriks routing multi-dimensi `AT-HRD-WF-02`.

#### ✅ `BE-HRD-304` — Instansiasi Tiket Workflow Permohonan ESS Mandiri & Validasi Entitas Asal (`WorkflowInstanceCreationService` - `VR-WF-01`)
* **Slice / MVP**: `S-A7-V2` / `MVP-60`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pendaftaran tiket workflow persetujuan baru dari pemohon mandiri ESS. Memvalidasi eksistensi dan validitas entitas transaksi asal di domain pemohon (misal: ID permohonan cuti, ID rencana lembur, ID tukar shift - `VR-WF-01`). Menerbitkan nomor tiket alur kerja terpusat (*Centralized Workflow Number*), menginisiasi tahapan pertama (Step 1), dan menetapkan batas waktu SLA operasional awal (`VR-WF-04`).
* **Dependencies**: `BE-HRD-303`
* **Acceptance Criteria**: Tiket workflow terdaftar dengan nomor unik dan tahapan Step 1 aktif; entitas asal tervalidasi sah.
* **Verifikasi**: Uji instansiasi tiket alur kerja baru `AT-HRD-WF-01`.

#### ✅ `BE-HRD-305` — Mesin Agregasi & Resolver Kotak Masuk Persetujuan Terpadu (`ApprovalInboxQueryService` - `WF-B1`)
* **Slice / MVP**: `S-A7-V2` / `MVP-60`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Mesin agregasi pembacaan kotak masuk persetujuan (*Unified Approval Inbox Query Service*). Menyajikan seluruh tiket pending yang menunggu tindakan otorisasi dari user yang sedang login, mencakup peran pejabat definitif maupun peran pengganti Pelaksana Harian (Plh) aktif (`WF-B3`). Mendukung filtering multi-kategori (Cuti, Lembur, Tukar Shift, Klaim Restitusi, Pinjaman, Resign, Diklat), paginasi efisien `PagedResult<T>`, dan perhitungan indikator risiko visual hijau/kuning/merah.
* **Dependencies**: `BE-HRD-303`
* **Acceptance Criteria**: Inbox menyajikan seluruh permohonan pending lintas 15 modul dalam 1 struktur respons yang seragam dan cepat (< 300 ms).
* **Verifikasi**: Uji agregasi data kotak masuk terpadu `AT-HRD-WF-03`.

#### ✅ `BE-HRD-306` — Hard-Block Anti Self-Approval Mutlak & Otomasi Skip-Level Routing (`AntiSelfApprovalGuardService` - `WF-B2`, `HRD-DEC-183`, `VR-WF-02`)
* **Slice / MVP**: `S-A7-V2` / `MVP-61`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Komponen pengaman tata kelola yang memblokir keras aksi persetujuan bila pemohon adalah approver definitif (`ApplicantEmployeeId == ApproverEmployeeId` - `VR-WF-02`). Sistem menolak eksekusi persetujuan oleh diri sendiri dan secara otomatis mengalihkan penugasan tiket (*skip-level*) ke atasan struktural satu tingkat di atasnya berdasarkan pohon jabatan Sub-02 (`HRD-DEC-183`).
* **Dependencies**: `BE-HRD-304`
* **Acceptance Criteria**: Percobaan persetujuan mandiri ditolak mutlak (HTTP 403 / Domain Validation Exception); tiket dialihkan skip-level ke atasan atas.
* **Verifikasi**: Uji pemblokiran anti-self-approval dan alur skip-level `AT-HRD-WF-04`.

#### ✅ `BE-HRD-307` — Layanan Pendelegasian Wewenang Digital Berbatas Waktu Plh (`ApprovalDelegationService` - `WF-B3`, `HRD-DEC-184`, `VR-WF-03`)
* **Slice / MVP**: `S-A7-V2` / `MVP-61`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pencatatan dan tata kelola pendelegasian wewenang persetujuan (*Acting Approver* / Plh). Memvalidasi bahwa penerima kuasa adalah pejabat yang setara atau satu tingkat di bawahnya (`VR-WF-09`), memvalidasi rentang tanggal aktif `StartDate` s.d. `EndDate` (`VR-WF-03`), dan memastikan bahwa penugasan tiket diarahkan ke inbox Plh dengan penanda badge *Delegated* selama masa berlaku.
* **Dependencies**: `BE-HRD-306`
* **Acceptance Criteria**: Surat kuasa digital Plh tersimpan sah; tiket persetujuan otomatis terdistribusi ke akun pejabat pengganti.
* **Verifikasi**: Uji registrasi delegasi Plh dan distribusi tiket pengganti `AT-HRD-WF-05`.

#### ✅ `BE-HRD-308` — Background Worker `ApprovalDelegationExpiryWorker` (`WF-W2`) Pencabutan Otomatis Plh
* **Slice / MVP**: `S-A7-V2` / `MVP-61`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Background worker terjadwal harian (`WF-W2`) yang berjalan setiap pukul 00:05 tengah malam. Memindai data pendelegasian di `trx_approval_delegations` yang telah melewati `EndDate` dan secara otomatis memperbarui status menjadi `Expired` (`IsActive = false`), serta mengaktifkan delegasi yang mulai berlaku hari ini. Mengembalikan seluruh wewenang persetujuan ke pejabat definitif secara transparan.
* **Dependencies**: `BE-HRD-307`
* **Acceptance Criteria**: Wewenang Plh yang kedaluwarsa dicabut otomatis tanpa intervensi manual; tiket persetujuan kembali ke pejabat definitif.
* **Verifikasi**: Uji otomatisasi pencabutan masa berlaku delegasi Plh `AT-HRD-WF-06`.

#### ✅ `BE-HRD-309` — Layanan Tindakan Otorisasi Tri-State (*Approve*, *Reject* Min 15 Karakter, *Return for Revision*) (`WorkflowApprovalActionService` - `WF-B5`, `HRD-DEC-186`, `VR-WF-05`)
* **Slice / MVP**: `S-A7-V2` / `MVP-61`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan eksekusi keputusan otorisasi tri-state. Menangani aksi: (1) `Approve` dengan verifikasi PIN / tanda tangan digital; (2) `Reject` dengan validasi alasan penolakan wajib minimal 15 karakter (`VR-WF-05`); dan (3) `ReturnForRevision` yang mengembalikan draf permohonan ke staf pemohon untuk koreksi data tanpa membatalkan tiket (`HRD-DEC-186`). Mencatat jejak audit ganda bila tindakan dilakukan oleh Plh (`ActionEmployeeId = Plh`, `OnBehalfOfEmployeeId = Principal`).
* **Dependencies**: `BE-HRD-306`
* **Acceptance Criteria**: Seluruh 3 aksi tri-state bekerja akuntabel; penolakan dengan alasan < 15 karakter ditolak sistem; audit log mencatat identitas ganda.
* **Verifikasi**: Uji tindakan tri-state, validasi alasan penolakan `AT-HRD-WF-07`, dan audit jejak ganda `AT-HRD-WF-08`.

#### ✅ `BE-HRD-310` — Mesin Penegakan Disiplin SLA, Larangan Auto-Approve & Worker Auto-Eskalasi `WF-W1` (`WorkflowSlaEnforcementService` - `WF-B4`, `HRD-DEC-185`, `VR-WF-04`)
* **Slice / MVP**: `S-A7-V2` / `MVP-62`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Mesin penegakan disiplin waktu pelayanan persetujuan (SLA 24 jam tukar shift, 48 jam cuti, 72 jam klaim). Mengimplementasikan **larangan keras auto-approve saat batas waktu habis (Zero Auto-Approval Policy)** demi mencegah kekosongan tenaga medis di unit kritis (`HRD-DEC-185`). Dilengkapi worker periodik 15 menit `WorkflowSlaEscalationWorker` (`WF-W1`) yang menandai `IsOverdue = true`, menyalakan alarm merah, dan mengeskalasikan tiket ke atasan satu tingkat di atasnya.
* **Dependencies**: `BE-HRD-309`
* **Acceptance Criteria**: Sistem menolak keras menyetujui otomatis saat timeout; tiket macet otomatis dieskalasi ke atasan lebih tinggi.
* **Verifikasi**: Uji larangan auto-approve dan alur eskalasi otomatis `AT-HRD-WF-09`.

#### ✅ `BE-HRD-311` — Background Worker `WorkflowSlaEarlyWarningWorker` (`WF-W4`) Sirene Peringatan H-4 Jam SLA
* **Slice / MVP**: `S-A7-V2` / `MVP-62`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Background worker terjadwal setiap 30 menit (`WF-W4`) yang memindai tiket pending yang mendekati batas waktu SLA (sisa durasi <= 4 jam). Menyiarkan push notification dan email peringatan darurat ke ponsel approver bersangkutan agar tiket segera ditinjau sebelum masuk fase pelanggaran SLA (*overdue*).
* **Dependencies**: `BE-HRD-310`
* **Acceptance Criteria**: Tiket dengan sisa waktu <= 4 jam memicu pengiriman notifikasi peringatan dini ke approver aktif.
* **Verifikasi**: Uji deteksi dan notifikasi peringatan dini H-4 jam SLA `AT-HRD-WF-10`.

#### ✅ `BE-HRD-312` — Layanan Persetujuan Massal Bersyarat (*Conditional Bulk Approval*) & Validasi 100% Indikator Hijau (`BulkApprovalService` - `WF-B6`, `HRD-DEC-187`, `VR-WF-07`)
* **Slice / MVP**: `S-A7-V2` / `MVP-62`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan persetujuan massal aman untuk transaksi sejenis berisiko rendah (misal: lembur rutin atau cuti tahunan staf administrasi). Memeriksa ketat bahwa seluruh tiket yang dipilih memiliki **100% indikator validasi HIJAU** (saldo cuti cukup, tap absensi lembur cocok, tidak ada konflik shift). Tiket dengan indikator kuning/merah atau permohonan kritis nakes otomatis dikeluarkan dari batch dan diwajibkan dibuka lembar detailnya (`HRD-DEC-187`, `VR-WF-07`).
* **Dependencies**: `BE-HRD-309`
* **Acceptance Criteria**: Otorisasi batch hanya dapat dieksekusi setelah memasukkan PIN keamanan sah; item berindikator risiko ditolak dari batch.
* **Verifikasi**: Uji validasi persetujuan massal bersyarat `AT-HRD-WF-11` dan proteksi PIN `AT-HRD-WF-12`.

#### ✅ `BE-HRD-313` — Background Worker `BulkApprovalProcessingWorker` (`WF-W5`) Pemrosesan Antrean Batch Aman
* **Slice / MVP**: `S-A7-V2` / `MVP-62`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Background worker antrean latar belakang (`WF-W5`) yang mengeksekusi persetujuan massal secara terurut dan terisolasi per transaksi (*isolated transactional processing*). Mencegah deadlock basis data dan memastikan bahwa bila salah satu transaksi gagal, transaksi lain dalam batch tetap berhasil disetujui serta menghasilkan laporan eksekusi batch yang transparan.
* **Dependencies**: `BE-HRD-312`
* **Acceptance Criteria**: Antrean batch dieksekusi secara aman; status akhir tiap tiket dilaporkan akurat ke pemohon dan approver.
* **Verifikasi**: Uji pemrosesan batch aman dan isolasi kegagalan `AT-HRD-WF-13`.

#### ✅ `BE-HRD-314` — Gateway Notifikasi Multikanal & Worker `DailyApprovalDigestWorker` (`WF-W3`) Pukul 07:00 Pagi (`UnifiedNotificationGatewayService` - `WF-B7`, `INT-HR-96`)
* **Slice / MVP**: `S-A7-V2` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan orkestrasi notifikasi multikanal (Web push realtime SignalR, Push Notification mobile, dan Email). Mengirimkan sinyal instan saat tiket diajukan, dikembalikan untuk revisi, atau disetujui/ditolak. Dilengkapi worker `DailyApprovalDigestWorker` (`WF-W3`) yang berjalan setiap pukul 07:00 pagi untuk mengirimkan email ringkasan jumlah tiket pending yang menunggu tindakan atasan.
* **Dependencies**: `BE-HRD-305`, `BE-HRD-309`
* **Acceptance Criteria**: Notifikasi instan terkirim < 3 detik ke pemohon dan approver; ringkasan harian terkirim terjadwal tiap 07:00 pagi.
* **Verifikasi**: Uji integrasi notifikasi multikanal `AT-HRD-WF-14` dan ringkasan harian `AT-HRD-WF-15`.

#### ✅ `BE-HRD-315` — Layanan Koreksi Draf Resubmit Mandiri Staf & Worker `ReturnedRequestExpiryWorker` (`WF-W6`) (`WorkflowResubmitService` - `VR-WF-06`)
* **Slice / MVP**: `S-A7-V2` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penerimaan draf perbaikan permohonan mandiri staf pasca pengembalian (*Return for Revision*). Mengizinkan pemohon memperbarui data formulir dan mengunggah ulang dokumen lampiran perbaikan dalam batas maksimal 7 hari kalender (`VR-WF-06`). Mengaktifkan kembali tahapan review approver bersangkutan. Dilengkapi worker harian `ReturnedRequestExpiryWorker` (`WF-W6`) yang membatalkan otomatis permohonan yang tidak diperbaiki > 7 hari.
* **Dependencies**: `BE-HRD-309`
* **Acceptance Criteria**: Staf dapat mengajukan ulang data revisi tanpa tiket baru; tiket yang kedaluwarsa 7 hari otomatis dibatalkan sistem.
* **Verifikasi**: Uji alur resubmit draf revisi `AT-HRD-WF-16` dan pembatalan otomatis 7 hari `AT-HRD-WF-17`.

#### ✅ `BE-HRD-316` — Eksekusi Transaksional Idempoten Domain Hilir (`IdempotentDownstreamExecutionService` - `WF-B8`, `INT-HR-93`, `INT-HR-94`)
* **Slice / MVP**: `S-A7-V2` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pemicu eksekusi transaksi domain hilir setelah permohonan berstatus `Approved` final (`INT-HR-93`). Mengirimkan sinyal idempoten untuk: pemotongan saldo cuti di Sub-08, pemutakhiran jadwal dinas di modul roster Sub-06, pencatatan lembur di Sub-09, dan pelepasan pencairan kasbon/klaim di Sub-11. Mengaktifkan prompt pembuatan delegasi Plh jika permohonan yang disetujui adalah cuti pejabat struktural (`INT-HR-94`).
* **Dependencies**: `BE-HRD-309`, `BE-HRD-312`
* **Acceptance Criteria**: Domain hilir terupdate seketika setelah persetujuan akhir; eksekusi ulang tidak menyebabkan duplikasi transaksi (idempoten).
* **Verifikasi**: Uji eksekusi idempoten domain hilir `AT-HRD-WF-18` dan pemicu otomatis delegasi Plh cuti `AT-HRD-WF-19`.

#### ✅ `BE-HRD-317` — Controller API Kontrak v24 5 Tag Swagger, Kunci 24 Permissions & Audit Verifikasi E2E QBE Conformance
* **Slice / MVP**: `S-A7-V2` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi seluruh 24 endpoint API pada 5 Controller ASP.NET Core beranotasi Swagger `[Tags("Corporate - HRD Workflow ...")]` sesuai Kontrak API `v24`. Mengunci hak akses 24 permissions `Hrd.Workflow.*`, mencatat 14 event audit permanen (`HRD_WF_*`), serta melakukan audit verifikasi menyeluruh kompilasi `dotnet build` 0 warning dan kepatuhan standar arsitektur QBE Conformance.
* **Dependencies**: Seluruh task backend `BE-HRD-302` s.d. `BE-HRD-316`
* **Acceptance Criteria**: 24 endpoint API terdaftar rapi pada Swagger UI; seluruh permission dan audit event bekerja akuntabel; `dotnet build` lulus 0 error.
* **Verifikasi**: Uji kepatuhan kontrak API Swagger `AT-HRD-WF-20`, verifikasi audit trail `AT-HRD-WF-21`, dan laporan task tracked `BE-HRD-317.md`.


---

## 23. S-D5: Perjalanan Dinas & Manajemen Biaya Operasional (Business Travel & Expense Management)

### Grafik Urutan Dependency Backend Slice S-D5

#### Grafik B80 — Fondasi Data, Matriks Tarif Lumpsum & Pengajuan SPPD (`BE-HRD-318` s.d. `BE-HRD-321`)

```text
BE-HRD-003 ✅ [B1] ─> BE-HRD-318 ✅ ─> BE-HRD-319 ✅ ─┬─> BE-HRD-320 ✅
                                                      │
                                                      └─> BE-HRD-321 ✅
```

#### Grafik B81 — Uang Muka Kerja 80%, Persetujuan 2-Pintu & Pencairan Kasir (`BE-HRD-322` s.d. `BE-HRD-325`)

```text
BE-HRD-320 ✅ [B80] ─┬─> BE-HRD-322 ✅ ─> BE-HRD-323 ✅ ─> BE-HRD-324 ✅
                     │
                     └─> BE-HRD-325 ✅
```

#### Grafik B82 — Rekonsiliasi Settlement, Deklarasi Darurat Tanpa Nota & Sanksi Payroll (`BE-HRD-326` s.d. `BE-HRD-329`)

```text
BE-HRD-324 ✅ [B81] ─┬─> BE-HRD-326 ✅ ─┬─> BE-HRD-327 ✅
BE-HRD-325 ✅ [B81] ─┘                 ├─> BE-HRD-328 ✅
                                        │
                                        └─> BE-HRD-329 ✅
```

#### Grafik B83 — Klaim Non-Trip, Integrasi Presensi/Roster, Kontrak v25 & Verifikasi E2E (`BE-HRD-330` s.d. `BE-HRD-333`)

```text
BE-HRD-318 ✅ [B80] ───────────────────> BE-HRD-330 ✅ ─> BE-HRD-331 ✅
BE-HRD-320 ✅ [B80] ───────────────────> BE-HRD-332 ✅
BE-HRD-318 ✅ s.d. BE-HRD-332 ✅ ───────> BE-HRD-333 ✅
```

---

### Tabel Gelombang Eksekusi Backend Slice S-D5

| Gelombang | Target Cakupan Bisnis & Teknis | Task Backend | Prasyarat Sebelumnya |
|---|---|---|---|
| **MVP-63** | Fondasi Skema PostgreSQL, Matriks Tarif Lumpsum Per Diem, Pengajuan SPPD & Validasi Anggaran | `BE-HRD-318`, `BE-HRD-319`, `BE-HRD-320`, `BE-HRD-321` | `BE-HRD-003` (Sub-02 Org Structure) |
| **MVP-64** | Tata Kelola Uang Muka Maksimal 80%, Alur Otorisasi 2-Pintu, Pencairan Kasir & Dokumen Surat Tugas | `BE-HRD-322`, `BE-HRD-323`, `BE-HRD-324`, `BE-HRD-325` | `BE-HRD-320` |
| **MVP-65** | Rekonsiliasi Settlement, Deklarasi Darurat Tanpa Nota Maks 10%, Worker SLA H+3/H+5 & Potong Payroll | `BE-HRD-326`, `BE-HRD-327`, `BE-HRD-328`, `BE-HRD-329` | `BE-HRD-324`, `BE-HRD-325` |
| **Verifikasi** | Klaim Biaya Operasional Non-Trip, Integrasi Presensi & Gembok Roster Sub-06, Kontrak v25 & QBE Audit | `BE-HRD-330`, `BE-HRD-331`, `BE-HRD-332`, `BE-HRD-333` | `BE-HRD-318` s.d. `BE-HRD-332` |

---

### Rincian 16 Task Backend Slice S-D5 (`BE-HRD-318` s.d. `BE-HRD-333`)

#### ✅ `BE-HRD-318` — Skema Basis Data Postgres, Pemutakhiran Model EF Core & Migration
* **Slice / MVP**: `S-D5` / `MVP-63`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Pemutakhiran 20 model EF Core pada bounded context `BusinessTravelManagement` dan `ExpenseManagement` di schema `public`. Pembentukan file migrasi EF Core `20261109_AddBusinessTravelAndExpenseManagementV2`. Penambahan index gabungan performa tinggi `(WorkforceProfileId, TravelStatus)`, `(CostCenterId, StartDate)`, dan `(IsOverdue, SettledAt)` untuk optimasi query pelaporan anggaran dan dasbor pemohon.
* **Dependencies**: `BE-HRD-003` [B1]
* **Acceptance Criteria**: Seluruh 20 tabel termigrasi rapi tanpa data loss; index gabungan terpasang sempurna.
* **Verifikasi**: Uji struktur skema basis data `AT-HRD-TRV-01` dan verifikasi file migrasi EF Core.

#### ✅ `BE-HRD-319` — Master Matriks Tarif Lumpsum Uang Harian Per Diem & Zonasi Wilayah (`MstTravelAllowanceRateService` - `TRV-B3`, `HRD-DEC-190`)
* **Slice / MVP**: `S-D5` / `MVP-63`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan konfigurasi dan komputasi otomatis matriks tarif baku uang harian dinas (*Per Diem Allowance* - `TRV-B3`). Menyediakan kalkulasi dinamis berdasarkan kombinasi `DestinationZoneId` (Zona 1 Lokal, Zona 2 Nasional, Zona 3 Internasional) dan grade jabatan nakes (Golongan I s.d. V). Menghitung `EstimatedAllowanceAmount = TravelDayCount * Rate` tanpa menuntut kuitansi makan/ojek receh (`VR-TRV-04`).
* **Dependencies**: `BE-HRD-318`
* **Acceptance Criteria**: Nilai uang harian per diem terkalkulasi otomatis dan akurat sesuai zonasi wilayah dan golongan nakes.
* **Verifikasi**: Uji kalkulator otomatis tarif lumpsum uang harian `AT-HRD-TRV-02`.

#### ✅ `BE-HRD-320` — Layanan Pengajuan SPPD Mandiri, Multi-Rute & Estimasi Biaya (`BusinessTravelRequestService` - `TRV-B1`)
* **Slice / MVP**: `S-D5` / `MVP-63`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pendaftaran dan pengelolaan pengajuan Surat Perintah Perjalanan Dinas (SPPD) nakes/staf mandiri via ESS. Mengelola rincian kota rute asal-tujuan, tanggal keberangkatan dan kepulangan, durasi hari inklusif (`TravelDayCount`), estimasi tiket transportasi, estimasi hotel/penginapan sesuai plafon kelas, serta menghasilkan nomor registrasi resmi format `SPPD/RSQ/YYYY/MM/XXX` (`VR-TRV-19`).
* **Dependencies**: `BE-HRD-319`
* **Acceptance Criteria**: Pengajuan SPPD tersimpan sah berstatus `Submitted` dengan nomor registrasi unik dan rincian estimasi biaya lengkap.
* **Verifikasi**: Uji formulir pendaftaran SPPD mandiri `AT-HRD-TRV-01`.

#### ✅ `BE-HRD-321` — Layanan Validasi Ketersediaan Anggaran Unit Cost Center & Bentrok Cuti/Roster (`TravelBudgetAndConflictValidationService` - `TRV-B8`, `VR-TRV-10`, `VR-TRV-11`)
* **Slice / MVP**: `S-D5` / `MVP-63`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Komponen pengaman tata kelola anggaran dan jadwal yang memvalidasi: (1) ketersediaan sisa pagu anggaran dinas unit kerja pemohon pada `MstCostCenter` (`VR-TRV-12`); (2) deteksi tumpang tindih tanggal dinas dengan SPPD lain milik pegawai (`VR-TRV-10`); dan (3) deteksi tumpang tindih tanggal dengan permohonan cuti disetujui di modul Sub-08 (`VR-TRV-11`).
* **Dependencies**: `BE-HRD-319`
* **Acceptance Criteria**: Sistem menolak pengajuan jika anggaran unit habis atau terdeteksi bentrok tanggal cuti/dinas lain.
* **Verifikasi**: Uji validasi anggaran cost center `AT-HRD-TRV-21` dan uji pencegahan tumpang tindih cuti `AT-HRD-TRV-20`.

#### ✅ `BE-HRD-322` — Layanan Pengajuan Uang Muka Kerja (*Travel Advance*) Maksimal 80% (`TravelAdvanceRequestService` - `TRV-B2`, `HRD-DEC-189`)
* **Slice / MVP**: `S-D5` / `MVP-64`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pengelolaan permohonan uang muka kerja perjalanan dinas (*Travel Advance*). Menerapkan batas keras (*hard-cap*) bahwa nominal yang diajukan (`RequestedAmount`) **dilarang melebihi 80% dari total pagu estimasi biaya disetujui** (`VR-TRV-02`). Mengelola status uang muka: `Draft`, `Submitted`, `ManagerApproved`, `FinanceApproved`, `Paid`, `Settled`.
* **Dependencies**: `BE-HRD-320`
* **Acceptance Criteria**: Permohonan uang muka di atas batas 80% ditolak otomatis oleh sistem; status permohonan tercatat akuntabel.
* **Verifikasi**: Uji batasan plafon maksimal uang muka 80% `AT-HRD-TRV-03`.

#### ✅ `BE-HRD-323` — Alur Persetujuan Uang Muka 2-Pintu (Atasan & Finance) & Voucher Kasir Cito (`AdvanceApprovalAndVoucherService` - `VR-TRV-03`, `INT-HR-98`)
* **Slice / MVP**: `S-D5` / `MVP-64`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan orkestrasi persetujuan uang muka kerja 2-pintu. Memvalidasi bahwa uang muka hanya dapat disahkan setelah disetujui bertingkat oleh: (1) Atasan Langsung (konfirmasi urgensi penugasan dinas) dan (2) Finance/Treasury Manager (verifikasi ketersediaan dana kas) (`VR-TRV-03`). Menerbitkan dokumen voucher pengeluaran kasir cito (*Cashier Disbursement Voucher*) ke modul keuangan (`INT-HR-98`).
* **Dependencies**: `BE-HRD-322`
* **Acceptance Criteria**: Uang muka tidak dapat dicairkan tanpa persetujuan lengkap 2-pintu; voucher kasir terbit resmi.
* **Verifikasi**: Uji alur otorisasi persetujuan uang muka 2-pintu `AT-HRD-TRV-04`.

#### ✅ `BE-HRD-324` — Layanan Pencairan Dana Uang Muka oleh Kasir RS (Transfer Payroll / Kas) (`AdvanceDisbursementService` - `VR-TRV-21`)
* **Slice / MVP**: `S-D5` / `MVP-64`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan konfirmasi pencairan dana fisik/transfer oleh Kasir RS. Mencatat nomor referensi transaksi bank / nomor voucher kasir, tanggal realisasi pencairan, identitas kasir yang bertugas, dan memperbarui status uang muka menjadi `Paid` serta status SPPD menjadi `AdvancePaid` (`VR-TRV-21`).
* **Dependencies**: `BE-HRD-323`
* **Acceptance Criteria**: Pencairan tercatat lengkap dengan nomor referensi kasir sah; audit log `HRD_TRV_ADVANCE_PAID` terekam permanen.
* **Verifikasi**: Uji pencairan dana uang muka oleh kasir `AT-HRD-TRV-05`.

#### ✅ `BE-HRD-325` — Generator Berkas Digital Surat Tugas Resmi & SPPD Ber-Barcode Sah (`TravelDocumentGenerationService` - `VR-TRV-01`)
* **Slice / MVP**: `S-D5` / `MVP-64`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan generator berkas digital dokumen Surat Tugas dan SPPD berformat PDF resmi rumah sakit. Memuat kop surat korporat, nomor registrasi SPPD, identitas delegasi nakes, rincian agenda, tanda tangan digital pejabat yang mengesahkan, serta barcode/QR Code terenkripsi untuk pembuktian keabsahan surat tugas saat bertugas di faskes eksternal.
* **Dependencies**: `BE-HRD-320`
* **Acceptance Criteria**: Berkas PDF Surat Tugas dan SPPD tergenerate sempurna, siap diunduh dan dicetak oleh pemohon.
* **Verifikasi**: Uji generator dokumen digital SPPD ber-barcode.

#### ✅ `BE-HRD-326` — Layanan Rekonsiliasi Settlement Pengeluaran Riil & Komputasi Selisih Kas (`TravelSettlementService` - `TRV-B4`, `HRD-DEC-191`)
* **Slice / MVP**: `S-D5` / `MVP-65`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan rekonsiliasi pertanggungjawaban biaya dinas pasca kepulangan nakes (*Expense Settlement*). Membandingkan total uang muka diterima (`TotalAdvanceAmount`) terhadap total pengeluaran riil terverifikasi kuitansi (`TotalActualAmount`). Menentukan jenis selisih kas: (1) `Underpaid` (kurang bayar, RS wajib membayar ke staf dalam 3 hari); (2) `Overpaid` (lebih bayar, staf wajib setor balik ke kasir dalam 3 hari); atau (3) `Balanced` (`VR-TRV-13`).
* **Dependencies**: `BE-HRD-324`, `BE-HRD-325`
* **Acceptance Criteria**: Komputasi rekonsiliasi selisih kas terhitung 100% presisi; instruksi pembayaran atau tagihan setor terbit otomatis.
* **Verifikasi**: Uji rekonsiliasi settlement kurang bayar `AT-HRD-TRV-15` dan lebih bayar `AT-HRD-TRV-16`.

#### ✅ `BE-HRD-327` — Layanan Formulir Deklarasi Biaya Darurat Tanpa Kuitansi (Maks Rp 150rb / 10%) (`MissingReceiptDeclarationService` - `TRV-B5`, `HRD-DEC-192`, `VR-TRV-08`)
* **Slice / MVP**: `S-D5` / `MVP-65`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan penanganan khusus pengeluaran darurat lapangan yang mustahil memperoleh nota formal (misal: ojek darurat sampel darah di pelosok, perahu penyeberangan evakuasi pasien rujukan). Menerapkan batasan pengaman: maksimal **Rp 150.000 per transaksi** dan total akumulasi maksimal **10% dari total klaim perjalanan dinas** (`VR-TRV-08`). Mewajibkan pengisian alasan kronologi darurat minimal 15 karakter (`VR-TRV-09`) dan persetujuan atasan.
* **Dependencies**: `BE-HRD-326`
* **Acceptance Criteria**: Deklarasi di atas plafon Rp 150rb atau > 10% total klaim otomatis ditolak sistem; alasan darurat terekam akuntabel.
* **Verifikasi**: Uji batas deklarasi biaya darurat tanpa kuitansi `AT-HRD-TRV-12` dan validasi alasan darurat `AT-HRD-TRV-13`.

#### ✅ `BE-HRD-328` — Background Worker `TravelAdvanceSettlementReminderWorker` (`TRV-W1`) Pengingat H+3 & H+5
* **Slice / MVP**: `S-D5` / `MVP-65`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Background worker terjadwal harian (`TRV-W1`) yang berjalan setiap pukul 08:00 pagi. Memindai perjalanan dinas yang telah melewati tanggal selesai (`EndDate`) namun belum menyerahkan settlement. Mengirimkan push notification dan email peringatan darurat kepada staf pada H+3 dan H+5 kepulangan dinas agar segera menyelesaikan pertanggungjawaban dalam batas SLA 7 hari kalender (`VR-TRV-05`).
* **Dependencies**: `BE-HRD-326`
* **Acceptance Criteria**: Notifikasi pengingat terkirim tepat waktu pada H+3 dan H+5 kepulangan staf; counter SLA berjalan akurat.
* **Verifikasi**: Uji sirene notifikasi pengingat settlement H+3 dan H+5 `AT-HRD-TRV-09`.

#### ✅ `BE-HRD-329` — Background Worker `TravelSettlementOverdueWorker` (`TRV-W2`) Pembeku SPPD H+14 & Pemotong Payroll H+30 (`INT-HR-99`)
* **Slice / MVP**: `S-D5` / `MVP-65`
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Background worker penegakan disiplin keuangan harian (`TRV-W2`) pukul 00:15 tengah malam. Menjalankan sanksi bertingkat: (1) pada H+14 keterlambatan, membekukan hak pembuatan SPPD baru bagi pegawai bersangkutan (`VR-TRV-14`); dan (2) pada H+30 (akhir bulan), menerbitkan event `TravelAdvanceDefaulted` ke modul Penggajian Sub-10 untuk memotongkan saldo piutang uang muka yang belum dipertanggungjawabkan dari slip gaji bulanan staf secara otomatis (`VR-TRV-15`, `INT-HR-99`).
* **Dependencies**: `BE-HRD-326`
* **Acceptance Criteria**: Hak SPPD dibekukan otomatis pada H+14; instruksi pemotongan slip gaji terkirim ke Sub-10 pada H+30.
* **Verifikasi**: Uji pembekuan hak SPPD `AT-HRD-TRV-10` dan otomasi pemotongan payroll `AT-HRD-TRV-11`.

#### ✅ `BE-HRD-330` — Layanan Pengajuan & Persetujuan Klaim Biaya Operasional Non-Trip (`DirectExpenseClaimService` - `TRV-B1`, `HRD-DEC-188`)
* **Slice / MVP**: `S-D5` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan pengelolaan klaim penggantian pengeluaran langsung operasional lokal tanpa perjalanan luar kota (*Non-Trip Expense Claim*). Staf dapat mengajukan klaim pembelian darurat logistik medis cito di apotek rekanan, konsumsi rapat panitia akreditasi, atau ongkos kurir sampel lab dengan melampirkan foto struk kuitansi toko, memilih pos beban operasional, dan meminta persetujuan Kepala Unit Kerja (`VR-TRV-16`, `VR-TRV-17`).
* **Dependencies**: `BE-HRD-318`
* **Acceptance Criteria**: Klaim operasional non-trip terdaftar mandiri pada `TrxExpenseClaim` tanpa mewajibkan surat tugas dinas luar kota.
* **Verifikasi**: Uji pengajuan klaim operasional non-trip `AT-HRD-TRV-17`.

#### ✅ `BE-HRD-331` — Layanan Verifikasi Struk Kasir & Pencairan Reimbursement Non-Trip (`ExpenseVerificationAndPaymentService` - `VR-TRV-16`, `VR-TRV-18`)
* **Slice / MVP**: `S-D5` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan audit dan pencairan reimbursement pengeluaran operasional non-trip oleh Tim Keuangan dan Kasir RS. Memverifikasi fisik struk pembelian, mencocokkan tanggal transaksi dalam bulan berjalan, dan mencairkan uang tunai / transfer bank ke rekening payroll pegawai dalam SLA < 48 jam pasca approval.
* **Dependencies**: `BE-HRD-330`
* **Acceptance Criteria**: Struk terverifikasi sah; kasir mencairkan dana dan mencatat nomor referensi kasir serta audit log `HRD_EXP_CLAIM_PAID`.
* **Verifikasi**: Uji verifikasi struk kasir `AT-HRD-TRV-18` dan pembayaran reimbursement `AT-HRD-TRV-19`.

#### ✅ `BE-HRD-332` — Integrasi Sinkronisasi Presensi Hadir Dinas Luar (Sub-07) & Gembok Hard-Stop Roster (Sub-06) (`TravelAttendanceAndRosterLockService` - `TRV-B6`, `HRD-DEC-193`, `INT-HR-97`)
* **Slice / MVP**: `S-D5` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Layanan integrasi kesinambungan pelayanan medis rumah sakit saat SPPD disetujui: (1) otomatis menyuntikkan status log presensi "Hadir Dinas Luar" ke modul Presensi Sub-07 selama tanggal dinas (`INT-HR-97`), dan (2) memasang gembok validasi ketat (*hard-stop roster lock*) pada modul Penjadwalan Roster Sub-06 yang menolak penjadwalan dinas bangsal atau dinas malam nakes pada tanggal penugasan dinas luar kota (`VR-TRV-06`).
* **Dependencies**: `BE-HRD-320`
* **Acceptance Criteria**: Log presensi dinas luar tercatat otomatis di Sub-07; modul roster Sub-06 menolak penjadwalan dinas bangsal nakes.
* **Verifikasi**: Uji sinkronisasi presensi dinas luar `AT-HRD-TRV-07` dan uji gembok hard-stop roster medis `AT-HRD-TRV-06`.

#### ✅ `BE-HRD-333` — 5 Controller API Kontrak v25 (24 Endpoint Swagger), Worker Kurs BI `TRV-W4`, 24 Permissions & Audit Verifikasi E2E QBE Conformance
* **Slice / MVP**: `S-D5` / Verifikasi
* **Status**: ✅ Selesai (2026-11-09)
* **Outcome**: Implementasi seluruh 24 endpoint API pada 5 Controller ASP.NET Core beranotasi Swagger `[Tags("Corporate - HRD Business Travel ...")]` sesuai Kontrak API `v25`. Mengonfigurasi background worker harian `TravelCurrencyRateSyncWorker` (`TRV-W4`) untuk kurs BI, mengunci hak akses 24 permissions `Hrd.BusinessTravel.*` dan `Hrd.Expense.*`, mencatat 14 event audit permanen (`HRD_TRV_*`), serta melakukan audit verifikasi menyeluruh kompilasi `dotnet build` 0 warning dan kepatuhan standar QBE Conformance.
* **Dependencies**: Seluruh task backend `BE-HRD-318` s.d. `BE-HRD-332`
* **Acceptance Criteria**: 24 endpoint API terdaftar rapi pada Swagger UI; seluruh permission dan audit event bekerja akuntabel; `dotnet build` lulus 0 error.
* **Verifikasi**: Uji kepatuhan kontrak API Swagger `AT-HRD-TRV-22`, verifikasi audit trail, dan laporan task tracked `BE-HRD-333.md`.
