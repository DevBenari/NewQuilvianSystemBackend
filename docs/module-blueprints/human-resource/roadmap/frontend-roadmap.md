# Roadmap Delivery Frontend — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 16
status: ACTIVE
status_synced_at: "11 November 2026 — FE-HRD-R01: 137 task diturunkan ke 🟡 sesuai audit frontend-unfinished-work.md; status register FE-HRD-058 s.d. 087 diisi; FE-HRD-216 s.d. 305 ditambahkan ke register; FE-HRD-027 disamakan dengan laporannya (🟡)"
planning_updated_at: "2026-11-09 - Slice S-B3 Administrasi Lembur Pegawai Rumah Sakit (sub-modul 09) diselesaikan secara penuh (FE-HRD-116 s.d. FE-HRD-129 ✅ PASS) mencakup 6 rute antarmuka, 14 task frontend, verifikasi build, dan seluruh laporan perubahan tracked; sebelumnya Slice S-B2 (FE-HRD-102 s.d. FE-HRD-115), Slice S-B4 (FE-HRD-075 s.d. FE-HRD-087), Slice S-C4-ONB (FE-HRD-058 s.d. FE-HRD-074)"
generated_at: "2026-10-26"
owners:
  - "Frontend Technical Owner — HR UI Lead"
approved_by:
  - "User Approved (26 Oktober 2026) — roadmap awal"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 3 November 2026 — desain S-D1 yang menjadi dasar task FE-HRD-029 s.d. FE-HRD-043"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 3 November 2026 — desain S-D2 yang menjadi dasar task FE-HRD-044 s.d. FE-HRD-057"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 4 November 2026 — desain S-C4-ONB beserta lima pilihan desain (HRD-DEC-096) yang menjadi dasar task FE-HRD-058 s.d. FE-HRD-074"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 4 November 2026 — desain S-B4 Penjadwalan, Roster, dan On-Call (HRD-DEC-097 s.d. HRD-DEC-103) yang menjadi dasar task FE-HRD-075 s.d. FE-HRD-087"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 4 November 2026 — desain S-B1 Presensi dan Kehadiran (HRD-DEC-104 s.d. HRD-DEC-109) yang menjadi dasar task FE-HRD-088 s.d. FE-HRD-101"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 4 November 2026 — desain S-B2 Cuti & Izin Pegawai Rumah Sakit (HRD-DEC-110 s.d. HRD-DEC-115) yang menjadi dasar task FE-HRD-102 s.d. FE-HRD-115"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 5 November 2026 — desain S-B3 Lembur & Kompensasi Pegawai Rumah Sakit (HRD-DEC-116 s.d. HRD-DEC-121) yang menjadi dasar task FE-HRD-116 s.d. FE-HRD-129"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 5 November 2026 — desain S-B5 Pemrosesan Penggajian Pegawai Rumah Sakit (HRD-DEC-122 s.d. HRD-DEC-127) yang menjadi dasar task FE-HRD-130 s.d. FE-HRD-143"
blueprint_manifest_revision: 27
decision_ids_s_d1: "HRD-DEC-058 s.d. HRD-DEC-072"
source_commits:
  backend: "26066c76e69b021f68b63b31bde2258cd9d1bb74 (branch MHamzah)"
  frontend: "66b7e5600e1707e4153ca07981ddef054856c8c9 (branch HamzahV2) — HEAD saat roadmap S-D1 ditulis"
contract_versions:
  - "API Contract v6 — task FE-HRD-001 s.d. FE-HRD-028"
  - "API Contract v7 bagian 13 — task S-D1"
  - "Frontend Architecture rev 3 bagian 12 — task S-D1"
  - "Permission Audit Matrix v7 bagian 11 — task S-D1"
  - "Validation Matrix v7 bagian 12 — pesan yang ditampilkan layar S-D1"
locked_input_hashes_s_d1:
  03-frontend-architecture.md: 7c2db1244ccd3fdc7ce81c3b1db6220cbfd0fee9d6cdfc70a42f18b981705bda
  04-prd-to-mvp.md: 8db8cdcd95cf70b46afa35cf3a2a8691eb78681027645aebed8625a5aea1b6ea
  contracts/api-contract.md: 772901e44a9f69213e2ac63404045288bec61742eba73a816f5f43faf9a01328
  contracts/permission-audit-matrix.md: e41eca8c8f9c53b0dd5f10a2c294892128e0c00d8eb760f67a48dcb8a5fcb182
  contracts/validation-matrix.md: 27055a3971cea05d93075a527bc18acbeb911727f0fbdb3fa137a54aece5bd31
  testing/acceptance-test-matrix.md: ee5e5bb1e29a9519f76d5995021f11cfaa8384c4888471b442d076d0ef44db9b
decision_ids_s_d2: "HRD-DEC-073 s.d. HRD-DEC-080"
contract_versions_s_d2:
  - "API Contract v8 bagian 14 — task S-D2"
  - "Frontend Architecture rev 4 bagian 13 — task S-D2"
  - "Permission Audit Matrix v8 bagian 12 — task S-D2"
  - "Validation Matrix v8 bagian 13 — pesan yang ditampilkan layar S-D2"
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
source_commits_s_c4_onb: "BE 8b17b75a (MHamzah), FE 44990b3d3 (HamzahV2)"
contract_versions_s_c4_onb:
  - "API Contract v9 bagian 15 — task S-C4-ONB"
  - "Frontend Architecture rev 5 bagian 14 — task S-C4-ONB"
  - "Permission Audit Matrix v9 bagian 13 — task S-C4-ONB"
  - "Validation Matrix v9 bagian 14 — pesan yang ditampilkan layar S-C4-ONB"
decision_ids_s_b4: "HRD-DEC-097 s.d. HRD-DEC-103, HRD-ASM-05 s.d. HRD-ASM-14"
source_commits_s_b4: "BE 8b17b75a (MHamzah), FE 44990b3d3 (HamzahV2)"
contract_versions_s_b4:
  - "API Contract v10 bagian 16 — task S-B4"
  - "Frontend Architecture rev 5 bagian 15 — task S-B4"
  - "Permission Audit Matrix v10 bagian 14 — task S-B4"
  - "Validation Matrix v10 bagian 15 — pesan yang ditampilkan layar S-B4"
decision_ids_s_b1: "HRD-DEC-104 s.d. HRD-DEC-109, HRD-ASM-15 s.d. HRD-ASM-18"
source_commits_s_b1: "BE 8b17b75a (MHamzah), FE 44990b3d3 (HamzahV2)"
contract_versions_s_b1:
  - "API Contract v11 bagian 17 — task S-B1"
  - "Frontend Architecture rev 6 bagian 16 — task S-B1"
  - "Permission Audit Matrix v11 bagian 15 — task S-B1"
  - "Validation Matrix v11 bagian 16 — pesan yang ditampilkan layar S-B1"
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
decision_ids_s_b6: "HRD-DEC-128 s.d. HRD-DEC-133, HRD-ASM-31 s.d. HRD-ASM-34"
source_commits_s_b6:
  backend: "8b17b75a (branch MHamzah)"
  frontend: "44990b3d3 (branch HamzahV2)"
contract_versions_s_b6:
  - "API Contract v15 bagian 21 — task S-B6"
  - "State Transition Matrix v15 bagian 19 — task S-B6"
  - "Validation Matrix v15 bagian 20 — task S-B6"
  - "Permission Audit Matrix v15 bagian 19 — task S-B6"
  - "Integration Contract v15 bagian 12 — task S-B6"
  - "Data Dictionary v15 bagian 16 — task S-B6"
  - "Acceptance Test Matrix v15 bagian 23 — task S-B6"
decision_ids_s_b3: "HRD-DEC-116 s.d. HRD-DEC-121, HRD-ASM-23 s.d. HRD-ASM-26"
source_commits_s_b3: "BE 8b17b75a (MHamzah), FE 44990b3d3 (HamzahV2)"
contract_versions_s_b3:
  - "API Contract v13 bagian 19 — task S-B3"
  - "Frontend Architecture rev 8 bagian 18 — task S-B3"
  - "Permission Audit Matrix v13 bagian 17 — task S-B3"
  - "Validation Matrix v13 bagian 18 — pesan yang ditampilkan layar S-B3"
  - "API Contract v14 bagian 20 — task FE-HRD-130 s.d. FE-HRD-143"
  - "Frontend Architecture rev 19 bagian 19 — layar FE-HRD-162 s.d. FE-HRD-167"
  - "Permission Audit Matrix v14 bagian 18 — task FE-HRD-130 s.d. FE-HRD-143"
  - "Validation Matrix v14 bagian 19 — pesan dan aturan validasi form payroll VR-PAY-01 s.d. VR-PAY-25"
locked_input_hashes_s_b3:
  00-interview-decisions.md: fe21aa8ca8aac54fec7276f22c4bd387af2d24a663d4cd455b4f823bb8586b24
  02-backend-architecture.md: 589393648594450bda8c918985ffd694a0ff932d26f0d48b24e1556bdacbac76
  03-frontend-architecture.md: 1aca4288fcb0a67170cb6e1da826f47d254cbc9a7ca57176929c0739fbde1733
  04-prd-to-mvp.md: 6cf75d826030bfa55e9a51f63338482096f6573bee3e47e83a6db222665cd012
  data/data-dictionary.md: 3b0fa5a79d19ad9fc70034cf9db3263802207fe45ed1350992f3b2fea17f51fb
  contracts/api-contract.md: 75b7f2403dd2f4866c208fb7fbe13f31e42c072879923a42f3b145b126a20988
  contracts/state-transition-matrix.md: deae7d13912ed4963171dfa921904d677e55c2623ef5a54b342760216dcf2472
  contracts/validation-matrix.md: add164c24920ee9e62ac43f9eee013d114f94d38a52394e444ec1f5c178554c4
  contracts/integration-contract.md: ae2c36c6df6457db3dc931e456e9a35e5a0564f3a83642daea532c41e58ade6d
  contracts/permission-audit-matrix.md: 0cb3a7577b55abdb44ae9bf4fa38313e7613d80ac0d688a82e1bc433c1d6a7b0
  flowcharts/administrasi-lembur.md: 4f67e1be5e1f43ada0c6a52040e4c4355235f43a22e1e6aca528f2cec8d15acc
  testing/acceptance-test-matrix.md: 4c4ab7420cb00feaabd870a038ecd4ab927417c774d5d39ce1d9103e06a3791e
```

**Apa yang berubah pada revision 14.**

1. **Empat belas task frontend `S-D3` baru**, `FE-HRD-158` s.d. `FE-HRD-171`, untuk membangun dan memverifikasi 6 layar baru (`FE-HRD-174` s.d. `FE-HRD-179`), modal dan drawer verifikasi pada sub-modul 12 (Benefit Management): client API service & adapter Axios/Redux slice, peta navigasi menu & guard RBAC, desain token & badge status kepesertaan BPJS, dasbor analitik manfaat & grafik utilisasi plafon (`FE-HRD-174`), katalog master paket manfaat 3-lapisan & setting diskon staf RS (`FE-HRD-175`), drawer matriks kelayakan grade staf ke kamar rawat inap, monitoring kepesertaan asuransi pegawai & aktivasi bertahap H+0 vs pasca-percobaan (`FE-HRD-176`), meja kerja verifikasi dokumen kependudukan KK/akta HR (`FE-HRD-177`), portal mandiri ESS keluarga saya & indikator kuota 1 pasangan + 3 anak, inquiry hak berobat nakes di RS sendiri & buku besar mutasi saldo plafon (`FE-HRD-178`), modal penyesuaian saldo manual HR, meja kerja verifikasi & settlement klaim sekunder COB (`FE-HRD-179`), formulir pengajuan klaim COB mandatory SEP BPJS di portal ESS, serta audit verifikasi build produksi standalone dan linter CI/CD.
2. **Grafik F42 s.d. F45 ditambahkan** beserta tabel gelombang eksekusi `S-D3` yang sepenuhnya bebas siklus.
3. **Register status task frontend diperbarui** mendaftarkan seluruh 14 task `S-D3` (`FE-HRD-158` s.d. `FE-HRD-171`).

**Apa yang berubah pada revision 13.**

1. **Empat belas task frontend `S-B6` baru**, `FE-HRD-144` s.d. `FE-HRD-157`, untuk membangun dan memverifikasi 6 layar baru (`FE-HRD-168` s.d. `FE-HRD-173`), modal dan drawer investigasi pada sub-modul 11 (Restitusi & Integrasi Pemotongan Penggajian): client API service & adapter Axios/Redux, desain token & badge status indikator rasio upah 50%, dasbor pemantauan realisasi potongan & tagihan tunda (`FE-HRD-168`), tabel peringatan pegawai mendekati batas 50% & trigger sinkronisasi Finance H-3, lembar kerja jadwal angsuran master pinjaman (`FE-HRD-170`), modal simulasi interaktif hard-cap 50% & breakdown prioritas 1-4, pelacak kuota plafon tahunan pegawai di portal ESS (`FE-HRD-169`), formulir pengajuan klaim mandiri ESS & dropzone kuitansi <60 hari, antrean verifikasi persetujuan 3-tier & anti-self-approval (`FE-HRD-172`), modal pratinjau zoom nota kuitansi & resume medis, drawer investigasi restitusi koreksi salah potong masa lalu non-pajak PPh 21 (`FE-HRD-171`), penerbitan voucher kasir cito 24/7 & penampil QR code, layar pengaturan plafon benefit per grade (`FE-HRD-173`), serta verifikasi build produksi dan linter CI/CD.
2. **Grafik F38 s.d. F41 ditambahkan** beserta tabel gelombang eksekusi `S-B6` yang sepenuhnya bebas siklus.
3. **Register status task frontend diperbarui** mendaftarkan seluruh 14 task `S-B6` (`FE-HRD-144` s.d. `FE-HRD-157`).

**Apa yang berubah pada revision 12.**

1. **Empat belas task frontend `S-B5` baru**, `FE-HRD-130` s.d. `FE-HRD-143`, untuk membangun dan memverifikasi 6 layar baru (`FE-HRD-162` s.d. `FE-HRD-167`), modal investigasi anomali variansi, alur persetujuan 3-jenjang anti-self-approval, trigger handoff Finance SHA-256, dan portal slip gaji mandiri ESS berproteksi PIN 6-digit.
2. **Grafik F34 s.d. F37 ditambahkan** beserta tabel gelombang eksekusi `S-B5` yang sepenuhnya bebas siklus.
3. **Register status task frontend diperbarui** mendaftarkan seluruh 14 task `S-B5` (`FE-HRD-130` s.d. `FE-HRD-143`).

**Apa yang berubah pada revision 11.**

1. **Empat belas task frontend `S-B3` baru**, `FE-HRD-116` s.d. `FE-HRD-129`, untuk 6 layar `FE-HRD-156` s.d.
   `FE-HRD-161` pada `03-frontend-architecture.md` bagian 18.
2. **Grafik F30 s.d. F33 ditambahkan** beserta tabel gelombang eksekusi frontend `S-B3` bebas siklus.
3. **Penyelarasan formulir dual-mode pra-dinas & klaim cito pasca-dinas (`FE-HRD-117`, `FE-HRD-118`)**, drawer investigasi anomali presensi biometrik (`FE-HRD-121`), kotak masuk persetujuan eskalasi Direktur Medis (`FE-HRD-123`), pemantau tiket saldo CTO (`FE-HRD-124`, `FE-HRD-125`), dan penutupan rekonsiliasi payroll (`FE-HRD-127`).

**Apa yang berubah pada revision 9.**

1. **Empat belas task frontend `S-B1` baru**, `FE-HRD-088` s.d. `FE-HRD-101`, untuk 6 layar `FE-HRD-144` s.d.
   `FE-HRD-149` pada `03-frontend-architecture.md` bagian 16.
2. **Grafik F22 s.d. F25 ditambahkan** beserta tabel gelombang eksekusi frontend `S-B1` bebas siklus.
3. **Penyelarasan portal ESS presensi (`FE-HRD-100`)**, form izin pulang cepat mandiri, dan integrasi
   peringatan `IncompletePunch` (1 punch permohonan koreksi).

**Apa yang berubah pada revision 8.**

1. **Tiga belas task frontend `S-B4`**, `FE-HRD-075` s.d. `FE-HRD-087`, untuk 6 layar `FE-HRD-138` s.d.
   `FE-HRD-143` pada `03-frontend-architecture.md` bagian 15.
2. **Grafik F18 s.d. F21 ditambahkan** beserta tabel gelombang eksekusi frontend `S-B4`.

**Apa yang berubah pada revision 6.**

1. **Empat belas task `S-D2` baru**, `FE-HRD-044` s.d. `FE-HRD-057`, untuk 13 layar `FE-HRD-110` s.d.
   `FE-HRD-122` pada `03-frontend-architecture.md` bagian 13.
2. **Grafik urutan dependency diperluas.** Menambahkan Grafik F10 (MVP-11), Grafik F11 (MVP-12),
   dan Grafik F12 (MVP-13 & Verifikasi S-D2) dengan representasi pohon teks yang sepenuhnya bebas siklus.
3. **Penyelarasan hak veto klinis dan proteksi privasi medis.** Layar evaluasi klinis `FE-HRD-051`
   dilengkapi dialog konfirmasi hak veto klinis mutlak, dan layar `FE-HRD-054` memproteksi riwayat
   diagnosa medis pelamar dari staf non-K3RS.

**Apa yang berubah pada revision 5.**

1. **Lima belas task `S-D1` baru**, `FE-HRD-029` s.d. `FE-HRD-043`, untuk 13 layar `FE-HRD-97` s.d.
   `FE-HRD-109` pada `03-frontend-architecture.md` bagian 12. Perhatikan dua jenis ID: `FE-HRD-0xx`
   (tiga digit) adalah **task**, sedangkan `FE-HRD-97` s.d. `FE-HRD-109` adalah **layar** pada
   dokumen arsitektur frontend.
2. **Grafik ditulis ulang per slice.** Grafik lama memakai karakter `┼` yang tidak sah, memuat 28
   task dalam satu blok, dan arah beberapa garisnya tidak dapat dibaca pasti. Hubungannya dibaca
   ulang begini dan kini tercatat di kolom `Dependency` register: `FE-HRD-003` s.d. `FE-HRD-007`
   menunggu `FE-HRD-001` dan `BE-HRD-004`; `FE-HRD-009` s.d. `FE-HRD-012` masing-masing menunggu
   satu task backend (`BE-HRD-006` s.d. `BE-HRD-009`); persilangan garis lama tidak dibaca sebagai
   sambungan.
3. **Tiga dependency disederhanakan** karena sudah tercakup lewat prasyarat lain:
   `FE-HRD-023` dan `FE-HRD-024` tidak lagi mencantumkan `BE-HRD-025` (sudah lewat `FE-HRD-022`),
   dan `FE-HRD-028` tidak lagi mencantumkan `FE-HRD-021`. Urutan kerja tidak berubah.
4. **Empat tanda diselaraskan.** Register sudah mencatat `FE-HRD-011`, `FE-HRD-015`,
   `FE-HRD-016`, dan `FE-HRD-018` sebagai 🟡 sejak `PLAN-REPAIR-HRD-001` (menunggu K-02/K-03),
   sementara kartu dan grafik lama masih ✅. Kartu dan grafik kini ikut 🟡; alasannya ditulis di
   baris status kartunya. Bukti terbaru: `FIX-HRD-001-06` dan `FIX-HRD-001-07` masih ⛔ pada
   `docs/plan-repair/plan-repair-001-layar-hr-kosong-salah-alamat-api.md`.

**Arti tanda status pada dokumen ini:**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Tampilan, alur interaksi, Redux slice/service, dan validasi form terbukti pada source serta laporan task tracked tersedia |
| 🟡 | Sebagian. Tampilan atau struktur rute sudah ada, namun integrasi API atau validasi belum penuh |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi: keputusan klinis, keselamatan medis, atau gerbang lain yang disebut namanya |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

Roadmap frontend HR memuat **43 task**: 28 task lama dan 15 task `S-D1`. Grafiknya dipecah menjadi
satu ringkasan antar-slice dan sembilan grafik per slice. **Revision 7** menambah 17 task `S-C4-ONB`
(`FE-HRD-058` s.d. `FE-HRD-074`) pada Grafik F13 s.d. F17.

Cara membaca:

- `A ─> B` artinya **A harus selesai dulu, baru B boleh dimulai**. Prasyarat selalu di kiri.
- `[BE]` = cermin baca-saja task backend pada `backend-roadmap.md`. Tandanya disalin dari sana.
- `[F1]` s.d. `[F8]` = cermin baca-saja task frontend yang tempat aslinya di grafik lain pada
  dokumen ini. Setiap task punya tepat satu tempat asli.
- Node dalam kurung kurawal adalah gerbang keputusan, bukan task.

### Grafik ringkasan antar-slice

Node pada grafik ini adalah slice, bukan task.

```text
IN-01 ✅ ─┬─> S0-B ✅ ─┬─> S-A1 ✅
          │            │
          │            └─> S-A2..S-A7 ✅
          │
          ├─> S-D1 MVP-7 ─> S-D1 MVP-8 ─> S-D1 MVP-9 ─> S-D1 MVP-10
          │
          └─> S-D2 MVP-11 ─> S-D2 MVP-12 ─> S-D2 MVP-13 ─> S-D2 Verifikasi

S-D2 MVP-13 ─> S-C4-ONB MVP-14 ─┬─> S-C4-ONB MVP-15 ─┐
                                │                    │
                                ├─> S-C4-ONB MVP-16 ─┴──┐
                                │                       │
                                └─> S-C4-ONB MVP-17 ────┴─> S-C4-ONB Verifikasi

S-C4-ONB Verifikasi ─> S-B4 MVP-18 ─> S-B4 MVP-19 ─> S-B4 MVP-20 ─> S-B4 Verifikasi

S-B4 Verifikasi ─> S-B1 MVP-21 ─> S-B1 MVP-22 ─> S-B1 MVP-23 ─> S-B1 Verifikasi
```

`S-B1` s.d. `S-B5` 🟡, `S-C2` s.d. `S-C5` 🟡, dan `S-D3` ✅ tidak menunggu slice frontend lain;
prasyaratnya task backend. `S-C1` dan `S-C6` ⛔ tertahan gerbang keputusan. `S-D2` sudah aktif sepenuhnya mengikuti persetujuan desain `v8`.

### Grafik F1 — Fondasi (`IN-01`, `S0-B`)

```text
FE-HRD-021 ✅ ─> FE-HRD-001 ✅ ─> FE-HRD-002 ✅
```

### Grafik F2 — Administrasi Kepegawaian (`S-A1`)

```text
                    BE-HRD-025 ✅ [BE] ─┐
                                        │
FE-HRD-002 ✅ [F1] ─┬───────────────────┴─> FE-HRD-022 ✅ ─┬─> FE-HRD-023 ✅
                    │                                      │
                    │                                      ├─> FE-HRD-024 ✅
                    │                                      │
                    │                                      └─> FE-HRD-028 ✅
                    │
                    │   BE-HRD-026 ✅ [BE] ─┐
                    │                       │
                    ├───────────────────────┴─> FE-HRD-025 ✅
                    │
                    │   BE-HRD-027 ✅ [BE] ─┐
                    │                       │
                    ├───────────────────────┴─> FE-HRD-026 🟡
                    │
                    │   BE-HRD-028 ✅ [BE] ─┐
                    │                       │
                    └───────────────────────┴─> FE-HRD-027 🟡
```

### Grafik F3 — Layanan mandiri dan kotak masuk (`S-A2` s.d. `S-A7`)

```text
                    BE-HRD-004 ✅ [BE] ─┐
                                        │
FE-HRD-001 ✅ [F1] ─────────────────────┴─┬─> FE-HRD-003 ✅ ─┬──┬──┬──┬─> FE-HRD-008 ✅
                                          │                  │  │  │  │
                                          ├─> FE-HRD-004 ✅ ─┘  │  │  │
                                          │                     │  │  │
                                          ├─> FE-HRD-005 ✅ ────┘  │  │
                                          │                        │  │
                                          ├─> FE-HRD-006 ✅ ───────┘  │
                                          │                           │
                                          └─> FE-HRD-007 ✅ ──────────┘
```

### Grafik F4 — Waktu kerja, pelatihan, dan kinerja (`S-B1` s.d. `S-B5`, `S-C2`, `S-C3`)

```text
BE-HRD-006 ✅ [BE] ─> FE-HRD-009 ✅ ─┬──┬─> FE-HRD-013 ✅
                                     │  │
BE-HRD-007 ✅ [BE] ─> FE-HRD-010 ✅ ─┘  │
                                        │
BE-HRD-008 ✅ [BE] ─> FE-HRD-011 🟡 ────┘

BE-HRD-009 ✅ [BE] ─> FE-HRD-012 ✅ ─> FE-HRD-015 🟡 ─> FE-HRD-016 🟡
```

### Grafik F5 — Lifecycle, disiplin, benefit, dan slice terblokir (`S-C4`, `S-C5`, `S-D3`, `S-C1`, `S-C6`)

```text
BE-HRD-014 ✅ [BE] ─> FE-HRD-017 ✅ ─> FE-HRD-018 🟡

BE-HRD-019 ✅ [BE] ─> FE-HRD-020 ✅

{DEC-HRD-008 ⛔} ─> FE-HRD-014 ⛔

{DEC-HRD-010 ⛔} ─> FE-HRD-019 ⛔
```

### Grafik F6 — `S-D1` `MVP-7`: fondasi layar, pengaturan, formasi, batch formasi awal

```text
                          BE-HRD-032 [BE] ─┐
                                           │
FE-HRD-021 ✅ [F1] ─┬─> FE-HRD-029 ✅ ─┬───┴─> FE-HRD-030 🟡
                    │                  │
BE-HRD-031 [BE] ────┘                  │   BE-HRD-033 [BE] ─┐
                                       │                    │
                                       └────────────────────┴─> FE-HRD-031 🟡
```

### Grafik F7 — `S-D1` `MVP-8`: angka beban, standar, gap langsung

```text
                      BE-HRD-034 [BE] ─┐
                                       │
FE-HRD-029 ✅ [F6] ─┬──────────────────┴─> FE-HRD-032 ─┐
                    │                                  │
                    │ BE-HRD-035 [BE] ─┐               │
                    │                  │               │
                    ├──────────────────┴─> FE-HRD-033 ─┴─> FE-HRD-034
                    │
                    │ BE-HRD-037 [BE] ─┐
                    │                  │
                    └──────────────────┴─> FE-HRD-035
```

### Grafik F8 — `S-D1` `MVP-9`: permintaan headcount, antrean rekrutmen, potret gap

```text
                      BE-HRD-042 [BE] ─┐
                                       │   BE-HRD-043 [BE] ─┐
                                       │                    │
FE-HRD-029 ✅ [F6] ─┬──────────────────┴────────────────────┴─> FE-HRD-039 ─┐
                    │                                                       │
                    │ BE-HRD-040 [BE] ─┐                                    │
                    │                  │                                    │
                    └──────────────────┴─> FE-HRD-036 ─┐                    │
                                                       │                    │
FE-HRD-035 [F7] ────┬──────────────────────────────────┴─> FE-HRD-037 ──────┴──┬─> FE-HRD-038
                    │                                                          │
                    │ BE-HRD-039 [BE] ─┐                                       │
                    │                  │                                       │
                    └──────────────────┴─> FE-HRD-040                          │
                                                                               │
BE-HRD-044 [BE] ───────────────────────────────────────────────────────────────┘
```

### Grafik F9 — `S-D1` `MVP-10`: rencana tahunan dan verifikasi keterjangkauan layar

```text
     BE-HRD-046 [BE] ─┐
                      │
FE-HRD-029 ✅ [F6] ───┴─> FE-HRD-041 ─> FE-HRD-042 ─┬──┬──┬──┬──┬──┬─> FE-HRD-043
                                                    │  │  │  │  │  │
FE-HRD-030 🟡 [F6] ─────────────────────────────────┘  │  │  │  │  │
                                                       │  │  │  │  │
FE-HRD-031 🟡 [F6] ────────────────────────────────────┘  │  │  │  │
                                                          │  │  │  │
FE-HRD-034 [F7] ──────────────────────────────────────────┘  │  │  │
                                                             │  │  │
FE-HRD-038 [F8] ─────────────────────────────────────────────┘  │  │
                                                                │  │
FE-HRD-039 [F8] ────────────────────────────────────────────────┘  │
                                                                   │
FE-HRD-040 [F8] ───────────────────────────────────────────────────┘
```

### Grafik F10 — `S-D2` `MVP-11`: Requisition, Lowongan, dan Direktori Pelamar

```text
BE-HRD-052 [BE] ─> FE-HRD-044 ─┬─> FE-HRD-045
                               │
BE-HRD-053 [BE] ─┬─────────────┴─> FE-HRD-046 ─> FE-HRD-047
                 │
BE-HRD-054 [BE] ─┴──────────────────────────────> FE-HRD-048
```

### Grafik F11 — `S-D2` `MVP-12`: Penapisan STR, Evaluasi Wawancara, Uji Klinis Veto, Penawaran & MCU

```text
FE-HRD-048 [F10] ─┬─> FE-HRD-049 ─┬─> FE-HRD-050
                  │               │
BE-HRD-055 [BE] ──┘               ├─> FE-HRD-051
                                  │
BE-HRD-056 [BE] ──────────────────┘

BE-HRD-057 [BE] ──> FE-HRD-052 ─┬─> FE-HRD-053
                                │
BE-HRD-058 [BE] ────────────────┤
                                │
BE-HRD-059 [BE] ────────────────┴─> FE-HRD-054
```

### Grafik F12 — `S-D2` `MVP-13` & Verifikasi: Talent Pool, Handoff Onboarding, dan Verifikasi Menu

```text
BE-HRD-060 [BE] ───────────────────────────> FE-HRD-055 ─┬─> FE-HRD-057
                                                         │
FE-HRD-053 [F11] ─┬────────────────────────> FE-HRD-056 ─┤
                  │                                      │
FE-HRD-054 [F11] ─┤                                      │
                  │                                      │
BE-HRD-061 [BE] ──┘                                      │
                                                         │
FE-HRD-045 [F10] ────────────────────────────────────────┤
                                                         │
FE-HRD-047 [F10] ────────────────────────────────────────┤
                                                         │
FE-HRD-050 [F11] ────────────────────────────────────────┤
                                                         │
FE-HRD-051 [F11] ────────────────────────────────────────┘
```

### Grafik F13 — `S-C4-ONB` `MVP-14`/`MVP-15`: fondasi layanan, template, dan kasus onboarding

```text
FE-HRD-058 ─┬──────────────────────┬─> FE-HRD-059
            │                      │
            │   BE-HRD-070 [BE] ───┘
            │
            └──────────────────────┬─> FE-HRD-060 ──┐
                                   │                │
                BE-HRD-074 [BE] ───┘                │
                                                    │
                BE-HRD-075 [BE] ────────────────────┴─> FE-HRD-061 ──┐
                                                                     │
                BE-HRD-076 [BE] ─────────────────────────────────────┴──┐
                                                                        │
                FE-HRD-056 [F12] ───────────────────────────────────────┴─> FE-HRD-062
```

### Grafik F14 — `S-C4-ONB` `MVP-14`/`MVP-16`: kontrak kerja, layar lama, dan amandemen tawaran

```text
FE-HRD-058 [F13] ─┬─> FE-HRD-063 ─┬──────────────────────┐
                  │               │                      │
BE-HRD-072 [BE] ──┘               │   BE-HRD-080 [BE] ───┴──┐
                                  │                         │
                                  │   BE-HRD-081 [BE] ──────┴─> FE-HRD-064
                                  │
                                  └──────────────────────┬─> FE-HRD-071
                                                         │
                                      BE-HRD-065 [BE] ───┘

FE-HRD-053 [F12] ─┬─> FE-HRD-072
                  │
BE-HRD-064 [BE] ──┘
```

### Grafik F15 — `S-C4-ONB`: masa percobaan, pendampingan klinis, dan batch data awal

```text
FE-HRD-058 [F13] ─┬─> FE-HRD-065
                  │
BE-HRD-079 [BE] ──┘

FE-HRD-058 [F13] ─┬─> FE-HRD-066
                  │
BE-HRD-073 [BE] ──┘

FE-HRD-058 [F13] ─┬─> FE-HRD-067
                  │
BE-HRD-082 [BE] ──┘
```

### Grafik F16 — `S-C4-ONB`: permintaan akses, pengaturan, dan Perlu Tindakan

```text
FE-HRD-058 [F13] ─┬─> FE-HRD-068
                  │
BE-HRD-077 [BE] ──┘

FE-HRD-058 [F13] ─┬─> FE-HRD-069
                  │
BE-HRD-069 [BE] ──┘

FE-HRD-058 [F13] ─┬─> FE-HRD-070
                  │
BE-HRD-084 [BE] ──┘
```

### Grafik F17 — `S-C4-ONB` `MVP-17` & Verifikasi: kotak masuk dan keterjangkauan layar

```text
FE-HRD-064 [F14] ──┐
                   │
FE-HRD-065 [F15] ──┴──┐
                      │
FE-HRD-067 [F15] ─────┴──┐
                         │
BE-HRD-068 [BE] ─────────┴─> FE-HRD-073 ──┐
                                          │
FE-HRD-059 [F13] ─────────────────────────┴──┐
                                             │
FE-HRD-062 [F13] ────────────────────────────┴──┐
                                                │
FE-HRD-066 [F15] ───────────────────────────────┴──┐
                                                   │
FE-HRD-068 [F16] ──────────────────────────────────┴──┐
                                                      │
FE-HRD-069 [F16] ─────────────────────────────────────┴──┐
                                                         │
FE-HRD-070 [F16] ────────────────────────────────────────┴──┐
                                                            │
FE-HRD-071 [F14] ───────────────────────────────────────────┴──┐
                                                               │
FE-HRD-072 [F14] ──────────────────────────────────────────────┴─> FE-HRD-074
```

`[BE]` = task backend pada `backend-roadmap.md`, cermin baca-saja. `[F12]` s.d. `[F16]` = cermin
baca-saja task frontend yang tempat aslinya di grafik itu.


**Pemeriksaan silang.** Jumlah pasangan prasyarat → task: F1 2, F2 11, F3 15, F4 9, F5 5, F6 6,
F7 8, F8 12, F9 10, F10 6, F11 11, F12 12 — total **107**. Jumlah entri kolom `Dependency` pada register juga **107** (42
untuk task lama, 36 untuk task `S-D1`, 29 untuk task `S-D2`). Grafiknya bebas siklus. **Revision 7, `S-C4-ONB`:** F13 9, F14 9, F15 6, F16 6,
F17 13 — **43** pasangan; kolom `Dependency` 17 task `S-C4-ONB` juga **43** entri. Total roadmap kini
**150** pasangan. Diperiksa dengan skrip: garis lurus, setiap task muncul tepat satu kali, bebas siklus.

---

## Tabel Gelombang Eksekusi

Gelombang dihitung dari prasyarat **frontend** saja. Prasyarat backend `[BE]` tidak menaikkan
nomor gelombang, tetapi ditulis di kolom kedua: task itu baru boleh dimulai setelah task backend-nya
✅. Hari ini **belum ada task `S-D1` frontend yang boleh dimulai**, karena seluruh prasyarat
backend-nya belum dikerjakan.

Urutan **rilis dan UAT** tetap `MVP-7` → `MVP-8` → `MVP-9` → `MVP-10` (`04-prd-to-mvp.md` bagian
21.20); gelombang di bawah hanya kapan sebuah layar paling cepat boleh dibangun.

| Gelombang | Boleh mulai setelah | Task Frontend |
| ---: | --- | --- |
| **1** | — dari sisi frontend; backend: `BE-HRD-006`, `BE-HRD-007`, `BE-HRD-008`, `BE-HRD-009`, `BE-HRD-014`, `BE-HRD-019` ✅ | `FE-HRD-021` ✅, `FE-HRD-009` ✅, `FE-HRD-010` ✅, `FE-HRD-011` 🟡, `FE-HRD-012` ✅, `FE-HRD-017` ✅, `FE-HRD-020` ✅ |
| **2** | `FE-HRD-021`, `FE-HRD-009` s.d. `FE-HRD-012`, `FE-HRD-017`; backend `BE-HRD-031` | `FE-HRD-001` ✅, `FE-HRD-013` ✅, `FE-HRD-015` 🟡, `FE-HRD-018` 🟡; **`FE-HRD-029`** (`MVP-7`) — menunggu `BE-HRD-031` |
| **3** | `FE-HRD-001`, `FE-HRD-015`, `FE-HRD-029`; backend `BE-HRD-004` ✅ dan task `S-D1` di kolom kanan | `FE-HRD-002` ✅, `FE-HRD-003` s.d. `FE-HRD-007` ✅, `FE-HRD-016` 🟡; `FE-HRD-030` (`BE-HRD-032`), `FE-HRD-031` (`BE-HRD-033`) — `MVP-7`; `FE-HRD-032` (`BE-HRD-034`), `FE-HRD-033` (`BE-HRD-035`), `FE-HRD-035` (`BE-HRD-037`) — `MVP-8`; `FE-HRD-036` (`BE-HRD-040`), `FE-HRD-039` (`BE-HRD-042`, `BE-HRD-043`) — `MVP-9`; `FE-HRD-041` (`BE-HRD-046`) — `MVP-10` |
| **4** | Prasyarat frontend gelombang 3 masing-masing; backend `BE-HRD-025` s.d. `BE-HRD-028` ✅, `BE-HRD-039` | `FE-HRD-008` ✅, `FE-HRD-022` ✅, `FE-HRD-025` ✅, `FE-HRD-026` 🟡, `FE-HRD-027` 🟡; `FE-HRD-034` (`MVP-8`); `FE-HRD-037`, `FE-HRD-040` (`BE-HRD-039`) — `MVP-9`; `FE-HRD-042` (`MVP-10`) |
| **5** | `FE-HRD-022`; `FE-HRD-037`, `FE-HRD-039`; backend `BE-HRD-044` | `FE-HRD-023` ✅, `FE-HRD-024` ✅, `FE-HRD-028` ✅; `FE-HRD-038` (`MVP-9`) — menunggu `BE-HRD-044` |
| **6** | `FE-HRD-030`, `FE-HRD-031`, `FE-HRD-034`, `FE-HRD-038`, `FE-HRD-039`, `FE-HRD-040`, `FE-HRD-042` | `FE-HRD-043` (verifikasi menu dan keterjangkauan layar `S-D1`) |
| **1** | — dari sisi frontend; backend: `BE-HRD-052` ✅ [BE], `BE-HRD-054` ✅ [BE], `BE-HRD-057` ✅ [BE], `BE-HRD-060` ✅ [BE] | `FE-HRD-044` ✅ (`MVP-11`), `FE-HRD-048` ✅ (`MVP-11`), `FE-HRD-052` ✅ (`MVP-12`), `FE-HRD-055` ✅ (`MVP-13`) |
| **2** | `FE-HRD-044` ✅, `FE-HRD-048` ✅, `FE-HRD-052` ✅; backend: `BE-HRD-053` ✅ [BE], `BE-HRD-055` ✅ [BE], `BE-HRD-058` ✅ [BE], `BE-HRD-059` ✅ [BE] | `FE-HRD-045` ✅ (`MVP-11`), `FE-HRD-046` ✅ (`MVP-11`), `FE-HRD-049` ✅ (`MVP-12`), `FE-HRD-053` ✅ (`MVP-13`), `FE-HRD-054` ✅ (`MVP-13`) |
| **3** | `FE-HRD-046` ✅, `FE-HRD-049` ✅, `FE-HRD-053` ✅, `FE-HRD-054` ✅; backend: `BE-HRD-056` ✅ [BE], `BE-HRD-061` ✅ [BE] | `FE-HRD-047` ✅ (`MVP-11`), `FE-HRD-050` ✅ (`MVP-12`), `FE-HRD-051` ✅ (`MVP-12`), `FE-HRD-056` ✅ (`MVP-13`) |
| **4** | Seluruh layar `FE-HRD-045` ✅, `FE-HRD-047` ✅, `FE-HRD-050` ✅, `FE-HRD-051` ✅, `FE-HRD-053` ✅, `FE-HRD-054` ✅, `FE-HRD-055` ✅, `FE-HRD-056` ✅ | `FE-HRD-057` ✅ (verifikasi menu, hak akses, dan keterjangkauan layar `S-D2`) |
| **⛔** | `{DEC-HRD-008}` — Komite Medik | `FE-HRD-014` ⛔ (Layar Kredensial Medis & SPK/RKK) |
| **⛔** | `{DEC-HRD-010}` — K3RS / MCU Pegawai | `FE-HRD-019` ⛔ (Layar Kesehatan Staf K3RS) |
| **1** | — dari sisi frontend; kontrak `v9` terkunci | `FE-HRD-058` ✅ (`S-C4-ONB` `MVP-14`, fondasi layanan) |
| **2** | `FE-HRD-058` ✅ | `FE-HRD-059` ✅, `FE-HRD-060` ✅, `FE-HRD-063` ✅ (`MVP-14`); `FE-HRD-066` ✅, `FE-HRD-068` ✅ (`MVP-15`); `FE-HRD-067` ✅, `FE-HRD-069` ✅, `FE-HRD-070` ✅ (`MVP-16`); `FE-HRD-065` ✅ (`MVP-17`) |
| **3** | `FE-HRD-053` ✅, `FE-HRD-060` ✅, `FE-HRD-063` ✅ | `FE-HRD-071` ✅, `FE-HRD-072` ✅ (`MVP-14`); `FE-HRD-061` ✅ (`MVP-15`); `FE-HRD-064` ✅ (`MVP-16`) |
| **4** | `FE-HRD-056` ✅, `FE-HRD-061` ✅, `FE-HRD-064` ✅, `FE-HRD-065` ✅, `FE-HRD-067` ✅ | `FE-HRD-062` ✅ (`MVP-15`); `FE-HRD-073` ✅ (`MVP-17`) |
| **5** | Seluruh layar `S-C4-ONB` gelombang 1–4 ✅ | `FE-HRD-074` ✅ (verifikasi menu, audit token CSS, dan keterjangkauan `S-C4-ONB`) |
| **1** | — dari sisi frontend; kontrak `v10` terkunci | `FE-HRD-075` ✅ (Service API Roster), `FE-HRD-083` ✅ (Service On-Call), `FE-HRD-086` ✅ (Settings view) — `S-B4` |
| **2** | `FE-HRD-075` ✅ | `FE-HRD-076` ✅ (Redux slice Roster Scheduling), `FE-HRD-084` ✅ (On-Call Monitoring), `FE-HRD-085` ✅ (Modal On-Call Call-In) |
| **3** | `FE-HRD-076` ✅ | `FE-HRD-077` ✅ (Indeks Roster), `FE-HRD-078` ✅ (Modal Salin/Pola), `FE-HRD-079` ✅ (Drawer Audit Validasi) |
| **4** | `FE-HRD-077` ✅, `FE-HRD-078` ✅, `FE-HRD-079` ✅ | `FE-HRD-080` ✅ (Layar Matriks Roster Detail & Sticky Grid), `FE-HRD-081` ✅ (Modal Ganti Shift Langsung Aktif), `FE-HRD-082` ✅ (Modal Koreksi Mundur 3 Hari) |
| **5** | Seluruh komponen, view, rute, dan service `S-B4` | `FE-HRD-087` ✅ (Validasi Ujung-ke-Ujung `S-B4`: Linter & Turbopack Build 0 Error) |

**Task frontend yang ikut tertahan gap backend.** Tidak ada task frontend yang langsung menunggu
`BE-HRD-045`, `BE-HRD-049`, `BE-HRD-050`, atau `BE-HRD-051` ⛔. Akibatnya pada layar: pesan
`VR-WFP-36`, `VR-WFP-21`, dan `VR-WFP-23` baru dapat diperiksa di layar setelah task backend itu
selesai; layar cukup menampilkan pesan apa pun yang dikirim backend.

---

## Keputusan UI yang masih terbuka

| Hal | Status | Perlakuan di task |
| --- | --- | --- |
| Nama grup "Perencanaan Tenaga Kerja", kesembilan butir menu, dan pengganti nama butir lama "Kebutuhan Tenaga Kerja" | `OPEN` — pemilik produk, `HRD-FE-04` | Usulan kerja dipakai apa adanya: grup "Perencanaan Tenaga Kerja", butir lama menjadi "Persyaratan Berkas Tenaga Kerja". Mengganti nama nanti cukup di satu tempat. Tidak memblokir |
| Urutan dan ikon butir menu | `DEV_DISCRETION` | Builder memilih, mengikuti grup HR tetangga |
| Penyusun standar dan rencana sebagai satu layar berbagian atau berlangkah | `DEV_DISCRETION` | Builder memilih dengan rekomendasi pada laporan task |
| Pemakaian ulang atau komponen baru per elemen | Diputuskan saat eksekusi | Mengikuti `base-component-decision-gate` pada `build-module-frontend` |
| Formasi tanpa tombol ubah; nilai `null` tampil "data belum tersedia"; biaya selalu berlabel "estimasi" | **Mengikat** — `HRD-DEC-062`, `HRD-DEC-061`, `HRD-DEC-065` | Menjadi acceptance criteria |
| Nama grup "Onboarding & Kontrak", kesembilan butir menu, dan label status `S-C4-ONB` | `OPEN` — pemilik produk, `HRD-FE-04` | Usulan kerja `03-frontend-architecture.md` 14.2 dan 14.6 dipakai apa adanya |
| Form keputusan kontrak, hasil masa percobaan, siap mandiri, dan batal bergabung sebagai modal atau panel | `DEV_DISCRETION` | Builder memilih, mengikuti layar HR tetangga |
| Tombol tidak berhak disembunyikan; aksi kasus mengikuti `availableActions`; nominal gaji hanya bila `amountVisible`; butir sistem terkunci | **Mengikat** — `03-frontend-architecture.md` 14.8, `HRD-DEC-033`, `HRD-DEC-086` | Menjadi acceptance criteria |

---

## Register Status Task Frontend

| Task ID | Slice | Ringkasan Layar & Komponen UI | Status | Dependency | Laporan Bukti |
| --- | --- | --- | :---: | --- | --- |
| `FE-HRD-001` | `S0-B` | Penyelarasan Rute Kebab-Case pada Endpoint Constants Master Data | ✅ | `FE-HRD-021` ✅ | [FE-HRD-001.md](../task/report/frontend/FE-HRD-001.md) |
| `FE-HRD-002` | `S-A1` | Layar Daftar Lintas-Pegawai Administrasi Kepegawaian (`/hr/workforce-core/*`) | ✅ | `FE-HRD-001` ✅ | [FE-HRD-002.md](../task/report/frontend/FE-HRD-002.md) |
| `FE-HRD-003` | `S-A2` | Portal Layanan Mandiri Cuti Pegawai (`/self-services/.../leave`) | ✅ | `FE-HRD-001` ✅, `BE-HRD-004` ✅ [BE] | [FE-HRD-003.md](../task/report/frontend/FE-HRD-003.md) |
| `FE-HRD-004` | `S-A3` | Portal Layanan Mandiri Lembur Pegawai (`/self-services/.../overtime`) | ✅ | `FE-HRD-001` ✅, `BE-HRD-004` ✅ [BE] | [FE-HRD-004.md](../task/report/frontend/FE-HRD-004.md) |
| `FE-HRD-005` | `S-A4` | Portal Layanan Mandiri Ubah Jadwal & Tukar Shift (`/self-services/.../schedule`) | ✅ | `FE-HRD-001` ✅, `BE-HRD-004` ✅ [BE] | [FE-HRD-005.md](../task/report/frontend/FE-HRD-005.md) |
| `FE-HRD-006` | `S-A5` | Portal Layanan Mandiri Koreksi Absensi Mandiri (`/self-services/.../attendance-correction`) | ✅ | `FE-HRD-001` ✅, `BE-HRD-004` ✅ [BE] | [FE-HRD-006.md](../task/report/frontend/FE-HRD-006.md) |
| `FE-HRD-007` | `S-A6` | Migrasi & Konsolidasi Form Absensi Harian ke Standar Self-Service Karyawan | ✅ | `FE-HRD-001` ✅, `BE-HRD-004` ✅ [BE] | [FE-HRD-007.md](../task/report/frontend/FE-HRD-007.md) |
| `FE-HRD-008` | `S-A7` | Layar Kotak Masuk Persetujuan Terpadu Atasan (Unified Approval Inbox) | ✅ | `FE-HRD-003` ✅, `FE-HRD-004` ✅, `FE-HRD-005` ✅, `FE-HRD-006` ✅, `FE-HRD-007` ✅ | [FE-HRD-008.md](../task/report/frontend/FE-HRD-008.md) |
| `FE-HRD-009` | `S-B1` | Layar Administrasi Monitoring Presensi & Rekapitulasi Kehadiran HR | ✅ | `BE-HRD-006` ✅ [BE] | [FE-HRD-009.md](../task/report/frontend/FE-HRD-009.md) |
| `FE-HRD-010` | `S-B2` | Layar Administrasi Alokasi & Penyesuaian Saldo Cuti Karyawan | ✅ | `BE-HRD-007` ✅ [BE] | [FE-HRD-010.md](../task/report/frontend/FE-HRD-010.md) (Diperbarui PLAN-REPAIR-HRD-001) |
| `FE-HRD-011` | `S-B3` | Layar Administrasi Verifikasi & Rekonsiliasi Jam Lembur HR | 🟡 | `BE-HRD-008` ✅ [BE] | [FE-HRD-011.md](../task/report/frontend/FE-HRD-011.md) (Menunggu K-03 / FIX-HRD-001-07) |
| `FE-HRD-012` | `S-B4` | Layar Matriks Penjadwalan Kerja & Roster Shift Instalasi 24/7 | ✅ | `BE-HRD-009` ✅ [BE] | [FE-HRD-012.md](../task/report/frontend/FE-HRD-012.md) (Diperbarui PLAN-REPAIR-HRD-001) |
| `FE-HRD-013` | `S-B5` | Layar Penggajian Restitusi & Potongan Cicilan Piutang RS | ✅ | `FE-HRD-009` ✅, `FE-HRD-010` ✅, `FE-HRD-011` 🟡 | [FE-HRD-013.md](../task/report/frontend/FE-HRD-013.md) (Diperbarui PLAN-REPAIR-HRD-001) |
| `FE-HRD-014` | `S-C1` | Layar Pengajuan Kredensial & Pengesahan Kewenangan Klinis Dokter | ⛔ | `{DEC-HRD-008 ⛔}` | Terblokir `{DEC-HRD-008}` |
| `FE-HRD-015` | `S-C2` | Layar Manajemen Pelatihan Wajib, Kehadiran Diklat, dan Sertifikasi | 🟡 | `FE-HRD-012` ✅ | [FE-HRD-015.md](../task/report/frontend/FE-HRD-015.md) (Menunggu K-02 / FIX-HRD-001-06) |
| `FE-HRD-016` | `S-C3` | Layar Siklus Evaluasi Kinerja, Pengisian Nilai KPI, dan Hasil Review | 🟡 | `FE-HRD-015` 🟡 | [FE-HRD-016.md](../task/report/frontend/FE-HRD-016.md) (Menunggu K-02 / FIX-HRD-001-06) |
| `FE-HRD-017` | `S-C4` | Layar Pemantauan Exit Clearance dan Kliring Aset Karyawan | ✅ | `BE-HRD-014` ✅ [BE] | [FE-HRD-017.md](../task/report/frontend/FE-HRD-017.md) (Diperbarui PLAN-REPAIR-HRD-001) |
| `FE-HRD-018` | `S-C5` | Layar Pencatatan Hubungan Karyawan, Kasus Disiplin, dan Surat Peringatan | 🟡 | `FE-HRD-017` ✅ | [FE-HRD-018.md](../task/report/frontend/FE-HRD-018.md) (Menunggu K-02 / FIX-HRD-001-06) |
| `FE-HRD-019` | `S-C6` | Layar Rekam Medis Kerja & Pemantauan Kesehatan Karyawan K3RS | ⛔ | `{DEC-HRD-010 ⛔}` | Terblokir `{DEC-HRD-010}` |
| `FE-HRD-020` | `S-D3` | Layar Pengelolaan Kepesertaan Benefit Karyawan & Penjamin Tanggungan | ✅ | `BE-HRD-019` ✅ [BE] | [FE-HRD-020.md](../task/report/frontend/FE-HRD-020.md) |
| `FE-HRD-021` | `IN-01` | Verifikasi dan Pemeliharaan 62 Kelompok Layar Master Data HR Terpasang | ✅ | — | Terpasang Aktif |
| `FE-HRD-022` | `S-A1` | Layar Daftar Dokumen SK Kepegawaian (`/hr/workforce-core/personnel-actions`) | ✅ | `FE-HRD-002` ✅, `BE-HRD-025` ✅ [BE] | [FE-HRD-022.md](../task/report/frontend/FE-HRD-022.md) (Diperbarui PLAN-REPAIR-HRD-002) |
| `FE-HRD-023` | `S-A1` | Rute Halaman & Daftar SK Kepegawaian (`/hr/personnel-action-orders`) | ✅ | `FE-HRD-022` ✅ | [FE-HRD-023.md](../task/report/frontend/FE-HRD-023.md) |
| `FE-HRD-024` | `S-A1` | Form Penerbitan SK, Upload Pindai Fisik, & Layar Detail Approval SK | ✅ | `FE-HRD-022` ✅ | [FE-HRD-024.md](../task/report/frontend/FE-HRD-024.md) (Diperbarui PLAN-REPAIR-HRD-002) |
| `FE-HRD-025` | `S-A1` | Antarmuka Profil Terpadu Pegawai (Employee 360) Dual-View Segregation | ✅ | `FE-HRD-002` ✅, `BE-HRD-026` ✅ [BE] | [FE-HRD-025.md](../task/report/frontend/FE-HRD-025.md) (Diperbarui PLAN-REPAIR-HRD-002) |
| `FE-HRD-026` | `S-A1` | Layar Batch Inisialisasi Penempatan Awal Pegawai (55 Staf Eksisting) | 🟡 | `FE-HRD-002` ✅, `BE-HRD-027` ✅ [BE] | [FE-HRD-026.md](../task/report/frontend/FE-HRD-026.md) (Menunggu FIX-HRD-002-09) |
| `FE-HRD-027` | `S-A1` | Form Permohonan Perubahan Data Kuasa HR (On-Behalf) Berkas Kuasa | 🟡 | `FE-HRD-002` ✅, `BE-HRD-028` ✅ [BE] | [FE-HRD-027.md](../task/report/frontend/FE-HRD-027.md) (🟡 menurut laporan: 0 dari 4 kriteria terbukti penuh; uji ulang lewat `FE-HRD-R56`) |
| `FE-HRD-026-ESS` | `S-A5` | Layar Layanan Mandiri Kelola Data Saya (Employee Self-Service Profile Change) | ✅ | `FE-HRD-001` ✅, `BE-HRD-028` ✅ [BE] | [PLAN-REPAIR-003](../docs/plan-repair/plan-repair-003-employee-self-service-profile-change.md) |
| `FE-HRD-028` | `S-A1` | Penguncian Form Master Data Pegawai (Lock Unit & Jabatan Read-Only) | ✅ | `FE-HRD-022` ✅ | [FE-HRD-028.md](../task/report/frontend/FE-HRD-028.md) |
| `FE-HRD-029` | `S-D1` · `MVP-7` | Fondasi layar `S-D1`: grup menu, konstanta endpoint, util tampilan, dan layar Pengaturan Perencanaan (`FE-HRD-109`) | ✅ | `FE-HRD-021` ✅, `BE-HRD-031` [BE] | [FE-HRD-029.md](../task/report/frontend/FE-HRD-029.md) |
| `FE-HRD-030` | `S-D1` · `MVP-7` | Layar Formasi Jabatan baca saja beserta riwayat versi (`FE-HRD-101`) | 🟡 | `FE-HRD-029`, `BE-HRD-032` [BE] | [FE-HRD-030.md](../task/report/frontend/FE-HRD-030.md) |
| `FE-HRD-031` | `S-D1` · `MVP-7` | Layar Batch Formasi Awal dan detailnya (`FE-HRD-102`, `FE-HRD-103`) | 🟡 | `FE-HRD-029`, `BE-HRD-033` [BE] | [FE-HRD-031.md](../task/report/frontend/FE-HRD-031.md) |
| `FE-HRD-032` | `S-D1` · `MVP-8` | Layar Angka Beban (`FE-HRD-100`) | | `FE-HRD-029`, `BE-HRD-034` [BE] | — |
| `FE-HRD-033` | `S-D1` · `MVP-8` | Layar Daftar Standar Kebutuhan Tenaga (`FE-HRD-98`) | | `FE-HRD-029`, `BE-HRD-035` [BE] | — |
| `FE-HRD-034` | `S-D1` · `MVP-8` | Layar Penyusun dan Detail Standar beserta rekomendasi (`FE-HRD-99`) | | `FE-HRD-032`, `FE-HRD-033` | — |
| `FE-HRD-035` | `S-D1` · `MVP-8` | Layar Pemantauan Gap Tenaga — gap langsung (`FE-HRD-97`) | | `FE-HRD-029`, `BE-HRD-037` [BE] | — |
| `FE-HRD-036` | `S-D1` · `MVP-9` | Layar Daftar Permintaan Headcount (`FE-HRD-106`) | | `FE-HRD-029`, `BE-HRD-040` [BE] | — |
| `FE-HRD-037` | `S-D1` · `MVP-9` | Form dan Detail Permintaan Headcount (`FE-HRD-107`) serta tombol "Ajukan" di Pemantauan Gap | | `FE-HRD-035`, `FE-HRD-036` | — |
| `FE-HRD-038` | `S-D1` · `MVP-9` | Aksi kursi pada detail permintaan: tautkan/lepas, ubah rencana pemenuhan, batalkan kursi | | `FE-HRD-037`, `FE-HRD-039`, `BE-HRD-044` [BE] | — |
| `FE-HRD-039` | `S-D1` · `MVP-9` | Layar Antrean Rekrutmen beserta dialog tautan pemenuhan (`FE-HRD-108`) | | `FE-HRD-029`, `BE-HRD-042` [BE], `BE-HRD-043` [BE] | — |
| `FE-HRD-040` | `S-D1` · `MVP-9` | Panel potret gap pada Pemantauan Gap Tenaga | | `FE-HRD-035`, `BE-HRD-039` [BE] | — |
| `FE-HRD-041` | `S-D1` · `MVP-10` | Layar Daftar Rencana Tahunan dan Addendum (`FE-HRD-104`) | | `FE-HRD-029`, `BE-HRD-046` [BE] | — |
| `FE-HRD-042` | `S-D1` · `MVP-10` | Layar Penyusun dan Detail Rencana (`FE-HRD-105`) | | `FE-HRD-041` | — |
| `FE-HRD-043` | `S-D1` · Verifikasi | Verifikasi menu, hak akses, dan keterjangkauan seluruh layar `S-D1` | | `FE-HRD-030`, `FE-HRD-031`, `FE-HRD-034`, `FE-HRD-038`, `FE-HRD-039`, `FE-HRD-040`, `FE-HRD-042` | — |
| `FE-HRD-044` | `S-D2` · `MVP-11` | Layar Antrean & Permintaan Requisition (`FE-HRD-110`, `/hr/recruitment/requisitions`) | ✅ | `BE-HRD-052` ✅ [BE] | [FE-HRD-044.md](../task/report/frontend/FE-HRD-044.md) |
| `FE-HRD-045` | `S-D2` · `MVP-11` | Layar Detail & Draf Requisition (`FE-HRD-111`, `/hr/recruitment/requisitions/[id]`) | ✅ | `FE-HRD-044` ✅ | [FE-HRD-045.md](../task/report/frontend/FE-HRD-045.md) |
| `FE-HRD-046` | `S-D2` · `MVP-11` | Layar Pengelolaan Lowongan Kerja (*Job Vacancies*) (`FE-HRD-112`, `/hr/recruitment/vacancies`) | ✅ | `FE-HRD-044` ✅, `BE-HRD-053` ✅ [BE] | [FE-HRD-046.md](../task/report/frontend/FE-HRD-046.md) |
| `FE-HRD-047` | `S-D2` · `MVP-11` | Layar Detail Lowongan & Pelamar Masuk (`FE-HRD-113`, `/hr/recruitment/vacancies/[id]`) | ✅ | `FE-HRD-046` ✅ | [FE-HRD-047.md](../task/report/frontend/FE-HRD-047.md) |
| `FE-HRD-048` | `S-D2` · `MVP-11` | Layar Direktori Data Pelamar & NIK Rehire Flow (`FE-HRD-114`, `/hr/recruitment/candidates`) | ✅ | `BE-HRD-054` ✅ [BE] | [FE-HRD-048.md](../task/report/frontend/FE-HRD-048.md) |
| `FE-HRD-049` | `S-D2` · `MVP-12` | Layar Detail Pelamar & Penapisan STR Klinis Otomatis (`FE-HRD-115`, `/hr/recruitment/candidates/[id]`) | ✅ | `FE-HRD-048` ✅, `BE-HRD-055` ✅ [BE] | [FE-HRD-049.md](../task/report/frontend/FE-HRD-049.md) |
| `FE-HRD-050` | `S-D2` · `MVP-12` | Layar Formulir Evaluasi Wawancara HR (`FE-HRD-116`, `/hr/recruitment/evaluations/hr/[id]`) | ✅ | `FE-HRD-049` ✅, `BE-HRD-056` ✅ [BE] | [FE-HRD-050.md](../task/report/frontend/FE-HRD-050.md) |
| `FE-HRD-051` | `S-D2` · `MVP-12` | Layar Formulir Uji Klinis / Asesor Unit dengan Hak Veto Klinis (`FE-HRD-117`, `/hr/recruitment/evaluations/clinical/[id]`) | ✅ | `FE-HRD-049` ✅, `BE-HRD-056` ✅ [BE] | [FE-HRD-051.md](../task/report/frontend/FE-HRD-051.md) |
| `FE-HRD-052` | `S-D2` · `MVP-12` | Layar Daftar Penawaran Kerja (*Job Offers*) (`FE-HRD-118`, `/hr/recruitment/offers`) | ✅ | `BE-HRD-057` ✅ [BE] | [FE-HRD-052.md](../task/report/frontend/FE-HRD-052.md) |
| `FE-HRD-053` | `S-D2` · `MVP-13` | Layar Detail Penawaran & Rantai Persetujuan Bertingkat T13/T14 (`FE-HRD-119`, `/hr/recruitment/offers/[id]`) | ✅ | `FE-HRD-052` ✅, `BE-HRD-058` ✅ [BE] | [FE-HRD-053.md](../task/report/frontend/FE-HRD-053.md) |
| `FE-HRD-054` | `S-D2` · `MVP-13` | Layar Pemantauan Kelaikan MCU Pra-Kerja & Masking Medis K3RS (`FE-HRD-120`, `/hr/recruitment/mcu-clearance`) | ✅ | `FE-HRD-052` ✅, `BE-HRD-059` ✅ [BE] | [FE-HRD-054.md](../task/report/frontend/FE-HRD-054.md) |
| `FE-HRD-055` | `S-D2` · `MVP-13` | Layar Bank Kandidat Cadangan (*Talent Pool*) & Fast-Track Promotion (`FE-HRD-121`, `/hr/recruitment/talent-pool`) | ✅ | `BE-HRD-060` ✅ [BE] | [FE-HRD-055.md](../task/report/frontend/FE-HRD-055.md) |
| `FE-HRD-056` | `S-D2` · `MVP-13` | Layar Serah Terima ke Onboarding (*Hiring Handoff*) (`FE-HRD-122`, `/hr/recruitment/handoffs`) | ✅ | `FE-HRD-053` ✅, `FE-HRD-054` ✅, `BE-HRD-061` ✅ [BE] | [FE-HRD-056.md](../task/report/frontend/FE-HRD-056.md) |
| `FE-HRD-057` | `S-D2` · Verifikasi | Verifikasi Menu, Hak Akses, dan Keterjangkauan Seluruh Layar Rekrutmen `S-D2` | ✅ | `FE-HRD-045` ✅, `FE-HRD-047` ✅, `FE-HRD-050` ✅, `FE-HRD-051` ✅, `FE-HRD-053` ✅, `FE-HRD-054` ✅, `FE-HRD-055` ✅, `FE-HRD-056` ✅ | [FE-HRD-057.md](../task/report/frontend/FE-HRD-057.md) |
| `FE-HRD-058` | `S-C4-ONB` · `MVP-14` | Fondasi layanan API dan label status | ✅ | — | [FE-HRD-058.md](../task/report/frontend/FE-HRD-058.md) |
| `FE-HRD-059` | `S-C4-ONB` · `MVP-14` | Template Onboarding (`FE-HRD-132`, `FE-HRD-133`) dan grup menu | ✅ | `FE-HRD-058`, `BE-HRD-070` [BE] | [FE-HRD-059.md](../task/report/frontend/FE-HRD-059.md) |
| `FE-HRD-060` | `S-C4-ONB` · `MVP-14` | Daftar dan Detail Kasus Onboarding (`FE-HRD-124`, `FE-HRD-125`) | ✅ | `FE-HRD-058`, `BE-HRD-074` [BE] | [FE-HRD-060.md](../task/report/frontend/FE-HRD-060.md) |
| `FE-HRD-061` | `S-C4-ONB` · `MVP-15` | Aksi checklist pada Detail Kasus Onboarding | ✅ | `FE-HRD-060`, `BE-HRD-075` [BE] | [FE-HRD-061.md](../task/report/frontend/FE-HRD-061.md) |
| `FE-HRD-062` | `S-C4-ONB` · `MVP-15` | Siap kerja, batal bergabung, dan tautan dari layar serah terima | ✅ | `FE-HRD-061`, `BE-HRD-076` [BE], `FE-HRD-056` | [FE-HRD-062.md](../task/report/frontend/FE-HRD-062.md) |
| `FE-HRD-063` | `S-C4-ONB` · `MVP-14` | Daftar dan Detail Kontrak Kerja (`FE-HRD-126`, `FE-HRD-127`) | ✅ | `FE-HRD-058`, `BE-HRD-072` [BE] | [FE-HRD-063.md](../task/report/frontend/FE-HRD-063.md) |
| `FE-HRD-064` | `S-C4-ONB` · `MVP-16` | Keputusan dan koreksi kontrak | ✅ | `FE-HRD-063`, `BE-HRD-080` [BE], `BE-HRD-081` [BE] | [FE-HRD-064.md](../task/report/frontend/FE-HRD-064.md) |
| `FE-HRD-065` | `S-C4-ONB` · `MVP-17` | Masa Percobaan (`FE-HRD-128`, `FE-HRD-129`) | ✅ | `FE-HRD-058`, `BE-HRD-079` [BE] | [FE-HRD-065.md](../task/report/frontend/FE-HRD-065.md) |
| `FE-HRD-066` | `S-C4-ONB` · `MVP-15` | Pendampingan Klinis (`FE-HRD-130`, `FE-HRD-131`) | ✅ | `FE-HRD-058`, `BE-HRD-073` [BE] | [FE-HRD-066.md](../task/report/frontend/FE-HRD-066.md) |
| `FE-HRD-067` | `S-C4-ONB` · `MVP-16` | Batch Data Awal Kontrak (`FE-HRD-134`, `FE-HRD-135`) | ✅ | `FE-HRD-058`, `BE-HRD-082` [BE] | [FE-HRD-067.md](../task/report/frontend/FE-HRD-067.md) |
| `FE-HRD-068` | `S-C4-ONB` · `MVP-15` | Permintaan Akses Pegawai Baru (`FE-HRD-136`) | ✅ | `FE-HRD-058`, `BE-HRD-077` [BE] | [FE-HRD-068.md](../task/report/frontend/FE-HRD-068.md) |
| `FE-HRD-069` | `S-C4-ONB` · `MVP-16` | Pengaturan Onboarding dan Kontrak (`FE-HRD-137`) | ✅ | `FE-HRD-058`, `BE-HRD-069` [BE] | [FE-HRD-069.md](../task/report/frontend/FE-HRD-069.md) |
| `FE-HRD-070` | `S-C4-ONB` · `MVP-16` | Perlu Tindakan (`FE-HRD-123`) dan penanda jumlah menu | ✅ | `FE-HRD-058`, `BE-HRD-084` [BE] | [FE-HRD-070.md](../task/report/frontend/FE-HRD-070.md) |
| `FE-HRD-071` | `S-C4-ONB` · `MVP-14` | Layar lama: tab riwayat kontrak baca saja, label "Probation", master pegawai | ✅ | `FE-HRD-063`, `BE-HRD-065` [BE] | [FE-HRD-071.md](../task/report/frontend/FE-HRD-071.md) |
| `FE-HRD-072` | `S-D2` · amandemen `MVP-14` | Jenis dan lama kontrak pada layar tawaran (`HRD-DEC-095`) | ✅ | `FE-HRD-053`, `BE-HRD-064` [BE] | [FE-HRD-072.md](../task/report/frontend/FE-HRD-072.md) |
| `FE-HRD-073` | `S-C4-ONB` · `MVP-17` | Empat jenis tugas `T15` s.d. `T18` di Kotak Masuk | ✅ | `FE-HRD-064`, `FE-HRD-065`, `FE-HRD-067`, `BE-HRD-068` [BE] | [FE-HRD-073.md](../task/report/frontend/FE-HRD-073.md) |
| `FE-HRD-074` | `S-C4-ONB` · Verifikasi | Verifikasi menu, hak akses, dan keterjangkauan seluruh layar `S-C4-ONB` | ✅ | `FE-HRD-059`, `FE-HRD-062`, `FE-HRD-066`, `FE-HRD-068`, `FE-HRD-069`, `FE-HRD-070`, `FE-HRD-071`, `FE-HRD-072`, `FE-HRD-073` | [FE-HRD-074.md](../task/report/frontend/FE-HRD-074.md) |
| `FE-HRD-075` | `S-B4` · `MVP-18` | Fondasi service client API dan Redux slice roster & on-call | ✅ | `BE-HRD-094` [BE] | [FE-HRD-075.md](../task/report/frontend/FE-HRD-075.md) |
| `FE-HRD-076` | `S-B4` · `MVP-18` | Komponen bilah kendali periode roster dan pemilih unit | ✅ | `FE-HRD-075` | [FE-HRD-076.md](../task/report/frontend/FE-HRD-076.md) |
| `FE-HRD-077` | `S-B4` · `MVP-18` | Layar kisi roster jadwal unit (`FE-HRD-138`) | ✅ | `FE-HRD-076` | [FE-HRD-077.md](../task/report/frontend/FE-HRD-077.md) |
| `FE-HRD-078` | `S-B4` · `MVP-18` | Modal editor penugasan shift individual & pendampingan | ✅ | `FE-HRD-077` | [FE-HRD-078.md](../task/report/frontend/FE-HRD-078.md) |
| `FE-HRD-079` | `S-B4` · `MVP-18` | Panel validasi kepatuhan keselamatan (B1-B7 dan W1-W7) | ✅ | `FE-HRD-077` | [FE-HRD-079.md](../task/report/frontend/FE-HRD-079.md) |
| `FE-HRD-080` | `S-B4` · `MVP-18` | Modal dialog penerbitan roster mandiri kepala unit | ✅ | `FE-HRD-079`, `BE-HRD-091` [BE] | [FE-HRD-080.md](../task/report/frontend/FE-HRD-080.md) |
| `FE-HRD-081` | `S-B4` · `MVP-19` | Modal mutasi shift dan penunjukan ganti petugas langsung | ✅ | `FE-HRD-077`, `BE-HRD-095` [BE] | [FE-HRD-081.md](../task/report/frontend/FE-HRD-081.md) |
| `FE-HRD-082` | `S-B4` · `MVP-19` | Penanda visual koreksi mundur dan dialog pembatasan 3 hari | ✅ | `FE-HRD-077`, `BE-HRD-096` [BE] | [FE-HRD-082.md](../task/report/frontend/FE-HRD-082.md) |
| `FE-HRD-083` | `S-B4` · `MVP-20` | Layar jadwal dan status siaga on-call (`FE-HRD-140`) | ✅ | `FE-HRD-075`, `BE-HRD-098` [BE] | [FE-HRD-083.md](../task/report/frontend/FE-HRD-083.md) |
| `FE-HRD-084` | `S-B4` · `MVP-20` | Modal pencatatan pemanggilan (*call-in*) dan SLA respon | ✅ | `FE-HRD-083`, `BE-HRD-099` [BE] | [FE-HRD-084.md](../task/report/frontend/FE-HRD-084.md) |
| `FE-HRD-085` | `S-B4` · `MVP-20` | Layar pengaturan parameter waktu kerja HRD (`FE-HRD-143`) | ✅ | `FE-HRD-075`, `BE-HRD-101` [BE] | [FE-HRD-085.md](../task/report/frontend/FE-HRD-085.md) |
| `FE-HRD-086` | `S-B4` · `MVP-20` | Penyelarasan portal ESS jadwal kerja dan siaga pegawai | ✅ | `FE-HRD-075`, `BE-HRD-090` [BE] | [FE-HRD-086.md](../task/report/frontend/FE-HRD-086.md) |
| `FE-HRD-087` | `S-B4` · Verifikasi | Verifikasi menu, hak akses peran, dan keterjangkauan layar S-B4 | ✅ | `FE-HRD-077`, `FE-HRD-083`, `FE-HRD-085` | [FE-HRD-087.md](../task/report/frontend/FE-HRD-087.md) |
| `FE-HRD-088` | `S-B1` · `MVP-21` | Service Client API Presensi, Perizinan, dan Pengaturan HR | ✅ | `BE-HRD-111` [BE] | [FE-HRD-088.md](../task/report/frontend/FE-HRD-088.md) |
| `FE-HRD-089` | `S-B1` · `MVP-21` | State management Redux presensi harian, pengecualian, dan koreksi | 🟡 | `FE-HRD-088` | [FE-HRD-089.md](../task/report/frontend/FE-HRD-089.md) |
| `FE-HRD-090` | `S-B1` · `MVP-21` | Layar `FE-HRD-144` — Monitoring Presensi Harian Staf Unit | 🟡 | `FE-HRD-089`, `BE-HRD-106` [BE] | [FE-HRD-090.md](../task/report/frontend/FE-HRD-090.md) |
| `FE-HRD-091` | `S-B1` · `MVP-21` | Drawer Riwayat Log Biometrik Mentah (*Immutable Raw Log Viewer*) | ✅ | `FE-HRD-090`, `BE-HRD-107` [BE] | [FE-HRD-091.md](../task/report/frontend/FE-HRD-091.md) |
| `FE-HRD-092` | `S-B1` · `MVP-23` | Layar `FE-HRD-148` — Penutupan Periode Kehadiran & Sinkronisasi Payroll |  | `FE-HRD-089`, `BE-HRD-115` [BE] | — |
| `FE-HRD-093` | `S-B1` · `MVP-23` | Dialog Guard Penutupan Periode (Blokir Koreksi Pending & Peringatan 1 Tap) |  | `FE-HRD-092`, `BE-HRD-113` [BE] | — |
| `FE-HRD-094` | `S-B1` · `MVP-23` | Modal Buka Kembali Periode Darurat (Reopen Period) HR Manager & Payroll Rollback |  | `FE-HRD-092`, `BE-HRD-114` [BE] | — |
| `FE-HRD-095` | `S-B1` · `MVP-22` | Layar `FE-HRD-145` — Manajemen & Persetujuan Koreksi Kehadiran |  | `FE-HRD-089`, `BE-HRD-110` [BE] | — |
| `FE-HRD-096` | `S-B1` · `MVP-22` | Modal Kuasa HR Koreksi Presensi On-Behalf (Jejak Inisiator & Bukti Wajib) |  | `FE-HRD-095`, `BE-HRD-110` [BE] | — |
| `FE-HRD-097` | `S-B1` · `MVP-22` | Layar `FE-HRD-146` — Permohonan & Persetujuan Izin Pulang Cepat Non-Deductible |  | `FE-HRD-089`, `BE-HRD-108` [BE] | — |
| `FE-HRD-098` | `S-B1` · `MVP-22` | Layar `FE-HRD-147` — Surat Tugas Dinas Luar & Home Care (GPS & Foto Kunjungan) |  | `FE-HRD-089`, `BE-HRD-109` [BE] | — |
| `FE-HRD-099` | `S-B1` · `MVP-21` | Layar `FE-HRD-149` — Pengaturan Kebijakan Presensi HR (Toleransi & Batas Hari) | ✅ | `FE-HRD-088`, `BE-HRD-105` [BE] | [FE-HRD-099.md](../task/report/frontend/FE-HRD-099.md) |
| `FE-HRD-100` | `S-B1` · `MVP-22` | Penyelarasan Portal ESS untuk Absensi & Perizinan Presensi |  | `FE-HRD-088`, `FE-HRD-006` [F3] | — |
| `FE-HRD-101` | `S-B1` · Verifikasi | Verifikasi menu, hak akses peran, dan keterjangkauan seluruh layar S-B1 |  | `FE-HRD-091`, `FE-HRD-093`, `FE-HRD-094`, `FE-HRD-096`, `FE-HRD-097`, `FE-HRD-098`, `FE-HRD-099`, `FE-HRD-100` | — |
| `FE-HRD-102` | `S-B2` · `MVP-24` | API service client, tipe TypeScript & Redux slice cuti | ✅ | `FE-HRD-007` ✅, `BE-HRD-120` [BE] | [FE-HRD-102.md](../task/report/frontend/FE-HRD-102.md) |
| `FE-HRD-103` | `S-B2` · `MVP-24` | Komponen metrik kartu saldo & progress bar pemakaian cuti tahunan | ✅ | `FE-HRD-102` ✅ | [FE-HRD-103.md](../task/report/frontend/FE-HRD-103.md) |
| `FE-HRD-104` | `S-B2` · `MVP-24` | Layar `FE-HRD-150` — Dasbor Kuota & Saldo Cuti ESS & drawer ledger | ✅ | `FE-HRD-102` ✅, `FE-HRD-103` ✅, `BE-HRD-120` [BE] | [FE-HRD-104.md](../task/report/frontend/FE-HRD-104.md) |
| `FE-HRD-105` | `S-B2` · `MVP-24` | Layar `FE-HRD-153` — Manajemen Alokasi Saldo, Carry-Forward & Runner HR | ✅ | `FE-HRD-102` ✅, `BE-HRD-120` [BE] | [FE-HRD-105.md](../task/report/frontend/FE-HRD-105.md) |
| `FE-HRD-106` | `S-B2` · `MVP-24` | Modal penyesuaian saldo resmi HR berdokumen audit (`LeaveBalanceAdjustmentModal`) | ✅ | `FE-HRD-105` ✅, `BE-HRD-128` [BE] | [FE-HRD-106.md](../task/report/frontend/FE-HRD-106.md) |
| `FE-HRD-107` | `S-B2` · `MVP-25` | Komponen kalender cuti, pemilih sesi half-day 0.5 hari & validasi saldo | ✅ | `FE-HRD-102` ✅ | [FE-HRD-107.md](../task/report/frontend/FE-HRD-107.md) |
| `FE-HRD-108` | `S-B2` · `MVP-25` | Modal unggah surat sakit dokter ber-SIP & masking privasi medis K3RS | ✅ | `FE-HRD-102` ✅, `BE-HRD-123` [BE] | [FE-HRD-108.md](../task/report/frontend/FE-HRD-108.md) |
| `FE-HRD-109` | `S-B2` · `MVP-25` | Layar `FE-HRD-151` — Formulir Pengajuan Cuti Mandiri & Cuti Sakit ESS | ✅ | `FE-HRD-107` ✅, `FE-HRD-108` ✅, `BE-HRD-122` [BE] | [FE-HRD-109.md](../task/report/frontend/FE-HRD-109.md) |
| `FE-HRD-110` | `S-B2` · `MVP-25` | Komponen banner peringatan kuota pelayanan (Safe Staffing Coverage Warning) | ✅ | `FE-HRD-102` ✅, `BE-HRD-121` [BE] | [FE-HRD-110.md](../task/report/frontend/FE-HRD-110.md) |
| `FE-HRD-111` | `S-B2` · `MVP-25` | Layar `FE-HRD-152` — Kotak Masuk Approval Atasan & Aksi Approve with Mitigation | ✅ | `FE-HRD-110` ✅, `BE-HRD-124` [BE] | [FE-HRD-111.md](../task/report/frontend/FE-HRD-111.md) |
| `FE-HRD-112` | `S-B2` · `MVP-26` | Layar `FE-HRD-154` — Inisiasi & Pemantauan Pemanggilan Darurat (Leave Recall) | ✅ | `FE-HRD-102` ✅, `BE-HRD-126` [BE] | [FE-HRD-112.md](../task/report/frontend/FE-HRD-112.md) |
| `FE-HRD-113` | `S-B2` · `MVP-26` | Kotak masuk approval Leave Recall HR Manager & restore saldo sisa | ✅ | `FE-HRD-112` ✅, `BE-HRD-127` [BE] | [FE-HRD-113.md](../task/report/frontend/FE-HRD-113.md) |
| `FE-HRD-114` | `S-B2` · `MVP-26` | Layar `FE-HRD-155` — Rekonsiliasi Akhir Cuti, Tabel Retrospektif & Payroll Close | ✅ | `FE-HRD-102` ✅, `BE-HRD-129` [BE] | [FE-HRD-114.md](../task/report/frontend/FE-HRD-114.md) |
| `FE-HRD-115` | `S-B2` · Verifikasi | Verifikasi ujung-ke-ujung antarmuka, linter, dan akses peran Slice S-B2 | ✅ | `FE-HRD-104` ✅, `FE-HRD-106` ✅, `FE-HRD-109` ✅, `FE-HRD-111` ✅, `FE-HRD-113` ✅, `FE-HRD-114` ✅ | [FE-HRD-115.md](../task/report/frontend/FE-HRD-115.md) |
| `FE-HRD-116` | `S-B3` · `MVP-27` | API service client, tipe TypeScript & Redux slice lembur | ✅ | `FE-HRD-007` ✅, `BE-HRD-131` [BE] | [FE-HRD-116.md](../task/report/frontend/FE-HRD-116.md) |
| `FE-HRD-117` | `S-B3` · `MVP-27` | Komponen form dual-mode SPKL & kalkulator kuota lembur PP 35/2021 | 🟡 | `FE-HRD-116` | [FE-HRD-117.md](../task/report/frontend/FE-HRD-117.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-118` | `S-B3` · `MVP-27` | Layar `FE-HRD-156` — Pengajuan SPKL Pra-Dinas & Klaim Cito Pasca-Dinas ESS | 🟡 | `FE-HRD-116`, `FE-HRD-117`, `BE-HRD-134` [BE], `BE-HRD-135` [BE] | [FE-HRD-118.md](../task/report/frontend/FE-HRD-118.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-119` | `S-B3` · `MVP-28` | Komponen kotak masuk persetujuan berjenjang & anti-self-approval | 🟡 | `FE-HRD-116`, `BE-HRD-137` [BE] | [FE-HRD-119.md](../task/report/frontend/FE-HRD-119.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-120` | `S-B3` · `MVP-28` | Layar `FE-HRD-157` — Kotak Masuk Approval Lembur Ruangan (Penyelia & Ka. Unit) | 🟡 | `FE-HRD-119`, `BE-HRD-137` [BE] | [FE-HRD-120.md](../task/report/frontend/FE-HRD-120.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-121` | `S-B3` · `MVP-28` | Drawer investigasi anomali presensi biometrik & bukti logbook fisik dinas | 🟡 | `FE-HRD-116`, `BE-HRD-139` [BE], `BE-HRD-140` [BE] | [FE-HRD-121.md](../task/report/frontend/FE-HRD-121.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-122` | `S-B3` · `MVP-28` | Layar `FE-HRD-158` — Pusat Verifikasi Presensi & Rekonsiliasi Discrepancy HR | 🟡 | `FE-HRD-121`, `BE-HRD-140` [BE] | [FE-HRD-122.md](../task/report/frontend/FE-HRD-122.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-123` | `S-B3` · `MVP-28` | Layar `FE-HRD-159-A` — Kotak Masuk Approval Eskalasi Direktur Medik | 🟡 | `FE-HRD-116`, `BE-HRD-138` [BE] | [FE-HRD-123.md](../task/report/frontend/FE-HRD-123.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-124` | `S-B3` · `MVP-29` | Komponen kartu tiket saldo hak libur pengganti CTO (90 hari masa aktif) | 🟡 | `FE-HRD-116`, `BE-HRD-141` [BE] | [FE-HRD-124.md](../task/report/frontend/FE-HRD-124.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-125` | `S-B3` · `MVP-29` | Layar `FE-HRD-159` — Monitoring Saldo Cuti Pengganti (CTO) & Riwayat Konversi | 🟡 | `FE-HRD-124`, `BE-HRD-141` [BE] | [FE-HRD-125.md](../task/report/frontend/FE-HRD-125.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-126` | `S-B3` · `MVP-29` | Komponen tabel rekonsiliasi & indikator rapel retrospektif lembur | 🟡 | `FE-HRD-116`, `BE-HRD-143` [BE] | [FE-HRD-126.md](../task/report/frontend/FE-HRD-126.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-127` | `S-B3` · `MVP-29` | Layar `FE-HRD-160` — Penutupan Periode Lembur, Cut-Off & Rekonsiliasi Payroll | 🟡 | `FE-HRD-126`, `BE-HRD-143` [BE], `BE-HRD-144` [BE] | [FE-HRD-127.md](../task/report/frontend/FE-HRD-127.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-128` | `S-B3` · `MVP-27` | Layar `FE-HRD-161` — Pengaturan Kebijakan & Kuota Lembur RS (PP 35/2021) | 🟡 | `FE-HRD-116`, `BE-HRD-132` [BE] | [FE-HRD-128.md](../task/report/frontend/FE-HRD-128.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-129` | `S-B3` · Verifikasi | Verifikasi ujung-ke-ujung antarmuka, linter, dan akses peran Slice S-B3 | ✅ | `FE-HRD-118`, `FE-HRD-120`, `FE-HRD-122`, `FE-HRD-123`, `FE-HRD-125`, `FE-HRD-127`, `FE-HRD-128` | [FE-HRD-129.md](../task/report/frontend/FE-HRD-129.md) |
| `FE-HRD-130` | `S-B5` / `MVP-30` | Fondasi frontend modul Payroll, Redux slice `payrollSlice.js`, Axios client & tipe data kanonikal | ✅ | `BE-HRD-147` [BE], `BE-HRD-149` [BE], `BE-HRD-153` [BE] | [FE-HRD-130.md](../task/report/frontend/FE-HRD-130.md) |
| `FE-HRD-131` | `S-B5` / `MVP-30` | Layanan API service frontend integrasi endpoint Run Cycles & Settings | ✅ | `FE-HRD-130` | [FE-HRD-131.md](../task/report/frontend/FE-HRD-131.md) |
| `FE-HRD-132` | `S-B5` / `MVP-30` | Layar `FE-HRD-162` — Dasbor Ringkasan & Monitoring Siklus Penggajian (`/hr/payroll/runs`) | ✅ | `FE-HRD-131` | [FE-HRD-132.md](../task/report/frontend/FE-HRD-132.md) |
| `FE-HRD-133` | `S-B5` / `MVP-30` | Layar `FE-HRD-163` — Wizard Inisiasi Batch & Validasi Precondition Gate Cut-off tgl 20 (`/hr/payroll/runs/new`) | ✅ | `FE-HRD-132` | [FE-HRD-133.md](../task/report/frontend/FE-HRD-133.md) |
| `FE-HRD-134` | `S-B5` / `MVP-30` | Layar `FE-HRD-164` — Lembar Kerja Kalkulasi Gaji, Komponen Rincian & Filter Unit/Profesi (`/hr/payroll/runs/[id]/worksheet`) | ✅ | `FE-HRD-133` | [FE-HRD-134.md](../task/report/frontend/FE-HRD-134.md) |
| `FE-HRD-135` | `S-B5` / `MVP-31` | Layar `FE-HRD-165` — Drawer Investigasi Anomali Variansi & Penginputan Justifikasi Audit (`/hr/payroll/runs/[id]/variances`) | 🟡 | `FE-HRD-134`, `BE-HRD-156` [BE] | [FE-HRD-135.md](../task/report/frontend/FE-HRD-135.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-136` | `S-B5` / `MVP-31` | Komponen Modal Detail Rincian Gaji Pegawai & Komparasi Slip Gaji vs Histori Bulan Lalu (`PayrollEmployeeDetailModal`) | ✅ | `FE-HRD-135` | [FE-HRD-136.md](../task/report/frontend/FE-HRD-136.md) |
| `FE-HRD-137` | `S-B5` / `MVP-31` | Layanan API service frontend & Redux actions untuk alur otorisasi 3-jenjang & anti-self-approval | ✅ | `FE-HRD-136`, `BE-HRD-157` [BE] | [FE-HRD-137.md](../task/report/frontend/FE-HRD-137.md) |
| `FE-HRD-138` | `S-B5` / `MVP-31` | Layar `FE-HRD-166` — Dasbor Otorisasi 3-Jenjang Penggajian & Rekapitulasi Eksekutif Finansial (`/hr/payroll/approvals`) | ✅ | `FE-HRD-137` | [FE-HRD-138.md](../task/report/frontend/FE-HRD-138.md) |
| `FE-HRD-139` | `S-B5` / `MVP-32` | Komponen Modal Konfirmasi Penguncian Immutable & Trigger Serah Terima Finance SHA-256 (`PayrollFinanceHandoffModal`) | ✅ | `FE-HRD-138`, `BE-HRD-158` [BE] | [FE-HRD-139.md](../task/report/frontend/FE-HRD-139.md) |
| `FE-HRD-140` | `S-B5` / `MVP-32` | Dasbor Pemantauan Status Serah Terima Finance & Webhook Reconciler (`/hr/payroll/handoffs`) | ✅ | `FE-HRD-139` | [FE-HRD-140.md](../task/report/frontend/FE-HRD-140.md) |
| `FE-HRD-141` | `S-B5` / `MVP-32` | Layar `FE-HRD-167` — Portal Slip Gaji Mandiri Pegawai (ESS) & Keypad Proteksi PIN Sekunder 6-Digit (`/ess/payslips`) | ✅ | `FE-HRD-140`, `BE-HRD-159` [BE] | [FE-HRD-141.md](../task/report/frontend/FE-HRD-141.md) |
| `FE-HRD-142` | `S-B5` / `MVP-30` | Layar Pengaturan Parameter Penggajian RS, Batas Plafon BPJS & Threshold Variansi (`/hr/payroll/settings`) | 🟡 | `FE-HRD-131` | [FE-HRD-142.md](../task/report/frontend/FE-HRD-142.md) (dikoreksi audit 11 November 2026, lihat [frontend-unfinished-work.md](frontend-unfinished-work.md)) |
| `FE-HRD-143` | `S-B5` / Verifikasi | Verifikasi ujung-ke-ujung antarmuka, linter CI/CD, build Next.js, dan hak akses peran Slice S-B5 | ✅ | `FE-HRD-133`, `FE-HRD-134`, `FE-HRD-135`, `FE-HRD-138`, `FE-HRD-141` | [FE-HRD-143.md](../task/report/frontend/FE-HRD-143.md) |
| `FE-HRD-144` | `S-B6` / `MVP-33` | ✅ PASS | Service API client, adapter TypeScript/Axios, Redux slice & state management restitusi dan potongan |
| `FE-HRD-145` | `S-B6` / `MVP-33` | ✅ PASS | Desain token, status badge, format mata uang & indikator rasio upah 50% |
| `FE-HRD-146` | `S-B6` / `MVP-33` | ✅ PASS | Layar `FE-HRD-168` — Dasbor Utama Restitusi & Pemotongan Gaji (Bagian 1: Ringkasan & Metrik) (`/corporate/hrd/payroll/deductions`) |
| `FE-HRD-147` | `S-B6` / `MVP-33` | ✅ PASS | Layar `FE-HRD-168` — Tabel Peringatan Rasio Potongan Gaji >40% & Trigger Sinkronisasi Finance (Bagian 2) |
| `FE-HRD-148` | `S-B6` / `MVP-33` | ✅ PASS | Layar `FE-HRD-170` — Lembar Kerja Jadwal Angsuran Master Pinjaman (Bagian 1) (`/corporate/hrd/payroll/deductions/schedules`) |
| `FE-HRD-149` | `S-B6` / `MVP-33` | ✅ PASS | Layar `FE-HRD-170` — Modal Simulasi Penegakan Hard-Cap 50% & Visualisasi Pemangkasan Berjenjang (Bagian 2) |
| `FE-HRD-150` | `S-B6` / `MVP-34` | ✅ PASS | Layar `FE-HRD-169` — Pelacak Kuota Plafon Tahunan Pegawai di Portal ESS (Bagian 1) (`/ess/restitution-claims`) |
| `FE-HRD-151` | `S-B6` / `MVP-34` | ✅ PASS | Layar `FE-HRD-169` — Formulir Pengajuan Klaim Restitusi Mandiri di ESS & Dropzone Kuitansi (Bagian 2) |
| `FE-HRD-152` | `S-B6` / `MVP-34` | 🟡 SEBAGIAN | Layar `FE-HRD-172` — Kotak Antrean Verifikasi Persetujuan Bertingkat 3-Tier (Bagian 1) (`/corporate/hrd/payroll/deductions/approvals`) |
| `FE-HRD-153` | `S-B6` / `MVP-34` | 🟡 SEBAGIAN | Komponen `ModalClaimDocumentPreview` — Pratinjau Interaktif Zoom Berkas Nota Kuitansi & Resume Medis |
| `FE-HRD-154` | `S-B6` / `MVP-35` | ✅ PASS | Komponen Drawer Slide-Over `FE-HRD-171` — Investigasi & Koreksi Restitusi Salah Potong Periode Lampau |
| `FE-HRD-155` | `S-B6` / `MVP-35` | 🟡 SEBAGIAN | Layar `FE-HRD-172` & `ModalVoucherQrViewer` — Penerbitan Voucher Kasir Cito 24/7 & Penampil QR Code (Bagian 2) |
| `FE-HRD-156` | `S-B6` / `MVP-35` | ✅ PASS | Layar `FE-HRD-173` — Pengaturan Kebijakan Plafon Benefit per Grade & Batas Pemotongan Upah (`/corporate/hrd/payroll/deductions/settings`) |
| `FE-HRD-157` | `S-B6` / Verifikasi | ✅ PASS | Verifikasi ujung-ke-ujung antarmuka UI/UX, linter CI/CD, build Next.js, dan hak akses peran Slice S-B6 |
| `FE-HRD-158` | `S-D3` / `MVP-36` | ✅ PASS | Service API client, adapter TypeScript/Axios, Redux slice & state management manfaat kesehatan |
| `FE-HRD-159` | `S-D3` / `MVP-36` | ✅ PASS | Registrasi peta navigasi sidebar, rute `/corporate/hrd/benefits/*`, rute ESS & permission guards |
| `FE-HRD-160` | `S-D3` / `MVP-36` | ✅ PASS | Komponen reusable & token UI manfaat (StatusBadge, kartu limit, meteran utilisasi, uploader KK/akta) |
| `FE-HRD-161` | `S-D3` / `MVP-36` | 🟡 SEBAGIAN | Layar `FE-HRD-174` — Dasbor Analitik Manfaat Kesehatan, Utilisasi Plafon & Alert Batas Usia Tanggungan |
| `FE-HRD-162` | `S-D3` / `MVP-36` | ✅ PASS | Layar `FE-HRD-175` — Master Katalog Paket Manfaat 3-Lapisan, Pagu Plafon & Diskon Tarif Staf RS |
| `FE-HRD-163` | `S-D3` / `MVP-36` | ✅ PASS | Komponen Drawer `FE-HRD-175` — Matriks Kelayakan Grade Staf Rumah Sakit ke Kelas Kamar Rawat Inap |
| `FE-HRD-164` | `S-D3` / `MVP-36` | ✅ PASS | Layar `FE-HRD-176` — Monitoring Kepesertaan Asuransi Pegawai, Aktivasi H+0 vs Pasca-Percobaan 3 Bulan |
| `FE-HRD-165` | `S-D3` / `MVP-36` | ✅ PASS | Layar `FE-HRD-177` — Antrean Meja Kerja Verifikasi Dokumen Kependudukan Tanggungan Keluarga HR |
| `FE-HRD-166` | `S-D3` / `MVP-36` | ✅ PASS | Layar `FE-HRD-177` (ESS View) — Portal Mandiri ESS Keluarga Saya & Indikator Kuota 1 Pasangan + 3 Anak |
| `FE-HRD-167` | `S-D3` / `MVP-37` | 🟡 SEBAGIAN | Layar `FE-HRD-178` — Inquiry Hak Berobat Staf di RS Sendiri & Buku Besar Mutasi Saldo Plafon SIMRS |
| `FE-HRD-168` | `S-D3` / `MVP-37` | 🟡 SEBAGIAN | Modal Dialog `FE-HRD-178` — Penyesuaian Manual Saldo Plafon Berobat RS oleh HR Manajer |
| `FE-HRD-169` | `S-D3` / `MVP-38` | ✅ PASS | Layar `FE-HRD-179` — Meja Kerja Monitoring, Audit Kuitansi & Settlement Klaim Koordinasi Manfaat (COB) |
| `FE-HRD-170` | `S-D3` / `MVP-38` | 🟡 SEBAGIAN | Layar `FE-HRD-179` (ESS View) — Formulir Mandiri Pengajuan Klaim COB Berlampiran Mandatory SEP BPJS |
| `FE-HRD-171` | `S-D3` / `MVP-38` & Verifikasi | ✅ PASS | Verifikasi ujung-ke-ujung antarmuka UI/UX, linter CI/CD, build Next.js, dan hak akses peran Slice S-D3 |
| `FE-HRD-172` | `S-D4` / `MVP-39` | ✅ PASS | Redux Toolkit Slice `hrServiceSlice` & Endpoint Client Axios (Kontrak v17) |
| `FE-HRD-173` | `S-D4` / `MVP-39` | 🟡 SEBAGIAN | Layar `FE-HRD-181` — Pengaturan Katalog Layanan SDM & Target Jam SLA (`/corporate/hrd/services/catalog`) |
| `FE-HRD-174` | `S-D4` / `MVP-39` | 🟡 SEBAGIAN | Editor Template Naskah Dinas Token Dinamis & Konfigurasi Pola Penomoran Gapless (`/corporate/hrd/services/catalog/templates/[id]`) |
| `FE-HRD-175` | `S-D4` / `MVP-39` | 🟡 SEBAGIAN | Layar `FE-HRD-180` (Dasbor KPI) — Dasbor Meja Bantuan (Helpdesk) SDM & 4 Kartu KPI SLA Operasional |
| `FE-HRD-176` | `S-D4` / `MVP-39` | 🟡 SEBAGIAN | Layar `FE-HRD-180` (Antrean 3-Tier) — Antrean Tiket Helpdesk Berjenjang & Indikator Countdown Timer SLA Real-Time |
| `FE-HRD-177` | `S-D4` / `MVP-39` | 🟡 SEBAGIAN | Komponen `ModalReassignTicket` — Modal Re-assign Penugasan Analis SDM Cepat & Penyeimbangan Beban Kerja |
| `FE-HRD-178` | `S-D4` / `MVP-39` | 🟡 SEBAGIAN | Layar `FE-HRD-182` (Katalog Tiles) — Portal Mandiri Permohonan Layanan SDM Staf (ESS) & Estimasi Jam SLA |
| `FE-HRD-179` | `S-D4` / `MVP-39` | 🟡 SEBAGIAN | Komponen `TicketSubmissionWizard` — Formulir Pengajuan Tiket ESS Multi-Langkah & Validasi Drag-and-Drop Lampiran 5MB |
| `FE-HRD-180` | `S-D4` / `MVP-40` | 🟡 SEBAGIAN | Layar `FE-HRD-183` & `DrawerTicketProcessing` — Drawer Pemrosesan Tiket Analis, Penampil Berkas Side-by-Side & Utas Klarifikasi |
| `FE-HRD-181` | `S-D4` / `MVP-40` | 🟡 SEBAGIAN | Komponen `ProfileChangeVerificationPanel` — Verifikasi Berkas Perubahan Rekening/Status & Eksekusi Mutasi Profil Sub-02 |
| `FE-HRD-182` | `S-D4` / `MVP-40` | 🟡 SEBAGIAN | Layar `FE-HRD-184` & `ModalDocumentAuthorization` — Modal Otorisasi Surat Resmi Pejabat, Gaji Read-Only & Generator QR Code |
| `FE-HRD-183` | `S-D4` / `MVP-40` | 🟡 SEBAGIAN | Komponen `ModalDocumentRevocation` — Modal Pencabutan Keabsahan Surat Resmi (Revokasi) Ber-Input Nomor BAP |
| `FE-HRD-184` | `S-D4` / `MVP-41` | 🟡 SEBAGIAN | Layar `FE-HRD-185` — Halaman Portal Publik Verifikasi Keabsahan Dokumen (`/verify/document/[hash]`) & Dialog Input 4-Digit PIN Masked Gaji |
| `FE-HRD-185` | `S-D4` / `MVP-41` & Verifikasi | ✅ PASS | Layar `FE-HRD-186` — Pusat Arsip Digital Kepegawaian (Dokumen Saya ESS), Kartu PIN Rahasia & Verifikasi Ujung-ke-Ujung Slice S-D4 |
| `FE-HRD-186` | `S-C1` / `MVP-42` | ✅ PASS | Redux Toolkit Slice `credentialingSlice`, Axios Client, CSS Module & Utilitas Display Kewenangan Klinis |
| `FE-HRD-187` | `S-C1` / `MVP-42` | 🟡 SEBAGIAN | Layar `FE-HRD-187` — Portal Kredensialing & Rekredensialing Mandiri Wizard 4-Langkah (ESS) |
| `FE-HRD-188` | `S-C1` / `MVP-42` | 🟡 SEBAGIAN | Layar `FE-HRD-188` — Dashboard Verifikasi Sumber Primer (PSV) Staf Kredensialing & Validasi SatuSehat SDMK |
| `FE-HRD-189` | `S-C1` / `MVP-42` | 🟡 SEBAGIAN | Radar Visual Peringatan Dini Lisensi SIP 5 Tahun & Banner Penegakan Fail-Closed EMR |
| `FE-HRD-190` | `S-C1` / `MVP-43` | 🟡 SEBAGIAN | Layar `FE-HRD-189` — Komponen Seleksi 3 Komite Mandiri & Katalog Master Buku Putih (Whitebook) Kewenangan Klinis |
| `FE-HRD-191` | `S-C1` / `MVP-43` | 🟡 SEBAGIAN | Layar `FE-HRD-190` — Meja Kerja Penilaian Asesmen Panel Mitra Bestari (Peer Review) & Anti Self-Assessment |
| `FE-HRD-192` | `S-C1` / `MVP-43` | 🟡 SEBAGIAN | Layar `FE-HRD-191` — Konsol Sidang Pleno Komite & Resolusi Konsensus Divergensi Skor Mitra Bestari |
| `FE-HRD-193` | `S-C1` / `MVP-43` | 🟡 SEBAGIAN | Layar `FE-HRD-191` — Konsol Pengesahan SPK & RKK Direktur Utama RS, e-Sign Legal & Live Preview PDF |
| `FE-HRD-194` | `S-C1` / `MVP-43` | 🟡 SEBAGIAN | Halaman Verifikasi Publik SPK & RKK Nakes Berbasis Pemindaian QR Code Ber-Hash Kriptografis |
| `FE-HRD-195` | `S-C1` / `MVP-44` | 🟡 SEBAGIAN | Layar `FE-HRD-192` — Direktori Registri Kewenangan Klinis (RKK) Rumah Sakit & Hak Order SIMRS |
| `FE-HRD-196` | `S-C1` / `MVP-44` | 🟡 SEBAGIAN | Dialog Penegakan Hard-Stop & Banner Lisensi Kedaluwarsa di SIMRS/EMR (Integrasi INT-HR-69) |
| `FE-HRD-197` | `S-C1` / `MVP-44` | 🟡 SEBAGIAN | Modal Persetujuan Co-Signature Supervisi & Timer 24 Jam Supervisor (Integrasi BE-HRD-217) |
| `FE-HRD-198` | `S-C1` / `MVP-44` | 🟡 SEBAGIAN | Komponen Life-Saving Emergency Override & Konsol Audit Insiden Mutu KMKP (Integrasi INT-HR-70) |
| `FE-HRD-199` | `S-C1` / `MVP-44` | 🟡 SEBAGIAN | Layar `FE-HRD-193` & `FE-HRD-194` — Konsol Penugasan Cepat Temporary & Emergency serta Radar Rekredensialing 3 Tahunan |
| `FE-HRD-200` | `S-C1` / Verifikasi | ✅ PASS | Verifikasi Integrasi Menyeluruh Next.js App Router, Audit Linting 0 Error & Standalone Production Build |
| `FE-HRD-201` | `S-C1-OPPE` / `MVP-45` | ✅ PASS | Inisialisasi Redux Slice `clinicalPerformanceSlice`, Axios Client API, CSS Module & Utilitas Display Kinerja Klinis |
| `FE-HRD-202` | `S-C1-OPPE` / `MVP-45` | 🟡 SEBAGIAN | Layar `FE-HRD-201` — Dashboard Kinerja Klinis & Visualisasi Radar OPPE Dokter Spesialis 6 Pilar ACGME/KARS |
| `FE-HRD-203` | `S-C1-OPPE` / `MVP-45` | 🟡 SEBAGIAN | Layar `FE-HRD-202` — Manajemen Periode & Pengaturan Matriks Indikator OPPE KSM (VR-OPP-01) |
| `FE-HRD-204` | `S-C1-OPPE` / `MVP-45` | 🟡 SEBAGIAN | Modal Pengaturan Ambang Batas & Bobot Indikator Spesifik KSM (`OppeIndicatorConfigModal`) |
| `FE-HRD-205` | `S-C1-OPPE` / `MVP-45` | 🟡 SEBAGIAN | Layar `FE-HRD-203` — Formulir Evaluasi Kualitatif Peer Review Sejawat KSM Skala Likert 1-5 |
| `FE-HRD-206` | `S-C1-OPPE` / `MVP-45` | 🟡 SEBAGIAN | Komponen Peringatan Kerahasiaan Hak Istimewa Peer Review Privilege & Modal Read-Audit Log |
| `FE-HRD-207` | `S-C1-OPPE` / `MVP-46` | 🟡 SEBAGIAN | Layar `FE-HRD-204` — Dashboard Pusat Deteksi Pemicu Kritis & Pelacakan Kasus FPPE (Alert Sentinel & > 2 SD) |
| `FE-HRD-208` | `S-C1-OPPE` / `MVP-46` | 🟡 SEBAGIAN | Modal Inisiasi Penugasan FPPE & Penetapan Dokter Proctor Senior (`FppeInitiateModal`) |
| `FE-HRD-209` | `S-C1-OPPE` / `MVP-46` | 🟡 SEBAGIAN | Layar `FE-HRD-205` — Dashboard Pemantauan Proctoring Staf Baru & Prosedur Baru (Target Kasus & Sisa Waktu) |
| `FE-HRD-210` | `S-C1-OPPE` / `MVP-46` | 🟡 SEBAGIAN | Modal Entri Logbook Kasus Terpseudonim & Antarmuka Verifikasi Proctor (`FppeCaseLogEntryModal`) |
| `FE-HRD-211` | `S-C1-OPPE` / `MVP-46` | 🟡 SEBAGIAN | Layar `FE-HRD-206` — Sidang Pleno Kesimpulan & Rekomendasi Wewenang FPPE (4 Opsi Putusan & Remediation Plan) |
| `FE-HRD-212` | `S-C1-OPPE` / `MVP-47` | 🟡 SEBAGIAN | Layar `FE-HRD-207` — Kalkulator Rapor Komposit 3 Tahun Rekredensialing (25:35:40) & Visualisasi Tier |
| `FE-HRD-213` | `S-C1-OPPE` / `MVP-47` | 🟡 SEBAGIAN | Modal Diskresi Penyesuaian Tier Kelayakan Rekredensialing (`RecredentialingTierOverrideModal`) |
| `FE-HRD-214` | `S-C1-OPPE` / `MVP-47` | 🟡 SEBAGIAN | Layar `FE-HRD-208` — Konsol Sinkronisasi Putusan Rekredensialing ke SIMRS EMR & Audit Bypass |
| `FE-HRD-215` | `S-C1-OPPE` / Verifikasi | ✅ PASS | Verifikasi Integrasi Menyeluruh Next.js App Router, Audit Linting 0 Error & Standalone Production Build |
| `FE-HRD-216` | `S-C2` / `MVP-48` | ✅ PASS | Inisialisasi Redux Slice `learningDevelopmentSlice.ts`, Axios Client API & DTO Types Kontrak `v20` |
| `FE-HRD-217` | `S-C2` / `MVP-48` | 🟡 SEBAGIAN | Dashboard Pelatihan Pegawai, Sertifikasi & Radar Kedaluwarsa (`FE-HRD-216`) |
| `FE-HRD-218` | `S-C2` / `MVP-48` | 🟡 SEBAGIAN | Konfigurasi Aturan Pelatihan Wajib Akreditasi & Penugasan Unit (`FE-HRD-217`) |
| `FE-HRD-219` | `S-C2` / `MVP-48` | 🟡 SEBAGIAN | Modal Konfigurasi Aturan Pelatihan Wajib & Ambang Nilai (`MandatoryTrainingRuleModal`) |
| `FE-HRD-220` | `S-C2` / `MVP-49` | 🟡 SEBAGIAN | Manajemen Sesi Pelatihan, Kalender Batch Kelas & Presensi Kehadiran (`FE-HRD-218`) |
| `FE-HRD-221` | `S-C2` / `MVP-49` | 🟡 SEBAGIAN | Modal Presensi QR Code / RFID & Rekapitulasi Hadir Sesi (`TrainingAttendanceQrModal`) |
| `FE-HRD-222` | `S-C2` / `MVP-49` | 🟡 SEBAGIAN | Lembar Penilaian Asesmen Teori Post-Test & Ujian Simulasi Klinis OSCE (`FE-HRD-219`) |
| `FE-HRD-223` | `S-C2` / `MVP-49` | 🟡 SEBAGIAN | Modal Rubrik Checklist OSCE & Tanda Tangan Digital Assessor (`OsceClinicalEvaluationModal`) |
| `FE-HRD-224` | `S-C2` / `MVP-49` | 🟡 SEBAGIAN | Registri Portofolio e-Sertifikat Pegawai & Verifikasi Dokumen Eksternal (`FE-HRD-220`) |
| `FE-HRD-225` | `S-C2` / `MVP-50` | 🟡 SEBAGIAN | Rencana Program Tahunan TNA & Monitoring Pagu Anggaran Unit (`FE-HRD-221`) |
| `FE-HRD-226` | `S-C2` / `MVP-50` | 🟡 SEBAGIAN | Tata Kelola Kontrak Ikatan Dinas Retensi Nakes & Perhitungan Denda Resign (`FE-HRD-222`) |
| `FE-HRD-227` | `S-C2` / `MVP-50` | 🟡 SEBAGIAN | Modal Draf Akta Ikatan Dinas & Rincian Tagihan Penalti Resign (`TrainingBondContractModal`) |
| `FE-HRD-228` | `S-C2` / `MVP-50` | 🟡 SEBAGIAN | Portofolio 50 SKP Nakes 5 Tahunan & Visual Progress Bar Radial (`FE-HRD-223`) |
| `FE-HRD-229` | `S-C2` / `MVP-50` | 🟡 SEBAGIAN | Modal Verifikasi ID Sertifikat & Sinkronisasi Plataran Sehat Kemenkes (`PlataranSehatSyncModal`) |
| `FE-HRD-230` | `S-C2` / Verifikasi | ✅ PASS | Verifikasi Integrasi Menyeluruh Next.js App Router, Linting 0 Error & UAT Acceptance `S-C2` |
| `FE-HRD-231` | `S-C3` / `MVP-51` | ✅ PASS | Redux Slice State, Base Types & Service API Client Kontrak v21 |
| `FE-HRD-232` | `S-C3` / `MVP-51` | 🟡 SEBAGIAN | Dashboard Kinerja & Karier Klinis Nakes (`FE-HRD-231`) |
| `FE-HRD-233` | `S-C3` / `MVP-51` | 🟡 SEBAGIAN | Katalog Indikator KPI & Core Values BerAKHLAK (`FE-HRD-232`) |
| `FE-HRD-234` | `S-C3` / `MVP-51` | 🟡 SEBAGIAN | Siklus Kinerja & Cascading Sasaran Mutu Unit (`FE-HRD-233`) |
| `FE-HRD-235` | `S-C3` / `MVP-51` | 🟡 SEBAGIAN | Modal Input Sasaran Kerja Pegawai & Bobot Target (`KpiGoalFormModal`) |
| `FE-HRD-236` | `S-C3` / `MVP-51` | 🟡 SEBAGIAN | Portal Lembar Penilaian Berjenjang Tiga Tingkat (`FE-HRD-234`) |
| `FE-HRD-237` | `S-C3` / `MVP-51` | 🟡 SEBAGIAN | Modal Peer Review Sejawat & Umpan Balik Kualitatif (`PeerReviewFeedbackModal`) |
| `FE-HRD-238` | `S-C3` / `MVP-52` | 🟡 SEBAGIAN | Konsol Sidang Kalibrasi Manajemen & Guiding Distribution (`FE-HRD-235`) |
| `FE-HRD-239` | `S-C3` / `MVP-52` | 🟡 SEBAGIAN | Modal Nota Justifikasi Kalibrasi Manajemen (`CalibrationJustificationModal`) |
| `FE-HRD-240` | `S-C3` / `MVP-52` | 🟡 SEBAGIAN | Tata Kelola PIP 60 Hari Kalender & Logbook Mingguan Mentor (`FE-HRD-236`) |
| `FE-HRD-241` | `S-C3` / `MVP-52` | 🟡 SEBAGIAN | Modal Logbook Mingguan & Sidang Putusan PIP (`PipWeeklyLogbookModal`) |
| `FE-HRD-242` | `S-C3` / `MVP-53` | 🟡 SEBAGIAN | Portofolio Kenaikan Jenjang Klinis Perawat/Nakes PK I-V (`FE-HRD-237`) |
| `FE-HRD-243` | `S-C3` / `MVP-53` | 🟡 SEBAGIAN | Portal Pengajuan Sanggah Banding Nilai SLA 7/14 Hari (`FE-HRD-238`) |
| `FE-HRD-244` | `S-C3` / `MVP-53` | 🟡 SEBAGIAN | Modal Putusan Majelis Banding Direktur SDM (`DisputeResolutionModal`) |
| `FE-HRD-245` | `S-C3` / Verifikasi | ✅ PASS | Verifikasi Menyeluruh Next.js App Router, Linting 0 Error & UAT Acceptance `S-C3` |
| `FE-HRD-246` | `S-C5` / `MVP-54` | ✅ PASS | Redux State Slice `employeeRelationSlice.ts`, Axios Services & Types Definition |
| `FE-HRD-247` | `S-C5` / `MVP-54` | ✅ PASS | Sidebar Menu Navigasi, Breadcrumbs & RBAC Route Guards `/hrd/relations` |
| `FE-HRD-248` | `S-C5` / `MVP-54` | 🟡 SEBAGIAN | Layar Dasbor Hubungan Industrial & Radar Exit Clearance (`/hrd/relations/dashboard`) |
| `FE-HRD-249` | `S-C5` / `MVP-54` | 🟡 SEBAGIAN | Layar Pelaporan Insiden Disiplin, Filter Just Culture RS & Validasi KMKP (`/hrd/relations/incidents`) |
| `FE-HRD-250` | `S-C5` / `MVP-54` | 🟡 SEBAGIAN | Komponen Dialog Modal Berita Acara Pemeriksaan (BAP) & Unggah Bukti Sidang (`BapRecordModal`) |
| `FE-HRD-251` | `S-C5` / `MVP-54` | 🟡 SEBAGIAN | Layar Penerbitan & Monitoring Surat Peringatan (SP 1-3 Masa 6 Bulan PP 35/2021) (`/hrd/relations/sanctions`) |
| `FE-HRD-252` | `S-C5` / `MVP-54` | 🟡 SEBAGIAN | Komponen Modal Sanggah Banding Sanksi SP & Risalah Putusan Sidang Direksi (`SanctionAppealModal`) |
| `FE-HRD-253` | `S-C5` / `MVP-55` | 🟡 SEBAGIAN | Layar Pengelolaan Skorsing Investigasi (*Paid Suspension*) & Status Akses SIMRS (`/hrd/relations/suspensions`) |
| `FE-HRD-254` | `S-C5` / `MVP-55` | 🟡 SEBAGIAN | Layar Pengaduan Keluhan Staf (Grievance ESS) & Notulen Mediasi SLA 14 Hari (`/hrd/relations/grievance`) |
| `FE-HRD-255` | `S-C5` / `MVP-55` | 🟡 SEBAGIAN | Komponen Modal & Portal Whistleblower Terenkripsi Asimetris Anti-Retaliasi (`WhistleblowerModal`) |
| `FE-HRD-256` | `S-C5` / `MVP-55` | 🟡 SEBAGIAN | Layar Manajemen Notulen & Risalah Forum Perundingan Bipartit RS (`/hrd/relations/bipartite`) |
| `FE-HRD-257` | `S-C5` / `MVP-56` | 🟡 SEBAGIAN | Layar Pengajuan Terminasi Kerja (Resign 30 Hari, Kontrak, Pensiun, SP3) & Radar PKWT (`/hrd/separations`) |
| `FE-HRD-258` | `S-C5` / `MVP-56` | 🟡 SEBAGIAN | Komponen Kalkulator Pesangon Deterministik PP 35/2021 & Lembar Rincian Finansial (`SeveranceCalculatorCard`) |
| `FE-HRD-259` | `S-C5` / `MVP-56` | 🟡 SEBAGIAN | Layar Pusat Kendali Exit Clearance Multi-Departemen 5 Unit (`/hrd/separations/exit-clearance`) |
| `FE-HRD-260` | `S-C5` / Verifikasi | 🟡 SEBAGIAN | Modal Signoff Kliring Unit, Indikator Gembok Payroll Final Sub-10 & Uji Linter Build |
| `FE-HRD-261` | `S-C6` / `MVP-57` | ✅ PASS | Setup TypeScript Interfaces, Redux Slice `occupationalHealthSlice.ts` & Axios Client |
| `FE-HRD-262` | `S-C6` / `MVP-57` | 🟡 SEBAGIAN | Komponen Visual K3RS: StatusBadge Kelaikan, Alert Kerahasiaan Medis & Indikator |
| `FE-HRD-263` | `S-C6` / `MVP-57` | 🟡 SEBAGIAN | Layar Dasbor Utama K3RS Rumah Sakit (`FE-HRD-261`) pada `/hrd/occupational-health` |
| `FE-HRD-264` | `S-C6` / `MVP-57` | 🟡 SEBAGIAN | Layar Rekam Kesehatan Staf & Evaluasi Fitness to Work (`FE-HRD-262`) |
| `FE-HRD-265` | `S-C6` / `MVP-57` | 🟡 SEBAGIAN | Modal Dialog Pengesahan Kelaikan Kerja Dokter Hiperkes (`FitnessToWorkModal.tsx`) |
| `FE-HRD-266` | `S-C6` / `MVP-57` | 🟡 SEBAGIAN | Layar Manajemen Pembatasan Tugas Medis (*Work Restriction*) (`FE-HRD-263`) |
| `FE-HRD-267` | `S-C6` / `MVP-57` | 🟡 SEBAGIAN | Modal Asesmen Kembali Bekerja (*Return-to-Work*) & Pelepasan Hard-Stop Roster |
| `FE-HRD-268` | `S-C6` / `MVP-58` | 🟡 SEBAGIAN | Layar Konsol Tanggap Darurat Pajanan Darah & Tertusuk Jarum (`FE-HRD-264`) |
| `FE-HRD-269` | `S-C6` / `MVP-58` | 🟡 SEBAGIAN | Widget Tombol Panik Darurat Tertusuk Jarum Mobile ESS Staf (`NeedleStickPanicButton.tsx`) |
| `FE-HRD-270` | `S-C6` / `MVP-58` | 🟡 SEBAGIAN | Layar Radar Imunisasi & Vaksinasi Tenaga Medis (`FE-HRD-265`) |
| `FE-HRD-271` | `S-C6` / `MVP-58` | 🟡 SEBAGIAN | Layar Surveilans Paparan Bahaya Ruangan Khusus TLD Radiasi (`FE-HRD-266`) |
| `FE-HRD-272` | `S-C6` / `MVP-59` | 🟡 SEBAGIAN | Layar Pusat Penjadwalan & Pelaksanaan MCU Berkala STARKES (`FE-HRD-267`) |
| `FE-HRD-273` | `S-C6` / `MVP-59` | 🟡 SEBAGIAN | Layar Registrasi Kecelakaan Kerja KAK/PAK & Olah TKP (`FE-HRD-268`) |
| `FE-HRD-274` | `S-C6` / `MVP-59` | 🟡 SEBAGIAN | Modal Generator Ekspor Formulir Klaim JKK BPJS Ketenagakerjaan Tahap I-II |
| `FE-HRD-275` | `S-C6` / Verifikasi | ✅ PASS | Integrasi Navigasi Sidebar K3RS, Indikator Hard-Stop Roster Sub-06 & Uji Linter Build E2E |
| `FE-HRD-276` | `S-A7-V2` / `MVP-60` | ✅ PASS | State Management Redux Toolkit `unifiedApprovalSlice.ts` & Sinkronisasi Token Auth |
| `FE-HRD-277` | `S-A7-V2` / `MVP-60` | 🟡 SEBAGIAN | Layar Utama Kotak Masuk Persetujuan Terpadu (`/hrd/approvals/inbox` - `FE-HRD-276`) |
| `FE-HRD-278` | `S-A7-V2` / `MVP-60` | 🟡 SEBAGIAN | Komponen Tab Filter Antrean Kotak Masuk (*Pending*, *Urgent H-4*, *Delegated*, *Approved*) |
| `FE-HRD-279` | `S-A7-V2` / `MVP-60` | 🟡 SEBAGIAN | Kartu Rangkuman Metrik Kotak Masuk & Penanda Badge SLA Realtime |
| `FE-HRD-280` | `S-A7-V2` / `MVP-61` | 🟡 SEBAGIAN | Layar Detail & Riwayat Permohonan Lengkap (`/hrd/approvals/[id]` - `FE-HRD-277`) |
| `FE-HRD-281` | `S-A7-V2` / `MVP-61` | 🟡 SEBAGIAN | Visual Alur Tahapan Persetujuan & Kartu Indikator Risiko Hijau/Merah |
| `FE-HRD-282` | `S-A7-V2` / `MVP-61` | 🟡 SEBAGIAN | Modal Dialog Persetujuan Tunggal & Verifikasi PIN Otorisasi |
| `FE-HRD-283` | `S-A7-V2` / `MVP-61` | 🟡 SEBAGIAN | Modal Dialog Penolakan Berkas (Wajib Min 15 Karakter) & Modal Pengembalian Revisi |
| `FE-HRD-284` | `S-A7-V2` / `MVP-62` | 🟡 SEBAGIAN | Layar Pengaturan Pendelegasian Wewenang Plh (`/hrd/approvals/delegation` - `FE-HRD-278`) |
| `FE-HRD-285` | `S-A7-V2` / `MVP-62` | 🟡 SEBAGIAN | Modal Surat Kuasa Digital Penunjukan Plh & Pemilihan Pengganti Sah |
| `FE-HRD-286` | `S-A7-V2` / `MVP-62` | 🟡 SEBAGIAN | Modal Persetujuan Massal Bersyarat (*Conditional Bulk Approval Modal*) & Filter Guard Item Berisiko |
| `FE-HRD-287` | `S-A7-V2` / `MVP-62` | 🟡 SEBAGIAN | Layar Konfigurasi Matriks Alur Persetujuan (`/hrd/approvals/matrix` - `FE-HRD-279`) & Konsol Monitoring Overdue (`FE-HRD-283`) |
| `FE-HRD-288` | `S-A7-V2` / Verifikasi | 🟡 SEBAGIAN | Layar Portal Layanan Mandiri Karyawan ESS (`/hrd/self-service` - `FE-HRD-280`) & Menu Cepat Permohonan |
| `FE-HRD-289` | `S-A7-V2` / Verifikasi | 🟡 SEBAGIAN | Layar Pelacak Riwayat Permohonan Mandiri (`FE-HRD-281`) & Lembar Perbaikan Draf Revisi (`FE-HRD-282`) |
| `FE-HRD-290` | `S-A7-V2` / Verifikasi | ✅ PASS | Pendaftaran Navigasi Sidebar, Prompt Otomasi Plh Cuti (`INT-HR-94`) & Uji Build Linting Standalone |
| `FE-HRD-291` | `S-D5` / `MVP-63` | ✅ PASS | State Management Redux Toolkit `businessTravelSlice.ts` & Sinkronisasi Token |
| `FE-HRD-292` | `S-D5` / `MVP-63` | 🟡 SEBAGIAN | Layar Dasbor Utama Perjalanan Dinas & Manajemen Biaya (`/hrd/travel/dashboard` - `FE-HRD-291`) |
| `FE-HRD-293` | `S-D5` / `MVP-63` | 🟡 SEBAGIAN | Kartu Analitik Realisasi Anggaran Unit Cost Center & Radar Settlement Aktif |
| `FE-HRD-294` | `S-D5` / `MVP-63` | 🟡 SEBAGIAN | Layar Matriks Tarif Lumpsum Uang Harian Per Diem & Zonasi Wilayah (`/hrd/travel/rates-and-policies` - `FE-HRD-297`) |
| `FE-HRD-295` | `S-D5` / `MVP-64` | 🟡 SEBAGIAN | Layar Pusat Penugasan & Riwayat SPPD Pegawai (`/hrd/travel/requests` - `FE-HRD-292`) |
| `FE-HRD-296` | `S-D5` / `MVP-64` | 🟡 SEBAGIAN | Layar Formulir Pengajuan SPPD Mandiri Multi-Step (`/hrd/travel/requests/create` - `FE-HRD-293`) |
| `FE-HRD-297` | `S-D5` / `MVP-64` | 🟡 SEBAGIAN | Komponen Kalkulator Otomatis Lumpsum Uang Harian & Pilihan Hotel Sesuai Plafon Grade |
| `FE-HRD-298` | `S-D5` / `MVP-64` | 🟡 SEBAGIAN | Layar Konsol Manajemen Uang Muka Kerja (Advance) (`/hrd/travel/advances` - `FE-HRD-294`) & Modal Pencairan Kasir |
| `FE-HRD-299` | `S-D5` / `MVP-65` | 🟡 SEBAGIAN | Layar Lembar Rekonsiliasi & Settlement Pengeluaran Riil (`/hrd/travel/settlements` - `FE-HRD-295`) |
| `FE-HRD-300` | `S-D5` / `MVP-65` | 🟡 SEBAGIAN | Modal Formulir Deklarasi Biaya Darurat Tanpa Kuitansi (*MissingReceiptDeclarationModal.tsx*) |
| `FE-HRD-301` | `S-D5` / `MVP-65` | 🟡 SEBAGIAN | Modal Tanda Terima Setor Balik Kasir Kelebihan Uang Muka (*RefundSettlementModal.tsx*) & Bukti Kurang Bayar |
| `FE-HRD-302` | `S-D5` / `MVP-65` | 🟡 SEBAGIAN | Layar Radar Pemantau Keterlambatan Settlement Overdue (`/hrd/travel/overdue-settlements` - `FE-HRD-298`) |
| `FE-HRD-303` | `S-D5` / Verifikasi | 🟡 SEBAGIAN | Layar Portal Klaim Pengeluaran Operasional Non-Trip (`/hrd/travel/expenses` - `FE-HRD-296`) |
| `FE-HRD-304` | `S-D5` / Verifikasi | 🟡 SEBAGIAN | Modal Upload Struk Bukti Kasir & Drawer Pratinjau Faktur Pajak / Boarding Pass |
| `FE-HRD-305` | `S-D5` / Verifikasi | ✅ PASS | Integrasi Navigasi Sidebar, Penanda Indikator Hard-Stop Roster Sub-06 & Uji Linter Build Standalone |


---

## Kartu Detail Task Frontend


### Grafik F18 — `S-B4` `MVP-18`: Service API, state Redux, dan layar kisi roster

```text
BE-HRD-094 [BE] ─> FE-HRD-075 ─> FE-HRD-076 ─> FE-HRD-077 ─┬─> FE-HRD-078
                                                             │
                                                             ├─> FE-HRD-079
                                                             │
                                                             ├─> FE-HRD-081
                                                             │
                                                             └─> FE-HRD-082
```

### Grafik F19 — `S-B4` `MVP-18`: Komponen validasi dan penerbitan roster mandiri

```text
FE-HRD-079 [F18] ──┐
                   │
BE-HRD-090 [BE] ───┴─> FE-HRD-080
```

### Grafik F20 — `S-B4` `MVP-19`: Ganti petugas dan koreksi mundur

```text
FE-HRD-081 [F18] ──┐
                   │
BE-HRD-096 [BE] ───┘

FE-HRD-082 [F18] ──┐
                   │
BE-HRD-098 [BE] ───┘
```

### Grafik F21 — `S-B4` `MVP-20` & Verifikasi: On-Call, pengaturan, dan verifikasi keterjangkauan layar

```text
BE-HRD-102 [BE] ─> FE-HRD-083 ─> FE-HRD-084 ─> FE-HRD-085

BE-HRD-088 [BE] ─> FE-HRD-086

FE-HRD-080 [F19] ──┐
                   │
FE-HRD-081 [F20] ──┼─> FE-HRD-087
                   │
FE-HRD-082 [F20] ──┼──┐
                   │  │
FE-HRD-085 [F21] ──┘  │
                      │
FE-HRD-086 [F21] ─────┘
```

### Grafik F22 — `S-B1` `MVP-21`: Client API, state Redux, dasbor presensi, drawer biometrik, dan setelan

```text
BE-HRD-111 [BE] ─┬─> FE-HRD-088 ✅ ─> FE-HRD-089 🟡 ─┬─> FE-HRD-090 🟡 ──┬─> FE-HRD-091 ✅
                 │                                   │                   │
BE-HRD-115 [BE] ─┘                                   └───────────────────┘
                                                     │
                                     BE-HRD-106 [BE] ┘
                                     BE-HRD-107 [BE] ────────────────────┘

BE-HRD-105 [BE] ────┬─> FE-HRD-099 ✅
                    │
FE-HRD-088 ✅ [F22] ┘
```

### Grafik F23 — `S-B1` `MVP-22`: Koreksi on-behalf, perizinan pulang cepat, dinas luar, dan penyelarasan ESS

```text
FE-HRD-089 🟡 [F22] ─┬─> FE-HRD-095 ─> FE-HRD-096
                     │
BE-HRD-110 [BE] ─────┘

FE-HRD-089 🟡 [F22] ─┬─> FE-HRD-097
                     │
BE-HRD-108 [BE] ─────┘

FE-HRD-089 🟡 [F22] ─┬─> FE-HRD-098
                     │
BE-HRD-109 [BE] ─────┘

FE-HRD-088 ✅ [F22] ─┬─> FE-HRD-100
                     │
FE-HRD-006 ✅ [F3] ──┘
```

### Grafik F24 — `S-B1` `MVP-23`: Penutupan periode, guard koreksi pending, dan buka kembali darurat

```text
FE-HRD-089 🟡 [F22] ─┬─> FE-HRD-092 ─┬───────────────────┬─> FE-HRD-093
                     │               │                   │
BE-HRD-115 [BE] ─────┘               │ BE-HRD-113 [BE] ──┘
                                     │
                                     └───────────────┬─> FE-HRD-094
                                                     │
                                     BE-HRD-114 [BE] ┘
```

### Grafik F25 — `S-B1` Verifikasi: Verifikasi keterjangkauan menu, hak akses peran, dan linter layar `S-B1`

```text
FE-HRD-091 ✅ [F22] ──┐
                      │
FE-HRD-093 [F24] ─────┤
                      │
FE-HRD-094 [F24] ─────┤
                      │
FE-HRD-096 [F23] ─────┼─> FE-HRD-101
                      │
FE-HRD-097 [F23] ─────┤
                      │
FE-HRD-098 [F23] ─────┤
                      │
FE-HRD-099 ✅ [F22] ──┤
                      │
FE-HRD-100 [F23] ─────┘
```

#### Tabel Gelombang Eksekusi Frontend `S-B1`

| Gelombang | Task yang dapat dimulai bersamaan | Yang menunggu gelombang ini |
| :---: | --- | --- |
| **1** | `FE-HRD-088` (client API presensi), `FE-HRD-099` (layar setelan HR), `FE-HRD-100` (penyelarasan portal ESS) | `FE-HRD-089` |
| **2** | `FE-HRD-089` (Redux slice presensi) | `FE-HRD-090`, `FE-HRD-092`, `FE-HRD-095`, `FE-HRD-097`, `FE-HRD-098` |
| **3** | `FE-HRD-090` (layar dasbor presensi), `FE-HRD-092` (layar rekapitulasi periode), `FE-HRD-095` (layar koreksi presensi), `FE-HRD-097` (layar izin pulang cepat), `FE-HRD-098` (layar surat dinas luar) | `FE-HRD-091`, `FE-HRD-093`, `FE-HRD-094`, `FE-HRD-096` |
| **4** | `FE-HRD-091` (drawer biometrik mentah), `FE-HRD-093` (dialog guard tutup periode), `FE-HRD-094` (modal buka kembali darurat HR Manager), `FE-HRD-096` (modal koreksi kuasa HR) | `FE-HRD-101` (verifikasi seluruh layar S-B1) |
| **5** | `FE-HRD-101` (verifikasi menu, hak akses peran, dan keterjangkauan seluruh layar S-B1) | Selesai dan siap UAT modul |

### Grafik F26 — `S-B2` `MVP-24`: Client API, dasbor kuota saldo ESS, buku besar ledger, dan manajemen carry-forward HR

```text
BE-HRD-120 [BE] ─┬─> FE-HRD-102 ─┬─> FE-HRD-103 ─> FE-HRD-104
FE-HRD-007 ✅ [F3] ┘               │
                                 └─> FE-HRD-105 ─┬─> FE-HRD-106
                                                 │
                                 BE-HRD-128 [BE] ┘
```

### Grafik F27 — `S-B2` `MVP-25`: Formulir cuti half-day, upload surat dokter privasi K3RS & approval banner coverage warning

```text
FE-HRD-102 [F26] ─┬─> FE-HRD-107 ──┐
                  │                │
BE-HRD-122 [BE] ──┘                │
                                   ├─> FE-HRD-109
FE-HRD-102 [F26] ─┬─> FE-HRD-108 ──┤
                  │                │
BE-HRD-123 [BE] ──┘                │
                                   │
FE-HRD-102 [F26] ─┬─> FE-HRD-110 ──┼─> FE-HRD-111
                  │                │
BE-HRD-121 [BE] ──┘                │
                                   │
BE-HRD-124 [BE] ───────────────────┘
```

### Grafik F28 — `S-B2` `MVP-26`: Layar inisiasi & persetujuan recall darurat serta rekonsiliasi akhir penutupan cuti

```text
FE-HRD-102 [F26] ─┬─> FE-HRD-112 ──┬─> FE-HRD-113
                  │                │
BE-HRD-126 [BE] ──┘                │ BE-HRD-127 [BE] ┘

FE-HRD-102 [F26] ─┬─> FE-HRD-114
                  │
BE-HRD-129 [BE] ──┘
```

### Grafik F29 — `S-B2` Verifikasi: Verifikasi keterjangkauan menu navigasi, modal drawer, dan linter layar `S-B2`

```text
FE-HRD-104 [F26] ──┐
                   │
FE-HRD-106 [F26] ──┤
                   │
FE-HRD-109 [F27] ──┼─> FE-HRD-115
                   │
FE-HRD-111 [F27] ──┤
                   │
FE-HRD-113 [F28] ──┤
                   │
FE-HRD-114 [F28] ──┘
```

#### Tabel Gelombang Eksekusi Frontend `S-B2`

| Gelombang | Task yang dapat dimulai bersamaan | Yang menunggu gelombang ini |
| :---: | --- | --- |
| **1** | `FE-HRD-102` (client API cuti & Redux slice) | `FE-HRD-103`, `FE-HRD-105`, `FE-HRD-107`, `FE-HRD-108`, `FE-HRD-110`, `FE-HRD-112`, `FE-HRD-114` |
| **2** | `FE-HRD-103` (komponen kartu metrik saldo), `FE-HRD-105` (layar alokasi carry-forward HR), `FE-HRD-107` (komponen pemilih kalender & half-day), `FE-HRD-108` (modal surat dokter K3RS), `FE-HRD-110` (banner Safe Staffing Coverage Warning), `FE-HRD-112` (layar monitoring recall cuti), `FE-HRD-114` (layar rekonsiliasi akhir payroll) | `FE-HRD-104`, `FE-HRD-106`, `FE-HRD-109`, `FE-HRD-111`, `FE-HRD-113` |
| **3** | `FE-HRD-104` (layar dasbor kuota saldo ESS `FE-HRD-150` & drawer ledger), `FE-HRD-106` (modal penyesuaian saldo HR), `FE-HRD-109` (layar formulir cuti ESS `FE-HRD-151`), `FE-HRD-111` (layar Approval Inbox atasan `FE-HRD-152`), `FE-HRD-113` (kotak masuk approval recall HR Manager) | `FE-HRD-115` (verifikasi seluruh layar S-B2) |
| **4** | `FE-HRD-115` (verifikasi menu navigasi, hak akses peran, dan linter layar S-B2) | Selesai dan siap UAT modul |


### Grafik F30 — `S-B3` `MVP-27`: Client API, form dual-mode ESS & pengaturan kebijakan lembur RS

```text
BE-HRD-131 [BE] ─┬─> FE-HRD-116 ─┬─> FE-HRD-117 ─┬─> FE-HRD-118
FE-HRD-007 ✅ [F3] ┘               │              │
                                 │              BE-HRD-134 [BE] ─┤
                                 │              BE-HRD-135 [BE] ─┘
                                 │
                                 └─┬─> FE-HRD-128
                                   │
                                   BE-HRD-132 [BE] ─┘
```

### Grafik F31 — `S-B3` `MVP-28`: Kotak masuk persetujuan ruangan, drawer discrepancy & eskalasi Direktur Medik

```text
FE-HRD-116 [F30] ─┬─> FE-HRD-119 ─┬─> FE-HRD-120
BE-HRD-137 [BE] ──┘               │
                                  BE-HRD-137 [BE] ──┘

FE-HRD-116 [F30] ─┬─> FE-HRD-121 ─┬─> FE-HRD-122
BE-HRD-139 [BE] ──┤               │
BE-HRD-140 [BE] ──┘               BE-HRD-140 [BE] ──┘

FE-HRD-116 [F30] ─┬─> FE-HRD-123
BE-HRD-138 [BE] ──┘
```

### Grafik F32 — `S-B3` `MVP-29`: Monitoring saldo CTO 90 hari & penutupan periode rekonsiliasi payroll

```text
FE-HRD-116 [F30] ─┬─> FE-HRD-124 ─┬─> FE-HRD-125
BE-HRD-141 [BE] ──┘               │
                                  BE-HRD-141 [BE] ──┘

FE-HRD-116 [F30] ─┬─> FE-HRD-126 ─┬─> FE-HRD-127
BE-HRD-143 [BE] ──┘               │
                                  BE-HRD-143 [BE] ──┤
                                  BE-HRD-144 [BE] ──┘
```

### Grafik F33 — `S-B3` Verifikasi: Verifikasi keterjangkauan navigasi, guard peran, dan linter layar `S-B3`

```text
FE-HRD-118 [F30] ──┐
                   │
FE-HRD-120 [F31] ──┤
                   │
FE-HRD-122 [F31] ──┼─> FE-HRD-129
                   │
FE-HRD-123 [F31] ──┤
                   │
FE-HRD-125 [F32] ──┤
                   │
FE-HRD-127 [F32] ──┤
                   │
FE-HRD-128 [F30] ──┘
```

#### Tabel Gelombang Eksekusi Frontend `S-B3`

| Gelombang | Task yang dapat dimulai bersamaan | Yang menunggu gelombang ini |
| :---: | --- | --- |
| **1** | `FE-HRD-116` (client API lembur & Redux slice) | `FE-HRD-117`, `FE-HRD-119`, `FE-HRD-121`, `FE-HRD-123`, `FE-HRD-124`, `FE-HRD-126`, `FE-HRD-128` |
| **2** | `FE-HRD-117` (komponen form dual-mode SPKL), `FE-HRD-119` (komponen inbox approval ruangan), `FE-HRD-121` (drawer discrepancy biometrik), `FE-HRD-123` (layar inbox Direktur Medik `FE-HRD-159-A`), `FE-HRD-124` (komponen kartu saldo CTO 90 hari), `FE-HRD-126` (komponen tabel rekonsiliasi rapel), `FE-HRD-128` (layar setelan kebijakan kuota RS `FE-HRD-161`) | `FE-HRD-118`, `FE-HRD-120`, `FE-HRD-122`, `FE-HRD-125`, `FE-HRD-127` |
| **3** | `FE-HRD-118` (layar ESS lembur `FE-HRD-156`), `FE-HRD-120` (layar approval lembur `FE-HRD-157`), `FE-HRD-122` (layar verifikasi & discrepancy HR `FE-HRD-158`), `FE-HRD-125` (layar monitoring CTO `FE-HRD-159`), `FE-HRD-127` (layar penutupan cut-off & rekonsiliasi payroll `FE-HRD-160`) | `FE-HRD-129` (verifikasi seluruh layar S-B3) |
| **4** | `FE-HRD-129` (verifikasi menu navigasi, hak akses peran, dan linter layar S-B3) | Selesai dan siap UAT modul |


#### Tabel Gelombang Eksekusi Frontend `S-B4`

| Gelombang | Task yang dapat dimulai bersamaan | Yang menunggu gelombang ini |
| :---: | --- | --- |
| **1** | `FE-HRD-075` (client API roster), `FE-HRD-083` (client API on-call), `FE-HRD-086` (layar setelan HR) | `FE-HRD-076`, `FE-HRD-084` |
| **2** | `FE-HRD-076` (state Redux roster), `FE-HRD-084` (layar kalender siaga) | `FE-HRD-077`, `FE-HRD-085` |
| **3** | `FE-HRD-077` (layar kisi roster), `FE-HRD-085` (modal call-in SLA) | `FE-HRD-078`, `FE-HRD-079`, `FE-HRD-081`, `FE-HRD-082` |
| **4** | `FE-HRD-078` (aksi salin/pola), `FE-HRD-079` (drawer validasi & alasan), `FE-HRD-081` (modal ganti petugas), `FE-HRD-082` (koreksi mundur & banner Closed) | `FE-HRD-080` |
| **5** | `FE-HRD-080` (aksi terbitkan roster mandiri) | `FE-HRD-087` (verifikasi layar S-B4) |
| **6** | `FE-HRD-087` (verifikasi menu, hak akses & keterjangkauan S-B4) | Selesai dan siap UAT modul |

### Grafik F42 — `S-D3` `MVP-36`: Client API, Redux Slice, Dasbor Analitik & Master Paket Manfaat 3-Lapisan

```text
FE-HRD-158 (Client API & Redux Slice) ───┬───> FE-HRD-160 (Komponen & Token UI) ───┬───> FE-HRD-161 (Dasbor FE-HRD-174)
BE-HRD-180 [BE] ────────────────────────┤                                         ├───> FE-HRD-162 (Master Paket FE-HRD-175)
FE-HRD-159 (Rute Menu & Guard RBAC) ────┘                                         └───> FE-HRD-163 (Drawer Matriks Grade)
```

### Grafik F43 — `S-D3` `MVP-36`: Pelacak Aktivasi Asuransi Pegawai, Portal ESS Keluarga & Verifikasi Dokumen HR

```text
FE-HRD-158 [F42] ───┬───> FE-HRD-164 (Monitoring Kepesertaan FE-HRD-176)
BE-HRD-184 [BE] ────┼───> FE-HRD-165 (Verifikasi Dokumen HR FE-HRD-177)
FE-HRD-160 [F42] ───┴───> FE-HRD-166 (Portal Mandiri ESS Keluarga)
```

### Grafik F44 — `S-D3` `MVP-37`: Layanan Berobat RS Quilvian Sendiri, Saldo Plafon & Mutasi Buku Besar Ledger

```text
FE-HRD-158 [F42] ───┬───> FE-HRD-167 (Inquiry Berobat & Ledger FE-HRD-178) ───> FE-HRD-168 (Modal Adjustment Saldo)
BE-HRD-187 [BE] ────┤
FE-HRD-160 [F42] ───┘
```

### Grafik F45 — `S-D3` `MVP-38` & Verifikasi: Formulir Klaim COB Mandatory SEP BPJS, Drawer Settlement & Verifikasi Build

```text
FE-HRD-158 [F42] ───┬───> FE-HRD-169 (Meja Kerja Klaim COB FE-HRD-179) ───┬───> FE-HRD-171 (Verifikasi Build Next.js & Linter)
BE-HRD-190 [BE] ────┼───> FE-HRD-170 (Formulir Klaim COB ESS) ────────────┤
FE-HRD-161 [F42] ───┤                                                     │
FE-HRD-162 [F42] ───┤                                                     │
FE-HRD-164 [F43] ───┤                                                     │
FE-HRD-165 [F43] ───┤                                                     │
FE-HRD-167 [F44] ───┴─────────────────────────────────────────────────────┘
```

#### Tabel Gelombang Eksekusi Frontend `S-D3`

| Gelombang | Task yang Dapat Dimulai Bersamaan | Yang Menunggu Gelombang Ini |
| :---: | --- | --- |
| **1** | `FE-HRD-158` (client API & Redux slice), `FE-HRD-159` (rute menu & RBAC guard), `FE-HRD-160` (komponen reusable & token visual) | `FE-HRD-161`, `FE-HRD-162`, `FE-HRD-164`, `FE-HRD-166`, `FE-HRD-167`, `FE-HRD-170` |
| **2** | `FE-HRD-161` (dasbor analitik manfaat `FE-HRD-174`), `FE-HRD-162` (layar master paket manfaat `FE-HRD-175`), `FE-HRD-164` (monitoring kepesertaan `FE-HRD-176`), `FE-HRD-166` (portal keluarga ESS) | `FE-HRD-163`, `FE-HRD-165`, `FE-HRD-167` |
| **3** | `FE-HRD-163` (drawer matriks grade staf), `FE-HRD-165` (antrean verifikasi berkas HR `FE-HRD-177`), `FE-HRD-167` (inquiry hak berobat & mutasi buku besar SIMRS `FE-HRD-178`), `FE-HRD-170` (form klaim COB ESS) | `FE-HRD-168`, `FE-HRD-169` |
| **4** | `FE-HRD-168` (modal penyesuaian manual saldo plafon HR), `FE-HRD-169` (meja kerja audit & settlement klaim COB `FE-HRD-179`) | `FE-HRD-171` |
| **5** | `FE-HRD-171` (verifikasi ujung-ke-ujung antarmuka UI/UX, linter CI/CD, build Next.js, dan hak akses peran Slice S-D3) | **Sign-off Teknis Frontend Slice S-D3** |

### `FE-HRD-001` — Penyelarasan Rute Kebab-Case pada Endpoint Constants Master Data
* **Slice**: `S0-B`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-001.md](../task/report/frontend/FE-HRD-001.md)
* **Bukti**: 8 file konstanta dan slice diperbarui ke kebab-case (`work-calendars`, `work-schedules`, `shift-groups`, `shift-patterns`, `disciplinary-action-types`, `employee-relation-case-types`, `sanction-types`, `violation-types`).

### `FE-HRD-002` — Layar Daftar Lintas-Pegawai Administrasi Kepegawaian
* **Slice**: `S-A1`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-002.md](../task/report/frontend/FE-HRD-002.md)
* **Bukti**: 6 rute aktif di `/hr/workforce-core/` (`employee-profile-changes`, `organization-assignments`, `position-assignments`, `manager-assignments`, `employment-histories`, `salary-assignments`) terhubung ke `WorkforceCoreCrossEmployeeService`.

### `FE-HRD-003` — Portal Layanan Mandiri Cuti Pegawai (ESS)
* **Slice**: `S-A2`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-003.md](../task/report/frontend/FE-HRD-003.md)
* **Bukti**: Halaman `/self-services/human-resource/leave` dengan kartu sisa saldo cuti, tabel riwayat, dan modal interaktif permohonan cuti baru.

### `FE-HRD-004` — Portal Layanan Mandiri Lembur Pegawai (ESS)
* **Slice**: `S-A3`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-004.md](../task/report/frontend/FE-HRD-004.md)
* **Bukti**: Halaman `/self-services/human-resource/overtime` dengan fitur dual-tab (Rencana Lembur Pre-Overtime & Klaim Lembur Post-Overtime) terhubung ke API ESS.

### `FE-HRD-005` — Portal Layanan Mandiri Ubah Jadwal & Tukar Shift (ESS)
* **Slice**: `S-A4`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-005.md](../task/report/frontend/FE-HRD-005.md)
* **Bukti**: Halaman `/self-services/human-resource/schedule` dengan tabel jadwal dinas aktif dan dua modal pengajuan (Ubah Jadwal & Tukar Shift).

### `FE-HRD-006` — Portal Layanan Mandiri Koreksi Absensi Mandiri (ESS)
* **Slice**: `S-A5`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-006.md](../task/report/frontend/FE-HRD-006.md)
* **Bukti**: Halaman `/self-services/human-resource/attendance-correction` dengan modal pengajuan perbaikan jam masuk/keluar terhubung ke backend.

### `FE-HRD-007` — Migrasi & Konsolidasi Form Absensi Harian ke Standar Self-Service Karyawan
* **Slice**: `S-A6`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-007.md](../task/report/frontend/FE-HRD-007.md)
* **Bukti**: Rute kanonikal `/self-services/human-resource/attendance` aktif mengarahkan ke `AttendanceEmployeeView`.

### `FE-HRD-008` — Layar Kotak Masuk Persetujuan Terpadu Atasan (Unified Approval Inbox)
* **Slice**: `S-A7`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-008.md](../task/report/frontend/FE-HRD-008.md)
* **Bukti**: Halaman `/hr/workflow/approval-inbox` dengan aksi Approve, Reject, Revision, terdaftar di sidebar menu `menu-items.jsx` (`corporateApprovalInbox`).

### `FE-HRD-009` — Layar Administrasi Monitoring Presensi & Rekapitulasi Kehadiran HR
* **Slice**: `S-B1`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-009.md](../task/report/frontend/FE-HRD-009.md)
* **Bukti**: Halaman `/hr/attendance` dengan dasbor kehadiran, modal koreksi on-behalf, dan pengamanan penutupan periode (closed period guard).

### `FE-HRD-010` — Layar Administrasi Alokasi & Penyesuaian Saldo Cuti Karyawan
* **Slice**: `S-B2`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-010.md](../task/report/frontend/FE-HRD-010.md)
* **Bukti**: Halaman `/hr/leave/adjustments` dengan rekap kuota cuti dan modal penyesuaian saldo berdokumen audit.

### `FE-HRD-011` — Layar Administrasi Verifikasi & Rekonsiliasi Jam Lembur HR
* **Slice**: `S-B3`
* **Status**: 🟡 **SEBAGIAN** — diselaraskan 3 November 2026 dengan register yang sudah mencatat 🟡 sejak `PLAN-REPAIR-HRD-001`. Semula ✅ Selesai (28 Oktober 2026). Yang belum: kontrol golongan C/D menunggu keputusan K-03 (`FIX-HRD-001-07` ⛔).
* **Laporan**: [FE-HRD-011.md](../task/report/frontend/FE-HRD-011.md)
* **Bukti**: Halaman `/hr/overtime/reconciliation` dengan perbandingan jam rencana vs jam mesin absensi aktual.

### `FE-HRD-012` — Layar Matriks Penjadwalan Kerja & Roster Shift Instalasi 24/7
* **Slice**: `S-B4`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-012.md](../task/report/frontend/FE-HRD-012.md)
* **Bukti**: Halaman `/hr/scheduling/roster` dengan modal buat periode roster serta aksi Publish dan Lock jadwal dinas.

### `FE-HRD-013` — Layar Penggajian Restitusi & Potongan Cicilan Piutang RS
* **Slice**: `S-B5`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-013.md](../task/report/frontend/FE-HRD-013.md)
* **Bukti**: Halaman `/hr/payroll/restitution-installment` dengan dual-tab (Restitusi & Cicilan) dan format nominal mata uang IDR.

### `FE-HRD-014` — Layar Pengajuan Kredensial & Pengesahan Kewenangan Klinis Dokter
* **Slice**: `S-C1`
* **Status**: ⛔ Terblokir (`DEC-HRD-008`).
* **Catatan**: Menunggu pengesahan wewenang dari Komite Medik rumah sakit. AI dilarang mengarang hak verifikasi kewenangan klinis dokter.

### `FE-HRD-015` — Layar Manajemen Pelatihan Wajib, Kehadiran Diklat, dan Sertifikasi
* **Slice**: `S-C2`
* **Status**: 🟡 **SEBAGIAN** — diselaraskan 3 November 2026 dengan register yang sudah mencatat 🟡 sejak `PLAN-REPAIR-HRD-001`. Semula ✅ Selesai (28 Oktober 2026). Yang belum: daftar lintas-pegawai menunggu keputusan K-02 (`FIX-HRD-001-06` ⛔).
* **Laporan**: [FE-HRD-015.md](../task/report/frontend/FE-HRD-015.md)
* **Bukti**: Halaman `/hr/learning/training-records` dengan pemantauan sertifikat kompetensi (BHD, PPI, KPRS) dan modal rekam pelatihan.

### `FE-HRD-016` — Layar Siklus Evaluasi Kinerja, Pengisian Nilai KPI, dan Hasil Review
* **Slice**: `S-C3`
* **Status**: 🟡 **SEBAGIAN** — diselaraskan 3 November 2026 dengan register yang sudah mencatat 🟡 sejak `PLAN-REPAIR-HRD-001`. Semula ✅ Selesai (28 Oktober 2026). Yang belum: daftar lintas-pegawai menunggu keputusan K-02 (`FIX-HRD-001-06` ⛔).
* **Laporan**: [FE-HRD-016.md](../task/report/frontend/FE-HRD-016.md)
* **Bukti**: Halaman `/hr/performance/reviews` dengan tabel skor capaian KPI, predikat kerja, dan modal inisiasi review baru.

### `FE-HRD-017` — Layar Pemantauan Exit Clearance dan Kliring Aset Karyawan
* **Slice**: `S-C4`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-017.md](../task/report/frontend/FE-HRD-017.md)
* **Bukti**: Halaman `/hr/exit-clearance` aktif dengan alur pemantauan checklist lintas-divisi (IT, Keuangan, Logistik, HR).

### `FE-HRD-018` — Layar Pencatatan Hubungan Karyawan, Kasus Disiplin, dan Surat Peringatan
* **Slice**: `S-C5`
* **Status**: 🟡 **SEBAGIAN** — diselaraskan 3 November 2026 dengan register yang sudah mencatat 🟡 sejak `PLAN-REPAIR-HRD-001`. Semula ✅ Selesai (28 Oktober 2026). Yang belum: daftar lintas-pegawai menunggu keputusan K-02 (`FIX-HRD-001-06` ⛔).
* **Laporan**: [FE-HRD-018.md](../task/report/frontend/FE-HRD-018.md)
* **Bukti**: Halaman `/hr/employee-relations/disciplinary-actions` dengan dasbor penanganan kasus dan modal penerbitan SP baru.

### `FE-HRD-019` — Layar Rekam Medis Kerja & Pemantauan Kesehatan Karyawan K3RS
* **Slice**: `S-C6`
* **Status**: ⛔ Terblokir (`DEC-HRD-010`).
* **Catatan**: Menunggu perumusan kebijakan privasi data rekam medis staf dan wewenang tim K3RS.

### `FE-HRD-020` — Layar Pengelolaan Kepesertaan Benefit Karyawan & Penjamin Tanggungan
* **Slice**: `S-D3`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-020.md](../task/report/frontend/FE-HRD-020.md)
* **Bukti**: Halaman `/hr/benefit/enrollments` aktif dengan rekap kepesertaan asuransi, nomor polis, dan anggota keluarga tanggungan.

### `FE-HRD-021` — Pemeliharaan dan Standarisasi 62 Layar Master Data HR
* **Slice**: `IN-01`
* **Status**: ✅ Selesai (Aktif di codebase).
* **Bukti**: 62 folder halaman di `src/app/hr/master-data/*` terhubung ke controller backend master data.


### `FE-HRD-022` — Layar Daftar Dokumen SK Kepegawaian (`/hr/workforce-core/personnel-action-orders`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026). Dikerjakan ulang 3 November 2026 (`PLAN-REPAIR-HRD-002`): tampilan dibangun ulang dengan base component, saringan Status/Kategori tampil, ringkasan dari `/summary`, menu sidebar ditambahkan; build lulus, `check-css-usage` 0 temuan, screenshot terlampir.
* **Laporan**: [FE-HRD-022.md](../task/report/frontend/FE-HRD-022.md)
* **Outcome**: Petugas HR rumah sakit memiliki dasbor terpusat untuk melihat, memantau, mencari, dan menyaring dokumen SK Kepegawaian (mutasi unit, promosi/demosi jabatan, penetapan atasan, perubahan gaji) lengkap dengan statistik status dan aksi navigasi.
* **Traceability**: `FE-HRD-08`, `HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`, `FR-HRD-013`, `03-frontend-architecture.md` §2.1 & §4.2a.
* **Contract Version**: API Contract v6, Frontend Architecture rev 2.
* **UI Authority & Base Component Decisions**:
  - Menggunakan `SummaryCards` Pola A untuk kartu statistik (Total SK, Menunggu Persetujuan, Terjadwal/Aktif, Ditolak/Dibatalkan).
  - Menggunakan `DataFilter` untuk pencarian nomor SK/nama pegawai dan saringan jenis aksi (`ActionType`) serta status SK.
  - Menggunakan `DataTable` terintegrasi dengan `RegionPagination` (mematuhi pola bebas pagination ganda).
  - Menggunakan `StatusBadge` untuk membedakan status (`Draft`, `PendingApproval`, `Scheduled`, `Applied`, `Rejected`, `Cancelled`).
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/personnel-action-orders/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/personnel-action-order-list-view.jsx`.
  3. Service client `src/lib/services/hr/personnel-action-order-service.js` (metode `getPersonnelActionOrders`).
* **Dependency**: `BE-HRD-025` [BE], `FE-HRD-002` ✅
* **Acceptance Criteria**:
  1. Halaman menampilkan daftar SK kepegawaian dengan kolom: Nomor SK, Pegawai, Jenis Aksi, Tanggal Efektif, Tanggal Pembukuan, Lampiran SK (ikon/indikator), Status, dan Aksi.
  2. Saringan jenis aksi dan status bekerja secara reaktif memicu pemanggilan API ber-query parameter.
  3. Menggunakan satu pagination bawaan `DataTable` tanpa kontrol duplikat.
  4. Penanganan state memuat (skeleton/spinner), data kosong (empty state), dan kegagalan API (InformationAlert dengan tombol Coba Lagi) terpasang.
* **Bukti Verifikasi**:
  - Kompilasi Next.js App Router (`npm run build` / lint pass 0 error).
  - Unit test headless Next.js / component mock (`node --test`).
* **Risiko & Pemilik**: Pemilik: Frontend Technical Owner. Risiko: respons lambat diatasi dengan pagination server-side.
* **Definition of Done**: Rute aktif, linter 0 error, integrasi API teruji, dokumen bukti pengujian tersedia.

### `FE-HRD-023` — Form Penerbitan SK Kepegawaian Baru (`/hr/workforce-core/personnel-action-orders/create`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026).
* **Laporan**: [FE-HRD-023.md](../task/report/frontend/FE-HRD-023.md)
* **Outcome**: Form penerbitan SK kepegawaian baru yang memandu petugas HR mengisi metadata SK, memilih penempatan/jabatan tujuan, dan mewajibkan unggah berkas pindai fisik bertanda tangan sebelum draft dapat diajukan ke jenjang persetujuan.
* **Traceability**: `FE-HRD-09`, `HRD-DEC-048`, `HRD-DEC-056`, `FR-HRD-013`, `VR-WFC-04`, `03-frontend-architecture.md` §2.1 & §4.2a.
* **Contract Version**: API Contract v6, Validation Matrix v6.
* **UI Authority & Base Component Decisions**:
  - Menggunakan `BaseCard` untuk segmentasi form: Bagian 1 (Identitas SK), Bagian 2 (Target Pegawai & Perubahan Penempatan), Bagian 3 (Unggah Pindai Berkas SK Fisik).
  - Kontrol input menggunakan `BaseInput`, `FilterSelect` (pencarian pegawai, unit, jabatan), `BaseDatePicker`.
  - Komponen upload berkas menggunakan validasi client-side: jenis file PDF/JPG/PNG dan ukuran maksimum 10MB.
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/personnel-action-orders/create/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/personnel-action-order-form-view.jsx`.
  3. Service client `src/lib/services/hr/personnel-action-order-service.js` (metode `createOrder`, `uploadAttachment`).
* **Dependency**: `FE-HRD-022` ✅ — disederhanakan 3 November 2026: `BE-HRD-025` ✅ [BE] sudah tercakup lewat `FE-HRD-022`, yang menunggunya langsung
* **Acceptance Criteria**:
  1. Form memvalidasi seluruh isian wajib: Nomor SK unik, Jenis Aksi, Tanggal Efektif, Pegawai, dan Berkas Pindai Fisik.
  2. Tombol simpan draft (`Save as Draft`) menyimpan record tanpa mewajibkan upload berkas, namun tombol ajukan persetujuan (`Submit for Approval`) menolak jika berkas lampiran belum diunggah (`VR-WFC-04`).
  3. Pemilihan jenis aksi mutasi/rotasi memunculkan form input unit tujuan dan jabatan tujuan secara dinamis.
  4. Notifikasi umpan balik sukses/gagal menggunakan toast / alert standar sistem.
* **Bukti Verifikasi**:
  - Form validation test, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: validasi tipe mime berkas di-guard di sisi frontend sebelum payload dikirim.
* **Definition of Done**: Form terintegrasi dengan backend API, lulus audit linter dan visual tokens.

### `FE-HRD-024` — Layar Detail & Aksi Persetujuan SK (`/hr/workforce-core/personnel-action-orders/[id]`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026). Dikerjakan ulang 3 November 2026 (`PLAN-REPAIR-HRD-002`): editor dan detail standar, pilihan bercari pengganti UUID, Tolak/Batal mengirim `reason` (sebelumnya HTTP 500), lampiran dapat diunduh dan diganti; build lulus, screenshot terlampir.
* **Laporan**: [FE-HRD-024.md](../task/report/frontend/FE-HRD-024.md)
* **Outcome**: Antarmuka detail dokumen SK kepegawaian yang menyajikan perbandingan komparatif (*before vs after* penempatan), pratinjau dokumen lampiran fisik SK, riwayat persetujuan, serta tombol aksi persetujuan/penolakan dan penerbitan SK kontra/pembatalan.
* **Traceability**: `FE-HRD-09a`, `HRD-DEC-031`, `HRD-DEC-048`, `HRD-DEC-057`, `FR-HRD-020`, `VR-WFC-05`, `VR-WFC-12`, `03-frontend-architecture.md` §2.1 & §4.2a.
* **Contract Version**: API Contract v6, State Transition Matrix v6.
* **UI Authority & Base Component Decisions**:
  - Menggunakan `BaseCard` dengan tata letak komparatif dua kolom (Kondisi Saat Ini vs Perubahan Baru).
  - Tampilan dokumen fisik terintegrasi dengan tautan unduh / preview PDF modal.
  - Dialog konfirmasi aksi persetujuan/penolakan menggunakan `BaseDialog` dengan isian alasan wajib pada penolakan/pembatalan.
  - Tombol persetujuan disembunyikan otomatis jika aktor login adalah pembuat draft SK (larangan self-approval, `VR-WFC-05`).
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/personnel-action-orders/[id]/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/personnel-action-order-detail-view.jsx`.
  3. Service client `src/lib/services/hr/personnel-action-order-service.js` (metode `getOrderById`, `approveOrder`, `rejectOrder`, `cancelOrder`).
* **Dependency**: `FE-HRD-022` ✅ — disederhanakan 3 November 2026: `BE-HRD-025` ✅ [BE] sudah tercakup lewat `FE-HRD-022`, yang menunggunya langsung
* **Acceptance Criteria**:
  1. Menampilkan detail atribut SK, riwayat audit pembuat, dan tanggal posting.
  2. Menampilkan tabel perubahan yang mencerminkan assignment unit, jabatan, manajer, atau gaji yang akan diubah.
  3. Tombol `Setujui` hanya aktif bagi pejabat yang memiliki hak dan bukan pembuat draft SK.
  4. Aksi `Batalkan SK` untuk SK yang sudah berstatus `Applied` mewajibkan penginputan nomor SK Kontra/Pembatalan resmi.
* **Bukti Verifikasi**:
  - Pengujian alur interaksi approval dan pembatalan, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: cegah double click pada tombol approval dengan state disabled loading.
* **Definition of Done**: Layar berfungsi penuh, dialog konfirmasi aktif, audit status transisi terbukti.

### `FE-HRD-025` — Antarmuka Profil Terpadu Pegawai (Employee 360) Dual-View (`/employee-360/[id]`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026). Dikerjakan ulang 3 November 2026 (`PLAN-REPAIR-HRD-002`): alamat API dibetulkan (sebelumnya 404), tampilan `BaseDetailView` + tab standar; mode HR terbukti dengan data nyata, mode atasan lewat respons dicegat.
* **Laporan**: [FE-HRD-025.md](../task/report/frontend/FE-HRD-025.md)
* **Outcome**: Layar ringkasan komprehensif 360 derajat profil staf rumah sakit yang cerdas mengenali peran login: jika dibuka oleh HR menampilkan profil lengkap (kedinasan, kompensasi, rekening, berkas keluarga, dan disiplin); jika dibuka oleh Atasan Langsung hanya menampilkan data kedinasan, riwayat jabatan, dan kompetensi bawahan aktif tanpa menampilkan data finansial/disiplin/keluarga.
* **Traceability**: `FE-HRD-09b`, `HRD-DEC-053`, `FR-HRD-018`, `VR-WFC-06`, `VR-WFC-07`, `03-frontend-architecture.md` §2.1 & §4.2b.
* **Contract Version**: API Contract v6, Frontend Architecture rev 2.
* **UI Authority & Base Component Decisions**:
  - Header profil menggunakan komponen `Hero` dengan foto, nama, NIP/ID profil, status kepegawaian, unit, dan jabatan utama.
  - Tab navigasi menggunakan `Tabs` Quilvian: Tab 1 (Informasi Umum & Kontak), Tab 2 (Penempatan & Riwayat Jabatan), Tab 3 (Atasan & Tim Kerja), Tab 4 (Kompetensi & Pelatihan), Tab 5 (Kompensasi & Rekening — khusus HR), Tab 6 (Disiplin & Catatan Khusus — khusus HR).
  - Tampilan tab finansial dan disiplin tidak di-render sama sekali pada DOM jika view mode adalah Direct Manager.
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/employee-360/[id]/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/employee-360-view.jsx`.
  3. Service client `src/lib/services/hr/employee-360-service.js` (metode `getEmployee360`).
* **Dependency**: `BE-HRD-026` [BE], `FE-HRD-002` ✅
* **Acceptance Criteria**:
  1. Halaman menampilkan seluruh riwayat perjalanan dinas pegawai dalam satu layar tabbed.
  2. Jika user login bertindak sebagai Atasan Langsung, tab Kompensasi dan Disiplin disembunyikan sepenuhnya dari tampilan navigasi dan DOM.
  3. Kolom NIK, tanggal lahir, dan rekening bank tidak muncul pada mode Atasan Langsung.
  4. Tersedia tombol kembali ke daftar staf atau pohon struktur organisasi.
* **Bukti Verifikasi**:
  - Verifikasi pengujian render tampilan untuk role HR vs Manager.
  - Linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: data leakage dihindari dari kontrak backend yang sudah me-masking payload.
* **Definition of Done**: Layar 360 derajat aktif dan teruji mematuhi batasan privasi data pegawai.

### `FE-HRD-026` — Layar Batch Inisialisasi Penempatan Awal Pegawai (`/initial-placement-batch`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 3 November 2026 (`PLAN-REPAIR-HRD-002`); semula ✅ Selesai (02 November 2026). Tampilan sudah standar, tetapi 1 dari 4 acceptance criteria terbukti: service masih memanggil endpoint yang tidak ada di backend. Blocker: `FIX-HRD-002-09` ⛔.
* **Laporan**: [FE-HRD-026.md](../task/report/frontend/FE-HRD-026.md)
* **Outcome**: Antarmuka khusus petugas HR untuk memverifikasi draf penempatan awal bagi 55 pegawai eksisting yang belum memiliki assignment terstruktur, melakukan validasi baris, dan memicu eksekusi batch satu kali dengan pemantauan progress.
* **Traceability**: `FE-HRD-09c`, `HRD-DEC-054`, `FR-HRD-019`, `VR-WFC-08`, `VR-WFC-09`, `03-frontend-architecture.md` §2.1.
* **Contract Version**: API Contract v6, Data Dictionary v6.
* **UI Authority & Base Component Decisions**:
  - Menggunakan `InformationAlert` dengan tipe warning yang menjelaskan bahwa batch ini merupakan inisialisasi awal satu kali.
  - Menggunakan `DataTable` yang menampilkan status validitas per baris pegawai (Valid / Duplikat / Data Belum Lengkap).
  - Tombol `Eksekusi Batch Penempatan` dilindungi modal konfirmasi ganda (`BaseDialog`).
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/initial-placement-batch/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx`.
  3. Service client `src/lib/services/hr/initial-placement-batch-service.js` (metode `getBatchStatus`, `executeBatch`).
* **Dependency**: `BE-HRD-027` [BE], `FE-HRD-002` ✅
* **Acceptance Criteria**:
  1. Layar menampilkan daftar 55 pegawai eksisting beserta usulan penempatan unit, jabatan, atasan, dan gaji awal.
  2. Terdapat indikator status validasi per baris; jika terdapat baris yang tidak valid, tombol eksekusi batch dinonaktifkan.
  3. Setelah tombol eksekusi dikonfirmasi, layar menampilkan progres dan status hasil eksekusi (Sukses / Rollback).
  4. Jika batch sudah berstatus `Executed`, form beralih ke mode read-only dan menampilkan ringkasan hasil inisialisasi.
* **Bukti Verifikasi**:
  - Pengujian render tabel batch, verifikasi disabled state saat data invalid, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: eksekusi ganda dicegah dengan idempotency key dan disabled button saat mutasi berjalan.
* **Definition of Done**: Rute aktif, eksekusi batch terhubung ke backend, audit laporan tersedia.

### `FE-HRD-027` — Form Permohonan Perubahan Data Kuasa HR (On-Behalf) (`/employee-profile-changes/on-behalf`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 3 November 2026 (`PLAN-REPAIR-HRD-002`); semula ✅ Selesai (02 November 2026). Tampilan dan pilihan pegawai sudah standar, tetapi 0 dari 4 acceptance criteria terpenuhi penuh: pengiriman ditolak 415 dan berkas bukti belum dapat diunggah. Blocker: `FIX-HRD-002-09` ⛔.
* **Laporan**: [FE-HRD-027.md](../task/report/frontend/FE-HRD-027.md)
* **Outcome**: Antarmuka bagi petugas HR untuk menginput permohonan perubahan data identitas/keluarga/kontak atas nama staf (on-behalf) yang berhalangan hadir atau membutuhkan asistensi administrasi, lengkap dengan pengunggahan dokumen dasar kuasa dan integrasi ke layar daftar review.
* **Traceability**: `FE-HRD-09d`, `HRD-DEC-052`, `FR-HRD-016`, `FR-HRD-017`, `VR-WFC-10`, `03-frontend-architecture.md` §2.1 & §4.2c, menutup temuan `ISS-HRD-001-T7` dan rencana perbaikan `FIX-HRD-001-09`.
* **Contract Version**: API Contract v6, Validation Matrix v6.
* **UI Authority & Base Component Decisions**:
  - Form terstruktur dengan `BaseCard` memuat: Pemilihan Pegawai Target (`FilterSelect`), Alasan Kuasa HR (`BaseInput` textarea), Field Data yang Ingin Diubah (`BaseInput`, `BaseDatePicker`), dan Unggah Berkas Kuasa / Surat Permohonan (`BaseFileUpload`).
  - Menampilkan alert peringatan bahwa permohonan kuasa ini wajib diverifikasi oleh petugas HR lain sebelum disahkan ke database profil.
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/employee-profile-changes/on-behalf/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/employee-profile-change-on-behalf-view.jsx`.
  3. Update service client `src/lib/services/hr/workforce-core-cross-employee-service.js` (metode `createOnBehalfProfileChange`).
* **Dependency**: `BE-HRD-028` [BE], `FE-HRD-002` ✅
* **Acceptance Criteria**:
  1. Petugas HR dapat memilih pegawai target dan mengisikan permohonan perubahan data atas nama pegawai tersebut.
  2. Field dokumen bukti kuasa wajib diunggah sebelum tombol `Kirim Permohonan` dapat diklik.
  3. Data permohonan yang dikirim muncul pada layar `/hr/workforce-core/employee-profile-changes` dengan badge `Kuasa HR (On-Behalf)`.
  4. Layar detail review menonaktifkan tombol verifikasi jika petugas yang login sama dengan pengaju on-behalf (`VR-WFC-10`).
* **Bukti Verifikasi**:
  - Verifikasi submission on-behalf, verifikasi flag audit, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: validasi segregasi verifikator ditegakkan di frontend dan backend.
* **Definition of Done**: Form terpasang, integrasi API on-behalf teruji, linter 0 error.

### `FE-HRD-028` — Penguncian Form Master Data Pegawai (Lock Unit & Jabatan Menjadi Read-Only)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026).
* **Laporan**: [FE-HRD-028.md](../task/report/frontend/FE-HRD-028.md)
* **Outcome**: Penyelarasan tata kelola pada form edit Master Data Pegawai (`/hr/master-data/employees/[id]`) dengan mengunci field pilihan Unit Organisasi dan Formasi Jabatan menjadi disabled / read-only, disertai banner edukasi yang mengarahkan user bahwa pemindahan unit dan penugasan jabatan wajib melalui penerbitan SK Kepegawaian resmi.
* **Traceability**: `HRD-DEC-047`, `FR-HRD-014`, `03-frontend-architecture.md` §3.3.
* **Contract Version**: Frontend Architecture rev 2, API Contract v6.
* **UI Authority & Base Component Decisions**:
  - Modifikasi `src/components/view/hr/master-data/employees/master-data-employee-view.jsx` atau sub-komponen form edit profilnya.
  - Menempatkan `InformationAlert` tipe info di atas field unit dan jabatan dengan pesan: *"Perubahan unit kerja dan jabatan tidak dapat dilakukan langsung pada form ini. Silakan gunakan menu SK Kepegawaian untuk proses mutasi, rotasi, atau promosi resmi."* disertai tombol tautan ke `/hr/workforce-core/personnel-action-orders/create`.
  - Input field unit organisasi dan jabatan diberi prop `disabled={true}` atau dirender sebagai teks statis dengan badge informasi.
* **Cakupan yang Diharapkan**:
  1. Modifikasi berkas form edit pegawai di `src/components/view/hr/master-data/employees/`.
* **Dependency**: `FE-HRD-022` ✅ — disederhanakan 3 November 2026: `FE-HRD-021` ✅ sudah tercakup lewat rantai `FE-HRD-021` → `FE-HRD-001` → `FE-HRD-002` → `FE-HRD-022`
* **Acceptance Criteria**:
  1. Pengguna tidak dapat lagi mengganti unit kerja atau jabatan secara langsung dari form master data pegawai.
  2. Banner petunjuk muncul dengan jelas mengarahkan alur ke modul SK Kepegawaian.
  3. Field data profil lainnya (alamat, kontak, data pribadi non-kedinasan) tetap dapat disunting seperti biasa.
* **Bukti Verifikasi**:
  - Pemeriksaan visual form edit master data pegawai, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: mencegah regresi pada fungsi edit master data pegawai lainnya.
* **Definition of Done**: Field unit & jabatan terkunci read-only, banner edukasi terpasang, linter pass 0 error.

---

## Kartu Detail Task Frontend — Slice `S-D1` Perencanaan Tenaga Kerja

### Catatan umum untuk seluruh kartu `S-D1`

| Hal | Ketentuan |
| --- | --- |
| Gambaran bisnis | Layar `S-D1` dipakai HR Admin, HR Manager, Kepala Unit, Finance / Anggaran, Direktur, Recruiter, dan Auditor untuk mengelola standar kebutuhan tenaga, formasi resmi, gap, permintaan tambahan tenaga, dan rencana tahunan. Contoh yang dipakai di seluruh kartu: ICU membuka 2 bed baru, Kepala ICU meminta +2 Perawat Pelaksana, satu kursi diisi pegawai baru dan satu lewat SK pindahan |
| Kontrak terkunci | API v7 bagian 13 dan Frontend Architecture rev 3 bagian 12; hash pada metadata. Layar **tidak** mengarang endpoint, kolom, atau status di luar kontrak |
| Persetujuan | Tidak ada layar persetujuan baru. Tugas `T9` s.d. `T13` muncul di Kotak Masuk Persetujuan (`FE-HRD-30`/`FE-HRD-31`, task `FE-HRD-008` ✅); layar detail dokumen `S-D1` dibuka dari tautan tugas itu |
| Letak berkas | Route `src/app/hr/workforce-planning/...`; view `src/components/view/hr/workforce-planning/...`; service di `src/lib/services/hr/`; konstanta endpoint di `src/lib/constants/hr/`; util tampilan di `src/utils/hr/workforce-planning/`. Nama berkas persisnya mengikuti tetangga terdekat (`DEV_DISCRETION`) |
| Pola layar | Daftar mengikuti Pola A (`03-frontend-architecture.md` bagian 4.1); layar dokumen bertahap mengikuti pola SK (bagian 4.2a). Pemakaian ulang atau pembuatan komponen baru diputuskan per elemen lewat `base-component-decision-gate` saat eksekusi |
| Keadaan yang mengikat (bagian 12.5) | Nilai `null` dari backend **MUST** tampil "data belum tersedia" dan **MUST NOT** diubah menjadi 0 (`UNKNOWN_IS_NOT_ZERO`). `409` → "Dokumen ini sudah diubah orang lain. Muat ulang sebelum menyimpan." tanpa membuang isian. Tombol Ajukan, Tautkan, dan Batalkan kursi nonaktif selama permintaan berjalan. Angka biaya selalu berlabel "estimasi" dan **MUST NOT** disimpan di `localStorage`, `sessionStorage`, maupun state yang dipersistenkan |
| Hak akses | Tombol dan kolom mengikuti butir hak akses `permission-audit-matrix.md` bagian 11. Menyembunyikan tombol hanyalah kenyamanan; penjaga sebenarnya di backend |
| Bukti verifikasi frontend | Mengikuti `rules/frontend/test-policy.md`: `npm run lint` (atau `npm run lint:errors`) tanpa error baru, `npm run build` berhasil, verifikasi manual setiap kontrol interaktif di peramban, dan `npm run test:unit` untuk suite yang ada. Test unit baru **direkomendasikan** untuk logika murni (format "data belum tersedia", label biaya, hitung tampilan sisa kursi), tidak diwajibkan |
| Data uji | Uji manual butuh backend `S-D1` yang berjalan dan data dev. Alur persetujuan sungguhan baru dapat diuji setelah `BE-HRD-050` (tertahan `HRD-DEC-032`); sebelum itu, layar dokumen diuji pada status yang dapat dibuat tanpa persetujuan (`Draft`, `Submitted`, `Returned`) |

### ✅ `FE-HRD-029` — Fondasi layar `S-D1` dan layar Pengaturan Perencanaan
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: ✅ Selesai (11 November 2026, `FE-HRD-R12`). Lint 0 error, build berhasil, uji browser dengan akun berhak dan tidak berhak. Laporan: [FE-HRD-029.md](../task/report/frontend/FE-HRD-029.md).
* **Outcome**: Menu HR punya grup baru "Perencanaan Tenaga Kerja", dan HR Manager dapat membuka layar Pengaturan Perencanaan untuk mengisi batas hari cuti panjang dan batas umur permintaan. Butir menu lama "Kebutuhan Tenaga Kerja" — yang sebenarnya membuka master persyaratan berkas — berganti nama agar tidak lagi bermakna sama.
* **Requirement/decision**: `03-frontend-architecture.md` bagian 12.1 (`FE-HRD-109`), 12.2, 12.3.5, 12.5; `HRD-TF-036`; `HRD-FE-04` (`OPEN`, usulan kerja); `HRD-DEC-061`, `HRD-DEC-071`; `VR-WFP-40`, `VR-WFP-41`.
* **Kontrak**: API v7 bagian 13 grup `Setting`; Frontend Architecture rev 3 bagian 12.2.
* **Kewenangan UI**: Nama grup dan butir memakai usulan kerja (`HRD-FE-04` `OPEN`); urutan dan ikon `DEV_DISCRETION`.
* **Reuse**: `src/utils/menu-sidebar/menu-items.jsx` (grup tingkat 1 di bawah "Sumber Daya Manusia" memakai field `subItems`); pola form HR yang sudah ada.
* **Cakupan**:
  1. Grup menu "Perencanaan Tenaga Kerja" dengan butir pertamanya, "Pengaturan Perencanaan". Butir lain ditambahkan oleh task layar masing-masing, agar tidak ada tautan mati.
  2. Nama butir lama `corporateWorkforceRequirement` (`menu-items.jsx` sekitar baris 252) beserta label pada view detail dan form-nya menjadi "Persyaratan Berkas Tenaga Kerja"; `pathname` tidak berubah.
  3. Konstanta endpoint untuk kesembilan base URL `api/v1/corporate/human-resource/workforce-planning/...`.
  4. Util tampilan bersama: nilai `null` → "data belum tersedia"; porsi kerja berformat koma (0,5); angka biaya berlabel "estimasi" atau kalimat "Biaya tidak ditampilkan untuk peran Anda." bila `costVisible = false`.
  5. Route, view, dan service layar Pengaturan Perencanaan (`/hr/workforce-planning/settings`).
* **Dependency**: `FE-HRD-021` ✅, `BE-HRD-031` [BE]
* **Acceptance criteria**:
  1. Grup "Perencanaan Tenaga Kerja" tampil di bawah "Sumber Daya Manusia" dan butir "Pengaturan Perencanaan" hanya terlihat oleh pemegang `WorkforcePlanningSetting : Read`.
  2. Tidak ada lagi dua butir menu bermakna "kebutuhan tenaga"; butir lama tetap membuka layar yang sama.
  3. Layar pengaturan menampilkan dua isian angka dan alasan. Nilai yang belum diisi tampil kosong dengan keterangan "belum diatur", bukan angka bawaan.
  4. Tombol Simpan hanya untuk pemegang `WorkforcePlanningSetting : Update`; pesan `VR-WFP-40` dan `VR-WFP-41` dari backend tampil di dekat isiannya; `409` ditangani sesuai catatan umum.
  5. Util "data belum tersedia" dan label biaya dipakai ulang oleh task `S-D1` berikutnya, bukan disalin.
* **Bukti verifikasi**: lint dan build; uji manual menu dan layar pengaturan dengan dua akun (punya dan tidak punya hak `Update`); test unit util tampilan direkomendasikan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: nama menu berganti lagi saat `HRD-FE-04` diputuskan — label dipusatkan agar cukup diubah di satu tempat.
* **Definition of Done**: Grup menu, util bersama, dan layar pengaturan aktif; lint dan build lulus; laporan tracked ada.

### 🟡 `FE-HRD-030` — Layar Formasi Jabatan baca saja beserta riwayat versi
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: 🟡 **SEBAGIAN** (11 November 2026, `FE-HRD-R12`). 4 dari 5 kriteria terbukti; riwayat versi (kriteria 2) belum terbukti karena belum ada data formasi di database pengembangan. Laporan: [FE-HRD-030.md](../task/report/frontend/FE-HRD-030.md).
* **Outcome**: HR, Kepala Unit, dan Direktur melihat berapa kursi resmi setiap unit × jabatan pada tanggal tertentu dan dokumen yang melahirkannya. Klik satu baris membuka riwayat versinya. Tidak ada tombol tambah, ubah, atau hapus — formasi hanya berubah lewat dokumen yang disetujui.
* **Requirement/decision**: `HRD-DEC-062`, `FR-HRD-098` s.d. `FR-HRD-100`, `AT-HRD-D1-06` (bagian layar), `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-101`).
* **Kontrak**: API v7 bagian 13 grup `Position Formation` (`GET /`, `GET /history`).
* **Kewenangan UI**: Ketiadaan tombol ubah **mengikat** (`HRD-DEC-062`); tampilan riwayat sebagai laci, dialog, atau halaman anak `DEV_DISCRETION`.
* **Reuse**: Pola A; util `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/position-formations`, view, service; butir menu "Formasi Jabatan".
* **Dependency**: `FE-HRD-029`, `BE-HRD-032` [BE]
* **Acceptance criteria**:
  1. Kolom: unit, jabatan, formasi, berlaku mulai, dokumen sumber; saringan rumah sakit, unit, jabatan, dan tanggal.
  2. Riwayat versi menampilkan seluruh versi beserta jenis dan nomor dokumen sumber, penyetuju, dan waktu persetujuan.
  3. Layar tidak punya tombol tambah, ubah, maupun hapus.
  4. Keadaan kosong: "Belum ada formasi. Isi lewat Batch Formasi Awal."; memuat dan gagal mengikuti Pola A.
  5. Butir menu hanya terlihat oleh pemegang `WorkforcePositionFormation : Read`.
* **Bukti verifikasi**: lint dan build; uji manual; tangkapan layar tanpa tombol ubah untuk `AT-HRD-D1-06`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar baca aktif sesuai kontrak; laporan tracked ada.

### 🟡 `FE-HRD-031` — Layar Batch Formasi Awal dan detailnya
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: 🟡 **SEBAGIAN** (11 November 2026, `FE-HRD-R12`). Daftar, form baru, dan penjelasan tombol Ajukan terbukti; kriteria 2, 3, 5, dan 6 baru terpetakan ke source karena uji tidak menyimpan batch ke database pengembangan. Laporan: [FE-HRD-031.md](../task/report/frontend/FE-HRD-031.md).
* **Outcome**: HR Admin mengunduh templat, mengisi baris formasi dari SK formasi resmi, memeriksa baris yang belum valid, mengunggah scan SK, lalu mengajukan. Tombol Ajukan baru aktif bila semua baris valid dan scan sudah ada.
* **Requirement/decision**: `HRD-DEC-070`, `FR-HRD-101`, `VR-WFP-11` s.d. `VR-WFP-14`, `UAT-31`, `UAT-32`, `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-102`, `FE-HRD-103`).
* **Kontrak**: API v7 bagian 13 grup `Initial Formation Batch` (10 endpoint).
* **Kewenangan UI**: Susunan detail batch `DEV_DISCRETION`; tombol Ajukan nonaktif selama ada baris tidak valid atau scan belum ada **mengikat**.
* **Reuse**: Pola A untuk daftar; pola unggah berkas `FE-HRD-024` (PDF/JPG/PNG, paling besar 10 MB); pola tabel validasi baris `FE-HRD-026`.
* **Cakupan**: Route `/hr/workforce-planning/initial-formation-batches` dan `/[id]`, view, service; butir menu "Batch Formasi Awal".
* **Dependency**: `FE-HRD-029`, `BE-HRD-033` [BE]
* **Acceptance criteria**:
  1. Daftar: nomor batch, dokumen sumber, status, jumlah baris valid/tidak valid; kosong → "Belum ada batch formasi awal."
  2. Detail menampilkan status per baris beserta pesannya, termasuk `VR-WFP-11` untuk unit × jabatan yang sudah berformasi.
  3. Unggah scan memeriksa jenis dan ukuran berkas di layar dan menampilkan pesan `VR-WFP-14` dari backend.
  4. Tombol Ajukan nonaktif selama `InvalidRows > 0` atau scan belum ada, dengan keterangan penyebabnya (`UAT-32`).
  5. Simpan, Ajukan, dan Batalkan hanya tampil pada `Draft`/`Returned` untuk pemegang butir yang sesuai; batch `Applied` hanya baca.
  6. Riwayat persetujuan tampil dari detail instance workflow yang sudah ada.
* **Bukti verifikasi**: lint dan build; uji manual unggah templat, validasi, unggah scan, dan keadaan tombol Ajukan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: pengguna mengira batch langsung berlaku — status `Submitted` diberi keterangan "menunggu persetujuan".
* **Definition of Done**: Dua layar aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-032` — Layar Angka Beban
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R26` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Kepala Unit atau HR mengisi dan memperbaiki angka beban langsung di layar daftar, misalnya "7 pasien ICU terisi per hari, Januari–Maret 2027, sumber: rekap sensus harian".
* **Requirement/decision**: `HRD-DEC-060`, `FR-HRD-096`, `VR-WFP-08`, `VR-WFP-09`, `VR-WFP-38`, `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-100`).
* **Kontrak**: API v7 bagian 13 grup `Staffing Workload`.
* **Kewenangan UI**: Isian langsung di layar ini (bagian 12.3.5); bentuk isian (baris tabel atau dialog) `DEV_DISCRETION`.
* **Reuse**: Pola A; util `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/staffing-workloads`, view, service; butir menu "Angka Beban".
* **Dependency**: `FE-HRD-029`, `BE-HRD-034` [BE]
* **Acceptance criteria**:
  1. Kolom: unit, jabatan, periode, angka, sumber; kosong → "Belum ada angka beban untuk periode ini."
  2. Tambah dan ubah hanya untuk pemegang `WorkforceStaffingWorkload : Create`/`: Update`.
  3. Pesan `VR-WFP-08` (sumber kosong) dan `VR-WFP-09` (isian ganda) tampil di dekat isiannya.
  4. Kepala Unit hanya melihat unitnya.
* **Bukti verifikasi**: lint dan build; uji manual tambah, ubah, dan isian ganda.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-033` — Layar Daftar Standar Kebutuhan Tenaga
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R26` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: HR dan Kepala Unit melihat seluruh versi standar per unit beserta statusnya, dan dapat membuat versi baru.
* **Requirement/decision**: `FR-HRD-092`, `VR-WFP-01`, `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-98`).
* **Kontrak**: API v7 bagian 13 grup `Staffing Standard` (`GET /`, `POST /`, `GET /filters/metadata`).
* **Kewenangan UI**: Pola A.
* **Reuse**: Pola A; util `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/staffing-standards`, view, service; butir menu "Standar Kebutuhan Tenaga".
* **Dependency**: `FE-HRD-029`, `BE-HRD-035` [BE]
* **Acceptance criteria**:
  1. Kolom: unit, versi, status, berlaku mulai, berlaku sampai; kosong → "Belum ada standar untuk saringan ini."
  2. Tombol "Buat versi baru" hanya untuk pemegang `WorkforceStaffingStandard : Create`; pesan `VR-WFP-01` tampil bila unit masih punya draf.
  3. Klik baris membuka layar penyusun dan detail standar (route `[id]`, dibangun di `FE-HRD-034`).
* **Bukti verifikasi**: lint dan build; uji manual.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar daftar aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-034` — Layar Penyusun dan Detail Standar beserta rekomendasi
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R26` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Penyusun standar mengisi baris jabatan menurut metodenya, memilih sendiri faktor cadangan dan cara pembulatan, mengisi skill mix per shift, dan melihat angka rekomendasi beserta rinciannya. Contoh rincian yang tampil: "Perawat Pelaksana: 7 ÷ 1 × 1 = 7 per shift; 7 × 3 = 21; 21 × 1,5 = 31,5 → 32".
* **Requirement/decision**: `HRD-DEC-060`, `FR-HRD-093` s.d. `FR-HRD-095`, `VR-WFP-02` s.d. `VR-WFP-07`, `UAT-29`, `UAT-30`, `03-frontend-architecture.md` bagian 12.3.2 (`FE-HRD-99`).
* **Kontrak**: API v7 bagian 13 grup `Staffing Standard` (`GET /{id}`, `PUT /{id}`, `GET /{id}/recommendation`, `POST /{id}/submit`, `POST /{id}/cancel`, `GET /filters/metadata`).
* **Kewenangan UI**: Satu layar berbagian atau berlangkah `DEV_DISCRETION`; metode Analisis Beban Kerja **tidak** muncul sebagai pilihan (mengikat).
* **Reuse**: Pola layar dokumen bertahap SK (`FE-HRD-024`); riwayat persetujuan dari detail instance workflow.
* **Cakupan**: Route `/hr/workforce-planning/staffing-standards/[id]`, view, service.
* **Dependency**: `FE-HRD-032`, `FE-HRD-033`
* **Acceptance criteria**:
  1. Kepala: nomor, unit, versi, status, tanggal berlaku, versi yang digantikan.
  2. Isian per baris menyesuaikan metode; kolom yang tidak dipakai metode itu disembunyikan. Pilihan metode diambil dari `filters/metadata` dan tidak memuat Analisis Beban Kerja.
  3. Faktor cadangan dan pembulatan tidak punya nilai awal di layar; pesan `VR-WFP-03`/`VR-WFP-04` tampil per baris.
  4. Panel rekomendasi menampilkan angka dan rincian teks dari backend; bila angka beban belum diisi tampil "angka beban belum diisi" beserta tautan ke layar Angka Beban (`FE-HRD-032`).
  5. Simpan, Ajukan, Batalkan hanya pada `Draft`/`Returned` sesuai butir hak; versi `Active`, `Scheduled`, dan `Superseded` hanya baca.
  6. Dibuka dari tautan tugas kotak masuk, layar menampilkan dokumen yang sama.
* **Bukti verifikasi**: lint dan build; uji manual dengan data `UAT-29` dan `UAT-30`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: rincian rekomendasi dihitung ulang di layar dan berbeda dari backend — layar **hanya** menampilkan rincian dari backend.
* **Definition of Done**: Penyusun dan detail standar aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-035` — Layar Pemantauan Gap Tenaga — gap langsung
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R26` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: HR dan Kepala Unit melihat lapisan tenaga dan tiga gap per unit × jabatan pada tanggal acuan. Sel yang datanya belum ada tertulis miring "data belum tersedia", dan baris yang belum lengkap diberi penanda. Contoh `UAT-33`: Budi paruh waktu terhitung 0,5, Sari yang Plt tidak dihitung, kolom cuti panjang "data belum tersedia".
* **Requirement/decision**: `HRD-DEC-061` (`UNKNOWN_IS_NOT_ZERO`), `FR-HRD-102` s.d. `FR-HRD-104`, `AT-HRD-D1-05b` (bagian layar), `UAT-33`, `03-frontend-architecture.md` bagian 12.3.1 (`FE-HRD-97`).
* **Kontrak**: API v7 bagian 13 grup `Staffing Gap` (`GET /`).
* **Kewenangan UI**: Penampilan `null` sebagai "data belum tersedia" **mengikat**; gaya miring dan penanda baris `DEV_DISCRETION`.
* **Reuse**: Pola A; util `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/staffing-gaps`, view, service; butir menu "Pemantauan Gap Tenaga". Tombol "Ajukan" ditambahkan di `FE-HRD-037`; panel potret di `FE-HRD-040`.
* **Dependency**: `FE-HRD-029`, `BE-HRD-037` [BE]
* **Acceptance criteria**:
  1. Saringan rumah sakit, unit, dan tanggal acuan.
  2. Kolom: formasi, terisi, akan kosong, cuti panjang, belum kompeten, gap permanen, gap sementara, gap kompetensi, rangkap, alih daya.
  3. Setiap nilai `null` tampil "data belum tersedia", tidak pernah 0; baris dengan penanda kelengkapan `false` diberi penanda "data belum lengkap".
  4. Kosong → "Belum ada formasi untuk saringan ini. Isi formasi awal lebih dulu."; gagal → "Data gap gagal dimuat." beserta Coba lagi.
  5. Kepala Unit hanya melihat unitnya.
* **Bukti verifikasi**: lint dan build; uji manual dengan pengaturan cuti panjang kosong; tangkapan layar untuk `AT-HRD-D1-05b`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: komponen tabel bawaan menampilkan `null` sebagai 0 — diperiksa khusus pada uji manual.
* **Definition of Done**: Layar gap aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-036` — Layar Daftar Permintaan Headcount
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R36` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: HR dan Kepala Unit melihat seluruh permintaan beserta jenis, status, dan jumlah kursi terisi, dan dapat menyaring permintaan yang "lewat batas" atau "melebihi formasi".
* **Requirement/decision**: `FR-HRD-115`, `FR-HRD-121`, `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-106`).
* **Kontrak**: API v7 bagian 13 grup `Headcount Request` (`GET /`, `GET /filters/metadata`).
* **Kewenangan UI**: Pola A.
* **Reuse**: Pola A; util `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/headcount-requests`, view, service; butir menu "Permintaan Headcount".
* **Dependency**: `FE-HRD-029`, `BE-HRD-040` [BE]
* **Acceptance criteria**:
  1. Kolom: nomor, unit, jabatan, jenis, status, kursi terisi/total; kosong → "Belum ada permintaan pada saringan ini."
  2. Saringan status, unit, jabatan, "lewat batas", dan "melebihi formasi".
  3. Tombol buat permintaan hanya untuk pemegang `WorkforceHeadcountRequest : Create`.
* **Bukti verifikasi**: lint dan build; uji manual.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar daftar aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-037` — Form dan Detail Permintaan Headcount, serta tombol "Ajukan" di Pemantauan Gap
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R36` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Kepala Unit menyusun permintaan per kursi — porsi kerja, sumber pemenuhan, orang yang digantikan — sambil melihat sisa kursi langsung beserta rinciannya, lalu mengajukan atau menarik kembali. Penyetuju melihat potret saat diajukan berdampingan dengan angka sekarang ("saat diajukan 3,5 — sekarang 2,5"). Finance pada langkahnya dapat mengoreksi biaya.
* **Requirement/decision**: `HRD-DEC-063`, `HRD-DEC-065`, `HRD-DEC-067`, `HRD-DEC-069` (`SUBMITTED_VS_CURRENT`); `FR-HRD-110`, `FR-HRD-113`, `FR-HRD-115`; `VR-WFP-20`, `VR-WFP-22`, `VR-WFP-24` s.d. `VR-WFP-28`, `VR-WFP-34`, `VR-WFP-37`; `UAT-34`, `UAT-37`, `UAT-38`; `AT-HRD-D1-16` (bagian layar); `03-frontend-architecture.md` bagian 12.3.4 (`FE-HRD-107`) dan 12.3.1 (tombol Ajukan).
* **Kontrak**: API v7 bagian 13 grup `Headcount Request` (`GET /{id}`, `GET /remaining-seats`, `POST /`, `PUT /{id}`, `POST /{id}/submit`, `POST /{id}/withdraw`, `POST /{id}/finance-adjustment`); `GET .../staffing-gaps` untuk angka sekarang.
* **Kewenangan UI**: Satu layar untuk form dan detail mengikuti bagian 12.3.4; susunan wilayahnya `DEV_DISCRETION`.
* **Reuse**: Pola layar dokumen bertahap; util biaya dan "data belum tersedia" `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/headcount-requests/[id]` (termasuk mode buat), view, service; tombol "Ajukan" pada setiap baris layar Pemantauan Gap yang membuka form berisi unit dan jabatan baris itu.
* **Dependency**: `FE-HRD-035`, `FE-HRD-036`
* **Acceptance criteria**:
  1. Kepala menampilkan nomor, unit, jabatan, jenis, status, penanda "melebihi formasi" dan "lewat batas".
  2. Panel sisa kursi menampilkan angka beserta rinciannya, misalnya "0,5 (formasi 30 − terisi 28,5 − permintaan lain 1)"; unit tanpa formasi menampilkan "Unit ini belum punya formasi; permintaan diproses sebagai di luar formasi."
  3. Isian kursi: porsi kerja 0,10–1,00, sumber luar/internal, dan orang yang digantikan untuk jenis pengganti; isian sementara meminta tanggal akhir dan orang yang digantikan sementara. Pesan `VR-WFP-22` (beserta angka sisa kursi), `VR-WFP-24` s.d. `VR-WFP-28` tampil di dekat isiannya (`UAT-38`).
  4. Ajukan dan Tarik kembali tampil sesuai butir hak dan status; sesudah mengajukan, sisa kursi dan gap dimuat ulang; `VR-WFP-34` tampil bila permintaan sudah siap direkrut.
  5. Potret saat diajukan dan angka sekarang tampil berdampingan pada dokumen yang sudah diajukan (`UAT-34`).
  6. Kolom biaya selalu berlabel "estimasi"; tanpa `WorkforcePlanningCost : Read` tampil "Biaya tidak ditampilkan untuk peran Anda."; isian koreksi biaya hanya tampil bagi penyetuju langkah Finance yang aktif, dan pesan `VR-WFP-20` tampil bila ditolak.
  7. Tombol "Ajukan" di Pemantauan Gap hanya untuk pemegang `WorkforceHeadcountRequest : Create`.
* **Bukti verifikasi**: lint dan build; uji manual mengikuti `UAT-37`, `UAT-38`; test unit format rincian sisa kursi direkomendasikan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: angka biaya ikut tersimpan di state yang dipersistenkan — diperiksa pada review.
* **Definition of Done**: Form dan detail permintaan aktif sesuai kontrak, tombol Ajukan di gap aktif; laporan tracked ada.

### `FE-HRD-038` — Aksi kursi pada detail permintaan: tautkan/lepas, ubah rencana pemenuhan, batalkan kursi
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R36` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Dari detail permintaan, HR menautkan kursi ke penempatan atau SK, melepasnya selama belum berlaku, HR Manager memindah sumber kursi yang belum terisi, dan Kepala Unit atau HR mengajukan pembatalan kursi yang belum terisi.
* **Requirement/decision**: `HRD-DEC-066`, `HRD-DEC-067`, `HRD-DEC-071`; `FR-HRD-117`, `FR-HRD-119`, `FR-HRD-120`; `VR-WFP-29` s.d. `VR-WFP-33`, `VR-WFP-35`, `VR-WFP-36`; `UAT-39`, `UAT-40`; `03-frontend-architecture.md` bagian 12.3.4 wilayah Kursi dan Tombol.
* **Kontrak**: API v7 bagian 13: `POST`/`DELETE /{id}/seats/{seatId}/fulfilment-link`, `POST /{id}/fulfilment-plan`, `POST /{id}/cancellations`.
* **Kewenangan UI**: Bentuk dialog `DEV_DISCRETION`; dialog tautan memakai ulang dialog `FE-HRD-039`.
* **Reuse**: Dialog tautan pemenuhan dari `FE-HRD-039`.
* **Cakupan**: Aksi per kursi dan riwayat (perubahan rencana pemenuhan, pembatalan, persetujuan) pada layar `FE-HRD-107`.
* **Dependency**: `FE-HRD-037`, `FE-HRD-039`, `BE-HRD-044` [BE]
* **Acceptance criteria**:
  1. Tautkan dan Lepas per kursi hanya untuk pemegang `WorkforceHeadcountRequest : LinkFulfilment`; pesan `VR-WFP-29` s.d. `VR-WFP-32` tampil apa adanya.
  2. Ubah rencana pemenuhan hanya untuk pemegang `: ChangeFulfilmentPlan`, meminta alasan, dan menampilkan `VR-WFP-33` bila ditolak.
  3. Batalkan kursi hanya untuk pemegang `: RequestCancellation`, untuk kursi `Open`/`Frozen`, meminta alasan; pesan `VR-WFP-35` dan `VR-WFP-36` tampil apa adanya.
  4. Kursi `Frozen` diberi keterangan "dibekukan sesudah addendum".
  5. Tombol aksi nonaktif selama permintaan berjalan; sesudah aksi, detail dimuat ulang.
  6. Riwayat perubahan rencana pemenuhan dan pembatalan tampil berurutan waktu.
* **Bukti verifikasi**: lint dan build; uji manual mengikuti `UAT-39` sejauh data dev memungkinkan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: klik ganda menautkan dua kali — tombol nonaktif selama permintaan berjalan.
* **Definition of Done**: Aksi kursi aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-039` — Layar Antrean Rekrutmen beserta dialog tautan pemenuhan
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R36` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Recruiter melihat kursi rekrut luar yang siap diisi — tanpa angka biaya — dan menautkan penempatan pegawai baru langsung dari antrean, karena Recruiter tidak punya hak membuka detail permintaan.
* **Requirement/decision**: `HRD-DEC-066` (`MANUAL_LINK_UNTIL_RECRUITMENT_EXISTS`), `FR-HRD-118`, `AT-HRD-D1-11` (Recruiter tanpa biaya), `AT-HRD-D1-14`, `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-108`) dan 12.4.
* **Kontrak**: API v7 bagian 13 grup `Recruitment Queue` dan `POST /{id}/seats/{seatId}/fulfilment-link`.
* **Kewenangan UI**: Ketiadaan kolom biaya **mengikat**; bentuk dialog tautan `DEV_DISCRETION`.
* **Reuse**: Pola A; pencarian penempatan memakai endpoint Workforce Core yang sudah ada.
* **Cakupan**: Route `/hr/workforce-planning/recruitment-queue`, view, service; dialog tautan pemenuhan yang dapat dipakai ulang `FE-HRD-038`; butir menu "Antrean Rekrutmen".
* **Dependency**: `FE-HRD-029`, `BE-HRD-042` [BE], `BE-HRD-043` [BE]
* **Acceptance criteria**:
  1. Kolom: nomor permintaan, unit, jabatan, kursi, tanggal dibutuhkan; **tanpa** kolom biaya; kosong → "Tidak ada kursi yang menunggu direkrut."
  2. Butir menu dan layar cukup dengan `WorkforceRecruitmentQueue : Read`.
  3. Tautkan hanya untuk pemegang `WorkforceHeadcountRequest : LinkFulfilment`; sesudah berhasil, antrean dimuat ulang.
  4. Pesan `VR-WFP-29` s.d. `VR-WFP-31` dari backend tampil apa adanya.
* **Bukti verifikasi**: lint dan build; uji manual dengan akun yang hanya memegang hak antrean dan tautan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: komponen tabel umum ikut menampilkan kolom biaya — kolom tidak diminta sama sekali.
* **Definition of Done**: Antrean dan dialog tautan aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-040` — Panel potret gap pada Pemantauan Gap Tenaga
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R36` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Dari Pemantauan Gap, pengguna membuka daftar potret — pengajuan, bulanan, dan susulan — dan detail setiap potret. Bulan tanpa potret tampil "potret tidak tersedia", dan potret susulan berlabel "direkonstruksi".
* **Requirement/decision**: `HRD-DEC-069`, `FR-HRD-105`, `AT-HRD-D1-16b` (bagian layar), `03-frontend-architecture.md` bagian 12.3.1 wilayah Potret bulanan.
* **Kontrak**: API v7 bagian 13 grup `Staffing Gap` (`GET /snapshots`, `GET /snapshots/{id}`).
* **Kewenangan UI**: Bentuk panel `DEV_DISCRETION`; potret tanpa tombol ubah maupun hapus **mengikat**.
* **Reuse**: Tabel gap `FE-HRD-035` untuk menampilkan baris potret.
* **Cakupan**: Tombol "Lihat potret bulanan", daftar, dan detail potret pada layar `FE-HRD-97`.
* **Dependency**: `FE-HRD-035`, `BE-HRD-039` [BE]
* **Acceptance criteria**:
  1. Daftar potret menampilkan jenis, tanggal acuan, dan bulan; bulan yang tidak punya potret tampil "potret tidak tersedia".
  2. Potret `Reconstructed` berlabel "direkonstruksi".
  3. Detail potret memakai aturan "data belum tersedia" yang sama dengan gap langsung.
  4. Tidak ada tombol ubah maupun hapus potret.
* **Bukti verifikasi**: lint dan build; uji manual dengan potret pengajuan yang dibuat `BE-HRD-040`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Panel potret aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-041` — Layar Daftar Rencana Tahunan dan Addendum
* **Slice / MVP**: `S-D1` / `MVP-10`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R27` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: HR, Finance, dan Direktur melihat rencana tahunan beserta addendumnya, statusnya, dan total estimasi biaya bagi yang berhak.
* **Requirement/decision**: `FR-HRD-106`, `FR-HRD-111`, `VR-WFP-15`, `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-104`).
* **Kontrak**: API v7 bagian 13 grup `Manpower Plan` (`GET /`, `POST /`, `GET /filters/metadata`).
* **Kewenangan UI**: Pola A.
* **Reuse**: Pola A; util biaya `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/manpower-plans`, view, service; butir menu "Rencana Tahunan".
* **Dependency**: `FE-HRD-029`, `BE-HRD-046` [BE]
* **Acceptance criteria**:
  1. Kolom: nomor, jenis (rencana/addendum), tahun, status, total estimasi; kosong → "Belum ada rencana untuk tahun ini."
  2. Kolom total biaya hanya bagi pemegang `WorkforcePlanningCost : Read` dan berlabel "estimasi".
  3. Tombol buat rencana hanya untuk pemegang `WorkforceManpowerPlan : Create`; pesan `VR-WFP-15` tampil apa adanya.
* **Bukti verifikasi**: lint dan build; uji manual dengan akun bertanda dan tanpa hak biaya.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar daftar aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-042` — Layar Penyusun dan Detail Rencana
* **Slice / MVP**: `S-D1` / `MVP-10`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R27` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: HR Admin menyusun rencana per baris unit × jabatan, menekan "Hitung ulang" untuk mengisi formasi saat ini, rekomendasi, dan biaya, menulis alasan bila usulan berbeda dari rekomendasi, lalu mengajukan. Rencana yang sudah disetujui hanya baca dan menawarkan "Buat addendum". Finance pada langkahnya mengisi koreksi biaya beserta alasan; angka awal tetap terlihat di sampingnya.
* **Requirement/decision**: `HRD-DEC-062`, `HRD-DEC-065`, `HRD-DEC-068`, `HRD-DEC-069`; `FR-HRD-107` s.d. `FR-HRD-111`; `VR-WFP-16` s.d. `VR-WFP-21`; `UAT-35`, `UAT-36`; `03-frontend-architecture.md` bagian 12.3.3 (`FE-HRD-105`).
* **Kontrak**: API v7 bagian 13 grup `Manpower Plan` (`GET /{id}`, `PUT /{id}`, `POST /{id}/addenda`, `POST /{id}/recalculate`, `POST /{id}/submit`, `POST /{id}/cancel`, `POST /{id}/finance-adjustment`, `GET /{id}/gap-snapshots`); `GET .../staffing-gaps` untuk angka sekarang.
* **Kewenangan UI**: Satu layar berbagian atau berlangkah `DEV_DISCRETION`.
* **Reuse**: Pola layar dokumen bertahap; panel potret dan angka sekarang seperti `FE-HRD-037`; util biaya `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/manpower-plans/[id]`, view, service.
* **Dependency**: `FE-HRD-041`
* **Acceptance criteria**:
  1. Kolom baris: unit, jabatan, grade, formasi kini, rekomendasi beserta rincian, usulan, perubahan, alasan, berlaku mulai, estimasi biaya, biaya perubahan.
  2. Kolom formasi kini, rekomendasi, dan biaya hanya diisi lewat "Hitung ulang", tidak dapat diketik.
  3. Pesan `VR-WFP-18` dan `VR-WFP-19` tampil per baris saat mengajukan.
  4. Simpan, Hitung ulang, Ajukan, Batalkan hanya pada `Draft`/`Returned`; rencana `Approved` hanya baca dengan tombol "Buat addendum" bagi pemegang `WorkforceManpowerPlan : Create`; mencoba mengubah menampilkan `VR-WFP-17` (`UAT-36`).
  5. Biaya berlabel "estimasi" dengan rincian seperti "2 × gaji tengah grade III × 9 bulan"; tanpa hak → kalimat `VR-WFP-37`; tarif tidak ada → "tarif belum tersedia".
  6. Isian koreksi biaya hanya bagi penyetuju langkah Finance yang aktif; angka awal dan koreksi tampil berdampingan beserta alasan (`UAT-35`); pesan `VR-WFP-20` dan `VR-WFP-21` dari backend tampil apa adanya.
  7. Potret saat diajukan tampil berdampingan dengan angka sekarang.
* **Bukti verifikasi**: lint dan build; uji manual mengikuti `UAT-35` dan `UAT-36` sejauh data dev memungkinkan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: angka biaya bocor ke pengguna tanpa hak lewat tooltip atau ekspor — tidak ada jalur tampilan biaya selain kolom yang dijaga `costVisible`.
* **Definition of Done**: Penyusun dan detail rencana aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-043` — Verifikasi menu, hak akses, dan keterjangkauan seluruh layar `S-D1`
* **Slice / MVP**: `S-D1` / Verifikasi
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R42` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Terbukti bahwa kesembilan butir menu `S-D1` terlihat sesuai hak akses setiap peran, ketiga belas layar dapat dicapai, butir lama tidak lagi bermakna sama, dan aturan tampilan yang mengikat dipatuhi di seluruh layar.
* **Requirement/decision**: `AT-HRD-D1-22`; bagian layar `AT-HRD-D1-05b`, `AT-HRD-D1-06`, `AT-HRD-D1-11`, `AT-HRD-D1-16`; `04-prd-to-mvp.md` bagian 21.19 butir "Seluruh layar `S-D1` dapat dicapai lewat menu"; `03-frontend-architecture.md` bagian 12.4.
* **Kontrak**: Frontend Architecture rev 3 bagian 12.2 dan 12.4; Permission v7 bagian 11.2.
* **Kewenangan UI**: Tidak menambah fitur; temuan dikembalikan ke task pemiliknya dan statusnya diturunkan.
* **Reuse**: Seluruh layar `FE-HRD-029` s.d. `FE-HRD-042`.
* **Cakupan**: Pemeriksaan manual per peran dan pencatatan tangkapan layar di laporan. Peran Finance / Anggaran, Direktur, dan Recruiter disimulasikan dengan akun uji yang diberi butir hak akses bagian 11.2; pemetaan peran produksi tetap milik Administrator (`HRD-DEC-032`).
* **Dependency**: `FE-HRD-030`, `FE-HRD-031`, `FE-HRD-034`, `FE-HRD-038`, `FE-HRD-039`, `FE-HRD-040`, `FE-HRD-042`
* **Acceptance criteria**:
  1. Untuk setiap peran pada bagian 12.4, butir menu yang terlihat sama dengan tabel butir hak akses bagian 12.2.
  2. Ketiga belas layar dapat dibuka dari menu atau dari layar induknya; layar detail dokumen juga dapat dibuka dari tautan tugas kotak masuk.
  3. Tidak ada dua butir menu bermakna "kebutuhan tenaga".
  4. Di seluruh layar, nilai `null` tampil "data belum tersedia"; Formasi Jabatan tanpa tombol ubah; Recruiter dan peran tanpa hak biaya tidak melihat angka biaya; potret tampil berdampingan dengan angka sekarang.
  5. Tidak ada angka biaya di `localStorage` maupun `sessionStorage` setelah membuka layar rencana dan permintaan.
* **Bukti verifikasi**: Tangkapan layar sidebar per peran dan per aturan di laporan; `npm run build` pada commit yang diverifikasi.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: akun uji tidak mewakili peran produksi — dicatat sebagai batas bukti.
* **Definition of Done**: Kelima kriteria terbukti dengan tangkapan layar; laporan tracked ada.


### `FE-HRD-044` — Layar Antrean & Permintaan Requisition
* **Slice / MVP**: `S-D2` / `MVP-11`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-044.md](../task/report/frontend/FE-HRD-044.md).
* **Outcome**: HR Recruiter dan Kepala Unit dapat melihat daftar antrean requisition formasi, status pengajuan, serta kuota kursi eksternal yang diizinkan dari alokasi WFP.
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-110`), `FR-REC-01`, `VR-REC-01`, `VR-REC-02`.
* **Kontrak**: API Contract v8 bagian 14.1 grup `Job Requisition`.
* **Kewenangan UI**: Standard table layout dengan filter status dan tombol "Buat Requisition".
* **Reuse**: `@/components/features/base-features/base-form-control`, `BaseButton`, `StatusBadge`.
* **Cakupan**: Route `/hr/recruitment/requisitions`, view, service, penambahan grup menu "Rekrutmen & Seleksi" pada `menu-items.jsx`.
* **Dependency**: `BE-HRD-052` [BE]
* **Acceptance criteria**:
  1. Menampilkan tabel daftar requisition: Nomor, Jabatan, Unit Kerja, Jumlah Kursi, Status, Tanggal Pengajuan.
  2. Tombol "Buat Requisition" membuka modal/form untuk memilih `HeadcountRequestId` yang berstatus `APPROVED`.
  3. Pesan validasi kursi `VR-REC-02` tampil jelas jika kursi melebihi alokasi WFP.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar daftar requisition aktif sesuai kontrak; laporan tracked tersedia.

### `FE-HRD-045` — Layar Detail & Draf Requisition
* **Slice / MVP**: `S-D2` / `MVP-11`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-045.md](../task/report/frontend/FE-HRD-045.md).
* **Outcome**: Pengguna dapat meninjau rincian requisition, justifikasi penambahan tenaga, alokasi kursi WFP terkait, serta melakukan aksi ajukan (*submit*) atau batalkan (*cancel*).
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-111`), `FR-REC-01`, `VR-REC-03`.
* **Kontrak**: API Contract v8 bagian 14.1.
* **Kewenangan UI**: Detail layout dengan timeline status.
* **Reuse**: Card token, `InformationAlert`, `StatusBadge`.
* **Cakupan**: Route `/hr/recruitment/requisitions/[id]`, view, service.
* **Dependency**: `FE-HRD-044`
* **Acceptance criteria**:
  1. Menampilkan detail lengkap requisition beserta tautan ke dokumen perencanaan WFP induk.
  2. Tombol "Ajukan" dan "Batalkan" aktif secara kondisional berdasarkan status dokumen (`DRAFT`).
  3. Konfirmasi pembatalan requisition memvalidasi ketiadaan lowongan aktif (`VR-REC-03`).
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar detail requisition selesai dan terverifikasi; laporan tracked tersedia.

### `FE-HRD-046` — Layar Pengelolaan Lowongan Kerja (*Job Vacancies*)
* **Slice / MVP**: `S-D2` / `MVP-11`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-046.md](../task/report/frontend/FE-HRD-046.md).
* **Outcome**: Pengelola rekrutmen dapat melihat daftar seluruh lowongan pekerjaan (draf, tayang/aktif, ditutup) beserta sisa kuota kursi dan penanda klinis `IsClinical`.
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-112`), `FR-REC-02`, `VR-REC-04`.
* **Kontrak**: API Contract v8 bagian 14.2 grup `Job Vacancy`.
* **Kewenangan UI**: List & filter view dengan badge kategori klinis/non-klinis.
* **Reuse**: Base form control, filter bar Quilvian.
* **Cakupan**: Route `/hr/recruitment/vacancies`, view, service, butir menu "Lowongan Kerja".
* **Dependency**: `FE-HRD-044`, `BE-HRD-053` [BE]
* **Acceptance criteria**:
  1. Tabel lowongan menyajikan Nomor Lowongan, Judul Posisi, Unit, Kuota Kursi, Pelamar Masuk, Status, Tanggal Berakhir.
  2. Kolom peran menampilkan badge visual pembeda posisi medis/klinis (`IsClinical`) vs non-klinis.
  3. Tombol aksi publikasi dan penutupan lowongan terhubung ke endpoint API backend.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar daftar lowongan kerja aktif dan lulus kompilasi; laporan tracked tersedia.

### `FE-HRD-047` — Layar Detail Lowongan & Pelamar Masuk
* **Slice / MVP**: `S-D2` / `MVP-11`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-047.md](../task/report/frontend/FE-HRD-047.md).
* **Outcome**: Recruiter dapat melihat deskripsi kualifikasi lowongan, kuota kursi, serta daftar seluruh berkas pelamar yang masuk pada lowongan tersebut berurutan per tahapan seleksi.
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-113`), `FR-REC-02`.
* **Kontrak**: API Contract v8 bagian 14.2 & 14.3.
* **Kewenangan UI**: Detail layout dengan segmented tab pelamar (Semua, Penapisan, Wawancara, Penawaran, Cadangan).
* **Reuse**: Segmented choices, tabel pelamar.
* **Cakupan**: Route `/hr/recruitment/vacancies/[id]`, view, service.
* **Dependency**: `FE-HRD-046`
* **Acceptance criteria**:
  1. Menampilkan profil lowongan, kuota formasi, dan tanggal tayang.
  2. Menampilkan sub-tabel pelamar dengan status tahapan saat ini.
  3. Tautan klik pada baris pelamar langsung membuka detail profil pelamar terkait.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar detail lowongan dan tab pelamar aktif; laporan tracked tersedia.

### `FE-HRD-048` — Layar Direktori Data Pelamar & NIK Rehire Flow
* **Slice / MVP**: `S-D2` / `MVP-11`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-048.md](../task/report/frontend/FE-HRD-048.md).
* **Outcome**: Recruiter dapat melihat direktori induk pelamar rumah sakit, mencari berdasarkan nama atau NIK, serta mendaftarkan pelamar baru dengan deteksi otomatis status mantan karyawan (*Rehire Badge*) (`HRD-DEC-075`).
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-114`), `FR-REC-03`, `VR-REC-05`, `VR-REC-06`, `UAT-REC-03`.
* **Kontrak**: API Contract v8 bagian 14.3 grup `Candidate Directory`.
* **Kewenangan UI**: Standard search & filter directory dengan modal pendaftaran pelamar.
* **Reuse**: Field validasi NIK 16 digit, `StatusBadge`.
* **Cakupan**: Route `/hr/recruitment/candidates`, view, service, butir menu "Direktori Pelamar".
* **Dependency**: `BE-HRD-054` [BE]
* **Acceptance criteria**:
  1. Form pendaftaran memvalidasi panjang dan format angka NIK (16 digit).
  2. Jika NIK terdeteksi mantan pegawai, form memunculkan alert "Mantan Karyawan RS Terdeteksi" dan otomatis menautkan rehire.
  3. Tabel direktori menyajikan Nama, NIK, Kontak, Posisi Terakhir Dilamar, dan Badge Rehire bila berlaku.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: integritas data NIK — ditegakkan lewat indeks unik dan normalisasi string.
* **Definition of Done**: Layar direktori pelamar dan form pendaftaran selesai; laporan tracked tersedia.

### `FE-HRD-049` — Layar Detail Pelamar & Penapisan STR Klinis Otomatis
* **Slice / MVP**: `S-D2` / `MVP-12`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-049.md](../task/report/frontend/FE-HRD-049.md).
* **Outcome**: Recruiter dapat meninjau berkas lamaran (CV, Ijazah, STR), tanggal kedaluwarsa STR, serta melihat status penapisan administrasi. Bila posisi klinis dan STR kedaluwarsa, layar menampilkan badge merah "Gugur Otomatis — STR Kedaluwarsa" (`HRD-DEC-076`).
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-115`), `FR-REC-04`, `VR-REC-07`, `VR-REC-08`, `AT-HRD-D2-05`, `UAT-REC-02`.
* **Kontrak**: API Contract v8 bagian 14.4 grup `Candidate Screening`.
* **Kewenangan UI**: Dokumen viewer & checklist screening form.
* **Reuse**: Komponen upload berkas, alert peringatan merah / hijau.
* **Cakupan**: Route `/hr/recruitment/candidates/[id]`, view, service.
* **Dependency**: `FE-HRD-048`, `BE-HRD-055` [BE]
* **Acceptance criteria**:
  1. Menampilkan daftar dokumen yang diunggah beserta tanggal masa berlaku STR.
  2. Pelamar klinis dengan STR kedaluwarsa menampilkan banner kegagalan otomatis dan mengunci tombol pelolosan ke tahap wawancara.
  3. Evaluator dapat mencatat catatan penapisan berkas dan menetapkan status kelulusan administrasi.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kepatuhan izin praktek klinis — disajikan transparan di UI.
* **Definition of Done**: Layar detail pelamar dan gerbang penapisan STR aktif; laporan tracked tersedia.

### `FE-HRD-050` — Layar Formulir Evaluasi Wawancara HR
* **Slice / MVP**: `S-D2` / `MVP-12`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-050.md](../task/report/frontend/FE-HRD-050.md).
* **Outcome**: HR Interviewer dapat mengisi formulir penilaian wawancara umum (sikap kerja, motivasi, komunikasi, kesesuaian budaya RS) dan memberikan rekomendasi hasil wawancara.
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-116`), `FR-REC-05`, `VR-REC-10`.
* **Kontrak**: API Contract v8 bagian 14.5 grup `Candidate Evaluation`.
* **Kewenangan UI**: Form scoring bertingkat dengan radio buttons dan catatan kualitatif.
* **Reuse**: Base form control, star/numeric rating component.
* **Cakupan**: Route `/hr/recruitment/evaluations/hr/[id]`, view, service.
* **Dependency**: `FE-HRD-049`, `BE-HRD-056` [BE]
* **Acceptance criteria**:
  1. Form evaluasi memuat parameter penilaian perilaku, kepribadian, dan komitmen RS.
  2. Tombol simpan dan submit mengirimkan skor total dan rekomendasi akhir HR (`RECOMMENDED`, `CONSIDERED`, `NOT_RECOMMENDED`).
  3. Akses form dijaga oleh permission evaluasi HR.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar formulir wawancara HR aktif sesuai kontrak; laporan tracked tersedia.

### `FE-HRD-051` — Layar Formulir Uji Klinis / Asesor Unit dengan Hak Veto Klinis
* **Slice / MVP**: `S-D2` / `MVP-12`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-051.md](../task/report/frontend/FE-HRD-051.md).
* **Outcome**: Kepala Ruang / Asesor Profesi medis dapat mengisi uji kompetensi klinis praktis, dan jika memberi penilaian `NOT_RECOMMENDED`, antarmuka menampilkan badge "Veto Klinis Unit Ditegakkan" yang memblokir proses hiring (`HRD-DEC-077`).
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-117`), `FR-REC-06`, `VR-REC-11`, `VR-REC-12`, `AT-HRD-D2-06`, `UAT-REC-04`.
* **Kontrak**: API Contract v8 bagian 14.5.
* **Kewenangan UI**: Form evaluasi klinis dengan peringatan tegas hak veto unit.
* **Reuse**: InformationAlert level bahaya (`danger`), modal konfirmasi veto.
* **Cakupan**: Route `/hr/recruitment/evaluations/clinical/[id]`, view, service.
* **Dependency**: `FE-HRD-049`, `BE-HRD-056` [BE]
* **Acceptance criteria**:
  1. Form hanya dapat diisi oleh pemegang hak akses asesor profesi/klinis.
  2. Pemilihan rekomendasi `NOT_RECOMMENDED` memunculkan dialog konfirmasi penguncian veto klinis.
  3. Setelah disubmit, status kandidat berubah dan antarmuka mengunci aksi penawaran kerja secara permanen.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko keselamatan pasien: penegakan veto klinis terlihat tegas di UI.
* **Definition of Done**: Layar uji klinis dengan badge hak veto aktif; laporan tracked tersedia.

### `FE-HRD-052` — Layar Daftar Penawaran Kerja (*Job Offers*)
* **Slice / MVP**: `S-D2` / `MVP-12`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-052.md](../task/report/frontend/FE-HRD-052.md).
* **Outcome**: Tim rekrutmen dapat memantau seluruh surat penawaran kerja bersyarat dan final, status respon kandidat, tier persetujuan, dan status kelaikan kerja.
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-118`), `FR-REC-07`, `VR-REC-13`.
* **Kontrak**: API Contract v8 bagian 14.6 grup `Job Offer`.
* **Kewenangan UI**: Filterable table dengan tag status offer dan indikator tier persetujuan.
* **Reuse**: Table component, status badge, action buttons.
* **Cakupan**: Route `/hr/recruitment/offers`, view, service, butir menu "Penawaran Kerja".
* **Dependency**: `BE-HRD-057` [BE]
* **Acceptance criteria**:
  1. Menampilkan daftar surat penawaran: Nomor Offer, Nama Kandidat, Posisi, Gaji Pokok Ditawarkan, Tier Persetujuan, Status.
  2. Tombol "Buat Penawaran" hanya aktif bagi kandidat yang telah lulus evaluasi HR dan Klinis.
  3. Indikator visual membedakan penawaran standar `T13` vs eskalasi `T14`.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar daftar penawaran kerja aktif dan lulus build; laporan tracked tersedia.

### `FE-HRD-053` — Layar Detail Penawaran & Rantai Persetujuan Bertingkat T13/T14
* **Slice / MVP**: `S-D2` / `MVP-13`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-053.md](../task/report/frontend/FE-HRD-053.md).
* **Outcome**: Pengguna dapat meninjau rincian penawaran kompensasi, evaluasi terhadap plafon `MstSalaryGrade`, memantau tahapan persetujuan pejabat, serta melakukan approval/reject sesuai wewenang (`HRD-DEC-079`).
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-119`), `FR-REC-09`, `VR-REC-14`, `VR-REC-15`, `VR-REC-16`, `UAT-REC-06`.
* **Kontrak**: API Contract v8 bagian 14.6.
* **Kewenangan UI**: Detail layout dengan step approval tracking dan form input respon persetujuan.
* **Reuse**: Stepper approval visual, modal approval, alert peringatan eskalasi direksi.
* **Cakupan**: Route `/hr/recruitment/offers/[id]`, view, service.
* **Dependency**: `FE-HRD-052`, `BE-HRD-058` [BE]
* **Acceptance criteria**:
  1. Jika penawaran gaji melebihi plafon grade, layar menampilkan banner oranye "Persetujuan Direktur Keuangan & Dirut Diperlukan (T14)".
  2. Tombol "Setujui" dan "Tolak" hanya muncul bagi akun penyetuju yang sedang aktif pada gilirannya.
  3. Riwayat jejak audit persetujuan (waktu, penyetuju, catatan) ditampilkan transparan.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kepatuhan tata kelola keuangan RS.
* **Definition of Done**: Layar detail penawaran dan stepper persetujuan aktif; laporan tracked tersedia.

### `FE-HRD-054` — Layar Pemantauan Kelaikan MCU Pra-Kerja & Masking Medis K3RS
* **Slice / MVP**: `S-D2` / `MVP-13`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-054.md](../task/report/frontend/FE-HRD-054.md).
* **Outcome**: HR Recruiter dan Dokter K3RS dapat memantau status pemeriksaan MCU pelamar pra-kerja. Untuk akun HR non-medis, seluruh detail diagnosa medis disembunyikan/dimasking, hanya menyajikan kesimpulan kelaikan (`FIT`, `FIT_WITH_RESTRICTIONS`, `UNFIT`) (`HRD-DEC-078`, `HRD-DEC-010`).
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-120`), `FR-REC-08`, `VR-REC-17`, `AT-HRD-D2-09`, `UAT-REC-05`.
* **Kontrak**: API Contract v8 bagian 14.7 grup `MCU Clearance`.
* **Kewenangan UI**: Monitoring dashboard dengan proteksi privasi data medis staf K3RS.
* **Reuse**: StatusBadge, privacy badge.
* **Cakupan**: Route `/hr/recruitment/mcu-clearance`, view, service, butir menu "Kelaikan MCU Pra-Kerja".
* **Dependency**: `FE-HRD-052`, `BE-HRD-059` [BE]
* **Acceptance criteria**:
  1. Daftar kandidat menyajikan tanggal rujukan MCU, fasilitas pemeriksa, dan kesimpulan kelaikan.
  2. Akun non-K3RS tidak memiliki kolom atau modal diagnosa penyakit; hanya badge kelaikan kerja yang tampil (`UAT-REC-05`).
  3. Dokter K3RS memiliki tombol aksi untuk mengunggah surat keterangan dokter dan memilih kesimpulan resmi.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead bersama Dokter K3RS. Risiko kerahasiaan medis staf.
* **Definition of Done**: Layar pemantauan MCU dan kontrol masking medis aktif; laporan tracked tersedia.

### `FE-HRD-055` — Layar Bank Kandidat Cadangan (*Talent Pool*) & Fast-Track Promotion
* **Slice / MVP**: `S-D2` / `MVP-13`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-055.md](../task/report/frontend/FE-HRD-055.md).
* **Outcome**: Pengelola rekrutmen dapat melihat daftar kandidat cadangan berperingkat yang siap dipromosikan cepat bila kursi lowongan kosong, memantau masa kedaluwarsa retensi (6 bulan), dan mengeksekusi promosi cepat (*Fast-Track*) (`HRD-DEC-080`).
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-121`), `FR-REC-11`, `FR-REC-12`, `VR-REC-18`, `VR-REC-19`, `UAT-REC-07`.
* **Kontrak**: API Contract v8 bagian 14.8 grup `Talent Pool`.
* **Kewenangan UI**: Leaderboard talent pool dengan countdown masa retensi dan tombol aksi promosi instan.
* **Reuse**: Badge peringkat skor, countdown timer/badge hari tersisa.
* **Cakupan**: Route `/hr/recruitment/talent-pool`, view, service, butir menu "Bank Kandidat (Talent Pool)".
* **Dependency**: `BE-HRD-060` [BE]
* **Acceptance criteria**:
  1. Menampilkan urutan peringkat kandidat berdasarkan skor akumulasi tes, posisi yang diminati, dan sisa hari retensi.
  2. Tombol "Promosi Cepat" memunculkan dialog konfirmasi penerbitan penawaran langsung ke kandidat cadangan peringkat 1.
  3. Kandidat dengan masa retensi habis (> 6 bulan) berstatus `EXPIRED` dan tombol promosinya terkunci (`VR-REC-18`).
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar bank kandidat dan fast-track promotion aktif; laporan tracked tersedia.

### `FE-HRD-056` — Layar Serah Terima ke Onboarding (*Hiring Handoff*)
* **Slice / MVP**: `S-D2` / `MVP-13`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-056.md](../task/report/frontend/FE-HRD-056.md).
* **Outcome**: HR dapat melihat daftar kandidat yang telah lolos seluruh seleksi, menyepakati penawaran final, dan dinyatakan fit MCU, lalu mengeksekusi serah terima data ke Sub-modul 05 (Onboarding) untuk pembuatan profil draf kepegawaian di Sub-modul 02 (`HRD-DEC-075`).
* **Requirement/decision**: `03-frontend-architecture.md` bagian 13.1 (`FE-HRD-122`), `FR-REC-10`, `VR-REC-20`, `AT-HRD-D2-11`.
* **Kontrak**: API Contract v8 bagian 14.9 grup `Hiring Handoff`.
* **Kewenangan UI**: Handoff summary checklist & action view.
* **Reuse**: Base form control, card summary.
* **Cakupan**: Route `/hr/recruitment/handoffs`, view, service, butir menu "Serah Terima Onboarding".
* **Dependency**: `FE-HRD-053`, `FE-HRD-054`, `BE-HRD-061` [BE]
* **Acceptance criteria**:
  1. Hanya menampilkan kandidat yang memenuhi syarat: Offer `FINAL_ACCEPTED` dan MCU `FIT`/`FIT_WITH_RESTRICTIONS` (`VR-REC-20`).
  2. Tombol "Kirim ke Onboarding" mengeksekusi transfer dan memunculkan konfirmasi nomor pegawai baru draf.
  3. Tautan langsung ke profil pegawai draf pada modul Administrasi Kepegawaian.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko integrasi sub-modul.
* **Definition of Done**: Layar serah terima onboarding aktif dan teruji; laporan tracked tersedia.

### `FE-HRD-057` — Verifikasi Menu, Hak Akses, dan Keterjangkauan Seluruh Layar Rekrutmen `S-D2`
* **Slice / MVP**: `S-D2` / Verifikasi
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-057.md](../task/report/frontend/FE-HRD-057.md).
* **Outcome**: Membuktikan bahwa seluruh 13 layar rekrutmen `S-D2` (`FE-HRD-110` s.d. `FE-HRD-122`) dapat diakses via navigasi sidebar menu "Rekrutmen & Seleksi", hak akses RBAC sesuai peran (HR Manager, Recruiter, Unit Assessor, K3RS Doctor, Finance Director, Dirut), dan aturan kerahasiaan medis staf ditaati.
* **Requirement/decision**: `AT-HRD-D2-12`, `03-frontend-architecture.md` bagian 13.2 & 13.3, `04-prd-to-mvp.md` bagian 22.6.
* **Kontrak**: Frontend Architecture rev 4 bagian 13.2, Permission v8 bagian 12.
* **Kewenangan UI**: Audit pengujian komprehensif, pencatatan screenshot verifikasi.
* **Reuse**: Seluruh layar `FE-HRD-044` s.d. `FE-HRD-056`.
* **Cakupan**: Verifikasi navigasi rute, role switcher check, dan kompilasi production build.
* **Dependency**: `FE-HRD-045`, `FE-HRD-047`, `FE-HRD-050`, `FE-HRD-051`, `FE-HRD-053`, `FE-HRD-054`, `FE-HRD-055`, `FE-HRD-056`
* **Acceptance criteria**:
  1. Seluruh 7 butir menu rekrutmen muncul sesuai hak akses peran di sidebar.
  2. Seluruh 13 layar dapat dibuka tanpa error JavaScript atau layout broken.
  3. Akun non-medis terbukti tidak dapat mengakses halaman formulir diagnosa medis K3RS.
  4. `npm run lint:errors` dan `npm run build` berhasil 0 error.
* **Bukti verifikasi**: Tangkapan layar navigasi menu per peran; output log `npm run build` sukses.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead bersama QA. Risiko integrasi navigasi.
* **Definition of Done**: Seluruh layar rekrutmen terverifikasi lulus audit; laporan tracked tersedia.


---

## Kartu Detail Task Frontend — Slice `S-C4-ONB` Onboarding dan Kontrak Kerja (sub-modul 05)

Ditambahkan pada revision 7, 4 November 2026, sesudah desain `S-C4-ONB` disetujui pemilik
(`HRD-DEC-096`). Sumber desain: `03-frontend-architecture.md` bagian 14; kontrak `v9` bagian
`S-C4-ONB` (API bagian 15, Validation bagian 14, Permission bagian 13).

**Catatan umum yang berlaku untuk setiap task di bawah:**

- **Bukti verifikasi frontend** mengikuti `rules/frontend/test-policy.md`: `npm run lint:errors`,
  `npm run build`, `npm run test:unit` untuk suite yang sudah ada, dan verifikasi manual kontrol
  interaktif. Test unit baru **direkomendasikan** hanya untuk logika murni — pemetaan label status,
  hitungan tanggal pratinjau, validasi berkas — dan tidak menahan status task.
- **Kewenangan UI** mengikuti `03-frontend-architecture.md` 14.8: tombol yang tidak berhak
  disembunyikan; tombol aksi kasus mengikuti `availableActions` dari backend; nama butir menu dan label
  status usulan kerja (`HRD-FE-04` `OPEN`); tab, modal, drawer, warna, ikon, dan urutan
  `DEV_DISCRETION`. Pemakaian ulang atau komponen baru diputuskan saat eksekusi lewat
  `base-component-decision-gate` pada `build-module-frontend`.
- **Menu.** Grup "Onboarding & Kontrak" didaftarkan pada task layar pertama yang dikerjakan
  (`FE-HRD-059`); setiap task layar berikutnya menambahkan butir menunya sendiri sebagai acceptance
  criteria. Berkas yang disunting: `src/utils/menu-sidebar/menu-items.jsx`.
- **Gaji.** Nominal gaji tidak disimpan di Redux maupun penyimpanan peramban (`HRD-DEC-040`).
- Pemilik seluruh task: Frontend HR UI Lead; pemilik keputusan produk: Pengguna (`HRD-DEC-015`).

| Task ID | MVP | Ringkasan |
| --- | --- | --- |
| `FE-HRD-058` | `MVP-14` | Fondasi layanan API dan label status |
| `FE-HRD-059` | `MVP-14` | Template Onboarding dan grup menu |
| `FE-HRD-060` | `MVP-14` | Daftar dan detail Kasus Onboarding |
| `FE-HRD-063` | `MVP-14` | Daftar dan detail Kontrak Kerja |
| `FE-HRD-071` | `MVP-14` | Perubahan layar lama: riwayat kontrak, label, master pegawai |
| `FE-HRD-072` | `MVP-14` | Amandemen layar tawaran `S-D2` |
| `FE-HRD-061` | `MVP-15` | Aksi checklist pada kasus |
| `FE-HRD-062` | `MVP-15` | Siap kerja, batal bergabung, dan tautan dari serah terima |
| `FE-HRD-066` | `MVP-15` | Pendampingan Klinis |
| `FE-HRD-068` | `MVP-15` | Permintaan Akses Pegawai Baru |
| `FE-HRD-064` | `MVP-16` | Keputusan dan koreksi kontrak |
| `FE-HRD-067` | `MVP-16` | Batch Data Awal Kontrak |
| `FE-HRD-069` | `MVP-16` | Pengaturan Onboarding dan Kontrak |
| `FE-HRD-070` | `MVP-16` | Perlu Tindakan |
| `FE-HRD-065` | `MVP-17` | Masa Percobaan |
| `FE-HRD-073` | `MVP-17` | Empat jenis tugas di Kotak Masuk |
| `FE-HRD-074` | Verifikasi | Verifikasi menu, hak akses, dan keterjangkauan |

### `FE-HRD-058` — Fondasi layanan API dan label status `S-C4-ONB`
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-058.md](../task/report/frontend/FE-HRD-058.md).
* **Outcome**: Seluruh layar sub-modul 05 memakai satu lapisan layanan API dan satu berkas pemetaan status ke label Indonesia.
* **Requirement/decision**: `03-frontend-architecture.md` 14.6, 14.7; `HRD-DEC-081`.
* **Kontrak**: API `v9` bagian 15.1 s.d. 15.9.
* **Kewenangan UI**: Tidak ada keputusan rupa.
* **Reuse**: Pola service HR yang sudah ada di `src/lib/services/hr/**` dan `InstanceAxios`.
* **Cakupan**: Berkas service per grup Swagger bagian 15; konstanta label status dan kategori Perlu Tindakan; penanganan `409` versi usang dan pesan `422` seragam.
* **Dependency**: —
* **Acceptance criteria**:
  1. Setiap endpoint bagian 15.1 s.d. 15.9 punya fungsi service; unggahan memakai `multipart/form-data`.
  2. Label status pada 14.6 berada di satu berkas konstanta dan tidak diulang di komponen.
  3. Jawaban `409` memunculkan pesan "Data ini sudah diubah orang lain. Muat ulang lalu ulangi."
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; test unit pemetaan label direkomendasikan.
* **Risiko & pemilik**: Rendah. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Lapisan layanan dan konstanta tersedia; laporan tracked tersedia.

### `FE-HRD-059` — Template Onboarding (`FE-HRD-132`, `FE-HRD-133`) dan grup menu
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-059.md](../task/report/frontend/FE-HRD-059.md).
* **Outcome**: HRD menyusun tiga lapis template dengan butir sistem yang terkunci, dan grup menu "Onboarding & Kontrak" muncul di sidebar.
* **Requirement/decision**: `HRD-DEC-086`; `03-frontend-architecture.md` 14.2, 14.3.7; `VR-ONB-20` s.d. `VR-ONB-25`.
* **Kontrak**: API `v9` bagian 15.2.
* **Kewenangan UI**: Susunan editor butir `DEV_DISCRETION`; butir sistem wajib tampil terkunci berlabel "Butir sistem".
* **Reuse**: Pola daftar pola A dan form master HR.
* **Cakupan**: Route `/hr/lifecycle/onboarding-templates` dan `/hr/lifecycle/onboarding-templates/[id]`; grup menu "Onboarding & Kontrak" beserta butir "Template Onboarding".
* **Dependency**: `FE-HRD-058`, `BE-HRD-070` [BE]
* **Acceptance criteria**:
  1. Grup menu terdaftar sebagai tingkat 1 di bawah Sumber Daya Manusia; butir "Template Onboarding" dijaga `WorkforceOnboardingTemplate : Read`.
  2. Butir sistem tidak dapat dihapus atau diubah jenisnya di layar.
  3. Pesan `VR-ONB-20` s.d. `VR-ONB-25` dari backend tampil di dekat tombol simpan.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual simpan template tiap lapis.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar template dan grup menu aktif; laporan tracked tersedia.

### `FE-HRD-060` — Daftar dan Detail Kasus Onboarding (`FE-HRD-124`, `FE-HRD-125`)
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-060.md](../task/report/frontend/FE-HRD-060.md).
* **Outcome**: HRD dan Kepala Unit melihat kasus onboarding berjalan, kemajuan checklist, kontrak pertama, status akun, dan peringatan.
* **Requirement/decision**: `HRD-DEC-083`; `03-frontend-architecture.md` 14.3.2, 14.3.3.
* **Kontrak**: API `v9` bagian 15.1 (`GET /`, `GET /summary`, `GET /{id}`).
* **Kewenangan UI**: Susunan kepala dan checklist `DEV_DISCRETION`; isi wilayah mengikuti tabel 14.3.3.
* **Reuse**: Pola daftar pola A; pola layar dokumen bertahap SK.
* **Cakupan**: Route `/hr/lifecycle/onboarding-cases` dan `/[id]` (baca saja pada task ini); butir menu "Kasus Onboarding".
* **Dependency**: `FE-HRD-058`, `BE-HRD-074` [BE]
* **Acceptance criteria**:
  1. Keempat keadaan daftar tampil dengan kalimat 14.3.2.
  2. Detail menampilkan butir per lapis, kontrak pertama, permintaan akun, dan peringatan; kasus tanpa Template Umum menampilkan kalimat 14.3.3.
  3. Kepala Unit tidak melihat kasus di luar unitnya; detail di luar cakupan menampilkan "Data tidak ditemukan."
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual per peran.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Daftar dan detail kasus aktif dan terjangkau dari menu; laporan tracked tersedia.

### `FE-HRD-061` — Aksi checklist pada Detail Kasus Onboarding
* **Slice / MVP**: `S-C4-ONB` / `MVP-15`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-061.md](../task/report/frontend/FE-HRD-061.md).
* **Outcome**: HRD dan Kepala Unit mencentang butir masing-masing, mengunggah bukti, memilih atasan langsung dan preceptor, mencatat hadir hari pertama, dan mengalihkan koordinator.
* **Requirement/decision**: `HRD-DEC-086`, `HRD-DEC-087`, `HRD-DEC-088`; `VR-ONB-03` s.d. `VR-ONB-10`, `VR-ONB-17`.
* **Kontrak**: API `v9` bagian 15.1 (hadir hari pertama, centang dan batal centang butir HRD dan unit, periksa ulang otomatis, alihkan).
* **Kewenangan UI**: Bentuk form centang `DEV_DISCRETION`; butir otomatis tidak punya tombol.
* **Reuse**: Komponen unggah berkas HR yang sudah ada.
* **Cakupan**: Aksi pada `FE-HRD-125`.
* **Dependency**: `FE-HRD-060`, `BE-HRD-075` [BE]
* **Acceptance criteria**:
  1. Butir HRD hanya dapat dicentang pemegang `WorkforceOnboardingCase : CompleteItem`; butir unit hanya pemegang `: CompleteUnitItem`.
  2. Butir wajib bukti menolak dicentang sebelum berkas dipilih; jenis dan ukuran berkas diperiksa sebelum unggah (`VR-ONB-09`).
  3. Pilihan atasan langsung dan preceptor hanya memuat pegawai aktif di unit itu.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual tiap jenis butir.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Seluruh aksi checklist dapat dipakai dari layar; laporan tracked tersedia.

### `FE-HRD-062` — Siap kerja, batal bergabung, dan tautan dari layar serah terima
* **Slice / MVP**: `S-C4-ONB` / `MVP-15`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-062.md](../task/report/frontend/FE-HRD-062.md).
* **Outcome**: HRD menyatakan siap kerja atau mencatat batal bergabung dari detail kasus, dan layar serah terima rekrutmen menautkan kasus yang terbentuk.
* **Requirement/decision**: `HRD-DEC-083`, `HRD-DEC-088`; `VR-ONB-11` s.d. `VR-ONB-14`.
* **Kontrak**: API `v9` bagian 15.1 (`declare-ready`, `cancel-join`).
* **Kewenangan UI**: Konfirmasi sebagai modal atau panel `DEV_DISCRETION`.
* **Reuse**: Layar serah terima `FE-HRD-122` milik `FE-HRD-056`.
* **Cakupan**: Dua aksi pada `FE-HRD-125`; tautan "Buka kasus onboarding" pada `FE-HRD-122`.
* **Dependency**: `FE-HRD-061`, `BE-HRD-076` [BE], `FE-HRD-056`
* **Acceptance criteria**:
  1. Nyatakan Siap Kerja nonaktif dengan keterangan "Masih ada {n} butir wajib" bila butir wajib belum lengkap.
  2. Batal Bergabung tidak tampil sesudah hadir hari pertama tercatat; alasan dan catatan wajib diisi.
  3. Sesudah serah terima berhasil, layar serah terima menampilkan tautan ke kasus yang terbentuk.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual kedua aksi.
* **Risiko & pemilik**: Menyentuh layar `S-D2`; perubahan dibatasi satu tautan. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Kedua titik akhir kasus dapat dijalankan dari layar; laporan tracked tersedia.

### `FE-HRD-063` — Daftar dan Detail Kontrak Kerja (`FE-HRD-126`, `FE-HRD-127`)
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-063.md](../task/report/frontend/FE-HRD-063.md).
* **Outcome**: HRD melihat kontrak dan rantainya, mencetak draf, menandai sudah ditandatangani dengan scan, dan mengunggah scan susulan kontrak data awal.
* **Requirement/decision**: `HRD-DEC-084`, `HRD-DEC-033`, `HRD-DEC-040`; `VR-ONB-30`, `VR-ONB-31`, `VR-ONB-45`.
* **Kontrak**: API `v9` bagian 15.3 (daftar, detail, data cetak, tandai ditandatangani, dokumen).
* **Kewenangan UI**: Bentuk cetak `DEV_DISCRETION`; nominal gaji hanya bila `amountVisible = true`.
* **Reuse**: Pola cetak dokumen HR yang sudah ada, bila ada.
* **Cakupan**: Route `/hr/lifecycle/employment-contracts` dan `/[id]`; butir menu "Kontrak Kerja".
* **Dependency**: `FE-HRD-058`, `BE-HRD-072` [BE]
* **Acceptance criteria**:
  1. Tidak ada isian ubah isi kontrak di layar; draf menampilkan "Ubah lewat revisi tawaran di Rekrutmen."
  2. Tandai Sudah Ditandatangani menolak tanpa berkas.
  3. Baris gaji pada cetak berbunyi "disembunyikan" bagi yang tidak berhak, dan nominal tidak tersimpan di Redux maupun penyimpanan peramban.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual cetak dengan dua peran.
* **Risiko & pemilik**: Privasi gaji. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar kontrak aktif dan terjangkau dari menu; laporan tracked tersedia.

### `FE-HRD-064` — Keputusan dan koreksi kontrak pada Detail Kontrak Kerja
* **Slice / MVP**: `S-C4-ONB` / `MVP-16`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-064.md](../task/report/frontend/FE-HRD-064.md).
* **Outcome**: Atasan mengajukan Perpanjang, Angkat jadi Pegawai Tetap, atau Tidak Diperpanjang dengan pratinjau total PKWT; HRD mencatat tanggal pemberitahuan dan mengajukan koreksi.
* **Requirement/decision**: `HRD-DEC-089`; `VR-ONB-35` s.d. `VR-ONB-44`.
* **Kontrak**: API `v9` bagian 15.3 (keputusan, `fixed-term-check`, koreksi).
* **Kewenangan UI**: Form sebagai panel atau modal `DEV_DISCRETION`.
* **Reuse**: Komponen unggah berkas.
* **Cakupan**: Aksi pada `FE-HRD-127`.
* **Dependency**: `FE-HRD-063`, `BE-HRD-080` [BE], `BE-HRD-081` [BE]
* **Acceptance criteria**:
  1. Pilihan Perpanjang menampilkan pratinjau total PKWT dan nonaktif dengan keterangan bila melewati 5 tahun.
  2. Tidak Diperpanjang wajib alasan; Catat Pemberitahuan hanya tampil untuk Tidak Diperpanjang yang disetujui.
  3. Koreksi wajib alasan dan scan perbaikan, dan menampilkan nilai sebelum dan sesudah pada detail.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; test unit hitungan pratinjau direkomendasikan; verifikasi manual tiga keputusan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Keputusan dan koreksi dapat diajukan dari layar; laporan tracked tersedia.

### `FE-HRD-065` — Masa Percobaan (`FE-HRD-128`, `FE-HRD-129`)
* **Slice / MVP**: `S-C4-ONB` / `MVP-17`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-065.md](../task/report/frontend/FE-HRD-065.md).
* **Outcome**: Atasan mengajukan Lulus atau Tidak Lulus beserta alasan dan lampiran; HRD dan atasan melihat riwayat penilaian dan lulus otomatis.
* **Requirement/decision**: `HRD-DEC-085`; `VR-ONB-55` s.d. `VR-ONB-59`.
* **Kontrak**: API `v9` bagian 15.4.
* **Kewenangan UI**: Susunan form `DEV_DISCRETION`; tidak ada tombol perpanjangan.
* **Reuse**: Pola daftar pola A.
* **Cakupan**: Route `/hr/lifecycle/probations` dan `/[id]`; butir menu "Masa Percobaan".
* **Dependency**: `FE-HRD-058`, `BE-HRD-079` [BE]
* **Acceptance criteria**:
  1. Form hasil tidak tampil sesudah tanggal akhir atau saat ada penilaian terbuka.
  2. Tidak Lulus menolak diajukan tanpa lampiran.
  3. Lulus otomatis tampil "Lulus otomatis pada {tanggal}".
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar masa percobaan aktif dan terjangkau dari menu; laporan tracked tersedia.

### `FE-HRD-066` — Pendampingan Klinis (`FE-HRD-130`, `FE-HRD-131`)
* **Slice / MVP**: `S-C4-ONB` / `MVP-15`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-066.md](../task/report/frontend/FE-HRD-066.md).
* **Outcome**: Kepala Unit mengganti preceptor dan menyatakan Siap Mandiri dengan lampiran; HRD membaca riwayat.
* **Requirement/decision**: `HRD-DEC-087`; `VR-ONB-60` s.d. `VR-ONB-63`.
* **Kontrak**: API `v9` bagian 15.5.
* **Kewenangan UI**: Susunan `DEV_DISCRETION`; tidak ada tombol persetujuan HRD.
* **Reuse**: Pola daftar pola A; komponen unggah berkas.
* **Cakupan**: Route `/hr/lifecycle/clinical-preceptorships` dan `/[id]`; butir menu "Pendampingan Klinis".
* **Dependency**: `FE-HRD-058`, `BE-HRD-073` [BE]
* **Acceptance criteria**:
  1. Siap Mandiri hanya tampil saat Didampingi dan menolak tanpa lampiran.
  2. Ganti preceptor wajib alasan; pilihan hanya tenaga klinis aktif di unit itu.
  3. Riwayat preceptor tampil dengan tanggal dari dan sampai.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar pendampingan aktif dan terjangkau dari menu; laporan tracked tersedia.

### `FE-HRD-067` — Batch Data Awal Kontrak (`FE-HRD-134`, `FE-HRD-135`)
* **Slice / MVP**: `S-C4-ONB` / `MVP-16`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-067.md](../task/report/frontend/FE-HRD-067.md).
* **Outcome**: HRD mengunduh templat, mengunggah, memperbaiki baris yang gagal, lalu mengajukan batch.
* **Requirement/decision**: `HRD-DEC-090`; `VR-ONB-65` s.d. `VR-ONB-70`.
* **Kontrak**: API `v9` bagian 15.6.
* **Kewenangan UI**: Mengikuti layar batch penempatan awal; detail rupa `DEV_DISCRETION`.
* **Reuse**: Layar batch penempatan awal `/hr/initial-placement-batch` sebagai pola.
* **Cakupan**: Route `/hr/lifecycle/initial-contract-batches` dan `/[id]`; butir menu "Batch Data Awal Kontrak".
* **Dependency**: `FE-HRD-058`, `BE-HRD-082` [BE]
* **Acceptance criteria**:
  1. Pesan per baris dari validasi backend tampil di baris yang bersangkutan.
  2. Tombol Ajukan nonaktif selama masih ada baris gagal.
  3. Kosong menampilkan "Belum ada baris. Unduh templat untuk mulai."
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual unggah templat contoh.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar batch aktif dan terjangkau dari menu; laporan tracked tersedia.

### `FE-HRD-068` — Permintaan Akses Pegawai Baru (`FE-HRD-136`)
* **Slice / MVP**: `S-C4-ONB` / `MVP-15`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-068.md](../task/report/frontend/FE-HRD-068.md).
* **Outcome**: Administrator/IT melihat permintaan akses kerja dan menandainya selesai; HRD mengulang pembuatan akun yang gagal.
* **Requirement/decision**: `HRD-DEC-088`; `VR-ONB-75` s.d. `VR-ONB-77`.
* **Kontrak**: API `v9` bagian 15.7.
* **Kewenangan UI**: `DEV_DISCRETION` untuk susunan.
* **Reuse**: Pola daftar pola A.
* **Cakupan**: Route `/hr/lifecycle/onboarding-access-requests`; butir menu "Permintaan Akses Pegawai Baru".
* **Dependency**: `FE-HRD-058`, `BE-HRD-077` [BE]
* **Acceptance criteria**:
  1. Tombol Tandai Selesai hanya tampil untuk permintaan akses kerja yang menunggu, bagi pemegang `WorkforceOnboardingAccessRequest : Complete`.
  2. Pesan gagal pembuatan akun tampil apa adanya, dan tombol Ulangi hanya untuk permintaan akun yang gagal.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar permintaan akses aktif dan terjangkau dari menu; laporan tracked tersedia.

### `FE-HRD-069` — Pengaturan Onboarding dan Kontrak (`FE-HRD-137`)
* **Slice / MVP**: `S-C4-ONB` / `MVP-16`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-069.md](../task/report/frontend/FE-HRD-069.md).
* **Outcome**: HR Manager mengubah empat angka pengingat; pemegang baca saja melihat isian baca saja.
* **Requirement/decision**: `HRD-DEC-085` butir 4, `HRD-DEC-089` butir 1 dan 8; `VR-ONB-80`.
* **Kontrak**: API `v9` bagian 15.9.
* **Kewenangan UI**: `DEV_DISCRETION`.
* **Reuse**: Pola pengaturan perencanaan `S-D1`.
* **Cakupan**: Route `/hr/lifecycle/settings`; butir menu "Pengaturan".
* **Dependency**: `FE-HRD-058`, `BE-HRD-069` [BE]
* **Acceptance criteria**:
  1. Simpan hanya tampil bagi pemegang `WorkforceLifecycleSetting : Update`.
  2. Rentang nilai diperiksa sebelum simpan dan pesan `VR-ONB-80` dari backend tampil.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar pengaturan aktif dan terjangkau dari menu; laporan tracked tersedia.

### `FE-HRD-070` — Perlu Tindakan (`FE-HRD-123`)
* **Slice / MVP**: `S-C4-ONB` / `MVP-16`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-070.md](../task/report/frontend/FE-HRD-070.md).
* **Outcome**: Setiap peran melihat apa yang harus ditindaklanjuti hari ini — pengganti pengingat yang tidak ada (`HRD-DEC-096` butir 3).
* **Requirement/decision**: `HRD-DEC-085`, `HRD-DEC-089`, `HRD-DEC-090`; `03-frontend-architecture.md` 14.3.1.
* **Kontrak**: API `v9` bagian 15.8.
* **Kewenangan UI**: Bentuk penyaring kategori `DEV_DISCRETION`.
* **Reuse**: Pola daftar pola A.
* **Cakupan**: Route `/hr/lifecycle/worklist`; butir menu "Perlu Tindakan" beserta penanda jumlah.
* **Dependency**: `FE-HRD-058`, `BE-HRD-084` [BE]
* **Acceptance criteria**:
  1. Kategori yang tidak berlaku bagi peran pemanggil tidak tampil.
  2. Tombol Buka membuka layar sesuai `targetType`.
  3. Kosong menampilkan "Tidak ada yang perlu ditindaklanjuti saat ini."
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual per peran.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar Perlu Tindakan aktif dan terjangkau dari menu; laporan tracked tersedia.

### `FE-HRD-071` — Perubahan layar lama: tab riwayat kontrak, label "Probation", dan form master pegawai
* **Slice / MVP**: `S-C4-ONB` / `MVP-14`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-071.md](../task/report/frontend/FE-HRD-071.md).
* **Outcome**: Layar lama tidak lagi dapat mengubah kontrak atau isian kepegawaian yang dikelola sub-modul 05, dan menautkan ke layar Kontrak Kerja.
* **Requirement/decision**: `HRD-DEC-085` butir 2, `HRD-DEC-096` butir 5; `HRD-TF-043`; `VR-ONB-32`, `VR-ONB-33`.
* **Kontrak**: API `v9` bagian 15.11.
* **Kewenangan UI**: Letak tautan `DEV_DISCRETION`.
* **Reuse**: Komponen yang ada.
* **Cakupan**: `src/components/features/workforce/workforce-resource-editor.jsx` (tab riwayat kontrak baca saja, isian `probationEndDate` dihapus); `workforce-profile-dashboard.jsx` (label "Probation" tidak dipakai untuk masa percobaan); `employee-grouped-form.config.jsx` (lima isian baca saja untuk pegawai yang punya kontrak).
* **Dependency**: `FE-HRD-063`, `BE-HRD-065` [BE]
* **Acceptance criteria**:
  1. Tab riwayat kontrak tidak punya tombol tambah, ubah, ubah status, jadikan berjalan, maupun hapus, dan menautkan "Kelola di Kontrak Kerja".
  2. Form master pegawai menampilkan lima isian baca saja berketerangan "Mengikuti kontrak kerja" untuk pegawai yang punya kontrak.
  3. Profil menampilkan "Masa percobaan s.d. {tanggal}" dari data masa percobaan bila ada.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`, `npm run test:unit` (suite yang ada); verifikasi manual editor workforce.
* **Risiko & pemilik**: Menyentuh komponen besar yang dipakai banyak layar; perubahan dibatasi tiga titik. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Ketiga layar lama selaras; laporan tracked tersedia.

### `FE-HRD-072` — Amandemen layar tawaran `S-D2`: jenis dan lama kontrak
* **Slice / MVP**: `S-D2` (amandemen dari sub-modul 05) / `MVP-14`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-072.md](../task/report/frontend/FE-HRD-072.md).
* **Outcome**: Recruiter memilih jenis kontrak dan lama kontrak saat menyusun tawaran; penyetuju dan surat tawaran menampilkannya beserta tanggal berakhir.
* **Requirement/decision**: `HRD-DEC-095`; `VR-ONB-50`, `VR-ONB-51`.
* **Kontrak**: API `v9` bagian 15.12.
* **Kewenangan UI**: Letak isian `DEV_DISCRETION`.
* **Reuse**: Form dan detail tawaran `FE-HRD-118`, `FE-HRD-119`.
* **Cakupan**: Form penyusunan tawaran, layar persetujuan, dan tampilan surat tawaran.
* **Dependency**: `FE-HRD-053`, `BE-HRD-064` [BE]
* **Acceptance criteria**:
  1. Jenis kontrak wajib dipilih; lama kontrak wajib 1–60 bulan untuk jenis yang mensyaratkan tanggal berakhir, dan tersembunyi untuk jenis lainnya.
  2. Tanggal berakhir hasil hitung tampil pada form, layar persetujuan, dan surat tawaran.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual tawaran PKWT dan PKWTT.
* **Risiko & pemilik**: Menyentuh layar `S-D2` yang sudah disetujui. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Isian kontrak tampil di tiga tempat; laporan tracked tersedia.

### `FE-HRD-073` — Empat jenis tugas `T15` s.d. `T18` di Kotak Masuk
* **Slice / MVP**: `S-C4-ONB` / `MVP-17`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-073.md](../task/report/frontend/FE-HRD-073.md).
* **Outcome**: Petugas HRD memutus hasil masa percobaan, keputusan kontrak, koreksi kontrak, dan batch data awal dari kotak masuk yang sudah ada, dengan tautan ke dokumennya.
* **Requirement/decision**: `HRD-DEC-081`, `HRD-DEC-018`.
* **Kontrak**: Kotak masuk bagian 6 kontrak API; jenis rujukan pada Integration `v9` bagian 13 (`INT-HR-30`).
* **Kewenangan UI**: Mengikuti kotak masuk `FE-HRD-30`/`FE-HRD-31`.
* **Reuse**: Kotak masuk persetujuan yang sudah ada.
* **Cakupan**: Pemetaan empat jenis rujukan ke label dan tautan `FE-HRD-129`, `FE-HRD-127`, `FE-HRD-135`.
* **Dependency**: `FE-HRD-064`, `FE-HRD-065`, `FE-HRD-067`, `BE-HRD-068` [BE]
* **Acceptance criteria**:
  1. Keempat jenis tugas tampil dengan label Indonesia dan tautan ke dokumen yang tepat.
  2. Pengaju tidak melihat tombol Setujui pada tugas pengajuannya sendiri.
* **Bukti verifikasi**: `npm run lint:errors`, `npm run build`; verifikasi manual satu tugas per jenis.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Keempat jenis tugas dapat diputus dari kotak masuk; laporan tracked tersedia.

### `FE-HRD-074` — Verifikasi menu, hak akses, dan keterjangkauan seluruh layar `S-C4-ONB`
* **Slice / MVP**: `S-C4-ONB` / Verifikasi
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-074.md](../task/report/frontend/FE-HRD-074.md).
* **Outcome**: Kelima belas layar `FE-HRD-123` s.d. `FE-HRD-137` terjangkau dari menu atau dari layar induknya, tombol tersembunyi bagi peran yang tidak berhak, dan perubahan layar lama berfungsi.
* **Requirement/decision**: `03-frontend-architecture.md` 14.1, 14.2, 14.5; `04-prd-to-mvp.md` 23.19.
* **Kontrak**: Seluruh kontrak `v9` bagian `S-C4-ONB` yang dikonsumsi layar.
* **Kewenangan UI**: Tidak ada keputusan baru.
* **Reuse**: Pola verifikasi `FE-HRD-043` dan `FE-HRD-057`.
* **Cakupan**: Pemeriksaan sidebar per peran, tautan antar layar, dan keempat keadaan tiap layar daftar.
* **Dependency**: `FE-HRD-059`, `FE-HRD-062`, `FE-HRD-066`, `FE-HRD-068`, `FE-HRD-069`, `FE-HRD-070`, `FE-HRD-071`, `FE-HRD-072`, `FE-HRD-073`
* **Acceptance criteria**:
  1. Kesembilan butir menu "Onboarding & Kontrak" tampil sesuai butir hak akses masing-masing peran pada 14.5.
  2. Setiap layar anak dapat dicapai dari layar induk yang disebut 14.1.
  3. Tidak ada tombol yang tampil lalu ditolak `403` bagi peran mana pun.
* **Bukti verifikasi**: Tangkapan layar sidebar per peran; `npm run lint:errors`, `npm run build`.
* **Risiko & pemilik**: Pemetaan peran `MAPPING_REQUIRED` membatasi peran yang dapat diuji. Pemilik: Frontend HR UI Lead bersama QA.
* **Definition of Done**: Seluruh layar terverifikasi terjangkau; laporan tracked tersedia.

---

## Rincian Task Frontend Slice `S-B4` — Penjadwalan, Roster, dan On-Call (`FE-HRD-075` s.d. `FE-HRD-087`)

### `FE-HRD-075` — Service Client API Roster Penjadwalan (`roster-scheduling-service.js` diperbarui)
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-075.md](../task/report/frontend/FE-HRD-075.md).
* **Outcome**: Service client Axios diperbarui untuk seluruh fungsi roster: get period, create, copy previous, apply pattern, validate, publish, quick replace, retroactive correct, dan close.
* **Requirement/decision**: `03-frontend-architecture.md` 15.1, API Contract `v10` bagian 16.
* **Kontrak**: API Contract `v10` tag `[Tags("HR Scheduling - Roster Period")]`.
* **Kewenangan UI**: DEV_DISCRETION pada wrapper error handling Axios.
* **Reuse**: `src/lib/services/hr/roster-scheduling-service.js`.
* **Cakupan**: Penambahan method API client, handling token auth & error response standard.
* **Dependency**: `BE-HRD-094` [BE]
* **Acceptance criteria**:
  1. Seluruh endpoint roster terhubung dengan tipe payload yang sesuai kontrak `v10`.
  2. Error response HTTP 422 dipetakan ke format pesan yang dimengerti komponen UI.
* **Bukti verifikasi**: API client integration test; TypeScript/JSDoc type checking.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Service client siap dikonsumsi komponen React, lulus lint.

### `FE-HRD-076` — State Management Redux Roster Unit bulanan & cache invalidation
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-076.md](../task/report/frontend/FE-HRD-076.md).
* **Outcome**: Redux slice `rosterSchedulingSlice` untuk menyimpan state periode aktif, daftar perawat, matriks shift, status validasi, dan mutasi optimistik.
* **Requirement/decision**: `03-frontend-architecture.md` 15.4, 15.5.
* **Kontrak**: `v10` `03-frontend-architecture.md`.
* **Kewenangan UI**: DEV_DISCRETION struktur slice state.
* **Reuse**: Redux Toolkit store.
* **Cakupan**: Action thunk `fetchRosterPeriod`, `validateRoster`, `publishRoster`, `replaceShift`.
* **Dependency**: `FE-HRD-075`
* **Acceptance criteria**:
  1. State merefleksikan perubahan sel jadwal tanpa lag visual (< 50ms).
  2. Invalidation cache terjadi otomatis saat publish atau ganti petugas berhasil.
* **Bukti verifikasi**: Redux DevTools profiling.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Redux store terdaftar, thunk async berjalan stabil.

### `FE-HRD-077` — Layar Kisi Roster Bulanan Unit (`FE-HRD-138` `/hr/scheduling/roster`)
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-077.md](../task/report/frontend/FE-HRD-077.md).
* **Outcome**: Layar utama kisi kalender roster bulanan unit dengan filter unit & bulan, sel kode shift warna-warni, penanda `Didampingi` (`HRD-DEC-087`, `HRD-DEC-100`), dan status badge periode.
* **Requirement/decision**: `HRD-DEC-098`, `HRD-DEC-100`, `03-frontend-architecture.md` 15.3.1.
* **Kontrak**: API Contract `v10` `/roster-periods`.
* **Kewenangan UI**: Pilihan warna badge shift dan layout density adalah `DEV_DISCRETION`.
* **Reuse**: `src/app/hr/scheduling/roster/page.jsx`, base table components.
* **Cakupan**: Refaktor halaman kisi roster, responsive scrolling horizontal tanggal.
* **Dependency**: `FE-HRD-076`
* **Acceptance criteria**:
  1. Grid menampilkan daftar perawat baris demi baris dan tanggal kolom demi kolom.
  2. Perawat berstatus `Didampingi` menampilkan badge kuning `[Didampingi — Preceptor]`.
  3. Status periode (`Draft`, `Validation`, `Published`, `Closed`) tampil jelas di header.
* **Bukti verifikasi**: Visual screen verification; `npm run lint:errors`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar berfungsi responsif, skeleton loading terpasang.

### `FE-HRD-078` — Komponen Aksi Massal: Salin Periode Lalu & Terapkan Pola Shift
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-078.md](../task/report/frontend/FE-HRD-078.md).
* **Outcome**: Tombol aksi dan dialog modal untuk menyalin seluruh jadwal dari bulan lalu atau menerapkan pola shift berulang (misal P-S-M-L-L) untuk mempercepat kerja kepala ruangan.
* **Requirement/decision**: `HRD-DEC-098`, `04-prd-to-mvp.md` `FR-SCH-02`, `FR-SCH-03`.
* **Kontrak**: API Contract `v10` `/copy-previous` dan `/apply-pattern`.
* **Kewenangan UI**: DEV_DISCRETION desain modal pilihan pola.
* **Reuse**: Base modal, dropdown selector.
* **Cakupan**: Komponen `CopyPreviousPeriodModal.jsx`, `ApplyShiftPatternModal.jsx`.
* **Dependency**: `FE-HRD-077`
* **Acceptance criteria**:
  1. Klik Salin Periode Lalu menampilkan konfirmasi dan mengisi seluruh grid seketika.
  2. Terapkan pola shift dapat diaplikasikan ke satu perawat atau seluruh perawat sekaligus.
* **Bukti verifikasi**: UAT simulasi pengisian 500 kotak shift < 2 detik.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Aksi massal berfungsi mulus tanpa refresh halaman penuh.

### `FE-HRD-079` — Komponen Drawer Validasi Jadwal: Blokir `B1`–`B7` & Peringatan `W1`–`W7` dengan Dropdown Alasan
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-079.md](../task/report/frontend/FE-HRD-079.md).
* **Outcome**: Drawer interaktif menampilkan hasil periksa jadwal: memisahkan error pemblokir (merah) dan peringatan (kuning), serta menyediakan dropdown pemilihan alasan wajib.
* **Requirement/decision**: `HRD-DEC-099`, `HRD-DEC-100`, `03-frontend-architecture.md` 15.3.2.
* **Kontrak**: Validation Matrix `v10` 15.1 & 15.2.
* **Kewenangan UI**: Animasi drawer dan ikon visual adalah `DEV_DISCRETION`.
* **Reuse**: Base drawer component, BaseSelect.
* **Cakupan**: Komponen `RosterValidationDrawer.jsx`.
* **Dependency**: `FE-HRD-077`, `BE-HRD-090` [BE]
* **Acceptance criteria**:
  1. Pelanggaran `B1`–`B7` ditampilkan merah dengan tombol navigasi ke sel bermasalah.
  2. Peringatan `W1`–`W7` menampilkan pilihan alasan wajib (`Kekurangan tenaga`, dll.).
  3. Mengisi alasan mengubah status peringatan menjadi terkonfirmasi (hijau/centang).
* **Bukti verifikasi**: UAT skenario jalur blokir dan jalur peringatan beralasan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Drawer interaktif, validasi form alasan bekerja tepat.

### `FE-HRD-080` — Aksi UI Tombol Terbitkan Roster Mandiri Kepala Unit dengan Validasi Form Alasan
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-080.md](../task/report/frontend/FE-HRD-080.md).
* **Outcome**: Tombol **Terbitkan** pada header roster: terkunci jika masih ada error pemblokir atau ada peringatan tanpa alasan; aktif saat validasi lolos; memicu publikasi mandiri (`HRD-DEC-098`).
* **Requirement/decision**: `HRD-DEC-098`, `03-frontend-architecture.md` 15.3.2.
* **Kontrak**: API Contract `v10` `/roster-periods/{id}/publish`.
* **Kewenangan UI**: Indikator disabled button dan modal konfirmasi publish adalah `DEV_DISCRETION`.
* **Reuse**: BaseButton, toast notification.
* **Cakupan**: Integrasi tombol publish, payload array `warningReasons`.
* **Dependency**: `FE-HRD-079`
* **Acceptance criteria**:
  1. Tombol Terbitkan disabled selama `blockingConflicts.length > 0` atau alasan belum lengkap.
  2. Klik Terbitkan mengirim alasan-alasan yang dipilih ke backend dan mengubah badge ke `Published`.
  3. Toast sukses muncul mengonfirmasi bahwa Kepala Bidang telah diberi tahu.
* **Bukti verifikasi**: UAT skenario sukses terbit.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Penerbitan mandiri sukses, UI ter-update tanpa reload.

### `FE-HRD-081` — Komponen Modal Ganti Petugas Cepat (*Quick Replacement*) dengan Alasan Wajib
* **Slice / MVP**: `S-B4` / `MVP-19`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-081.md](../task/report/frontend/FE-HRD-081.md).
* **Outcome**: Modal popover saat sel shift pada roster terbit diklik: memungkinkan Kepala Unit menunjuk staf pengganti, memilih alasan wajib, dan langsung aktif seketika tanpa approval (`HRD-DEC-101`).
* **Requirement/decision**: `HRD-DEC-101`, `03-frontend-architecture.md` 15.3.3.
* **Kontrak**: API Contract `v10` `/shift-assignments/{id}/replace`.
* **Kewenangan UI**: Desain form popover/modal adalah `DEV_DISCRETION`.
* **Reuse**: BaseModal, BaseSelect, RadioGroup.
* **Cakupan**: Komponen `QuickShiftReplacementModal.jsx`.
* **Dependency**: `FE-HRD-077`, `BE-HRD-096` [BE]
* **Acceptance criteria**:
  1. Modal menampilkan nama staf asal dan waktu shift secara otomatis.
  2. Dropdown pengganti menyaring perawat unit yang memenuhi syarat keselamatan.
  3. Konfirmasi penggantian langsung memperbarui nama staf di sel jadwal dan status roster tetap `Published`.
* **Bukti verifikasi**: UAT mutasi shift darurat pasca-terbit.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Penggantian shift langsung efektif secara visual dan data.

### `FE-HRD-082` — Aksi UI Koreksi Mundur Jadwal (<= 3 hari) & Proteksi Periode Terkunci (`Closed` Banner)
* **Slice / MVP**: `S-B4` / `MVP-19`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-082.md](../task/report/frontend/FE-HRD-082.md).
* **Outcome**: Kontrol koreksi tanggal mundur dibatasi maks 3 hari kalender ke belakang dengan dialog alasan; bila periode berstatus `Closed`, seluruh sel tabel dikunci (*read-only*) dengan banner informatif.
* **Requirement/decision**: `HRD-DEC-102`, `03-frontend-architecture.md` 15.3.1.
* **Kontrak**: API Contract `v10` `/retroactive-correct`, `VR-SCH-17`, `VR-SCH-18`.
* **Kewenangan UI**: Desain banner penguncian periode adalah `DEV_DISCRETION`.
* **Reuse**: InformationAlert, Banner lock.
* **Cakupan**: Handler edit mundur dan render conditional grid terkunci.
* **Dependency**: `FE-HRD-077`, `BE-HRD-098` [BE]
* **Acceptance criteria**:
  1. Sel jadwal tanggal > 3 hari lalu tidak memunculkan opsi edit bagi peran Kepala Unit.
  2. Periode `Closed` menonaktifkan seluruh tombol aksi (Buat, Salin, Ganti Petugas).
  3. Banner penguncian abu-abu tampil menjelaskan periode telah ditutup oleh Payroll.
* **Bukti verifikasi**: UAT skenario edit mundur dan inspeksi layar saat status Closed.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Proteksi UI presisi sesuai aturan bisnis.

### `FE-HRD-083` — Service Client API On-Call & SLA (`on-call-service.js`)
* **Slice / MVP**: `S-B4` / `MVP-20`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-083.md](../task/report/frontend/FE-HRD-083.md).
* **Outcome**: Service client Axios untuk manajemen siaga on-call: fetch jadwal siaga, create on-call, confirm on-call, dan call-in pencatatan waktu darurat.
* **Requirement/decision**: `HRD-DEC-103`, API Contract `v10` tag `[Tags("HR Scheduling - On-Call Management")]`.
* **Kontrak**: API Contract `v10` 16.2.
* **Kewenangan UI**: DEV_DISCRETION struktur helper client.
* **Reuse**: Axios client base.
* **Cakupan**: Pembuatan `src/lib/services/hr/on-call-service.js`.
* **Dependency**: `BE-HRD-102` [BE]
* **Acceptance criteria**:
  1. Seluruh endpoint on-call terhubung stabil.
  2. Response data memuat atribut SLA dan status `Active`.
* **Bukti verifikasi**: Client integration test.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Service siap dipakai komponen UI on-call.

### `FE-HRD-084` — Layar Kalender Penugasan Siaga (*On-Call Coverage*) (`FE-HRD-141` `/hr/scheduling/on-call`)
* **Slice / MVP**: `S-B4` / `MVP-20`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-084.md](../task/report/frontend/FE-HRD-084.md).
* **Outcome**: Layar kalender dan daftar penugasan siaga dokter spesialis dan staf penunjang cito per unit dan per tanggal, dengan filter profesi dan status badge (`Scheduled`, `Confirmed`, `Active`).
* **Requirement/decision**: `HRD-DEC-103`, `03-frontend-architecture.md` 15.3.4.
* **Kontrak**: API Contract `v10` `/on-call-assignments`.
* **Kewenangan UI**: Pilihan visualisasi kalender vs list view adalah `DEV_DISCRETION`.
* **Reuse**: Calendar base component, StatusBadge.
* **Cakupan**: Pembuatan `src/app/hr/scheduling/on-call/page.jsx`.
* **Dependency**: `FE-HRD-083`
* **Acceptance criteria**:
  1. Menampilkan jadwal siaga dokter dan perawat cito secara terpisah.
  2. Dokter siaga yang sedang aktif menampilkan badge hijau `Active`.
  3. Menyediakan tombol aksi **Panggil Siaga** untuk unit yang berwenang.
* **Bukti verifikasi**: Visual screen test; responsivitas tampilan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Halaman siaga terpasang dan dapat diakses dari menu.

### `FE-HRD-085` — Komponen Modal Panggil Siaga (*Call-In*) & Display Badge Pemenuhan SLA Menit
* **Slice / MVP**: `S-B4` / `MVP-20`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-085.md](../task/report/frontend/FE-HRD-085.md).
* **Outcome**: Modal pemanggilan siaga darurat bagi unit pemanggil (Lab, IGD, OK): input waktu panggil, input waktu kedatangan dokter, dan otomatis menampilkan indikator kepatuhan target SLA respon menit (`MET_SLA` / `OVER_SLA`).
* **Requirement/decision**: `HRD-DEC-103`, `03-frontend-architecture.md` 15.3.4.
* **Kontrak**: API Contract `v10` `/on-call-assignments/{id}/call-in`.
* **Kewenangan UI**: Visualisasi badge SLA dan timer hitung mundur adalah `DEV_DISCRETION`.
* **Reuse**: BaseModal, DateTimePicker.
* **Cakupan**: Komponen `OnCallCallInModal.jsx`.
* **Dependency**: `FE-HRD-084`
* **Acceptance criteria**:
  1. Form mencatat waktu panggilan telepon dan waktu tiba dokter.
  2. Selisih menit dihitung real-time dan dibandingkan dengan target SLA.
  3. Badge hijau `[MEMENUHI SLA (25 min)]` tampil bila tiba tepat waktu.
* **Bukti verifikasi**: UAT skenario pemanggilan dokter siaga lab/cito.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Modal berfungsi presisi, kalkulasi SLA akurat.

### `FE-HRD-086` — Layar Pengaturan Kebijakan Penjadwalan HR (`FE-HRD-143` `/hr/scheduling/settings`)
* **Slice / MVP**: `S-B4` / `MVP-18`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-086.md](../task/report/frontend/FE-HRD-086.md).
* **Outcome**: Layar admin HR untuk menyetel angka jeda istirahat minimal (default 11 jam), batas hari kerja berturut-turut, toleransi koreksi mundur (default 3 hari), dan toggle eskalasi aturan ke blocking.
* **Requirement/decision**: `HRD-DEC-099`, `HRD-DEC-102`, `03-frontend-architecture.md` 15.1.
* **Kontrak**: API Contract `v10` tag `[Tags("HR Scheduling - Settings")]`.
* **Kewenangan UI**: Layout grouping form pengaturan adalah `DEV_DISCRETION`.
* **Reuse**: BaseForm, NumberInput, Switch.
* **Cakupan**: Pembuatan `src/app/hr/scheduling/settings/page.jsx`.
* **Dependency**: `BE-HRD-088` [BE]
* **Acceptance criteria**:
  1. Admin HR dapat mengubah nilai parameter waktu kerja dengan validasi rentang.
  2. Perubahan tersimpan dan langsung menjadi acuan mesin validasi roster.
* **Bukti verifikasi**: UAT update parameter penjadwalan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar pengaturan berfungsi, hanya dapat dibuka peran admin.

### `FE-HRD-087` — Verifikasi menu, hak akses peran, dan keterjangkauan seluruh layar `S-B4`
* **Slice / MVP**: `S-B4` / Verifikasi
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-087.md](../task/report/frontend/FE-HRD-087.md).
* **Outcome**: Seluruh layar `S-B4` (`FE-HRD-138` s.d. `FE-HRD-143`) terverifikasi terjangkau dari navigasi sidebar, hak akses peran teruji (Kepala Unit vs Kepala Bidang vs Admin), dan lolos uji linter CI/CD.
* **Requirement/decision**: `03-frontend-architecture.md` 15.2, 15.4; `04-prd-to-mvp.md` 24.5.
* **Kontrak**: Seluruh kontrak `v10` bagian `S-B4`.
* **Kewenangan UI**: Tidak ada keputusan baru.
* **Reuse**: Pola verifikasi `FE-HRD-074`.
* **Cakupan**: Verifikasi sidebar per peran, uji loading/empty/error state, `npm run lint:errors`, `npm run build`.
* **Dependency**: `FE-HRD-080`, `FE-HRD-081`, `FE-HRD-082`, `FE-HRD-085`, `FE-HRD-086`
* **Acceptance criteria**:
  1. Butir menu Penjadwalan tampil sesuai hak akses masing-masing peran.
  2. Kepala Bidang memiliki hak pantau *read-only* tanpa tombol ubah yang aktif.
  3. `npm run lint:errors` dan `npm run build` berhasil dengan 0 error.
* **Bukti verifikasi**: Tangkapan layar sidebar per peran; laporan hasil lint & build.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead bersama QA.
* **Definition of Done**: Seluruh layar teruji, tidak ada broken link atau crash JS.


### ✅ `FE-HRD-088` — Service Client API Presensi, Perizinan, dan Pengaturan HR (`attendance-service.js`)
* **Slice / MVP**: `S-B1` / `MVP-21`
* **Status**: ✅ Selesai (11 November 2026, `FE-HRD-R13`). Seluruh alamat cocok dengan controller backend; GET harian, ringkasan, log mentah, dan pengaturan terbukti 200 di browser. Laporan: [FE-HRD-088.md](../task/report/frontend/FE-HRD-088.md).
* **Outcome**: Service client Axios untuk konsumsi API presensi: pemantauan harian, perizinan pulang cepat, surat dinas luar, koreksi on-behalf, penutupan periode, dan parameter sistem.
* **Requirement/decision**: API Contract `v11` Bagian 17.
* **Kontrak**: API Contract `v11` 17.2.
* **Kewenangan UI**: Struktur modul helper client adalah `DEV_DISCRETION`.
* **Reuse**: Axios client base, error interceptor.
* **Cakupan**: `src/lib/services/hr/attendance-service.js`, `early-leave-service.js`, `official-duty-service.js`.
* **Dependency**: `BE-HRD-111` [BE], `BE-HRD-115` [BE]
* **Acceptance criteria**:
  1. Seluruh fungsi API terdefinisi sesuai spesifikasi endpoint backend.
  2. Serialisasi parameter filter tanggal, unit kerja, dan pagination akurat.
* **Bukti verifikasi**: Integration test client service.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Helper service teruji dan siap diinjeksi ke komponen UI.

### 🟡 `FE-HRD-089` — Redux Slice State Management Presensi Harian & Periode (`attendanceSlice.js`)
* **Slice / MVP**: `S-B1` / `MVP-21`
* **Status**: 🟡 **SEBAGIAN** (11 November 2026, `FE-HRD-R13`). Fetch harian, ringkasan, dan filter ada; status periode belum, menyusul `FE-HRD-R29` (MVP-23). Laporan: [FE-HRD-089.md](../task/report/frontend/FE-HRD-089.md).
* **Outcome**: State management Redux untuk sinkronisasi data presensi harian, status filter periode, cache data perizinan, dan status penutupan.
* **Requirement/decision**: `03-frontend-architecture.md` Bagian 16.2.
* **Kontrak**: Frontend Architecture `v11` 16.2.
* **Kewenangan UI**: Redux action & selector structure adalah `DEV_DISCRETION`.
* **Reuse**: Redux Toolkit `createSlice`.
* **Cakupan**: `src/lib/redux/slices/hr/attendanceSlice.js`.
* **Dependency**: `FE-HRD-088`
* **Acceptance criteria**:
  1. Reducer menangani aksi fetch presensi harian, update filter, dan status periode.
  2. Selektor menyediakan data tersaring untuk komponen presentasional.
* **Bukti verifikasi**: Unit test Redux slice reducer.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: State Redux terintegrasi ke store utama tanpa kebocoran state.

### 🟡 `FE-HRD-090` — Layar Dasbor Administrasi & Monitoring Presensi Harian (`FE-HRD-144` `/hr/attendance`)
* **Slice / MVP**: `S-B1` / `MVP-21`
* **Status**: 🟡 **SEBAGIAN** (11 November 2026, `FE-HRD-R13`). Kriteria 1 terbukti di browser; kriteria 2 belum terbukti tanpa data shift malam; kriteria 3: menit terlambat merah, jam masuk ditandai warna karena kontrak tabel menetralkan huruf tebal. Laporan: [FE-HRD-090.md](../task/report/frontend/FE-HRD-090.md).
* **Outcome**: Halaman dasbor monitoring presensi harian staf RS: ringkasan kartu statistik (Hadir Tepat Waktu, Terlambat, Izin Pulang Cepat, Dinas Luar, Incomplete Punch, Alfa), tabel rekap harian, dan tombol navigasi audit.
* **Requirement/decision**: `HRD-DEC-104`, `HRD-DEC-105`, `HRD-DEC-106`, `03-frontend-architecture.md` 16.3.1.
* **Kontrak**: API Contract `v11` `/daily`, State Transition `v11` 15.1.
* **Kewenangan UI**: Tata letak grid kartu ringkasan dan pewarnaan badge adalah `DEV_DISCRETION`.
* **Reuse**: `SummaryCards`, `DataTable`, `StatusBadge`, `BaseDatePicker`.
* **Cakupan**: `src/app/hr/attendance/page.jsx`, `attendance-daily-view.jsx`.
* **Dependency**: `FE-HRD-089`, `BE-HRD-106` [BE]
* **Acceptance criteria**:
  1. Status `IncompletePunch` tampil dengan badge kuning peringatan `[1 Punch - Perlu Koreksi]` (`HRD-DEC-106`).
  2. Shift malam lintas hari menampilkan tanggal dinas hari H dan jam keluar hari H+1 dengan jelas (`HRD-DEC-104`).
  3. Keterlambatan > 10 menit menampilkan durasi menit merah dan jam masuk tebal (`HRD-DEC-105`).
* **Bukti verifikasi**: UAT visual dasbor presensi harian.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Dasbor informatif, responsif, dan terhubung real-time ke data harian.

### ✅ `FE-HRD-091` — Komponen Drawer Riwayat Log Mesin Biometrik Mentah (`RawPunchAuditDrawer.jsx`)
* **Slice / MVP**: `S-B1` / `MVP-21`
* **Status**: ✅ Selesai (11 November 2026, `FE-HRD-R13`). Log mentah dan label `[DATA MENTAH MESIN - IMMUTABLE]` terbukti di browser; tampil lewat `ConfirmModal` sampai `DEC-FER-01`. Laporan: [FE-HRD-091.md](../task/report/frontend/FE-HRD-091.md).
* **Outcome**: Drawer samping baca saja yang membedah rekaman log mentah mesin absensi per pegawai: waktu tap detik presisi, ID mesin, metode verifikasi, tanpa kontrol pengeditan.
* **Requirement/decision**: `HRD-DEC-104`, `03-frontend-architecture.md` 16.3.1.
* **Kontrak**: API Contract `v11` `/raw-logs`.
* **Kewenangan UI**: Format timeline vs tabel di dalam drawer adalah `DEV_DISCRETION`.
* **Reuse**: `BaseDrawer`, `BaseBadge`, `TimelineView`.
* **Cakupan**: `src/components/view/hr/attendance/RawPunchAuditDrawer.jsx`.
* **Dependency**: `FE-HRD-090`, `BE-HRD-107` [BE]
* **Acceptance criteria**:
  1. Menampilkan seluruh tap mentah pegawai pada rentang tanggal terpilih.
  2. Menegaskan label `[DATA MENTAH MESIN - IMMUTABLE]` untuk kepastian hukum audit.
* **Bukti verifikasi**: UAT inspeksi jejak log biometrik mesin.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Drawer audit berfungsi cepat, informatif, dan aman.

### `FE-HRD-092` — Layar Rekapitulasi & Penutupan Periode Absensi (`FE-HRD-145` `/hr/attendance/periods`)
* **Slice / MVP**: `S-B1` / `MVP-23`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R29` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Layar monitoring periode bulanan absensi HR: rekap total jam kerja riil, total jam dinas luar, total potongan keterlambatan, status periode (`Open`, `Closed`), dan aksi tutup periode.
* **Requirement/decision**: `HRD-DEC-108`, `03-frontend-architecture.md` 16.3.2.
* **Kontrak**: API Contract `v11` `/periods`, State Transition `v11` 15.3.
* **Kewenangan UI**: Desain banner periode terkunci adalah `DEV_DISCRETION`.
* **Reuse**: `DataTable`, `StatusBadge`, `InformationAlert`.
* **Cakupan**: `src/app/hr/attendance/periods/page.jsx`.
* **Dependency**: `FE-HRD-089`, `BE-HRD-115` [BE]
* **Acceptance criteria**:
  1. Menampilkan daftar periode dengan total staf dan persentase kepatuhan kehadiran.
  2. Tombol Tutup Periode memicu dialog verifikasi kelayakan.
  3. Periode berstatus `Closed` menampilkan banner penguncian abu-abu dan menonaktifkan aksi ubah.
* **Bukti verifikasi**: UAT penutupan periode bulanan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar rekapitulasi periode berfungsi presisi dan sinkron dengan payroll.

### `FE-HRD-093` — Komponen Dialog Guard Penutupan Periode & Peringatan Koreksi Pending (`PeriodClosingGuardModal.jsx`)
* **Slice / MVP**: `S-B1` / `MVP-23`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R29` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Modal konfirmasi penutupan periode dengan guard ketat: jika ada koreksi pending, tombol ditolak dan memunculkan daftar pengajuan yang memblokir (`VR-ATT-15`); jika bersih, menampilkan peringatan konversi `IncompletePunch` ke `Absent`.
* **Requirement/decision**: `HRD-DEC-106`, `HRD-DEC-108`, `03-frontend-architecture.md` 16.3.2.
* **Kontrak**: Validation Matrix `v11` `VR-ATT-15`, `VR-ATT-16`.
* **Kewenangan UI**: Tata letak tabel daftar blocking di dalam modal adalah `DEV_DISCRETION`.
* **Reuse**: `BaseModal`, `BaseButton`, `InformationAlert`.
* **Cakupan**: `src/components/view/hr/attendance/PeriodClosingGuardModal.jsx`.
* **Dependency**: `FE-HRD-092`, `BE-HRD-113` [BE]
* **Acceptance criteria**:
  1. Menolak aksi tutup dan menyajikan tautan ke koreksi pending bila data belum bersih (`VR-ATT-15`).
  2. Menampilkan ringkasan jumlah `IncompletePunch` yang akan otomatis menjadi `Absent` (`VR-ATT-16`).
* **Bukti verifikasi**: UAT validasi penolakan penutupan saat ada permohonan pending.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Guard dialog mencegah penutupan prematur.

### `FE-HRD-094` — Komponen Modal Buka Kembali Periode Darurat HR Manager (`PeriodEmergencyReopenModal.jsx`)
* **Slice / MVP**: `S-B1` / `MVP-23`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R29` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Modal khusus peran HR Manager untuk membuka kembali periode tertutup: input alasan pembukaan wajib (min 15 karakter) dan peringatan keras potensi *rollback* data serah terima penggajian.
* **Requirement/decision**: `HRD-DEC-108`, `03-frontend-architecture.md` 16.3.2.
* **Kontrak**: API Contract `v11` `/reopen`.
* **Kewenangan UI**: Visualisasi dialog konfirmasi tingkat bahaya (*High Risk Alert*) adalah `DEV_DISCRETION`.
* **Reuse**: `BaseModal`, `BaseTextArea`, `InformationAlert` (varian danger).
* **Cakupan**: `src/components/view/hr/attendance/PeriodEmergencyReopenModal.jsx`.
* **Dependency**: `FE-HRD-092`, `BE-HRD-114` [BE]
* **Acceptance criteria**:
  1. Hanya muncul untuk pengguna dengan peran HR Manager (`HrdAttendance.Period.Reopen`).
  2. Validasi mencegah submit bila isian alasan kosong atau kurang dari 15 karakter.
  3. Menampilkan peringatan tegas bahwa pembukaan memicu rollback serah terima payroll (`HRD-DEC-108`).
* **Bukti verifikasi**: UAT skenario buka kembali periode darurat.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Modal darurat terpasang dengan pengamanan ganda.

### `FE-HRD-095` — Layar Monitoring & Koreksi Presensi On-Behalf HR (`FE-HRD-146` `/hr/attendance/corrections`)
* **Slice / MVP**: `S-B1` / `MVP-22`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R28` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Layar bagi tim HR untuk memantau seluruh permohonan koreksi absensi yang diajukan mandiri via ESS maupun yang diajukan on-behalf oleh HR, lengkap dengan saringan status dan penanda initiator.
* **Requirement/decision**: `HRD-DEC-028`, `03-frontend-architecture.md` 16.3.3.
* **Kontrak**: API Contract `v11` `/corrections`.
* **Kewenangan UI**: Saringan status dan filter jenis pengajuan adalah `DEV_DISCRETION`.
* **Reuse**: `DataTable`, `DataFilter`, `StatusBadge`.
* **Cakupan**: `src/app/hr/attendance/corrections/page.jsx`.
* **Dependency**: `FE-HRD-089`, `BE-HRD-110` [BE]
* **Acceptance criteria**:
  1. Menampilkan kolom Nomor Koreksi, Nama Pegawai, Tanggal Presensi, Jam Koreksi, Initiator (ESS vs HR Kuasa), dan Status.
  2. Tombol `+ Koreksi Kuasa HR` tersedia untuk peran yang berwenang.
* **Bukti verifikasi**: UAT daftar koreksi presensi.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar koreksi komprehensif membedakan jalur mandiri vs jalur kuasa HR.

### `FE-HRD-096` — Komponen Modal Aksi Koreksi Presensi Kuasa HR (`OnBehalfCorrectionModal.jsx`)
* **Slice / MVP**: `S-B1` / `MVP-22`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R28` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Modal form pengajuan koreksi presensi atas nama pegawai: pemilih staf, pemilih tanggal presensi, input jam masuk/keluar baru, input alasan wajib, dan kontrol upload surat pernyataan/bukti kendala ESS.
* **Requirement/decision**: `HRD-DEC-028`, `03-frontend-architecture.md` 16.3.3.
* **Kontrak**: Validation Matrix `v11` `VR-ATT-05`, `VR-ATT-06`.
* **Kewenangan UI**: Layout grouping form adalah `DEV_DISCRETION`.
* **Reuse**: `BaseModal`, `FilterSelect`, `BaseDatePicker`, `FileUpload`.
* **Cakupan**: `src/components/view/hr/attendance/OnBehalfCorrectionModal.jsx`.
* **Dependency**: `FE-HRD-095`
* **Acceptance criteria**:
  1. Form memvalidasi pengisian alasan dan pemilihan berkas bukti pendukung.
  2. Pengiriman form mencatat ID HR yang login sebagai initiator.
  3. Berhasil memicu notifikasi transparansi ke akun ESS pegawai yang bersangkutan.
* **Bukti verifikasi**: UAT pembuatan koreksi on-behalf oleh HR.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Form koreksi kuasa HR beroperasi aman dan bebas kendala multipart.

### `FE-HRD-097` — Layar Pengelolaan Izin Pulang Cepat Non-Deductible (`FE-HRD-147` `/hr/attendance/early-leave`)
* **Slice / MVP**: `S-B1` / `MVP-22`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R28` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Layar monitoring perizinan pulang cepat staf: daftar pengajuan, jam rencana keluar, jam aktual mesin, status persetujuan, dan status waiver dispensasi keterlambatan/kehadiran.
* **Requirement/decision**: `HRD-DEC-107`, `HRD-Q-47`, `03-frontend-architecture.md` 16.3.4.
* **Kontrak**: API Contract `v11` `/early-leave`.
* **Kewenangan UI**: Layout kartu metrik dan tabel adalah `DEV_DISCRETION`.
* **Reuse**: `DataTable`, `StatusBadge`, `BaseCard`.
* **Cakupan**: `src/app/hr/attendance/early-leave/page.jsx`.
* **Dependency**: `FE-HRD-089`, `BE-HRD-108` [BE]
* **Acceptance criteria**:
  1. Menegaskan label `[TIDAK MEMOTONG CUTI]` pada header atau keterangan informasi (`HRD-DEC-107`).
  2. Menampilkan perbandingan jam izin diajukan vs jam tap aktual mesin.
  3. Tombol detail membuka riwayat persetujuan atasan langsung.
* **Bukti verifikasi**: UAT monitoring izin pulang cepat.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar izin pulang cepat mematuhi aturan non-deductible.

### `FE-HRD-098` — Layar Pengelolaan Surat Penugasan Dinas Luar Rumah Sakit (`FE-HRD-148` `/hr/attendance/official-duty`)
* **Slice / MVP**: `S-B1` / `MVP-22`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R28` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Layar pencatatan dan monitoring surat dinas luar medis/non-medis: daftar surat penugasan ambulans rujukan, kurir lab cito, workshop/diklat, home care, serta status verifikasi atasan.
* **Requirement/decision**: `HRD-DEC-109`, `03-frontend-architecture.md` 16.3.5.
* **Kontrak**: API Contract `v11` `/official-duty`.
* **Kewenangan UI**: Desain filter kategori penugasan dinas luar adalah `DEV_DISCRETION`.
* **Reuse**: `DataTable`, `StatusBadge`, `BaseButton`.
* **Cakupan**: `src/app/hr/attendance/official-duty/page.jsx`.
* **Dependency**: `FE-HRD-089`, `BE-HRD-109` [BE]
* **Acceptance criteria**:
  1. Menampilkan nomor surat tugas resmi, tujuan, dan jam penugasan (`HRD-DEC-109`).
  2. Menyediakan tombol cetak/unduh surat tugas dinas luar bertanda tangan.
  3. Status yang disetujui menampilkan badge hijau `[OnDuty Disetujui]`.
* **Bukti verifikasi**: UAT alur administrasi surat tugas dinas luar.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar surat dinas luar terhubung dengan pengesahan status OnDuty.

### ✅ `FE-HRD-099` — Layar Pengaturan Parameter Presensi & Toleransi Waktu HR (`FE-HRD-149` `/hr/attendance/settings`)
* **Slice / MVP**: `S-B1` / `MVP-21`
* **Status**: ✅ Selesai (11 November 2026, `FE-HRD-R13`). Rentang 0–30 menit, akses ditolak untuk akun tanpa hak, dan simpan dengan notifikasi terbukti di browser. Parameter cut-off shift malam tidak ada di kontrak backend. Laporan: [FE-HRD-099.md](../task/report/frontend/FE-HRD-099.md).
* **Outcome**: Layar admin HR untuk mengonfigurasi parameter sistem: toleransi keterlambatan (10 menit), cut-off pemrosesan shift malam, dan batas auto-convert `IncompletePunch`.
* **Requirement/decision**: `HRD-DEC-105`, `HRD-DEC-106`, `03-frontend-architecture.md` 16.1.
* **Kontrak**: API Contract `v11` tag `[Tags("HR Attendance - Settings")]`.
* **Kewenangan UI**: Tata letak grouping form parameter adalah `DEV_DISCRETION`.
* **Reuse**: `BaseForm`, `NumberInput`, `Switch`, `BaseCard`.
* **Cakupan**: `src/app/hr/attendance/settings/page.jsx`.
* **Dependency**: `FE-HRD-088`, `BE-HRD-105` [BE]
* **Acceptance criteria**:
  1. Nilai toleransi dapat diubah dalam rentang 0–30 menit dengan validasi client (`VR-ATT-17`).
  2. Hanya dapat diakses pengguna dengan peran HR Manager atau Admin Sistem.
  3. Simpan perubahan menampilkan pesan konfirmasi dan tersimpan di server.
* **Bukti verifikasi**: UAT penyesuaian parameter toleransi absensi.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar pengaturan berfungsi stabil dan terproteksi hak akses.

### `FE-HRD-100` — Penyelarasan Portal Layanan Mandiri Karyawan ESS untuk Absensi & Perizinan Presensi
* **Slice / MVP**: `S-B1` / `MVP-22`
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R28` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Pembaruan layar ESS presensi pegawai (`/self-services/human-resource/attendance`): notifikasi pengingat bila terjadi `IncompletePunch`, form permohonan izin pulang cepat mandiri, dan form klaim surat dinas luar.
* **Requirement/decision**: `HRD-DEC-106`, `HRD-DEC-107`, `HRD-DEC-109`, `03-frontend-architecture.md` 16.4.
* **Kontrak**: API Contract `v11` `/self-services/attendance/**`.
* **Kewenangan UI**: Tampilan widget alert incomplete punch di ESS adalah `DEV_DISCRETION`.
* **Reuse**: `InformationAlert`, `BaseModal`, `BaseForm`.
* **Cakupan**: `src/app/self-services/human-resource/attendance/page.jsx`.
* **Dependency**: `FE-HRD-088`, `FE-HRD-006` [F3]
* **Acceptance criteria**:
  1. Jika ada hari dengan status `IncompletePunch`, tampil banner kuning dengan tombol `Ajukan Koreksi Sekarang` (`HRD-DEC-106`).
  2. Pegawai dapat mengajukan izin pulang cepat atau mengunggah surat tugas dinas luar langsung dari ESS.
* **Bukti verifikasi**: UAT pengalaman staf pada portal mandiri ESS presensi.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Portal ESS terintegrasi dengan alur perizinan dan peringatan presensi baru.

### `FE-HRD-101` — Verifikasi menu, hak akses peran, dan keterjangkauan seluruh layar `S-B1`
* **Slice / MVP**: `S-B1` / Verifikasi
* **Status**: Belum dikerjakan. Prasyarat backend sudah ✅ menurut audit 11 November 2026; dijadwalkan lewat `FE-HRD-R43` pada [frontend-unfinished-work.md](frontend-unfinished-work.md).
* **Outcome**: Seluruh 6 layar baru `S-B1` (`FE-HRD-144` s.d. `FE-HRD-149`) dan ekstensi ESS terverifikasi terjangkau dari sidebar menu `Presensi & Kehadiran`, hak akses peran teruji (Staff vs HR Admin vs HR Manager), dan lolos uji linter CI/CD.
* **Requirement/decision**: `03-frontend-architecture.md` 16.2, 16.5; `04-prd-to-mvp.md` 25.5.
* **Kontrak**: Seluruh kontrak `v11` bagian `S-B1`.
* **Kewenangan UI**: Tidak ada keputusan produk baru.
* **Reuse**: Pola verifikasi `FE-HRD-087`.
* **Cakupan**: Audit sidebar menu, uji loading/empty/error state, `npm run lint:errors`, `npm run build`.
* **Dependency**: `FE-HRD-091`, `FE-HRD-093`, `FE-HRD-094`, `FE-HRD-096`, `FE-HRD-097`, `FE-HRD-098`, `FE-HRD-099`, `FE-HRD-100`
* **Acceptance criteria**:
  1. Butir menu Presensi tampil terstruktur di sidebar sesuai hak akses.
  2. HR Admin dapat mengakses dasbor, koreksi kuasa, dan izin pulang cepat.
  3. Tombol buka kembali periode hanya tampil untuk peran HR Manager.
  4. `npm run lint:errors` dan `npm run build` sukses 0 error.
* **Bukti verifikasi**: Tangkapan layar navigasi sidebar per peran, laporan lint/build.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead bersama QA.
* **Definition of Done**: Seluruh layar teruji, bebas deadlink dan bebas crash JavaScript.


---

## Rincian Task Slice `S-B2` — Layar Cuti & Izin Pegawai Rumah Sakit (`FE-HRD-102` s.d. `FE-HRD-115`)

### `FE-HRD-102` — Service Client API Cuti, Perizinan, dan Redux State Management (`leave-service.js`, `leaveSlice.js`)
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-102.md](../task/report/frontend/FE-HRD-102.md).
* **Outcome**: Modul komunikasi API Axios terpusat dan state management Redux Toolkit untuk kuota saldo, mutasi buku besar, pengajuan cuti ESS, approval berindikator kuota, recall darurat, dan rekonsiliasi payroll.
* **Requirement/decision**: `03-frontend-architecture.md` Bagian 17; API Contract `v12` Bagian 18.
* **Kontrak**: API Contract `v12` seluruh tag `[Tags("HR Leave - ...")]`.
* **Kewenangan UI**: Struktur internal Redux store adalah `DEV_DISCRETION`.
* **Reuse**: `apiClient.js`, konfigurasi Redux store HRD.
* **Cakupan**: `src/lib/services/hr/leave-service.js`, `src/lib/store/slices/hr/leave-slice.js`.
* **Dependency**: `BE-HRD-120` [BE], `FE-HRD-007` ✅ [F3]
* **Acceptance criteria**:
  1. Service membungkus seluruh endpoint API cuti, perizinan, recall, dan penutupan periode.
  2. Redux slice mengelola state saldo aktif, daftar mutasi ledger, dan daftar approval in-flight.
  3. Penanganan error terstandarisasi menangani HTTP 400 (saldo kurang), HTTP 409 (jadwal bentrok / immutability reverse).
* **Bukti verifikasi**: Unit test service API / integrasi Redux thunk.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Service client siap dikonsumsi oleh seluruh komponen dan layar cuti.

### `FE-HRD-103` — Komponen Metrik Kartu Saldo & Progress Bar Pemakaian Cuti Tahunan (`LeaveBalanceMetricCards`)
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-103.md](../task/report/frontend/FE-HRD-103.md).
* **Outcome**: Komponen reusable untuk menampilkan ringkasan hak cuti tahunan berjalan (12 hari), saldo aktif dapat dipakai, kuota carry-forward (disertai lencana peringatan kedaluwarsa 30 Juni per `HRD-DEC-110`), dan bar visualisasi persentase pemakaian.
* **Requirement/decision**: `HRD-DEC-110`, `03-frontend-architecture.md` 17.2.1.
* **Kontrak**: API Contract `v12` `LeaveBalanceSummaryDto`.
* **Kewenangan UI**: Tata letak grid kartu dan palet warna progress bar adalah `DEV_DISCRETION`.
* **Reuse**: `Card`, `Badge`, `Progress` dari base component Quilvian.
* **Cakupan**: `src/components/features/hr/leave/LeaveBalanceMetricCards.jsx`.
* **Dependency**: `FE-HRD-102`
* **Acceptance criteria**:
  1. Kartu menampilkan kuota hak berjalan (12 hari), saldo aktif, dan saldo terpakai.
  2. Jika ada sisa saldo carry-forward, muncul lencana kuning: *"Kedaluwarsa 30 Juni"* (`HRD-DEC-110`).
  3. Komponen reaktif terhadap perubahan saldo pasca submit cuti atau pembatalan.
* **Bukti verifikasi**: Storybook/UI isolated testing, verifikasi responsivitas mobile & desktop.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Komponen visual saldo beroperasi rapi dan akurat menyajikan data ledger.

### `FE-HRD-104` — Layar `FE-HRD-150` — Dasbor Kuota & Saldo Cuti Pegawai ESS (`/hr/leave/my-balance`) & Drawer Buku Besar
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-104.md](../task/report/frontend/FE-HRD-104.md).
* **Outcome**: Layar portal mandiri karyawan (ESS) untuk melihat saldo cuti pribadi, countdown kedaluwarsa kuota bawaan 30 Juni, tombol cepat ajukan cuti baru, serta tabel drawer riwayat mutasi buku besar (`TrxLeaveBalanceLedger`).
* **Requirement/decision**: `HRD-DEC-110`, `03-frontend-architecture.md` 17.1, 17.2.1.
* **Kontrak**: API Contract `v12` `GET /leave/balances/my`, `GET /leave/balances/ledger`.
* **Kewenangan UI**: Gaya tampilan drawer mutasi adalah `DEV_DISCRETION`.
* **Reuse**: `LeaveBalanceMetricCards`, `BaseTable`, `BaseDrawer`, `BaseButton`.
* **Cakupan**: `src/app/hr/leave/my-balance/page.jsx`, `src/components/features/hr/leave/LeaveLedgerDrawer.jsx`.
* **Dependency**: `FE-HRD-102`, `FE-HRD-103`, `BE-HRD-120` [BE]
* **Acceptance criteria**:
  1. Pegawai dapat melihat transparansi saldo cuti aktif dan riwayat mutasi kredit/debit.
  2. Drawer menampilkan jenis transaksi (`Entitlement`, `CarryForward`, `Taken`, `CancellationRestore`, `RecallRestore`, `Forfeiture`).
  3. Tombol `Ajukan Cuti Baru` mengarahkan ke formulir pengajuan `/hr/leave/request`.
* **Bukti verifikasi**: UAT mandiri pegawai pada layar `/hr/leave/my-balance`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar berfungsi responsif, data mutasi ledger tersaji transparan.

### `FE-HRD-105` — Layar `FE-HRD-153` — Manajemen Alokasi Saldo, Carry-Forward & Runner Kedaluwarsa HR (`/hr/leave/balances-admin`)
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-105.md](../task/report/frontend/FE-HRD-105.md).
* **Outcome**: Layar administrasi HR untuk memantau saldo seluruh staf rumah sakit per unit kerja, widget pemantauan sisa carry-forward menjelang 30 Juni, dan tombol manual eksekusi runner pemotongan kedaluwarsa carry-forward.
* **Requirement/decision**: `HRD-DEC-110`, `03-frontend-architecture.md` 17.2.4.
* **Kontrak**: API Contract `v12` `GET /leave/balances/admin`, `POST /leave/balances/carry-forward/run`.
* **Kewenangan UI**: Tata letak tabel pemantauan agregat adalah `DEV_DISCRETION`.
* **Reuse**: `BaseTable`, `BaseBadge`, `BasePagination`, `SearchBar`.
* **Cakupan**: `src/app/hr/leave/balances-admin/page.jsx`.
* **Dependency**: `FE-HRD-102`, `BE-HRD-120` [BE]
* **Acceptance criteria**:
  1. Filter unit kerja dan pencarian staf berfungsi cepat dengan pagination server-side.
  2. Menampilkan kolom hak berjalan, saldo bawaan, saldo terpakai, dan sisa aktif per pegawai.
  3. Tombol eksekusi runner carry-forward menampilkan modal konfirmasi sebelum eksekusi massal.
* **Bukti verifikasi**: UAT peran HR Leave Administrator pada layar `/hr/leave/balances-admin`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar terpasang atribut proteksi permission `[AccessPermission("LeaveBalance", "Manage")]`.

### `FE-HRD-106` — Modal Penyesuaian Saldo Resmi HR Berdokumen Audit (`LeaveBalanceAdjustmentModal`)
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-106.md](../task/report/frontend/FE-HRD-106.md).
* **Outcome**: Dialog modal khusus HR Manager untuk menyesuaikan saldo cuti staf secara resmi pasca penutupan periode payroll (`HRD-DEC-115`), mewajibkan input nomor dokumen SK/audit dan alasan operasional tertulis.
* **Requirement/decision**: `HRD-DEC-115`, `03-frontend-architecture.md` 17.3, Validation Matrix `v12` `VR-LEV-18`.
* **Kontrak**: API Contract `v12` `POST /leave/balances/adjustment`.
* **Kewenangan UI**: Bentuk modal dialog adalah `DEV_DISCRETION`.
* **Reuse**: `BaseModal`, `BaseInput`, `BaseSelect`, `BaseButton`.
* **Cakupan**: `src/components/features/hr/leave/LeaveBalanceAdjustmentModal.jsx`.
* **Dependency**: `FE-HRD-105`, `BE-HRD-128` [BE]
* **Acceptance criteria**:
  1. Input nomor dokumen audit dan alasan wajib diisi (tombol submit disable bila kosong).
  2. Memilih arah penyesuaian: Penambahan (+) atau Pengurangan (-) hari kerja.
  3. Sukses submit memicu refresh data saldo pada layar admin dan mencatat jejak audit trail.
* **Bukti verifikasi**: UAT penyesuaian saldo resmi HR dengan simulasi dokumen audit.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Modal terhubung aman ke backend dengan validasi ketat dokumen audit.

### `FE-HRD-107` — Komponen Kalender Cuti, Pemilih Sesi Half-Day 0.5 Hari & Validasi Saldo (`LeaveDateRangePicker`)
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-107.md](../task/report/frontend/FE-HRD-107.md).
* **Outcome**: Komponen pemilih rentang tanggal cuti pintar yang otomatis mendeteksi hari libur nasional / lepas jaga, menyediakan toggle opsi Cuti Setengah Hari (*Half-Day Leave* 0.5 hari sesi Pagi atau Siang/Sore per `HRD-ASM-20`), dan memvalidasi kecukupan saldo secara real-time.
* **Requirement/decision**: `HRD-ASM-20`, `03-frontend-architecture.md` 17.2.2.
* **Kontrak**: Validation Matrix `v12` `VR-LEV-03` s.d. `VR-LEV-05`.
* **Kewenangan UI**: Desain widget kalender interaktif adalah `DEV_DISCRETION`.
* **Reuse**: `DateRangePicker`, `SegmentedControl`, `Badge` Quilvian.
* **Cakupan**: `src/components/features/hr/leave/LeaveDateRangePicker.jsx`.
* **Dependency**: `FE-HRD-102`
* **Acceptance criteria**:
  1. Toggle cuti setengah hari hanya aktif bila jenis cuti yang dipilih adalah Cuti Tahunan (`VR-LEV-05`).
  2. Memilih half-day otomatis mengunci durasi permohonan tepat 0.5 hari kerja (`VR-LEV-04`).
  3. Menampilkan indikator visual peringatan merah bila durasi hari melebihi sisa saldo aktif pegawai.
* **Bukti verifikasi**: Unit test komponen pemilih tanggal dan kalkulasi hari efektif.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Komponen teruji stabil dan memvalidasi batas kuota secara presisi.

### `FE-HRD-108` — Modal Unggah Surat Sakit Dokter Ber-SIP & Masking Privasi Medis K3RS (`MedicalCertificateUploadModal`)
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-108.md](../task/report/frontend/FE-HRD-108.md).
* **Outcome**: Komponen unggah berkas pindaian PDF/JPG Surat Keterangan Sakit dokter resmi ber-SIP untuk cuti sakit $\ge 2$ hari (`HRD-DEC-112`), dilengkapi input nama faskes dan nomor SIP, serta teks jaminan privasi medis staf K3RS.
* **Requirement/decision**: `HRD-DEC-112`, `03-frontend-architecture.md` 17.2.2, 17.3.
* **Kontrak**: Validation Matrix `v12` `VR-LEV-06` s.d. `VR-LEV-08`.
* **Kewenangan UI**: Zona dropzone unggah berkas adalah `DEV_DISCRETION`.
* **Reuse**: `FileUpload`, `InformationAlert`, `BaseModal`.
* **Cakupan**: `src/components/features/hr/leave/MedicalCertificateUploadModal.jsx`.
* **Dependency**: `FE-HRD-102`, `BE-HRD-123` [BE]
* **Acceptance criteria**:
  1. Pengajuan sakit $\ge 2$ hari wajib mengunggah berkas dokter resmi ber-SIP sah (`VR-LEV-06`).
  2. Input nama faskes penerbit dan nomor SIP dokter divalidasi tidak boleh kosong (`VR-LEV-08`).
  3. Menampilkan banner informasi kepatuhan privasi: *"Diagnosa medis Anda dilindungi privasi K3RS dan tidak ditampilkan ke atasan langsung"*.
* **Bukti verifikasi**: UAT alur unggah berkas surat sakit dan validasi ekstensi/ukuran file.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Berkas terunggah aman ke storage backend dan metadata SIP tervalidasi.

### `FE-HRD-109` — Layar `FE-HRD-151` — Formulir Pengajuan Cuti Mandiri & Cuti Sakit ESS (`/hr/leave/request`)
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-109.md](../task/report/frontend/FE-HRD-109.md).
* **Outcome**: Layar formulir lengkap pengajuan cuti tahunan, cuti setengah hari (0.5 hari), izin alasan penting, cuti bersama, dan cuti sakit bagi seluruh staf rumah sakit melalui portal mandiri ESS.
* **Requirement/decision**: `HRD-DEC-110`, `HRD-DEC-112`, `HRD-ASM-19`, `HRD-ASM-20`, `03-frontend-architecture.md` 17.2.2.
* **Kontrak**: API Contract `v12` `POST /leave/requests`.
* **Kewenangan UI**: Tata letak form dua kolom adalah `DEV_DISCRETION`.
* **Reuse**: `LeaveDateRangePicker`, `MedicalCertificateUploadModal`, `BaseForm`, `BaseButton`.
* **Cakupan**: `src/app/hr/leave/request/page.jsx`.
* **Dependency**: `FE-HRD-107`, `FE-HRD-108`, `BE-HRD-122` [BE]
* **Acceptance criteria**:
  1. Pegawai dapat memilih jenis cuti, rentang tanggal, nomor kontak darurat, dan alasan cuti.
  2. Cuti setengah hari 0.5 hari terintegrasi mulus dengan pemilih sesi pagi/siang.
  3. Cuti sakit $\ge 2$ hari otomatis memicu pembukaan form upload surat dokter ber-SIP.
  4. Submit sukses menampilkan notifikasi keberhasilan dan mengarahkan ke dasbor riwayat.
* **Bukti verifikasi**: UAT pengajuan mandiri ESS untuk cuti tahunan, half-day, dan cuti sakit.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Formulir pengajuan cuti siap pakai di lingkungan staging dan produksi.

### `FE-HRD-110` — Komponen Banner Peringatan Kuota Pelayanan Staf (`SafeStaffingCoverageWarningBanner`)
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-110.md](../task/report/frontend/FE-HRD-110.md).
* **Outcome**: Komponen banner peringatan ketersediaan staf (*Safe Staffing Coverage Warning*) yang muncul pada kotak masuk approval atasan bila cuti yang diajukan menyebabkan jumlah perawat/staf dinas berada di bawah rasio minimal aman RS (`HRD-DEC-111`).
* **Requirement/decision**: `HRD-DEC-111`, `03-frontend-architecture.md` 17.2.3, 17.3.
* **Kontrak**: API Contract `v12` `LeaveApprovalInboxItemDto.HasStaffingCoverageWarning`.
* **Kewenangan UI**: Warna banner oranye/merah dan tata letak advisory adalah `DEV_DISCRETION`.
* **Reuse**: `Alert`, `Badge`, `BaseModal` Quilvian.
* **Cakupan**: `src/components/features/hr/leave/SafeStaffingCoverageWarningBanner.jsx`.
* **Dependency**: `FE-HRD-102`, `BE-HRD-121` [BE]
* **Acceptance criteria**:
  1. Jika kuota dinas aman, tampil lencana hijau: *"Ketersediaan staf dinas shift memenuhi standar aman"*.
  2. Jika kuota dinas kritis, tampil banner peringatan kuota layanan lengkap dengan rincian perawat dinas tersisa vs kuota minimal unit (`HRD-DEC-111`).
  3. Menyediakan tombol pintas simulasi untuk melihat daftar perawat yang berdinas pada shift tersebut.
* **Bukti verifikasi**: UI testing peringatan kuota pada berbagai kondisi rasio ketenagaan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Banner kuota tampil reaktif membantu atasan menjaga keselamatan pasien.

### `FE-HRD-111` — Layar `FE-HRD-152` — Kotak Masuk Approval Atasan & Aksi Approve with Mitigation (`/hr/leave/approval-inbox`)
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-111.md](../task/report/frontend/FE-HRD-111.md).
* **Outcome**: Layar kotak masuk persetujuan cuti bawahan bagi Kepala Ruangan/Instalasi: evaluasi kuota staf, pratinjau surat sakit berprivasi medis K3RS (diagnosa disamarkan), aksi Approve, aksi *Approve with Mitigation* (catatan perawat pengganti), revisi, dan penolakan operasional.
* **Requirement/decision**: `HRD-DEC-111`, `HRD-DEC-112`, `03-frontend-architecture.md` 17.2.3.
* **Kontrak**: API Contract `v12` tag `[Tags("HR Leave - Approvals & Coverage")]`.
* **Kewenangan UI**: Tata letak kartu permohonan pada inbox adalah `DEV_DISCRETION`.
* **Reuse**: `SafeStaffingCoverageWarningBanner`, `BaseTable`, `BaseModal`, `StatusBadge`.
* **Cakupan**: `src/app/hr/leave/approval-inbox/page.jsx`.
* **Dependency**: `FE-HRD-110`, `BE-HRD-124` [BE]
* **Acceptance criteria**:
  1. Atasan dapat melihat daftar permohonan cuti staf unit dengan filter status dan tanggal.
  2. Pada permohonan kritis, tombol `Approve with Mitigation` aktif dan mewajibkan atasan mencatat nama staf pengganti sebelum submit (`HRD-DEC-111`).
  3. Berkas surat sakit bawahan menampilkan masa istirahat dan SIP dokter dengan diagnosa klinis disamarkan (*Masked: Terlindungi Privasi Medis* per `HRD-DEC-112`).
* **Bukti verifikasi**: UAT Kepala Ruangan saat menyetujui cuti aman dan cuti berkondisi kuota kritis.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Kotak masuk approval berfungsi sempurna, mitigasi kuota tercatat rapi.

### `FE-HRD-112` — Layar `FE-HRD-154` — Inisiasi & Pemantauan Pemanggilan Darurat (Leave Recall) (`/hr/leave/recalls`)
* **Slice / MVP**: `S-B2` / `MVP-26`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-112.md](../task/report/frontend/FE-HRD-112.md).
* **Outcome**: Layar monitoring staf yang sedang menjalani cuti aktif dan dialog modal inisiasi pemanggilan dinas darurat (*Leave Recall*) oleh Kepala Ruangan saat terjadi KLB, bencana alam, atau lonjakan pasien kritis ICU (`HRD-DEC-113`).
* **Requirement/decision**: `HRD-DEC-113`, `03-frontend-architecture.md` 17.2.5, 17.3.
* **Kontrak**: API Contract `v12` tag `[Tags("HR Leave - Recall Management")]`.
* **Kewenangan UI**: Tampilan kartu pemantauan staf cuti adalah `DEV_DISCRETION`.
* **Reuse**: `BaseTable`, `BaseModal`, `BaseBadge`, `BaseInput`.
* **Cakupan**: `src/app/hr/leave/recalls/page.jsx`, `src/components/features/hr/leave/LeaveRecallInitiationModal.jsx`.
* **Dependency**: `FE-HRD-102`, `BE-HRD-126` [BE]
* **Acceptance criteria**:
  1. Menampilkan daftar staf yang sedang cuti aktif beserta tanggal mulai-selesai dan nomor kontak darurat.
  2. Modal inisiasi recall menghitung otomatis estimasi hari cuti yang batal dinikmati berdasarkan tanggal efektif kembali dinas.
  3. Kategori darurat (KLB, Bencana, Pasien Kritis) dan alasan tertulis wajib diisi sebelum usulan recall disubmit.
* **Bukti verifikasi**: UAT inisiasi recall darurat oleh Kepala Ruangan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Usulan recall darurat tersubmit rapi masuk ke antrean HR Manager.

### `FE-HRD-113` — Kotak Masuk Approval Leave Recall HR Manager & Restore Saldo Sisa (`LeaveRecallApprovalInbox`)
* **Slice / MVP**: `S-B2` / `MVP-26`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-113.md](../task/report/frontend/FE-HRD-113.md).
* **Outcome**: Layar antrean persetujuan resmi pemanggilan dinas darurat khusus untuk HR Manager (`HRD-DEC-113`, `HRD-Q-36`): meninjau justifikasi darurat, menyetujui pemanggilan yang otomatis memicu restore saldo sisa ke ledger, atau menolak usulan recall.
* **Requirement/decision**: `HRD-DEC-113`, `03-frontend-architecture.md` 17.2.5.
* **Kontrak**: API Contract `v12` `POST /leave/recalls/{id}/approve`, `POST /leave/recalls/{id}/reject`.
* **Kewenangan UI**: Tata letak panel review darurat adalah `DEV_DISCRETION`.
* **Reuse**: `BaseModal`, `InformationAlert`, `StatusBadge`, `BaseButton`.
* **Cakupan**: `src/components/features/hr/leave/LeaveRecallApprovalInbox.jsx`.
* **Dependency**: `FE-HRD-112`, `BE-HRD-127` [BE]
* **Acceptance criteria**:
  1. Hanya peran HR Manager yang dapat melihat dan mengeksekusi tombol `Setujui Pemanggilan & Pulihkan Saldo` (`LEV-B5`).
  2. Persetujuan recall menampilkan dialog konfirmasi pemulihan jumlah hari sisa ke saldo ledger staf.
  3. Eksekusi sukses memperbarui status tiket recall menjadi `Executed` dan memicu notifikasi pemulihan tugas ke staf.
* **Bukti verifikasi**: UAT persetujuan recall oleh HR Manager dan verifikasi penambahan saldo pulih.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Otorisasi tunggal HR Manager pada alur recall terbukti efektif dan aman.

### `FE-HRD-114` — Layar `FE-HRD-155` — Rekonsiliasi Akhir Cuti, Tabel Retrospektif & Payroll Close (`/hr/leave/reconciliations`)
* **Slice / MVP**: `S-B2` / `MVP-26`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-114.md](../task/report/frontend/FE-HRD-114.md).
* **Outcome**: Dasbor rekonsiliasi akhir cuti bulanan bagi HR Payroll Officer: memantau cuti pending saat cut-off yang akan direkonsiliasi retrospektif bulan depan (`HRD-DEC-114`), tombol penguncian periode cuti (*Payroll Close*), dan penonaktifan pembatalan bebas (immutabilitas status Completed per `HRD-DEC-115`).
* **Requirement/decision**: `HRD-DEC-114`, `HRD-DEC-115`, `03-frontend-architecture.md` 17.2.6.
* **Kontrak**: API Contract `v12` tag `[Tags("HR Leave - Reconciliations & Close")]`.
* **Kewenangan UI**: Tata letak tabel rekonsiliasi retrospektif adalah `DEV_DISCRETION`.
* **Reuse**: `BaseTable`, `BaseBadge`, `BaseModal`, `InformationAlert`.
* **Cakupan**: `src/app/hr/leave/reconciliations/page.jsx`.
* **Dependency**: `FE-HRD-102`, `BE-HRD-129` [BE]
* **Acceptance criteria**:
  1. Tabel cuti pending menampilkan label retrospektif (tidak memotong gaji periode berjalan).
  2. Tabel penyesuaian retrospektif menampilkan daftar cuti periode lampau yang baru disetujui untuk diserahkan ke payroll bulan ini.
  3. Tombol `Tutup & Kunci Periode Cuti` mengunci seluruh transaksi menjadi read-only dan menonaktifkan tombol pembatalan biasa (`HRD-DEC-115`).
* **Bukti verifikasi**: UAT penutupan periode cuti dan verifikasi immutabilitas pasca-close.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar rekonsiliasi bulanan beroperasi akurat dan siap integrasi ke Payroll (10).

### `FE-HRD-115` — Verifikasi ujung-ke-ujung antarmuka, linter, dan akses peran Slice `S-B2`
* **Slice / MVP**: `S-B2` / Verifikasi
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-115.md](../task/report/frontend/FE-HRD-115.md).
* **Outcome**: Seluruh 6 layar baru `S-B2` (`FE-HRD-150` s.d. `FE-HRD-155`) dan modal dialog pendukung terverifikasi terjangkau dari sidebar menu `Cuti & Perizinan`, hak akses peran teruji (Staf ESS vs Atasan Unit vs HR Manager), dan lolos uji linter CI/CD.
* **Requirement/decision**: `03-frontend-architecture.md` 17.1, 17.2; `04-prd-to-mvp.md` 26.5 (DoD `S-B2`).
* **Kontrak**: Seluruh kontrak `v12` bagian `S-B2`.
* **Kewenangan UI**: Tidak ada keputusan produk baru.
* **Reuse**: Pola verifikasi `FE-HRD-101`.
* **Cakupan**: Audit sidebar menu, uji loading/empty/error state, `npm run lint:errors`, `npm run build`.
* **Dependency**: `FE-HRD-104`, `FE-HRD-106`, `FE-HRD-109`, `FE-HRD-111`, `FE-HRD-113`, `FE-HRD-114`
* **Acceptance criteria**:
  1. Butir menu Cuti & Perizinan tampil terstruktur di sidebar sesuai hak akses peran.
  2. Staf umum hanya dapat mengakses dasbor saldo mandiri dan formulir pengajuan cuti ESS.
  3. Kotak masuk approval hanya tampil bagi Kepala Ruangan/Unit; aksi recall HR Manager hanya tampil bagi peran HR Manager.
  4. `npm run lint:errors` dan `npm run build` sukses 0 error.
* **Bukti verifikasi**: Tangkapan layar navigasi sidebar per peran, laporan lint/build bersih.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead bersama QA.
* **Definition of Done**: Seluruh layar teruji, bebas deadlink, dan bebas crash JavaScript.


### `FE-HRD-116` — API service client, tipe TypeScript & Redux slice lembur
* **Slice / MVP**: `S-B3` / `MVP-27`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-116.md](../task/report/frontend/FE-HRD-116.md).
* **Outcome**: Service Axios modular (`overtimeService.js`), tipe TypeScript/JSDoc kontrak data lembur v13, dan Redux slice (`overtimeSlice.js`) untuk mengelola state permohonan SPKL, klaim cito, approval, verifikasi discrepancy, saldo CTO, dan penutupan periode payroll.
* **Requirement/decision**: `03-frontend-architecture.md` 18.2.1, API Contract `v13`.
* **Kontrak**: API Contract `v13` seluruh endpoint `/api/v1/hr/overtimes/*`.
* **Kewenangan UI**: `DEV_DISCRETION` untuk struktur Redux state tree & action thunks.
* **Reuse**: Pola service client Axios dari `FE-HRD-102` / `FE-HRD-088`.
* **Cakupan**: `src/services/hr/overtimeService.js`, `src/store/slices/hr/overtimeSlice.js`.
* **Dependency**: `FE-HRD-007` ✅, `BE-HRD-131` [BE]
* **Acceptance criteria**:
  1. Client Axios terhubung ke seluruh endpoint tag `HR Overtime - *` dengan interceptor token otorisasi.
  2. Redux slice menangani state normalisasi permohonan lembur, daftar verifikasi, saldo CTO, dan loading/error lifecycle.
  3. Penanganan error memetakan respon RFC 7807 (ProblemDetails) ke notifikasi toast terstruktur.
* **Bukti verifikasi**: Uji unit Redux reducers dan mock API contract matching.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Service teruji, bebas regression, dan tipe data sinkron kontrak v13.

### 🟡 `FE-HRD-117` — Komponen form dual-mode SPKL & kalkulator kuota lembur PP 35/2021
* **Slice / MVP**: `S-B3` / `MVP-27`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R23`. Laporan lama: [FE-HRD-117.md](../task/report/frontend/FE-HRD-117.md).
* **Outcome**: Komponen reusable `OvertimeFormSwitch` dengan toggle mode: (1) Rencana Terencana SPKL H-1 (termasuk multi-select penugasan tim medis `IsCollectivePlan`), dan (2) Klaim Darurat Cito Pasca-Dinas H+1 (dengan input wajib Nomor RM / No. Insiden IGD / ICU per `HRD-DEC-116`). Dilengkapi kalkulator kuota real-time peringatan batas 4 jam/hari dan 18 jam/minggu (PP 35/2021) dengan indikator eskalasi Direktur Medik.
* **Requirement/decision**: `HRD-DEC-116`, `HRD-DEC-118`, `03-frontend-architecture.md` 18.2.2.
* **Kontrak**: API Contract `v13` tag `[Tags("HR Overtime - Plans & Claims")]`.
* **Kewenangan UI**: `DEV_DISCRETION` untuk interaksi toggle form dan visual meter kuota PP 35/2021.
* **Reuse**: `@/components/features/base-features/base-form-control`, `BaseBadge`, `BaseSwitch`.
* **Cakupan**: `src/components/features/hr/overtime/OvertimeFormSwitch.jsx`, `src/components/features/hr/overtime/OvertimeQuotaMeter.jsx`.
* **Dependency**: `FE-HRD-116`
* **Acceptance criteria**:
  1. Toggle mode merender form dinamis: SPKL H-1 (waktu rencana, alasan, delegasi tim) vs Cito H+1 (waktu aktual, nomor RM/kasus darurat).
  2. Validasi mencegah submit mode Cito jika nomor RM kosong (`VR-OT-03`).
  3. Visual meter menghitung akumulasi jam lembur mingguan dan menampilkan badge peringatan kuning jika mendekati 18 jam dan merah jika melampaui.
* **Bukti verifikasi**: Uji interaksi form, validasi boundary input, dan simulasi meter kuota.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Komponen reusable siap dipasang pada layar ESS.

### 🟡 `FE-HRD-118` — Layar `FE-HRD-156` — Pengajuan SPKL Pra-Dinas & Klaim Cito Pasca-Dinas ESS
* **Slice / MVP**: `S-B3` / `MVP-27`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 4 dari 4 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R23`. Laporan lama: [FE-HRD-118.md](../task/report/frontend/FE-HRD-118.md).
* **Outcome**: Halaman portal ESS `/hr/self-service/overtime` bagi staf rumah sakit untuk mengajukan rencana SPKL pra-dinas mandiri/tim, mengajukan klaim cito darurat pasca-dinas dengan melampirkan berkas bukti tindakan medis/surat tugas dinas, memantau riwayat pengajuan, serta melihat status persetujuan berjenjang.
* **Requirement/decision**: `HRD-DEC-116`, `HRD-DEC-117`, `03-frontend-architecture.md` 18.2.1.
* **Kontrak**: API Contract `v13` tag `[Tags("HR Overtime - Plans & Claims")]`.
* **Kewenangan UI**: `MANDATORY` mengikuti pola ESS portal HR Quilvian.
* **Reuse**: `OvertimeFormSwitch`, `OvertimeQuotaMeter`, `BaseTable`, `StatusBadge`, `InformationAlert`.
* **Cakupan**: `src/app/hr/self-service/overtime/page.jsx`.
* **Dependency**: `FE-HRD-116`, `FE-HRD-117`, `BE-HRD-134` [BE], `BE-HRD-135` [BE]
* **Acceptance criteria**:
  1. Staf dapat mengajukan rencana SPKL terencana H-1 dengan pilihan kompensasi (Uang Lembur vs Libur Pengganti CTO).
  2. Staf dinas darurat dapat mengajukan klaim cito H+1 dengan input nomor RM dan upload berkas bukti logbook dinas.
  3. Riwayat pengajuan menampilkan pill status dinamis (`Draft`, `Submitted`, `ApprovedRoom`, `ApprovedHR`, `Rejected`, `EscalatedDirector`).
  4. Pengajuan kolektif oleh Penanggung Jawab Tim merender daftar anggota tim medis yang ditugaskan.
* **Bukti verifikasi**: UAT alur pengajuan pra-dinas dan klaim cito mandiri staf IGD/Rawat Inap.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar ESS responsif, form validasi bekerja optimal, dan data tersimpan ke backend.

### 🟡 `FE-HRD-119` — Komponen kotak masuk persetujuan berjenjang & anti-self-approval
* **Slice / MVP**: `S-B3` / `MVP-28`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R23`. Laporan lama: [FE-HRD-119.md](../task/report/frontend/FE-HRD-119.md).
* **Outcome**: Komponen kartu/tabel inbox persetujuan lembur ruangan dengan penguncian mutlak tombol aksi (*disabled button*) jika penilai adalah pemohon lembur sendiri (anti-self-approval per `HRD-DEC-117`), menampilkan rincian rencana jam vs waktu aktual shift, alasan penugasan, dan catatan persetujuan berjenjang.
* **Requirement/decision**: `HRD-DEC-117`, `03-frontend-architecture.md` 18.2.2.
* **Kontrak**: API Contract `v13` tag `[Tags("HR Overtime - Approvals")]`.
* **Kewenangan UI**: `DEV_DISCRETION` untuk tata letak indikator jenjang persetujuan (Penyelia -> Ka. Ruangan).
* **Reuse**: `BaseButton`, `BaseBadge`, `BaseCard`, `InformationAlert`.
* **Cakupan**: `src/components/features/hr/overtime/OvertimeApprovalTable.jsx`, `src/components/features/hr/overtime/AntiSelfApprovalNotice.jsx`.
* **Dependency**: `FE-HRD-116`, `BE-HRD-137` [BE]
* **Acceptance criteria**:
  1. Jika `applicant_employee_id == current_user_employee_id`, tombol `Setujui` dan `Tolak` dinonaktifkan dengan banner `Self-Approval Dilarang: Menunggu Penilai Pengganti`.
  2. Aksi persetujuan berjenjang mencatat catatan rekomendasi supervisi sebelum diteruskan ke Kepala Unit.
  3. Konfirmasi tolak wajib mengisi alasan penolakan tertulis (`VR-OT-11`).
* **Bukti verifikasi**: Uji isolasi komponen persetujuan dengan ID pemohon sama vs berbeda.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Proteksi anti-self-approval terverifikasi aktif pada antarmuka.

### 🟡 `FE-HRD-120` — Layar `FE-HRD-157` — Kotak Masuk Approval Lembur Ruangan (Penyelia & Ka. Unit)
* **Slice / MVP**: `S-B3` / `MVP-28`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R23`. Laporan lama: [FE-HRD-120.md](../task/report/frontend/FE-HRD-120.md).
* **Outcome**: Halaman `/hr/overtime/approvals` bagi Kepala Ruangan / Kepala Instalasi / Supervisor Medis untuk memeriksa pengajuan lembur tim, menyetujui, menolak, atau mengembalikan dengan catatan perbaikan sebelum diteruskan ke verifikasi HR.
* **Requirement/decision**: `HRD-DEC-117`, `03-frontend-architecture.md` 18.2.1.
* **Kontrak**: API Contract `v13` tag `[Tags("HR Overtime - Approvals")]`.
* **Kewenangan UI**: `MANDATORY` mengikuti standar kotak masuk persetujuan Quilvian.
* **Reuse**: `OvertimeApprovalTable`, `AntiSelfApprovalNotice`, `BaseModal`, `StatusBadge`.
* **Cakupan**: `src/app/hr/overtime/approvals/page.jsx`.
* **Dependency**: `FE-HRD-119`, `BE-HRD-137` [BE]
* **Acceptance criteria**:
  1. Menampilkan filter tab: Menunggu Persetujuan, Riwayat Persetujuan Ruangan, dan Eskalasi.
  2. Mendukung batch approval untuk pengajuan SPKL terencana tim medis yang telah diverifikasi kelengkapannya.
  3. Memuat badge peringatan jika pengajuan lembur berasal dari klaim cito darurat pasca-dinas dengan batas H+1.
* **Bukti verifikasi**: UAT alur persetujuan ruangan oleh Kepala Ruangan Rawat Inap & IGD.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar approval ruangan siap pakai dan terintegrasi aman dengan backend.

### 🟡 `FE-HRD-121` — Drawer investigasi anomali presensi biometrik & bukti logbook fisik dinas
* **Slice / MVP**: `S-B3` / `MVP-28`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R35`. Laporan lama: [FE-HRD-121.md](../task/report/frontend/FE-HRD-121.md).
* **Outcome**: Drawer modal detail `OvertimeDiscrepancyDrawer` untuk HR Attendance Officer: menampilkan perbandingan 3 panel (Waktu SPKL Rencana vs Waktu Aktual Biometrik Mesin Finger/Face vs Logbook Fisik Ruangan), rincian selisih menit (discrepancy), indikator aturan batas minimum 30 menit (`MinOvertimeDurationRule`), dan formulir penetapan durasi lembur yang diakui resmi dengan kewajiban upload bukti logbook fisik bertandatangan Ka. Ruangan (`HRD-DEC-119`).
* **Requirement/decision**: `HRD-DEC-119`, `03-frontend-architecture.md` 18.2.2.
* **Kontrak**: API Contract `v13` tag `[Tags("HR Overtime - Verifications")]`.
* **Kewenangan UI**: `DEV_DISCRETION` untuk tata letak visual perbandingan 3-way time match.
* **Reuse**: `BaseDrawer`, `BaseButton`, `BaseFileInput`, `InformationAlert`, `BaseBadge`.
* **Cakupan**: `src/components/features/hr/overtime/OvertimeDiscrepancyDrawer.jsx`.
* **Dependency**: `FE-HRD-116`, `BE-HRD-139` [BE], `BE-HRD-140` [BE]
* **Acceptance criteria**:
  1. Visualisasi 3-panel menunjukkan jam rencana, jam tap mesin presensi, dan jam klaim dengan selisih yang di-highlight warna.
  2. Menampilkan peringatan otomatis jika durasi aktual kurang dari 30 menit (aturan 0 menit lembur diakui per `HRD-DEC-119`).
  3. Tombol simpan keputusan verifikasi mewajibkan input jam disetujui, catatan investigasi HR, dan upload berkas bukti logbook fisik jika terdapat discrepancy > 15 menit.
* **Bukti verifikasi**: Uji interaksi drawer pada data uji discrepancy presensi (under-hours, over-hours, missing-tap).
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Drawer interaktif teruji dan mampu menangani kasus anomali presensi secara akurat.

### 🟡 `FE-HRD-122` — Layar `FE-HRD-158` — Pusat Verifikasi Presensi & Rekonsiliasi Discrepancy HR
* **Slice / MVP**: `S-B3` / `MVP-28`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R35`. Laporan lama: [FE-HRD-122.md](../task/report/frontend/FE-HRD-122.md).
* **Outcome**: Halaman pusat kendali `/hr/overtime/verifications` bagi staf HR Attendance untuk memverifikasi seluruh SPKL/klaim lembur yang telah disetujui ruangan terhadap data tap mesin presensi Sub-Modul 07, menyortir kasus anomali/discrepancy, dan menerbitkan persetujuan final verifikasi lembur berbayar.
* **Requirement/decision**: `HRD-DEC-119`, `03-frontend-architecture.md` 18.2.1.
* **Kontrak**: API Contract `v13` tag `[Tags("HR Overtime - Verifications")]`.
* **Kewenangan UI**: `MANDATORY` mengikuti pola pusat verifikasi HR Quilvian.
* **Reuse**: `OvertimeDiscrepancyDrawer`, `BaseTable`, `BaseBadge`, `FilterPanel`.
* **Cakupan**: `src/app/hr/overtime/verifications/page.jsx`.
* **Dependency**: `FE-HRD-121`, `BE-HRD-140` [BE]
* **Acceptance criteria**:
  1. Menampilkan daftar verifikasi dengan filter: Unit Kerja, Tanggal Lembur, Status Discrepancy (Match, Under-Hours, Over-Hours, No-Tap).
  2. Baris dengan discrepancy memiliki tombol aksi `Investigasi & Sesuaikan` yang membuka `OvertimeDiscrepancyDrawer`.
  3. Fitur `Verifikasi Cepat Massal` (Batch Verify) untuk seluruh baris berstatus `Match 100%` tanpa anomali.
* **Bukti verifikasi**: UAT pusat verifikasi lembur staf HR dengan data simulasi mesin presensi.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar verifikasi operasional penuh dengan filtering responsif dan audit jejak penyesuaian.

### 🟡 `FE-HRD-123` — Layar `FE-HRD-159-A` — Kotak Masuk Approval Eskalasi Direktur Medik
* **Slice / MVP**: `S-B3` / `MVP-28`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R35`. Laporan lama: [FE-HRD-123.md](../task/report/frontend/FE-HRD-123.md).
* **Outcome**: Halaman `/hr/overtime/medical-director-inbox` khusus bagi Direktur Pelayanan Medik (dan Direktur Terkait) untuk mengevaluasi dan memberikan otorisasi khusus terhadap kasus lembur darurat cito yang melampaui kuota PP 35/2021 (> 4 jam/hari atau > 18 jam/minggu) demi penyelamatan nyawa pasien (*life-saving*) dan kontinuitas pelayanan vital rumah sakit (`HRD-DEC-118`).
* **Requirement/decision**: `HRD-DEC-118`, `03-frontend-architecture.md` 18.2.1.
* **Kontrak**: API Contract `v13` tag `[Tags("HR Overtime - Medical Director Approvals")]`.
* **Kewenangan UI**: `MANDATORY` dengan penegakan izin akses khusus `MedicalDirector` / `HrManager`.
* **Reuse**: `BaseTable`, `BaseBadge`, `BaseModal`, `InformationAlert`.
* **Cakupan**: `src/app/hr/overtime/medical-director-inbox/page.jsx`.
* **Dependency**: `FE-HRD-116`, `BE-HRD-138` [BE]
* **Acceptance criteria**:
  1. Hanya dapat diakses oleh akun dengan wewenang Direktur Medik atau HR Manager (dilindungi route guard).
  2. Menampilkan rincian justifikasi klinis: Nomor Rekam Medis (RM), Unit Pelayanan (IGD/ICU/OK), dokter operator, dan akumulasi jam lembur staf.
  3. Tombol aksi `Beri Otorisasi Khusus Darurat` membuka modal konfirmasi dengan peringatan kepatuhan regulasi Kemenaker dan audit trail permanen.
* **Bukti verifikasi**: UAT skenario eskalasi darurat operasi cito malam hari oleh Direktur Medik.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Halaman terproteksi ketat, modal konfirmasi otorisasi berfungsi aman.

### 🟡 `FE-HRD-124` — Komponen kartu tiket saldo hak libur pengganti CTO (90 hari masa aktif)
* **Slice / MVP**: `S-B3` / `MVP-29`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R35`. Laporan lama: [FE-HRD-124.md](../task/report/frontend/FE-HRD-124.md).
* **Outcome**: Komponen reusable `CompensatoryTimeOffCard` yang menampilkan saldo hak libur pengganti staf, tanggal kedaluwarsa 90 hari kalender sejak SPKL disetujui (`HRD-DEC-120`), visual countdown hari aktif tersisa, serta riwayat konversi ke mutasi kredit cuti pada Sub-Modul 08 (Cuti & Izin).
* **Requirement/decision**: `HRD-DEC-120`, `03-frontend-architecture.md` 18.2.2.
* **Kontrak**: API Contract `v13` tag `[Tags("HR Overtime - Compensatory Time-Off")]`.
* **Kewenangan UI**: `DEV_DISCRETION` untuk indikator visual sisa hari aktif (hijau >30 hari, oranye 10-30 hari, merah <10 hari).
* **Reuse**: `BaseCard`, `BaseBadge`, `ProgressBar`.
* **Cakupan**: `src/components/features/hr/overtime/CompensatoryTimeOffCard.jsx`.
* **Dependency**: `FE-HRD-116`, `BE-HRD-141` [BE]
* **Acceptance criteria**:
  1. Menampilkan total jam/hari saldo CTO yang masih aktif dan tanggal batas pakai.
  2. Visual countdown memberikan sinyal warna mendesak jika sisa masa berlaku < 14 hari kalender.
  3. Menampilkan label status tiket: `Active`, `FullyUsed`, `PartiallyUsed`, `Expired`.
* **Bukti verifikasi**: Uji unit komponen dengan variasi sisa hari kedaluwarsa dan status tiket.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Komponen saldo CTO informatif, akurat, dan siap diintegrasikan.

### 🟡 `FE-HRD-125` — Layar `FE-HRD-159` — Monitoring Saldo Cuti Pengganti (CTO) & Riwayat Konversi
* **Slice / MVP**: `S-B3` / `MVP-29`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R35`. Laporan lama: [FE-HRD-125.md](../task/report/frontend/FE-HRD-125.md).
* **Outcome**: Halaman `/hr/overtime/compensatory-leave` dengan mode tampilan ganda: (1) Dasbor mandiri pegawai ESS untuk melihat saldo tiket CTO dan opsi tombol langsung ajukan cuti (terhubung ke Sub-Modul 08), dan (2) Dasbor HR Officer untuk memantau seluruh saldo CTO aktif pegawai RS, audit tiket kedaluwarsa hangus, dan rekapitulasi konversi.
* **Requirement/decision**: `HRD-DEC-120`, `03-frontend-architecture.md` 18.2.1.
* **Kontrak**: API Contract `v13` tag `[Tags("HR Overtime - Compensatory Time-Off")]`.
* **Kewenangan UI**: `MANDATORY` mengikuti standar dasbor saldo Quilvian.
* **Reuse**: `CompensatoryTimeOffCard`, `BaseTable`, `StatusBadge`, `BaseModal`.
* **Cakupan**: `src/app/hr/overtime/compensatory-leave/page.jsx`.
* **Dependency**: `FE-HRD-124`, `BE-HRD-141` [BE]
* **Acceptance criteria**:
  1. Tampilan ESS menampilkan kartu saldo CTO pegawai yang sedang login beserta riwayat tiket SPKL asalnya.
  2. Tombol `Gunakan untuk Permohonan Cuti` menavigasi pengguna langsung ke formulir cuti Sub-Modul 08 dengan tipe cuti ter-prefill `Cuti Pengganti Lembur (CTO)`.
  3. Tampilan HR menampilkan rekapitulasi saldo CTO lintas unit dan riwayat saldo yang telah hangus karena melewati 90 hari.
* **Bukti verifikasi**: UAT penelusuran saldo CTO staf dari tanggal lembur hingga konversi cuti.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar monitoring CTO berfungsi sempurna pada mode ESS maupun HR Admin.

### 🟡 `FE-HRD-126` — Komponen tabel rekonsiliasi & indikator rapel retrospektif lembur
* **Slice / MVP**: `S-B3` / `MVP-29`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R35`. Laporan lama: [FE-HRD-126.md](../task/report/frontend/FE-HRD-126.md).
* **Outcome**: Komponen reusable `OvertimeReconciliationTable` untuk menampilkan data lembur yang masuk dalam cut-off tanggal 20 bulan berjalan vs klaim cito/SPKL yang terlambat diverifikasi pasca-cut-off yang ditandai sebagai `RetrospectiveAdjustment` (akan dirapel otomatis pada payroll bulan berikutnya per `HRD-DEC-121`), dilengkapi filter status verifikasi dan tombol ekspor audit.
* **Requirement/decision**: `HRD-DEC-121`, `03-frontend-architecture.md` 18.2.2.
* **Kontrak**: API Contract `v13` tag `[Tags("HR Overtime - Reconciliations & Close")]`.
* **Kewenangan UI**: `DEV_DISCRETION` untuk penataan tag badge penanda rapel retrospektif.
* **Reuse**: `BaseTable`, `BaseBadge`, `BaseButton`, `InformationAlert`.
* **Cakupan**: `src/components/features/hr/overtime/OvertimeReconciliationTable.jsx`.
* **Dependency**: `FE-HRD-116`, `BE-HRD-143` [BE]
* **Acceptance criteria**:
  1. Menampilkan tag `Termasuk Payroll Berjalan` untuk SPKL yang terverifikasi sebelum cut-off (tgl 20).
  2. Menampilkan tag `Rapel Bulan Depan (Retrospektif)` untuk klaim cito atau verifikasi yang selesai setelah tanggal 20.
  3. Kalkulator total ringkasan menampilkan: total jam lembur reguler, total jam lembur libur/hari besar, dan total jam CTO yang tidak dibayar tunai.
* **Bukti verifikasi**: Uji coba rendering komponen dengan data transaksi di sekitar tanggal cut-off (19-22).
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Komponen tabel rekonsiliasi akurat dan informatif bagi petugas HR.

### 🟡 `FE-HRD-127` — Layar `FE-HRD-160` — Penutupan Periode Lembur, Cut-Off & Rekonsiliasi Payroll
* **Slice / MVP**: `S-B3` / `MVP-29`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R35`. Laporan lama: [FE-HRD-127.md](../task/report/frontend/FE-HRD-127.md).
* **Outcome**: Halaman `/hr/overtime/reconciliations` bagi HR Payroll Officer untuk melakukan rekonsiliasi akhir bulanan, memeriksa rekapitulasi jam lembur per pegawai/unit, menjalankan aksi resmi `Tutup & Kunci Periode Lembur` (*Payroll Close*), serta menyerahkan data transaksi lembur terkunci ke Modul 10 (Payroll) secara aman.
* **Requirement/decision**: `HRD-DEC-121`, `03-frontend-architecture.md` 18.2.1.
* **Kontrak**: API Contract `v13` tag `[Tags("HR Overtime - Reconciliations & Close")]`.
* **Kewenangan UI**: `MANDATORY` mengikuti pola penutupan periode payroll Quilvian.
* **Reuse**: `OvertimeReconciliationTable`, `BaseModal`, `InformationAlert`, `StatusBadge`.
* **Cakupan**: `src/app/hr/overtime/reconciliations/page.jsx`.
* **Dependency**: `FE-HRD-126`, `BE-HRD-143` [BE], `BE-HRD-144` [BE]
* **Acceptance criteria**:
  1. Menampilkan ringkasan eksekutif jam lembur per departemen dan estimasi beban finansial.
  2. Tombol `Tutup & Kunci Periode Lembur` membuka modal dialog konfirmasi peringatan penguncian data permanen (immutability).
  3. Setelah periode dikunci, seluruh transaksi pada periode tersebut berubah menjadi read-only dan tombol serah-terima ke Modul Payroll aktif.
* **Bukti verifikasi**: UAT proses penutupan periode tanggal 20 dan verifikasi penguncian transaksi.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar penutupan periode lembur beroperasi tanpa cela dan siap integrasi payroll.

### 🟡 `FE-HRD-128` — Layar `FE-HRD-161` — Pengaturan Kebijakan & Kuota Lembur RS (PP 35/2021)
* **Slice / MVP**: `S-B3` / `MVP-27`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R35`. Laporan lama: [FE-HRD-128.md](../task/report/frontend/FE-HRD-128.md).
* **Outcome**: Halaman `/hr/overtime/settings` bagi HR Administrator untuk mengonfigurasi parameter operasional lembur rumah sakit: batas kuota jam harian (default 4 jam) dan mingguan (default 18 jam), aturan pembulatan durasi (minimum 30 menit), masa berlaku saldo CTO (default 90 hari kalender), tanggal cut-off payroll bulanan (default tgl 20), serta penugasan wewenang persetujuan eskalasi medis.
* **Requirement/decision**: `HRD-DEC-116` s.d. `HRD-DEC-121`, `03-frontend-architecture.md` 18.2.1.
* **Kontrak**: API Contract `v13` tag `[Tags("HR Overtime - Settings")]`.
* **Kewenangan UI**: `MANDATORY` mengikuti pola form pengaturan modul HR.
* **Reuse**: `@/components/features/base-features/base-form-control`, `BaseButton`, `InformationAlert`.
* **Cakupan**: `src/app/hr/overtime/settings/page.jsx`.
* **Dependency**: `FE-HRD-116`, `BE-HRD-132` [BE]
* **Acceptance criteria**:
  1. Form pengaturan menampilkan nilai parameter kuota PP 35/2021 yang dapat disesuaikan dengan validasi batas wajar.
  2. Dilengkapi penjelasan tooltip regulasi ketenagakerjaan dan kebijakan operasional rumah sakit.
  3. Aksi simpan merekam audit log riwayat perubahan parameter oleh HR Admin.
* **Bukti verifikasi**: Uji simpan parameter konfigurasi lembur dan verifikasi penerapan pada kalkulator kuota.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar pengaturan lembur tersimpan rapi dan mengendalikan parameter sistem secara dinamis.

### `FE-HRD-129` — Verifikasi ujung-ke-ujung antarmuka, linter, dan akses peran Slice `S-B3`
* **Slice / MVP**: `S-B3` / Verifikasi
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-129.md](../task/report/frontend/FE-HRD-129.md).
* **Outcome**: Seluruh 6 layar baru `S-B3` (`FE-HRD-156` s.d. `FE-HRD-161`) dan modal drawer investigasi terverifikasi terjangkau dari sidebar menu navigasi `Manajemen Lembur`, hak akses peran teruji (Staf Medis ESS vs Supervisor Ruangan vs HR Attendance vs Direktur Medik vs HR Payroll), dan lolos uji ketat linter CI/CD.
* **Requirement/decision**: `03-frontend-architecture.md` 18.1, 18.2; `04-prd-to-mvp.md` 27.5 (DoD `S-B3`).
* **Kontrak**: Seluruh kontrak `v13` bagian `S-B3`.
* **Kewenangan UI**: Tidak ada keputusan produk baru.
* **Reuse**: Pola audit verifikasi `FE-HRD-115` / `FE-HRD-101`.
* **Cakupan**: Audit sidebar menu navigasi, uji loading/empty/error state, `npm run lint:errors`, `npm run build`.
* **Dependency**: `FE-HRD-118`, `FE-HRD-120`, `FE-HRD-122`, `FE-HRD-123`, `FE-HRD-125`, `FE-HRD-127`, `FE-HRD-128`
* **Acceptance criteria**:
  1. Butir menu Manajemen Lembur tampil rapi di sidebar sesuai hak akses peran pengguna yang login.
  2. Staf umum hanya dapat mengakses pengajuan ESS dan pemantau saldo CTO mandiri.
  3. Kotak masuk persetujuan ruangan hanya tampil bagi Kepala Ruangan/Supervisor; kotak masuk eskalasi darurat hanya tampil bagi Direktur Medik.
  4. `npm run lint:errors` dan `npm run build` berhasil 0 error.
* **Bukti verifikasi**: Tangkapan layar navigasi sidebar per peran, laporan lint dan build produksi bersih.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead bersama QA.
* **Definition of Done**: Seluruh antarmuka lembur teruji, bebas tautan mati (broken link), dan bebas crash JavaScript.


---

## 22. Rincian Task Frontend Slice `S-B5` — Pemrosesan Penggajian Pegawai Rumah Sakit (Sub-modul 10, Kontrak v14)

### `FE-HRD-130` — Fondasi frontend modul Payroll, Redux slice `payrollSlice.js`, Axios client & tipe data kanonikal
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-130.md](../task/report/frontend/FE-HRD-130.md).
* **Outcome**: Fondasi arsitektur frontend untuk modul penggajian V2: Redux Toolkit slice `payrollSlice.js`, Axios HTTP client terspesialisasi dengan penanganan idempotency key dan token otentikasi peran HRD, pemetaan enum status siklus hidup kanonikal (`PayrollRunStatus`, `VarianceAlertStatus`, `FinanceHandoffStatus`, `PayslipStatus`), serta tipe data TypeScript/JSDoc rincian gaji.
* **Requirement/decision**: `HRD-DEC-122` s.d. `HRD-DEC-127`, `03-frontend-architecture.md` 19.3 & 19.4.
* **Kontrak**: API Contract `v14` Bagian 20, State Transition Matrix `v14` Bagian 18.
* **Kewenangan UI**: Standar state management Redux Quilvian.
* **Reuse**: `src/store/index.js`, Axios instance template Quilvian.
* **Cakupan**: `src/store/slices/payrollSlice.js`, `src/services/payroll/payrollConstants.js`.
* **Dependency**: `BE-HRD-147` [BE], `BE-HRD-149` [BE], `BE-HRD-153` [BE]
* **Acceptance criteria**:
  1. Redux slice mengelola state batch aktif, daftar variansi, riwayat approval, dan status handoff Finance.
  2. Seluruh status kanonikal dan badge warna terdefinisi konsisten sesuai token Quilvian.
  3. Konfigurasi Axios menyertakan interceptor error global untuk penanganan kode status HTTP 400, 403, 409, 422.
* **Bukti verifikasi**: Unit test Redux slice dan verifikasi registrasi store Next.js.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Store dan utilitas frontend payroll terdaftar rapi tanpa error runtime.

### `FE-HRD-131` — Layanan API service frontend integrasi endpoint Run Cycles & Settings
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-131.md](../task/report/frontend/FE-HRD-131.md).
* **Outcome**: Berkas service frontend `src/services/payroll/payrollService.js` yang membungkus pemanggilan API backend untuk siklus batch penggajian (`[Tags("HR Payroll - Run Cycles")]`), kueri precondition check, dan pembacaan konfigurasi parameter (`[Tags("HR Payroll - Settings")]`).
* **Requirement/decision**: `HRD-DEC-122`, `03-frontend-architecture.md` 19.4.
* **Kontrak**: API Contract `v14` Bagian 20 (Tag 1 & Tag 6).
* **Kewenangan UI**: Service layer terisolasi.
* **Reuse**: Axios client dari `FE-HRD-130`.
* **Cakupan**: `src/services/payroll/payrollService.js`.
* **Dependency**: `FE-HRD-130`
* **Acceptance criteria**:
  1. Fungsi `getPayrollRuns`, `getPreconditionCheck`, `createPayrollRun`, `calculatePayrollRun` terdefinisi lengkap.
  2. Payload request dan tipe response sesuai kontrak JSON Swagger tanpa deviasi skema.
  3. Penanganan error memetakan pesan kegagalan validasi backend secara transparan ke UI.
* **Bukti verifikasi**: Uji integrasi service API via mock/sandbox backend.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Seluruh fungsi pemanggilan API teruji dan mengembalikan Promise terstruktur.

### `FE-HRD-132` — Layar `FE-HRD-162` — Dasbor Ringkasan & Monitoring Siklus Penggajian (`/hr/payroll/runs`)
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-132.md](../task/report/frontend/FE-HRD-132.md).
* **Outcome**: Halaman dasbor utama `/hr/payroll/runs` bagi Payroll Officer dan HR Manager untuk memantau status siklus penggajian bulanan rumah sakit, ringkasan KPI eksekutif (total take-home pay, jumlah staf terbayar, rasio variansi, status serah terima Finance), tabel riwayat batch dengan filter tahun/bulan, serta tombol navigasi inisiasi batch baru.
* **Requirement/decision**: `HRD-DEC-122`, `03-frontend-architecture.md` 19.2.1.
* **Kontrak**: API Contract `v14` `GET /api/v1/hr/payroll/runs`.
* **Kewenangan UI**: `MANDATORY` mengikuti pola dasbor transaksi Quilvian.
* **Reuse**: `Card`, `StatusBadge`, `BaseButton`, `DataTable`, `InformationAlert`.
* **Cakupan**: `src/app/hr/payroll/runs/page.jsx`.
* **Dependency**: `FE-HRD-131`
* **Acceptance criteria**:
  1. Menampilkan 4 kartu ringkasan metrik finansial utama siklus aktif.
  2. Tabel daftar batch memuat nomor batch, periode, status badge berwarna, total nilai bruto, total neto, dan aksi detail.
  3. Tombol "Buat Siklus Payroll Baru" mengarahkan pengguna ke wizard inisiasi batch.
* **Bukti verifikasi**: UAT dasbor monitoring dan verifikasi responsivitas layar desktop.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar dasbor payroll tampil elegan dan menyajikan status batch secara akurat.

### `FE-HRD-133` — Layar `FE-HRD-163` — Wizard Inisiasi Batch & Validasi Precondition Gate Cut-off tgl 20 (`/hr/payroll/runs/new`)
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-133.md](../task/report/frontend/FE-HRD-133.md).
* **Outcome**: Halaman wizard `/hr/payroll/runs/new` untuk memandu Payroll Officer menginisiasi siklus penggajian baru, mencakup pemilihan tahun/bulan periode, visualisasi verifikasi gerbang prasyarat 3 modul waktu kerja (Presensi 07, Cuti 08, Lembur 09), checklist penguncian cut-off tgl 20, dan tombol submit inisiasi draft batch.
* **Requirement/decision**: `HRD-DEC-122`, `03-frontend-architecture.md` 19.2.1.
* **Kontrak**: API Contract `v14` `/runs/precondition-check` & `POST /runs`, `VR-PAY-01`, `VR-PAY-02`.
* **Kewenangan UI**: `MANDATORY` wizard alur terkontrol.
* **Reuse**: `@/components/features/base-features/base-form-control`, `BaseButton`, `InformationAlert`, `StatusBadge`.
* **Cakupan**: `src/app/hr/payroll/runs/new/page.jsx`.
* **Dependency**: `FE-HRD-132`
* **Acceptance criteria**:
  1. Wizard memeriksa status ketiga modul waktu kerja saat periode dipilih; status terbuka/terkunci ditampilkan jelas.
  2. Tombol inisiasi dinonaktifkan (*disabled*) dan peringatan informatif muncul jika ada modul yang belum terkunci (`PAY-B1`).
  3. Submit inisiasi sukses mengarahkan pengguna ke lembar kerja kalkulasi batch (`FE-HRD-164`).
* **Bukti verifikasi**: UAT skenario `UAT-PAY-01` dan pengujian simulasi penolakan modul terbuka.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Wizard inisiasi payroll mencegah pembuatan batch cacat prasyarat.

### `FE-HRD-134` — Layar `FE-HRD-164` — Lembar Kerja Kalkulasi Gaji, Komponen Rincian & Filter Unit/Profesi (`/hr/payroll/runs/[id]/worksheet`)
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-134.md](../task/report/frontend/FE-HRD-134.md).
* **Outcome**: Halaman lembar kerja utama penggajian `/hr/payroll/runs/[id]/worksheet` untuk mengeksekusi kalkulasi batch, memfilter data pegawai berdasarkan unit kerja instalasi dan kelompok profesi medis (Dokter Spesialis, Dokter Umum, Perawat, Staf Non-Klinis), menyajikan tabel komponen gaji (pokok, tunjangan medis, lembur PP 35, premi malam, potongan BPJS, PPh 21 TER, pinjaman koperasi), serta indikator badge peringatan variansi anomali.
* **Requirement/decision**: `HRD-DEC-123`, `HRD-DEC-124`, `03-frontend-architecture.md` 19.2.1.
* **Kontrak**: API Contract `v14` `POST /calculate` & `GET /runs/{id}/items`.
* **Kewenangan UI**: `MANDATORY` layout spreadsheet-like table Quilvian.
* **Reuse**: `DataTable`, `BaseButton`, `StatusBadge`, `DropdownFilter`, `SearchInput`.
* **Cakupan**: `src/app/hr/payroll/runs/[id]/worksheet/page.jsx`.
* **Dependency**: `FE-HRD-133`
* **Acceptance criteria**:
  1. Tombol "Hitung Seluruh Pegawai" memicu proses kalkulasi backend dengan progress indicator visual.
  2. Tabel menyajikan rincian penghasilan kotor, total potongan, dan gaji bersih (*take-home pay*) per pegawai.
  3. Baris pegawai dengan variansi net pay $\ge 20\%$ diberi highlight warna kuning dan icon peringatan.
  4. Tombol aksi membuka drawer investigasi variansi atau modal rincian slip gaji individual.
* **Bukti verifikasi**: UAT skenario `UAT-PAY-02` dan verifikasi akurasi tampilan angka rupiah.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Lembar kerja kalkulasi gaji berfungsi interaktif, cepat, dan transparan.

### 🟡 `FE-HRD-135` — Layar `FE-HRD-165` — Drawer Investigasi Anomali Variansi & Penginputan Justifikasi Audit (`/hr/payroll/runs/[id]/variances`)
* **Slice / MVP**: `S-B5` / `MVP-31`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Komponen atau layarnya tidak dapat dibuka dari rute mana pun. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R51`. Laporan lama: [FE-HRD-135.md](../task/report/frontend/FE-HRD-135.md).
* **Outcome**: Drawer samping / tab audit `/hr/payroll/runs/[id]/variances` bagi Payroll Officer untuk menginvestigasi seluruh tiket anomali deviasi gaji yang terdeteksi (*Variance Alerts*), menyajikan perbandingan *side-by-side* rincian slip gaji bulan lalu vs bulan berjalan, komponen pemicu lonjakan/penurunan, serta form penginputan justifikasi audit wajib (minimum 20 karakter) untuk menyelesaikan alert.
* **Requirement/decision**: `HRD-DEC-124`, `03-frontend-architecture.md` 19.2.1.
* **Kontrak**: API Contract `v14` `GET /variances` & `POST /variances/{id}/resolve`, `VR-PAY-14`, `VR-PAY-15`.
* **Kewenangan UI**: `MANDATORY` drawer investigasi anomali finansial.
* **Reuse**: `BaseDrawer`, `@/components/features/base-features/base-form-control`, `BaseButton`, `StatusBadge`.
* **Cakupan**: `src/app/hr/payroll/runs/[id]/variances/page.jsx`, `src/components/payroll/VarianceResolutionDrawer.jsx`.
* **Dependency**: `FE-HRD-134`, `BE-HRD-156` [BE]
* **Acceptance criteria**:
  1. Drawer menampilkan grafik/tabel komparasi komponen pemicu deviasi (misal: lonjakan lembur cito atau penyesuaian rapel).
  2. Input justifikasi memvalidasi panjang minimum 20 karakter; tombol simpan dinonaktifkan jika teks terlalu pendek.
  3. Penyelesaian tiket mengubah status badge menjadi `Resolved` dan mengurangi counter anomali pending di lembar kerja.
* **Bukti verifikasi**: UAT skenario `UAT-PAY-03` dan verifikasi penolakan karakter < 20.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Drawer investigasi variansi memfasilitasi audit anomali secara tuntas dan terdokumentasi.

### `FE-HRD-136` — Komponen Modal Detail Rincian Gaji Pegawai & Komparasi Slip Gaji vs Histori Bulan Lalu (`PayrollEmployeeDetailModal`)
* **Slice / MVP**: `S-B5` / `MVP-31`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-136.md](../task/report/frontend/FE-HRD-136.md).
* **Outcome**: Komponen dialog modal pop-up yang menyajikan pratinjau slip gaji lengkap seorang pegawai pada batch berjalan, rincian per komponen tunjangan fungsional medis/shift, formula potongan pajak TER dan BPJS, perbandingan delta nominal dengan bulan sebelumnya, serta riwayat catatan investigasi khusus pegawai tersebut.
* **Requirement/decision**: `HRD-DEC-123`, `HRD-DEC-124`, `03-frontend-architecture.md` 19.2.2.
* **Kontrak**: API Contract `v14` `GET /runs/{id}/items/{detailId}`.
* **Kewenangan UI**: `MANDATORY` modal pratinjau slip gaji internal HR.
* **Reuse**: `BaseModal`, `Badge`, `InformationAlert`, format moneter IDR.
* **Cakupan**: `src/components/payroll/PayrollEmployeeDetailModal.jsx`.
* **Dependency**: `FE-HRD-135`
* **Acceptance criteria**:
  1. Modal menyajikan struktur slip gaji profesional (Penghasilan Bruto, Potongan Resmi, Penghasilan Neto).
  2. Kolom selisih (+/-) menampilkan perbandingan nominal terhadap take-home pay bulan lalu.
  3. Menampilkan riwayat tiket variansi yang melekat pada pegawai tersebut bila ada.
* **Bukti verifikasi**: Uji pratinjau modal detail pada berbagai profil nakes dan staf RS.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Modal detail rincian gaji menyajikan visualisasi data remunerasi yang jelas dan mudah dipahami.

### `FE-HRD-137` — Layanan API service frontend & Redux actions untuk alur otorisasi 3-jenjang & anti-self-approval
* **Slice / MVP**: `S-B5` / `MVP-31`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-137.md](../task/report/frontend/FE-HRD-137.md).
* **Outcome**: Berkas service frontend dan Redux actions untuk alur persetujuan berjenjang penggajian (`[Tags("HR Payroll - Approvals")]`), validasi pemisahan peran (*Segregation of Duties*), deteksi hak otorisasi pengguna aktif, dan penanganan pemblokiran approval diri sendiri (*Anti-Self-Approval*).
* **Requirement/decision**: `HRD-DEC-125`, `03-frontend-architecture.md` 19.4.
* **Kontrak**: API Contract `v14` Bagian 20 (Tag 3), `VR-PAY-16` s.d. `VR-PAY-20`.
* **Kewenangan UI**: Service authorization layer.
* **Reuse**: Redux dispatch dan session peran pengguna.
* **Cakupan**: `src/services/payroll/payrollApprovalService.js`.
* **Dependency**: `FE-HRD-136`, `BE-HRD-157` [BE]
* **Acceptance criteria**:
  1. Fungsi `submitToHrReview`, `approveHrReview`, `approveFinalDirector`, `rejectPayrollRun` terdefinisi lengkap.
  2. Sistem frontend menyembunyikan tombol persetujuan dan menampilkan banner informasi bila HR Manager mendeteksi baris gajinya sendiri dalam batch.
  3. Alasan penolakan wajib diinput saat aksi `reject` dipilih.
* **Bukti verifikasi**: Uji unit aksi approval dan simulasi pencegahan anti-self-approval.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Service alur otorisasi beroperasi aman dan menegakkan pemisahan wewenang.

### `FE-HRD-138` — Layar `FE-HRD-166` — Dasbor Otorisasi 3-Jenjang Penggajian & Rekapitulasi Eksekutif Finansial (`/hr/payroll/approvals`)
* **Slice / MVP**: `S-B5` / `MVP-31`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-138.md](../task/report/frontend/FE-HRD-138.md).
* **Outcome**: Halaman persetujuan eksekutif `/hr/payroll/approvals` bagi HR Manager dan Direktur SDM/Keuangan untuk meninjau batch penggajian yang menunggu otorisasi (`WaitingHrReview` / `WaitingFinalApproval`), menyajikan rekapitulasi beban gaji per unit kerja dan kelompok tenaga medis, status penyelesaian seluruh tiket variansi (wajib 0 pending), riwayat stempel otorisasi berjenjang, serta tombol persetujuan final (*Final Sign-off*).
* **Requirement/decision**: `HRD-DEC-125`, `03-frontend-architecture.md` 19.2.1.
* **Kontrak**: API Contract `v14` `GET /approvals` & `POST /approvals/final`, `VR-PAY-20`.
* **Kewenangan UI**: `MANDATORY` dasbor persetujuan eksekutif.
* **Reuse**: `Card`, `StatusBadge`, `BaseButton`, `InformationAlert`, `TimelineAuditView`.
* **Cakupan**: `src/app/hr/payroll/approvals/page.jsx`.
* **Dependency**: `FE-HRD-137`
* **Acceptance criteria**:
  1. Menampilkan ringkasan eksekutif total payroll, total pajak PPh 21, dan total iuran BPJS rumah sakit.
  2. Tombol "Setujui Final & Kunci Permanen" hanya aktif bagi role Direktur SDM/Keuangan.
  3. Timeline audit menampilkan jejak persetujuan Tier 1 (Officer), Tier 2 (HR Manager), dan Tier 3 (Direktur).
* **Bukti verifikasi**: UAT skenario `UAT-PAY-04` dan verifikasi alur otorisasi multi-role.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar otorisasi eksekutif beroperasi sempurna dan memfasilitasi audit persetujuan transparan.

### `FE-HRD-139` — Komponen Modal Konfirmasi Penguncian Immutable & Trigger Serah Terima Finance SHA-256 (`PayrollFinanceHandoffModal`)
* **Slice / MVP**: `S-B5` / `MVP-32`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-139.md](../task/report/frontend/FE-HRD-139.md).
* **Outcome**: Komponen dialog modal konfirmasi pembekuan permanen batch penggajian (*Immutable Lock*), visualisasi verifikasi nilai checksum SHA-256 yang dibangkitkan, ringkasan rekapitulasi rekening bank transfer payroll, serta tombol eksekusi serah terima berkas instruksi pembayaran ke Modul Finance (`INT-HR-53`).
* **Requirement/decision**: `HRD-DEC-126`, `03-frontend-architecture.md` 19.2.2.
* **Kontrak**: API Contract `v14` `POST /handoffs/dispatch`, `VR-PAY-21`.
* **Kewenangan UI**: `MANDATORY` modal peringatan kritis transaksi finansial.
* **Reuse**: `BaseModal`, `InformationAlert`, `BaseButton`, `CodeSnippet` (penampil hash SHA-256).
* **Cakupan**: `src/components/payroll/PayrollFinanceHandoffModal.jsx`.
* **Dependency**: `FE-HRD-138`, `BE-HRD-158` [BE]
* **Acceptance criteria**:
  1. Modal menampilkan peringatan keras bahwa setelah diserahkan ke Finance, data batch terkunci mutlak dan tidak dapat diedit kembali oleh HR.
  2. Menampilkan nilai hash SHA-256 integritas payload dan jumlah total nominal uang yang diserahterimakan.
  3. Konfirmasi eksekusi sukses mengubah status batch menjadi `HandoffDispatched` dan mengarahkan ke dasbor pemantauan handoff.
* **Bukti verifikasi**: UAT skenario `UAT-PAY-05` dan pengujian visual verifikasi checksum.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Modal konfirmasi serah terima Finance menjamin kepatuhan gembok data immutable.

### `FE-HRD-140` — Dasbor Pemantauan Status Serah Terima Finance & Webhook Reconciler (`/hr/payroll/handoffs`)
* **Slice / MVP**: `S-B5` / `MVP-32`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-140.md](../task/report/frontend/FE-HRD-140.md).
* **Outcome**: Halaman pemantauan `/hr/payroll/handoffs` bagi tim payroll untuk memantau status serah terima berkas di modul Keuangan secara langsung: status transmisi (`Dispatched`), konfirmasi pencairan bank (`PaymentConfirmed`), atau sinyal penolakan berkas (`PaymentRejected`) beserta detail catatan koreksi dari Finance.
* **Requirement/decision**: `HRD-DEC-126`, `03-frontend-architecture.md` 19.2.1.
* **Kontrak**: API Contract `v14` `GET /handoffs`, Integration Contract `INT-HR-54`.
* **Kewenangan UI**: `MANDATORY` monitoring antarmuka integrasi.
* **Reuse**: `DataTable`, `StatusBadge`, `BaseButton`, `InformationAlert`.
* **Cakupan**: `src/app/hr/payroll/handoffs/page.jsx`.
* **Dependency**: `FE-HRD-139`
* **Acceptance criteria**:
  1. Menampilkan daftar paket serah terima keuangan, nomor batch, timestamp pengiriman, dan status penerimaan.
  2. Bila status berubah menjadi `PaymentConfirmed`, sistem menampilkan indikator sukses dan mengonfirmasi slip gaji siap rilis tgl 25.
  3. Bila status ditolak (`PaymentRejected`), sistem menyajikan alasan penolakan dari Finance dan menyediakan tombol pembukaan gembok koreksi (*Unlock for Correction*).
* **Bukti verifikasi**: UAT skenario `UAT-PAY-05` dan simulasi penerimaan callback sukses/gagal.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Dasbor pemantauan handoff memberikan visibilitas penuh atas status pencairan gaji di Keuangan.

### `FE-HRD-141` — Layar `FE-HRD-167` — Portal Slip Gaji Mandiri Pegawai (ESS) & Keypad Proteksi PIN Sekunder 6-Digit (`/ess/payslips`)
* **Slice / MVP**: `S-B5` / `MVP-32`
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-141.md](../task/report/frontend/FE-HRD-141.md).
* **Outcome**: Halaman portal mandiri pegawai `/ess/payslips` pada modul Employee Self-Service (ESS) untuk melihat dan mengunduh slip gaji bulanan, dilengkapi komponen keypad virtual input PIN sekunder 6-digit rahasia, penanganan pembatasan salah PIN (maks 3 kali dengan penalti lockout 30 menit), pratinjau slip gaji interaktif, dan tombol unduh berkas PDF resmi terenkripsi password tanggal lahir.
* **Requirement/decision**: `HRD-DEC-127`, `HRD-ASM-30`, `03-frontend-architecture.md` 19.2.1.
* **Kontrak**: API Contract `v14` `[Tags("HR Payroll - Self-Service Payslips")]`, `VR-PAY-22`, `VR-PAY-24`.
* **Kewenangan UI**: `MANDATORY` portal ESS mobile-friendly & secure pin pad.
* **Reuse**: `Card`, `BaseButton`, `PinInputControl`, `InformationAlert`.
* **Cakupan**: `src/app/ess/payslips/page.jsx`, `src/components/payroll/PinVerificationModal.jsx`.
* **Dependency**: `FE-HRD-140`, `BE-HRD-159` [BE]
* **Acceptance criteria**:
  1. Slip gaji hanya dapat diakses per tanggal 25 bulan berjalan atau setelah status batch `Completed`.
  2. Akses rincian nominal gaji tertutup sebelum PIN sekunder 6-digit berhasil divalidasi.
  3. Kegagalan input PIN 3 kali berturut-turut memicu penguncian keypad selama 30 menit dengan countdown visual.
  4. Unduhan dokumen PDF slip gaji menghasilkan berkas resmi ber-password kombinasi tanggal lahir pegawai.
* **Bukti verifikasi**: Test case `AT-HRD-PAY-20`, UAT skenario `UAT-PAY-05`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead bersama ESS Team.
* **Definition of Done**: Portal slip gaji mandiri ESS beroperasi aman, responsif pada ponsel, dan menjaga konfidensialitas remunerasi.

### 🟡 `FE-HRD-142` — Layar Pengaturan Parameter Penggajian RS, Batas Plafon BPJS & Threshold Variansi (`/hr/payroll/settings`)
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Komponen atau layarnya tidak dapat dibuka dari rute mana pun. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R51`. Laporan lama: [FE-HRD-142.md](../task/report/frontend/FE-HRD-142.md).
* **Outcome**: Halaman konfigurasi `/hr/payroll/settings` bagi HR Administrator untuk menyesuaikan parameter operasional penggajian rumah sakit: batas upah tertinggi BPJS Ketenagakerjaan JP, batas upah maksimal BPJS Kesehatan, nominal baku premi shift malam dinas, ambang deviasi persentase variansi (default 20%), ambang deviasi nominal rupiah (default Rp 2.000.000), serta tanggal cut-off bulanan (default tgl 20).
* **Requirement/decision**: `HRD-DEC-122`, `HRD-DEC-124`, `03-frontend-architecture.md` 19.2.1.
* **Kontrak**: API Contract `v14` `[Tags("HR Payroll - Settings")]`.
* **Kewenangan UI**: `MANDATORY` form pengaturan modul HR.
* **Reuse**: `@/components/features/base-features/base-form-control`, `BaseButton`, `InformationAlert`.
* **Cakupan**: `src/app/hr/payroll/settings/page.jsx`.
* **Dependency**: `FE-HRD-131`
* **Acceptance criteria**:
  1. Form pengaturan menampilkan nilai parameter aktif dengan validasi rentang nilai wajar.
  2. Dilengkapi penjelasan tooltip regulasi ketenagakerjaan dan batas plafon resmi pemerintah.
  3. Aksi simpan mencatat riwayat audit log perubahan parameter konfigurasi.
* **Bukti verifikasi**: Uji simpan parameter penggajian dan verifikasi persistensi nilai baru.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar pengaturan payroll tersimpan rapi dan mengendalikan parameter sistem secara fleksibel.

### `FE-HRD-143` — Verifikasi ujung-ke-ujung antarmuka, linter CI/CD, build Next.js, dan hak akses peran Slice `S-B5`
* **Slice / MVP**: `S-B5` / Verifikasi
* **Status**: Selesai (✅ PASS). Laporan: [FE-HRD-143.md](../task/report/frontend/FE-HRD-143.md).
* **Outcome**: Seluruh 6 layar baru `S-B5` (`FE-HRD-162` s.d. `FE-HRD-167`), drawer investigasi anomali variansi, portal slip gaji ESS ber-PIN, dan modal serah terima Finance terverifikasi dapat diakses dari sidebar menu navigasi, hak akses peran teruji (Staf ESS vs Payroll Officer vs HR Manager vs Direktur SDM/Keuangan), dan lolos audit ketat linter serta kompilasi produksi Next.js.
* **Requirement/decision**: `03-frontend-architecture.md` 19.1, 19.2; `04-prd-to-mvp.md` 28.5 (DoD `S-B5`).
* **Kontrak**: Seluruh kontrak `v14` bagian `S-B5`.
* **Kewenangan UI**: Kepatuhan penuh standar desain antarmuka Quilvian.
* **Reuse**: Pola audit verifikasi `FE-HRD-129` / `FE-HRD-115`.
* **Cakupan**: Audit menu navigasi sidebar, uji loading/empty/error state, `npm run lint:errors`, `npm run build`.
* **Dependency**: `FE-HRD-133`, `FE-HRD-134`, `FE-HRD-135`, `FE-HRD-138`, `FE-HRD-141`
* **Acceptance criteria**:
  1. Menu Penggajian tampil di sidebar sesuai hak akses peran pengguna aktif.
  2. Staf umum hanya dapat mengakses portal slip gaji mandiri ESS pribadinya.
  3. Dasbor kalkulasi dan drawer variansi hanya dapat diakses Payroll Officer dan HR Manager.
  4. `npm run lint:errors` dan `npm run build` berhasil 0 error kompilasi.
* **Bukti verifikasi**: Tangkapan layar navigasi sidebar per peran, laporan lint dan build produksi bersih.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead bersama QA.
* **Definition of Done**: Seluruh antarmuka penggajian teruji, bebas tautan mati (broken link), dan siap pakai.


### ✅ `FE-HRD-144` — Service API client, adapter TypeScript/Axios, Redux slice & state management restitusi dan potongan
* **Slice / MVP**: `S-B6` / `MVP-33`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Modul komunikasi data frontend terpadu mencakup service client Axios, transformer data DTO backend ke camelCase, Redux Toolkit slice untuk mengelola state jadwal angsuran, klaim restitusi ESS, antrean approval, dan cache kuota benefit tahunan.
* **Requirement/decision**: `03-frontend-architecture.md` Bagian 20.1, API Contract `v15` Bagian 21.
* **Kontrak**: API Contract `v15` 4 tag grup Swagger.
* **Kewenangan UI**: `MANDATORY` fondasi data frontend.
* **Reuse**: Axios client instance `@/lib/axios`, Redux store root reducer `@/lib/redux/store`.
* **Cakupan**: `src/lib/services/hr/deduction-service.js`, `src/lib/services/hr/restitution-service.js`, `src/lib/redux/slices/restitution-deduction-slice.js`.
* **Dependency**: `BE-HRD-166` [BE], `BE-HRD-169` [BE]
* **Acceptance criteria**:
  1. Service client mencakup seluruh pemanggilan API untuk 4 tag grup Swagger (jadwal cicilan, klaim restitusi, voucher kasir, pengaturan).
  2. Redux slice menampung state terisolasi: `schedules`, `claims`, `approvals`, `simulationResult`, `settings`.
  3. Penanganan error terpusat melempar pesan validasi ramah pengguna dari respon backend 400/409/413.
  4. Mendukung optimistik UI update untuk perubahan status persetujuan berkas.
* **Bukti verifikasi**: Unit test Redux slice action & reducer, mock API call testing.
* **Risiko & pemilik**: Pemilik: Frontend HR Lead.
* **Definition of Done**: Service data dan Redux slice terdaftar aktif dan siap dikonsumsi seluruh komponen UI S-B6.

### ✅ `FE-HRD-145` — Desain token, status badge, format mata uang & indikator rasio upah 50%
* **Slice / MVP**: `S-B6` / `MVP-33`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Kumpulan komponen visual pendukung mencakup status badge siklus klaim restitusi, badge prioritas cicilan 1-4, format mata uang Rupiah otomatis, progress bar kuota plafon tahunan, serta indikator visual penegakan hard-cap 50% PP 36/2021.
* **Requirement/decision**: `03-frontend-architecture.md` Bagian 20.2, State Transition Matrix `v15` Bagian 19.
* **Kontrak**: State Transition Matrix `v15` `RestitutionClaimStatus`, `DeductionScheduleStatus`.
* **Kewenangan UI**: Kepatuhan penuh desain sistem Quilvian.
* **Reuse**: `@/components/features/base-features/base-badge`, design tokens warna Quilvian.
* **Cakupan**: `src/components/features/hr/deductions/StatusBadges.jsx`, `src/components/features/hr/deductions/CapProgressBar.jsx`.
* **Dependency**: `FE-HRD-144`
* **Acceptance criteria**:
  1. Status badge merender seluruh 11 status klaim restitusi dengan padanan warna semantik yang tepat.
  2. Indikator rasio upah kotor menampilkan gradasi warna: Hijau (<40%), Oranye (40-49%), dan Merah Tebal (>=50%).
  3. Formatter mata uang Rupiah (`formatRupiah`) memformat angka tanpa toleransi pembulatan yang salah.
  4. Progress bar kuota plafon menampilkan rasio kuota terpakai vs kuota tersisa secara proporsional.
* **Bukti verifikasi**: Storybook / visual snapshot rendering badge dan progress bar.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Specialist.
* **Definition of Done**: Komponen visual token dan status badge teruji dan dapat digunakan ulang di seluruh layar modul.

### ✅ `FE-HRD-146` — Layar `FE-HRD-168` — Dasbor Utama Restitusi & Pemotongan Gaji (Bagian 1: Ringkasan & Metrik)
* **Slice / MVP**: `S-B6` / `MVP-33`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Halaman dasbor eksekutif `/corporate/hrd/payroll/deductions` yang menampilkan ringkasan metrik total potongan terjadwal, nominal terealisasi pada slip gaji berjalan, total tagihan yang ditunda (*deferred carry-over*), serta antrean klaim restitusi menunggu approval.
* **Requirement/decision**: `HRD-DEC-128`, `03-frontend-architecture.md` Bagian 20.2.1.
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Installment Schedules")]`.
* **Kewenangan UI**: `MANDATORY` tata letak kartu metrik eksekutif.
* **Reuse**: BaseCard, MetricWidget, InformationAlert.
* **Cakupan**: `src/app/corporate/hrd/payroll/deductions/page.jsx`, `src/components/features/hr/deductions/DashboardMetrics.jsx`.
* **Dependency**: `FE-HRD-144`, `FE-HRD-145`
* **Acceptance criteria**:
  1. Halaman memuat 4 kartu ringkasan metrik utama dengan pembaruan data real-time dari API.
  2. Menampilkan banner status siklus jadwal cicilan aktif (terkunci per tanggal 20).
  3. Menyediakan tombol aksi utama: *"Tarik Snapshot Keuangan"* dan *"Simulasi Plafon 50%"*.
  4. Menampilkan indikator loading skeleton saat data metrik sedang ditarik dari server.
* **Bukti verifikasi**: Uji rendering layar dasbor, snapshot test tampilan kartu metrik.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Halaman dasbor metrik potongan berfungsi penuh dan mematuhi hak akses `HR_Deduction_View`.

### ✅ `FE-HRD-147` — Layar `FE-HRD-168` — Tabel Peringatan Rasio Potongan Gaji >40% & Trigger Sinkronisasi Finance (Bagian 2)
* **Slice / MVP**: `S-B6` / `MVP-33`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Tabel pemantauan pegawai berisiko yang memiliki total potongan cicilan mendekati batas regulasi (>40% Gross Pay), daftar cicilan yang mengalami penundaan (*carry-over*), serta modal dialog konfirmasi sinkronisasi data H-3 dari modul Keuangan.
* **Requirement/decision**: `HRD-DEC-128`, `HRD-DEC-129`, `03-frontend-architecture.md` Bagian 20.2.1 & 20.3.
* **Kontrak**: API Contract `v15` `POST /deductions/schedules/sync-finance`, Validation Matrix `v15` `VR-DED-01`.
* **Kewenangan UI**: `MANDATORY` tabel peringatan dini risiko penggajian.
* **Reuse**: BaseTable, SearchInput, PaginationControl, ModalFinanceSyncConfirm.
* **Cakupan**: `src/components/features/hr/deductions/HighDeductionRatioTable.jsx`, `src/components/features/hr/deductions/FinanceSyncModal.jsx`.
* **Dependency**: `FE-HRD-146`
* **Acceptance criteria**:
  1. Tabel menampilkan daftar staf dengan rasio potongan >40% lengkap dengan badge status plafon (Aman / Mentok 50%).
  2. Dialog konfirmasi sinkronisasi menampilkan rekapitulasi data cicilan baru yang masuk dari Finance.
  3. Tombol aksi detail membuka rincian kontrak pinjaman pegawai bersangkutan.
  4. Filter tab memungkinkan pergantian tampilan antara daftar peringatan rasio dan daftar tagihan tertunda (*Deferred List*).
* **Bukti verifikasi**: Pengujian tabel interaktif, filter pagination, verifikasi eksekusi sinkronisasi Finance.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Specialist.
* **Definition of Done**: Tabel peringatan rasio potong beroperasi lancar dan memfasilitasi pencegahan over-deduction.

### ✅ `FE-HRD-148` — Layar `FE-HRD-170` — Lembar Kerja Jadwal Angsuran Master Pinjaman (Bagian 1)
* **Slice / MVP**: `S-B6` / `MVP-33`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Lembar kerja teknis `/corporate/hrd/payroll/deductions/schedules` bagi HR Payroll Specialist untuk mengelola master angsuran pinjaman dari Finance, melihat sisa pokok pinjaman, tenor terbayar, dan mengajukan jeda cicilan resmi.
* **Requirement/decision**: `HRD-DEC-128`, `HRD-DEC-129`, `03-frontend-architecture.md` Bagian 20.2.3.
* **Kontrak**: API Contract `v15` `GET /deductions/schedules`, `PUT /deductions/schedules/{id}/pause`.
* **Kewenangan UI**: `MANDATORY` lembar kerja manajemen pinjaman.
* **Reuse**: BaseTable, FilterDropdown, BaseModal, StatusBadge.
* **Cakupan**: `src/app/corporate/hrd/payroll/deductions/schedules/page.jsx`, `src/components/features/hr/deductions/DeductionScheduleTable.jsx`.
* **Dependency**: `FE-HRD-144`, `FE-HRD-145`
* **Acceptance criteria**:
  1. Menampilkan seluruh jadwal cicilan aktif dengan kolom NIP, nama pegawai, kategori potongan, prioritas 1-4, dan sisa saldo.
  2. Fitur pencarian cepat berdasarkan nama staf atau nomor kontrak pinjaman Finance (`external_reference_id`).
  3. Modal aksi jeda cicilan memvalidasi input alasan wajib diisi dan pilihan durasi maksimal 3 bulan (`VR-DED-20`).
  4. Mendukung ekspor data jadwal angsuran ke format spreadsheet Excel/CSV.
* **Bukti verifikasi**: Uji pencarian, filter kategori, aksi jeda cicilan, export excel.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Lembar kerja angsuran pinjaman berjalan responsif dan melayani manipulasi data cicilan secara aman.

### ✅ `FE-HRD-149` — Layar `FE-HRD-170` — Modal Simulasi Penegakan Hard-Cap 50% & Visualisasi Pemangkasan Berjenjang (Bagian 2)
* **Slice / MVP**: `S-B6` / `MVP-33`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Modal simulasi interaktif yang memproyeksikan eksekusi pemotongan gaji berjenjang terhadap proyeksi upah kotor pegawai, memvisualisasikan komponen yang terpotong penuh, terpotong parsial, dan yang ditunda akibat batas 50% PP 36/2021.
* **Requirement/decision**: `HRD-DEC-128`, `03-frontend-architecture.md` Bagian 20.2.3.
* **Kontrak**: API Contract `v15` `POST /deductions/schedules/simulate-cap`, Validation Matrix `v15` `VR-DED-01` s.d. `VR-DED-03`.
* **Kewenangan UI**: `MANDATORY` visualisasi perhitungan simulasi payroll.
* **Reuse**: BaseModal, BaseButton, ProgressBar, Card.
* **Cakupan**: `src/components/features/hr/deductions/SimulationCapModal.jsx`.
* **Dependency**: `FE-HRD-148`
* **Acceptance criteria**:
  1. Modal menerima input ID pegawai dan proyeksi Gross Pay untuk menghitung batas maksimal 50%.
  2. Menampilkan rincian eksekusi Prioritas 1 (Ganti Rugi), 2 (Kasbon), 3 (Koperasi), dan 4 (Paguyuban) secara runtut.
  3. Tagihan yang melebihi kuota ditandai visual dengan badge oranye `PartiallyDeducted` dan `DeferredDueToSalaryCap`.
  4. Menampilkan teks kesimpulan: *"Total tagihan ditunda Rp X otomatis dialihkan ke siklus penggajian bulan depan."*
* **Bukti verifikasi**: Uji interaksi simulasi perhitungan batas 50%, verifikasi skenario UAT `UAT-DED-01`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Specialist.
* **Definition of Done**: Modal simulasi hard-cap 50% berjalan interaktif dan memudahkan verifikasi batas sebelum payroll final dieksekusi.

### ✅ `FE-HRD-150` — Layar `FE-HRD-169` — Pelacak Kuota Plafon Tahunan Pegawai di Portal ESS (Bagian 1)
* **Slice / MVP**: `S-B6` / `MVP-34`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Komponen portal ESS `/ess/restitution-claims` yang menampilkan pelacak kuota saldo benefit tahunan staf (kacamata kerja, faskes luar darurat) secara transparan lengkap dengan riwayat klaim yang pernah diajukan.
* **Requirement/decision**: `HRD-DEC-130`, `03-frontend-architecture.md` Bagian 20.2.2.
* **Kontrak**: API Contract `v15` `GET /restitutions/claims/my-claims`.
* **Kewenangan UI**: `MANDATORY` antarmuka portal layanan mandiri staf (ESS).
* **Reuse**: BaseCard, ProgressBar, StatusBadge.
* **Cakupan**: `src/app/ess/restitution-claims/page.jsx`, `src/components/features/hr/restitution/EssQuotaTracker.jsx`.
* **Dependency**: `FE-HRD-144`, `FE-HRD-145`
* **Acceptance criteria**:
  1. Menampilkan progress bar kuota kacamata kerja (plafon tahunan, nominal terpakai, dan sisa kuota tersedia).
  2. Menampilkan progress bar kuota faskes luar darurat sesuai golongan pemohon.
  3. Tabel histori menampilkan daftar seluruh klaim staf, tanggal pengeluaran, nominal disetujui, dan badge status terkini.
  4. Dilengkapi tombol aksi *"Ajukan Klaim Baru"* yang memicu formulir input pengajuan.
* **Bukti verifikasi**: Uji rendering ESS kuota tracker per grade level staf, snapshot test.
* **Risiko & pemilik**: Pemilik: Frontend ESS UI Lead.
* **Definition of Done**: Layar pelacak kuota plafon ESS aktif dan menampilkan data kuota yang sinkron dengan backend.

### ✅ `FE-HRD-151` — Layar `FE-HRD-169` — Formulir Pengajuan Klaim Restitusi Mandiri di ESS & Dropzone Kuitansi (Bagian 2)
* **Slice / MVP**: `S-B6` / `MVP-34`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Formulir pengajuan klaim mandiri digital bagi staf rumah sakit untuk memilih kategori manfaat, mengisi nominal nota, mengunggah foto kuitansi/struk asli dan resume medis via dropzone interaktif, serta opsi klaim darurat medis nakes cito.
* **Requirement/decision**: `HRD-DEC-130`, `HRD-DEC-133`, `03-frontend-architecture.md` Bagian 20.2.2.
* **Kontrak**: API Contract `v15` `POST /restitutions/claims`, Validation Matrix `v15` `VR-DED-09`, `VR-DED-10`, `VR-DED-15`.
* **Kewenangan UI**: `MANDATORY` form pengajuan mandiri ESS.
* **Reuse**: `@/components/features/base-features/base-form-control`, BaseButton, FileDropzone.
* **Cakupan**: `src/components/features/hr/restitution/EssClaimForm.jsx`.
* **Dependency**: `FE-HRD-150`
* **Acceptance criteria**:
  1. Form memvalidasi input tanggal nota maksimal mundur 60 hari dari hari ini (`VR-DED-09`).
  2. Validasi sisi klien menolak submit jika nominal klaim melebihi sisa kuota plafon yang tersedia (`VR-DED-10`).
  3. File dropzone mendukung pratinjau instan foto kuitansi dan menolak berkas non-PDF/JPG atau ukuran > 5MB (`VR-DED-15`).
  4. Staf medis IGD dapat mencentang opsi darurat cito *"Insiden Pajanan Medis Cito (Pencairan Kasir 24/7)"*.
* **Bukti verifikasi**: Uji form submit di ESS, boundary test tanggal nota > 60 hari, verifikasi skenario UAT `UAT-DED-03`.
* **Risiko & pemilik**: Pemilik: Frontend ESS UI Specialist.
* **Definition of Done**: Form pengajuan klaim ESS berfungsi mulus dan mengunggah berkas nota asli secara aman.

### 🟡 `FE-HRD-152` — Layar `FE-HRD-172` — Kotak Antrean Verifikasi Persetujuan Bertingkat 3-Tier (Bagian 1)
* **Slice / MVP**: `S-B6` / `MVP-34`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Komponen atau layarnya tidak dapat dibuka dari rute mana pun. 4 dari 4 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R40`. Laporan lama: [FE-HRD-152.md](../task/report/frontend/FE-HRD-152.md).
* **Outcome**: Panel persetujuan terpadu `/corporate/hrd/payroll/deductions/approvals` bagi Kepala Ruangan, HR C&B, dan Manajer SDM untuk meninjau berkas klaim bawahan, memverifikasi kuitansi fisik, menyesuaikan nominal disetujui, dan mengeksekusi approval tanpa celah self-approval.
* **Requirement/decision**: `HRD-DEC-130`, `03-frontend-architecture.md` Bagian 20.2.5.
* **Kontrak**: API Contract `v15` `GET /restitutions/claims/pending-approval`, `POST /restitutions/claims/{id}/approve`.
* **Kewenangan UI**: `MANDATORY` antarmuka persetujuan manajerial.
* **Reuse**: ApprovalInboxWidget, BaseButton, StatusBadge.
* **Cakupan**: `src/app/corporate/hrd/payroll/deductions/approvals/page.jsx`, `src/components/features/hr/restitution/ApprovalQueueList.jsx`.
* **Dependency**: `FE-HRD-144`, `FE-HRD-145`
* **Acceptance criteria**:
  1. Antrean menampilkan berkas klaim sesuai peran otorisasi aktif (Tahap Supervisor, Tahap HR C&B, Tahap Manajer SDM).
  2. Tombol persetujuan dinonaktifkan secara otomatis jika pemohon klaim adalah akun yang sedang login (*Anti-Self-Approval*, `VR-DED-08`).
  3. Petugas HR C&B dapat menyesuaikan kolom *Approved Amount* sesuai verifikasi kuitansi fisik (`VR-DED-23`).
  4. Penolakan berkas klaim memunculkan dialog wajib isi catatan alasan penolakan.
* **Bukti verifikasi**: Uji login multi-peran, verifikasi pemblokiran self-approval, verifikasi skenario UAT `UAT-DED-03`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Panel persetujuan 3-tier beroperasi ketat dan menjamin tata kelola otorisasi yang sah.

### 🟡 `FE-HRD-153` — Komponen `ModalClaimDocumentPreview` — Pratinjau Interaktif Zoom Berkas Nota Kuitansi & Resume Medis
* **Slice / MVP**: `S-B6` / `MVP-34`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Komponen atau layarnya tidak dapat dibuka dari rute mana pun. 4 dari 4 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R40`. Laporan lama: [FE-HRD-153.md](../task/report/frontend/FE-HRD-153.md).
* **Outcome**: Dialog pratinjau dokumen kuitansi interaktif yang dilengkapi fitur perbesaran (*zoom in/out*), rotasi gambar 90 derajat, dan filter kontras tinggi agar petugas HR C&B dapat membaca nomor nota, tanggal, dan cap stempel faskes luar secara jelas.
* **Requirement/decision**: `03-frontend-architecture.md` Bagian 20.3.
* **Kontrak**: API Contract `v15` `receipt_attachment_url`, `medical_resume_attachment_url`.
* **Kewenangan UI**: Komponen utilitas pemeriksaan dokumen medis/nota.
* **Reuse**: BaseModal, React-Image-Viewer / canvas zoom utility.
* **Cakupan**: `src/components/features/hr/restitution/ModalClaimDocumentPreview.jsx`.
* **Dependency**: `FE-HRD-152`
* **Acceptance criteria**:
  1. Modal menampilkan dokumen kuitansi dalam resolusi penuh tanpa distorsi gambar.
  2. Kontrol zoom (50% s.d. 300%) dan rotasi (90°, 180°, 270°) berfungsi responsif via tombol maupun mouse wheel.
  3. Tab navigasi memungkinkan perpindahan instan antara berkas kuitansi asli dan berkas resume medis dokter.
  4. Menampilkan watermark status verifikasi pada pratinjau dokumen.
* **Bukti verifikasi**: Uji interaksi zoom/rotasi berkas struk optik dan kuitansi klinik eksternal.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Specialist.
* **Definition of Done**: Komponen pratinjau dokumen mempermudah verifikasi keabsahan kuitansi fisik secara digital.

### ✅ `FE-HRD-154` — Komponen Drawer Slide-Over `FE-HRD-171` — Investigasi & Koreksi Restitusi Salah Potong Periode Lampau
* **Slice / MVP**: `S-B6` / `MVP-35`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Komponen drawer geser (*slide-over*) untuk menginvestigasi keluhan salah potong gaji bulan-bulan lalu, mencari nomor slip gaji lampau, memilih baris potongan keliru, dan menerbitkan kompensasi pengembalian uang pada slip gaji berjalan tanpa mengubah slip final masa lalu.
* **Requirement/decision**: `HRD-DEC-131`, `03-frontend-architecture.md` Bagian 20.2.4.
* **Kontrak**: API Contract `v15` `POST /restitutions/corrections`, Validation Matrix `v15` `VR-DED-04`, `VR-DED-05`.
* **Kewenangan UI**: `MANDATORY` antarmuka koreksi penggajian terkontrol.
* **Reuse**: BaseDrawer, BaseForm, DatePicker, CurrencyInput.
* **Cakupan**: `src/components/features/hr/deductions/PastPeriodCorrectionDrawer.jsx`.
* **Dependency**: `FE-HRD-144`, `FE-HRD-145`, `FE-HRD-148`
* **Acceptance criteria**:
  1. Drawer memuat data rincian pemotongan slip gaji masa lalu yang dipilih oleh HR Officer.
  2. Radio button memungkinkan pemilihan tepat satu komponen pemotongan yang keliru.
  3. Form mewajibkan input alasan justifikasi audit minimal 20 karakter sebelum tombol submit aktif.
  4. Komponen tujuan otomatis terkunci pada `RestitutionAdjustment` bertanda bebas pajak PPh 21 (`VR-DED-05`).
* **Bukti verifikasi**: Uji investigasi salah potong di UI, verifikasi penautan nomor slip asal, skenario UAT `UAT-DED-04`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Drawer koreksi restitusi beroperasi ber-audit penuh dan menjamin integritas slip lama.

### 🟡 `FE-HRD-155` — Layar `FE-HRD-172` & `ModalVoucherQrViewer` — Penerbitan Voucher Kasir Cito 24/7 & Penampil QR Code (Bagian 2)
* **Slice / MVP**: `S-B6` / `MVP-35`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Komponen atau layarnya tidak dapat dibuka dari rute mana pun. 4 dari 4 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R40`. Laporan lama: [FE-HRD-155.md](../task/report/frontend/FE-HRD-155.md).
* **Outcome**: Antarmuka bagi Direktur Medis/SDM untuk mengotorisasi klaim darurat medis nakes cito, mencetak voucher kasir elektronik ber-token SHA-256, menampilkan QR code untuk scan loket Kasir RS 24/7, serta visualisasi peringatan keselamatan anti-bayar ganda.
* **Requirement/decision**: `HRD-DEC-133`, `03-frontend-architecture.md` Bagian 20.2.5 & 20.3.
* **Kontrak**: API Contract `v15` `POST /restitutions/claims/{id}/issue-cashier-voucher`, Validation Matrix `v15` `VR-DED-06`.
* **Kewenangan UI**: `MANDATORY` antarmuka kasir darurat cito.
* **Reuse**: BaseModal, QrCodeViewer, PrintUtility.
* **Cakupan**: `src/components/features/hr/restitution/ModalVoucherQrViewer.jsx`.
* **Dependency**: `FE-HRD-152`
* **Acceptance criteria**:
  1. Tombol *"Terbitkan Voucher Kasir Cito 24/7"* hanya muncul pada berkas klaim bertanda `IsCitoEmergency == true`.
  2. Dialog menampilkan QR code voucher kasir, token 6 digit, batas waktu kedaluwarsa 48 jam, dan nominal tunai.
  3. Tampil peringatan merah: *"Klaim ini dicairkan tunai di Kasir RS dan otomatis DIKECUALIKAN dari slip gaji bulanan."*
  4. Menyediakan tombol cetak fisik PDF tanda terima kasir dan tombol kirim voucher ke WhatsApp staf pemohon.
* **Bukti verifikasi**: Uji pembentukan QR voucher cito, verifikasi skenario UAT `UAT-DED-05`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Specialist.
* **Definition of Done**: Antarmuka penerbitan voucher kasir cito beroperasi cepat dan melindungi keselamatan nakes krisis medis.

### ✅ `FE-HRD-156` — Layar `FE-HRD-173` — Pengaturan Kebijakan Plafon Benefit per Grade & Batas Pemotongan Upah
* **Slice / MVP**: `S-B6` / `MVP-35`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Halaman konfigurasi `/corporate/hrd/payroll/deductions/settings` bagi HR Administrator untuk mengelola batas kuota kacamata kerja/medis per grade, masa kerja minimal, toleransi batas peringatan rasio potong (default 40%), dan batas hard-cap upah 50% PP 36/2021.
* **Requirement/decision**: `HRD-DEC-128`, `HRD-DEC-130`, `03-frontend-architecture.md` Bagian 20.2.6.
* **Kontrak**: API Contract `v15` `[Tags("HR Deductions - Settings")]`.
* **Kewenangan UI**: `MANDATORY` halaman pengaturan kebijakan modul.
* **Reuse**: `@/components/features/base-features/base-form-control`, BaseButton, InformationAlert, Table.
* **Cakupan**: `src/app/corporate/hrd/payroll/deductions/settings/page.jsx`, `src/components/features/hr/deductions/SettingsPolicyForm.jsx`.
* **Dependency**: `FE-HRD-144`, `FE-HRD-145`
* **Acceptance criteria**:
  1. Tabel menampilkan matriks seluruh grade level dengan batas kuota rupiah tahunan dan durasi masa kerja.
  2. Field ambang batas pemotongan upah non-regulasi terkunci pada default 50.00% berlandaskan Pasal 65 PP 36/2021.
  3. Toggle perizinan cito kasir per grade dapat diaktifkan/dinonaktifkan oleh administrator.
  4. Penyimpanan perubahan parameter mencatat riwayat log audit perubahan pengaturan.
* **Bukti verifikasi**: Uji simpan parameter konfigurasi plafon dan verifikasi persistensi nilai baru.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead.
* **Definition of Done**: Layar pengaturan kebijakan restitusi dan potongan tersimpan rapi dan mengendalikan parameter sistem.

### ✅ `FE-HRD-157` — Verifikasi ujung-ke-ujung antarmuka UI/UX, linter CI/CD, build Next.js, dan hak akses peran Slice S-B6
* **Slice / MVP**: `S-B6` / Verifikasi
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Audit komprehensif kepatuhan antarmuka frontend, zero error linter CI/CD (`npm run lint:errors`), verifikasi kompilasi produksi standalone Next.js App Router (`npm run build`), kepatuhan akses peran RBAC, serta pemenuhan seluruh kriteria UAT `UAT-DED-01` s.d. `UAT-DED-05`.
* **Requirement/decision**: `03-frontend-architecture.md` Bagian 20.1 & 20.2, `04-prd-to-mvp.md` Bagian 29.4 & 29.5.
* **Kontrak**: Seluruh kontrak operasional `v15` Slice `S-B6`.
* **Kewenangan UI**: Kepatuhan penuh standar desain antarmuka Quilvian.
* **Reuse**: Prosedur audit pre-deploy frontend Quilvian.
* **Cakupan**: Audit menu navigasi sidebar, uji loading/empty/error state, `npm run lint:errors`, `npm run build`.
* **Dependency**: `FE-HRD-147`, `FE-HRD-149`, `FE-HRD-151`, `FE-HRD-153`, `FE-HRD-154`, `FE-HRD-155`, `FE-HRD-156`
* **Acceptance criteria**:
  1. Menu Restitusi & Pemotongan tampil di sidebar sesuai hak akses peran pengguna aktif.
  2. Staf umum hanya dapat mengakses portal klaim mandiri ESS pribadinya (`/ess/restitution-claims`).
  3. Dasbor pemantauan dan lembar kerja angsuran hanya dapat diakses HR C&B, Payroll Officer, dan Manajer SDM.
  4. `npm run lint:errors` dan `npm run build` berhasil 100% dengan 0 error kompilasi.
* **Bukti verifikasi**: Laporan lint CI/CD bersih, tangkapan layar navigasi sidebar per peran, laporan verifikasi UAT.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead bersama QA.
* **Definition of Done**: Seluruh antarmuka restitusi dan pemotongan teruji, bebas broken link, dan siap rilis produksi.

### ✅ `FE-HRD-158` — Service API client, adapter TypeScript/Axios, Redux slice & state management manfaat kesehatan
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Modul service client API Axios `benefitsApi.js`, adapter serialisasi data DTO, Redux toolkit slice `benefitsSlice.js` dengan state terisolasi (plans, enrollments, dependents, ledger, claims, analytics), penanganan async thunk, dan selector teroptimasi memoized.
* **Requirement/decision**: `03-frontend-architecture.md` Bagian 21.2, `contracts/api-contract.md` Bagian 22 (Kontrak `v16`).
* **Kewenangan UI**: Standar state management Redux Toolkit & Axios instance Quilvian.
* **Reuse**: Axios client instance terpusat, JWT bearer interceptor.
* **Cakupan**: `src/lib/api/hr/benefitsApi.js`, `src/redux/slices/hr/benefitsSlice.js`.
* **Dependency**: Tidak ada (fondasi frontend pertama S-D3).
* **Acceptance criteria**:
  1. API client mengimplementasikan seluruh endpoint dari 4 grup tag Swagger `[Tags]`.
  2. Redux slice memelihara state loading, data cache, error message, dan pagination metadata.
* **Bukti verifikasi**: Unit test Redux reducers dan mock service API.
* **Risiko & pemilik**: Kegagalan refresh token; mitigasi: penanganan error 401 terpusat di axios interceptor.
* **Definition of Done**: Service API dan slice Redux terdaftar pada root reducer dan lolos build linter.

### ✅ `FE-HRD-159` — Registrasi peta navigasi sidebar, rute `/corporate/hrd/benefits/*`, rute ESS & permission guards
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Pendaftaran 6 rute menu internal HRD `/corporate/hrd/benefits/*` dan 3 rute mandiri ESS `/ess/*` pada konfigurasi sidebar navigasi Quilvian, didampingi komponen PermissionGuard pelindung hak akses peran RBAC.
* **Requirement/decision**: `03-frontend-architecture.md` Bagian 21.1 (Peta Butir Menu).
* **Kewenangan UI**: Standar navigasi sidebar Quilvian UI.
* **Reuse**: `PermissionGuard`, `SidebarNav`, config menu terpusat.
* **Cakupan**: `src/config/navigation/hrBenefitsNav.js`, Next.js route middleware.
* **Dependency**: `FE-HRD-158`
* **Acceptance criteria**:
  1. Menu Manajemen Manfaat hanya muncul bagi pengguna dengan hak `HR_Benefit:View`.
  2. Menu ESS Keluarga Saya dan Klaim COB muncul bagi seluruh pegawai aktif.
* **Bukti verifikasi**: Pengujian navigasi visual antar peran pengguna berbeda.
* **Risiko & pemilik**: Broken link pada rute dinamis; mitigasi: route constants terdefinisi rapi.
* **Definition of Done**: Seluruh tautan menu terdaftar dan terlindungi guard otentikasi.

### ✅ `FE-HRD-160` — Komponen reusable & token UI manfaat (StatusBadge, kartu limit, meteran utilisasi, uploader KK/akta)
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Koleksi komponen UI reusable pendukung modul benefit: `BenefitStatusBadge` (Draft/Active/Suspended/Terminated), `BalanceMeterBar` (progress bar penyerapan plafon), `QuotaCounterWidget` (indikator kuota anak 1-3), dan `DocumentDropzoneUpload` (file uploader dengan preview PDF/JPG maks 5 MB).
* **Requirement/decision**: `03-frontend-architecture.md` Bagian 21.2 & 21.3.
* **Kewenangan UI**: Design token Quilvian (@/components/features/base-features/base-form-control, card tokens).
* **Reuse**: BaseButton, StatusBadge, BaseProgress, FileUpload.
* **Cakupan**: `src/components/features/hr/benefits/common/*.jsx`.
* **Dependency**: `FE-HRD-158`
* **Acceptance criteria**:
  1. Komponen memvalidasi file lampiran otomatis (format PDF/JPG/PNG dan ukuran <= 5 MB).
  2. Progress bar utilisasi saldo berubah warna: hijau (<50%), kuning (50-80%), oranye (>80%), merah (100%).
* **Bukti verifikasi**: Storybook / isolated component testing, responsivitas elemen UI.
* **Risiko & pemilik**: Kegagalan rendering preview file di mobile; mitigasi: fallback link unduh berkas.
* **Definition of Done**: Seluruh komponen UI reusable siap dipakai oleh halaman-halaman fitur.

### 🟡 `FE-HRD-161` — Layar `FE-HRD-174` — Dasbor Analitik Manfaat Kesehatan, Utilisasi Plafon & Alert Batas Usia Tanggungan
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R24`. Laporan lama: [FE-HRD-161.md](../task/report/frontend/FE-HRD-161.md).
* **Outcome**: Halaman dasbor analitik `/corporate/hrd/benefits/dashboard` menampilkan 4 kartu KPI kepesertaan BPJS dan anggaran plafon RS, grafik batang serapan plafon kuartalan rawat inap vs jalan, dan tabel peringatan dini tanggungan mendekati usia 21/25 tahun.
* **Requirement/decision**: `03-frontend-architecture.md` Bagian 21.2.1.
* **Kewenangan UI**: Standar dasbor eksekutif Quilvian.
* **Reuse**: MetricCard, ChartComponent, DataTable, InformationAlert.
* **Cakupan**: `src/app/corporate/hrd/benefits/dashboard/page.jsx`, `src/components/features/hr/benefits/DashboardWidgets.jsx`.
* **Dependency**: `FE-HRD-158`, `FE-HRD-159`, `FE-HRD-160`
* **Acceptance criteria**:
  1. Kartu metrik menampilkan total pegawai terdaftar BPJS TK, BPJS Kes, dan total pagu terpakai secara akurat.
  2. Tabel peringatan menandai anak yang membutuhkan pembaruan surat keterangan kuliah per 31 Agustus.
* **Bukti verifikasi**: Pengujian visual dasbor, verifikasi respons filter periode.
* **Risiko & pemilik**: Slow query analitik; mitigasi: backend endpoint teroptimasi agregasi.
* **Definition of Done**: Dasbor analitik aktif dan menyajikan data operasional secara interaktif.

### ✅ `FE-HRD-162` — Layar `FE-HRD-175` — Master Katalog Paket Manfaat 3-Lapisan, Pagu Plafon & Diskon Tarif Staf RS
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Halaman `/corporate/hrd/benefits/plans` untuk mengelola katalog paket manfaat 3-lapisan, tabel daftar paket dengan pagu limit tahunan, toggle mandatory COB BPJS, dan modal tambah/edit paket benefit.
* **Requirement/decision**: `HRD-DEC-134`, `03-frontend-architecture.md` Bagian 21.2.2.
* **Kewenangan UI**: Standar tabel master dan modal form Quilvian.
* **Reuse**: Table, ModalForm, CurrencyInput, SwitchToggle.
* **Cakupan**: `src/app/corporate/hrd/benefits/plans/page.jsx`, `src/components/features/hr/benefits/BenefitPlanFormModal.jsx`.
* **Dependency**: `FE-HRD-158`, `FE-HRD-159`, `FE-HRD-160`
* **Acceptance criteria**:
  1. Tabel menampilkan seluruh paket aktif beserta persentase diskon tarif staf RS.
  2. Form memvalidasi bahwa paket asuransi swasta tambahan wajib mencentang opsi COB BPJS.
* **Bukti verifikasi**: Pengujian operasi CRUD paket manfaat di lingkungan dev.
* **Risiko & pemilik**: Inkonsistensi data kode paket; mitigasi: field kode paket terkunci saat edit.
* **Definition of Done**: Halaman master katalog paket beroperasi penuh dan terhubung ke API.

### ✅ `FE-HRD-163` — Komponen Drawer `FE-HRD-175` — Matriks Kelayakan Grade Staf Rumah Sakit ke Kelas Kamar Rawat Inap
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Komponen drawer geser (*slide-over*) pada layar katalog paket untuk mengonfigurasi pemetaan matriks Grade Level staf (Direksi, Spesialis, Keperawatan, Staf Umum) terhadap hak kelas kamar rawat inap (Kelas 2, Kelas 1, VIP, VVIP) dan plafon spesifik.
* **Requirement/decision**: `HRD-DEC-134`, `03-frontend-architecture.md` Bagian 21.2.2.
* **Kewenangan UI**: Standar SlideOver Drawer Quilvian.
* **Reuse**: BaseDrawer, SelectDropdown, Badge.
* **Cakupan**: `src/components/features/hr/benefits/GradeEligibilityDrawer.jsx`.
* **Dependency**: `FE-HRD-162`
* **Acceptance criteria**:
  1. Drawer menampilkan tabel matriks grade yang mudah diedit per baris.
  2. Perubahan hak kelas kamar tersimpan dan langsung tercermin pada data kelayakan nakes.
* **Bukti verifikasi**: Uji interaksi penyimpanan matriks grade.
* **Risiko & pemilik**: Duplikasi mapping grade; mitigasi: validasi kombinasi unique grade + employment type.
* **Definition of Done**: Drawer matriks kelayakan grade selesai dan terintegrasi dengan tabel master.

### ✅ `FE-HRD-164` — Layar `FE-HRD-176` — Monitoring Kepesertaan Asuransi Pegawai, Aktivasi H+0 vs Pasca-Percobaan 3 Bulan
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Halaman `/corporate/hrd/benefits/enrollments` untuk memantau status kepesertaan seluruh staf rumah sakit, membedakan status aktif cepat BPJS TK H+0 Onboarding dengan penahanan paket kesehatan masa percobaan 3 bulan, serta modal pemutakhiran nomor kartu BPJS Kes/TK & polis swasta.
* **Requirement/decision**: `HRD-DEC-138`, `03-frontend-architecture.md` Bagian 21.2.3.
* **Kewenangan UI**: Standar tabel filter dan modal form Quilvian.
* **Reuse**: TableWithFilters, SearchInput, StatusBadge, Modal.
* **Cakupan**: `src/app/corporate/hrd/benefits/enrollments/page.jsx`, `src/components/features/hr/benefits/PolicyNumberModal.jsx`.
* **Dependency**: `FE-HRD-158`, `FE-HRD-159`, `FE-HRD-160`
* **Acceptance criteria**:
  1. Pegawai masa percobaan ditandai badge kuning `PendingActivation (Masa Percobaan)`.
  2. Form modal memvalidasi format 13 digit untuk nomor BPJS Kesehatan dan 11 digit untuk BPJS TK.
* **Bukti verifikasi**: Uji pencarian data staf, validasi form nomor polis.
* **Risiko & pemilik**: Salah ketik nomor identitas BPJS; mitigasi: visual feedback validasi regex digit.
* **Definition of Done**: Layar monitoring kepesertaan aktif dan memudahkan audit HR.

### ✅ `FE-HRD-165` — Layar `FE-HRD-177` — Antrean Meja Kerja Verifikasi Dokumen Kependudukan Tanggungan Keluarga HR
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Halaman meja kerja `/corporate/hrd/benefits/dependents` bagi HR Benefit Officer untuk memeriksa berkas pendaftaran tanggungan keluarga ESS, menampilkan perbandingan data NIK/KK dengan preview dokumen scan KK/Akta berdampingan, serta dialog persetujuan atau penolakan disertai alasan.
* **Requirement/decision**: `HRD-DEC-135`, `03-frontend-architecture.md` Bagian 21.2.4.
* **Kewenangan UI**: Standar split-screen verification workstation Quilvian.
* **Reuse**: DocumentViewerModal, BaseButton, TextArea.
* **Cakupan**: `src/app/corporate/hrd/benefits/dependents/page.jsx`, `src/components/features/hr/benefits/DependentVerificationModal.jsx`.
* **Dependency**: `FE-HRD-158`, `FE-HRD-160`
* **Acceptance criteria**:
  1. Penampil dokumen mendukung zoom dan rotasi gambar scan Kartu Keluarga dan Akta.
  2. Tombol penolakan mewajibkan pengisian catatan alasan penolakan minimal 15 karakter.
* **Bukti verifikasi**: Uji alur verifikasi berkas keluarga oleh HR di lingkungan dev.
* **Risiko & pemilik**: Dokumen scan buram; mitigasi: instruksi upload jelas pada form ESS.
* **Definition of Done**: Meja kerja verifikasi tanggungan keluarga beroperasi efisien dan aman.

### ✅ `FE-HRD-166` — Layar `FE-HRD-177` (ESS View) — Portal Mandiri ESS Keluarga Saya & Indikator Kuota 1 Pasangan + 3 Anak
* **Slice / MVP**: `S-D3` / `MVP-36`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Halaman portal mandiri `/ess/my-dependents` bagi staf nakes untuk melihat daftar keluarga yang terdaftar, kartu indikator sisa kuota (Pasangan: 1/1, Anak: 2/3), tombol pendaftaran baru, dan dropzone upload dokumen scan KK, Akta Nikah/Lahir, atau Surat Keterangan Mahasiswa Aktif bagi anak usia 21-25 tahun.
* **Requirement/decision**: `HRD-DEC-135`, `03-frontend-architecture.md` Bagian 21.2.4.
* **Kewenangan UI**: Portal mandiri ESS ramah ponsel/tablet (mobile-first layout).
* **Reuse**: CardWidget, FileDropzone, ProgressBar, Alert.
* **Cakupan**: `src/app/ess/my-dependents/page.jsx`, `src/components/features/ess/FamilyRegistrationForm.jsx`.
* **Dependency**: `FE-HRD-158`, `FE-HRD-160`
* **Acceptance criteria**:
  1. Tombol tambah anak dinonaktifkan otomatis dengan tooltip penjelasan jika kuota 3 anak telah terpenuhi.
  2. Form mewajibkan unggah surat keterangan dekanat jika tanggal lahir anak menunjukkan usia > 21 tahun.
* **Bukti verifikasi**: Pengujian form pendaftaran keluarga dari akun staf biasa di portal ESS.
* **Risiko & pemilik**: Pendaftaran keluarga non-inti; mitigasi: pilihan hubungan keluarga dikunci hanya Pasangan dan Anak.
* **Definition of Done**: Pegawai dapat mendaftarkan tanggungan keluarga secara mandiri dan transparan.

### 🟡 `FE-HRD-167` — Layar `FE-HRD-178` — Inquiry Hak Berobat Staf di RS Sendiri & Buku Besar Mutasi Saldo Plafon SIMRS
* **Slice / MVP**: `S-D3` / `MVP-37`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R24`. Laporan lama: [FE-HRD-167.md](../task/report/frontend/FE-HRD-167.md).
* **Outcome**: Halaman `/corporate/hrd/benefits/inhouse-ledger` untuk mengecek hak subsidi nakes secara online, kartu visual sisa saldo plafon dan hak diskon staf, serta tabel histori mutasi buku besar double-entry `HrdBenefitBalanceMovement` (alokasi tahunan, pemotongan kunjungan RS, penyesuaian).
* **Requirement/decision**: `HRD-DEC-136`, `03-frontend-architecture.md` Bagian 21.2.5.
* **Kewenangan UI**: Standar buku besar dan search inquiry Quilvian.
* **Reuse**: SearchInput, LedgerTable, BalanceCard, ExportButton.
* **Cakupan**: `src/app/corporate/hrd/benefits/inhouse-ledger/page.jsx`, `src/components/features/hr/benefits/BenefitLedgerTable.jsx`.
* **Dependency**: `FE-HRD-158`, `FE-HRD-160`
* **Acceptance criteria**:
  1. Pencarian via NIP langsung menampilkan hak kelas kamar rawat inap, diskon tarif, dan sisa saldo plafon.
  2. Tabel mutasi menyajikan saldo sebelum, debit pemakaian, kredit alokasi, saldo akhir, dan no. registrasi SIMRS.
* **Bukti verifikasi**: Verifikasi konsistensi saldo buku besar terhadap data API.
* **Risiko & pemilik**: Kerahasiaan data kunjungan poli staf; mitigasi: kolom diagnosis medis tidak ditampilkan di modul HR.
* **Definition of Done**: Buku besar saldo plafon berobat internal RS transparan dan akurat.

### 🟡 `FE-HRD-168` — Modal Dialog `FE-HRD-178` — Penyesuaian Manual Saldo Plafon Berobat RS oleh HR Manajer
* **Slice / MVP**: `S-D3` / `MVP-37`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R24`. Laporan lama: [FE-HRD-168.md](../task/report/frontend/FE-HRD-168.md).
* **Outcome**: Dialog modal pop-up bagi HR Manajer untuk melakukan adjustment saldo plafon nakes disertai catatan alasan pembatalan tindakan SIMRS, input nomor referensi berita acara, dan konfirmasi keamanan ganda.
* **Requirement/decision**: `HRD-DEC-136`, `03-frontend-architecture.md` Bagian 21.2.5.
* **Kewenangan UI**: Modal form otorisasi khusus Manajer.
* **Reuse**: ModalDialog, CurrencyInput, TextArea, BaseButton.
* **Cakupan**: `src/components/features/hr/benefits/BalanceAdjustmentModal.jsx`.
* **Dependency**: `FE-HRD-167`
* **Acceptance criteria**:
  1. Modal hanya dapat diakses oleh user dengan hak akses peran `HR_Benefit_InHouse:Adjust`.
  2. Sistem menolak penyesuaian yang akan mengakibatkan saldo akhir bernilai negatif.
* **Bukti verifikasi**: Uji input penyesuaian manual dan pencatatan audit log.
* **Risiko & pemilik**: Koreksi sepihak tanpa bukti; mitigasi: mandatory pengisian nomor referensi surat pembatalan SIMRS.
* **Definition of Done**: Penyesuaian manual saldo plafon tercatat aman pada buku besar.

### ✅ `FE-HRD-169` — Layar `FE-HRD-179` — Meja Kerja Monitoring, Audit Kuitansi & Settlement Klaim Koordinasi Manfaat (COB)
* **Slice / MVP**: `S-D3` / `MVP-38`
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Halaman meja kerja `/corporate/hrd/benefits/cob-claims` bagi tim Kompensasi & Benefit untuk mengaudit klaim biaya kesehatan penjamin sekunder berlampiran SEP BPJS Kesehatan, memverifikasi kuitansi selisih naik kelas, menyetujui nominal santunan, dan memproses settlement pelunasan.
* **Requirement/decision**: `HRD-DEC-137`, `03-frontend-architecture.md` Bagian 21.2.6.
* **Kewenangan UI**: Workstation tabel filter dan drawer settlement klaim.
* **Reuse**: Table, Drawer, DocumentPreview, CurrencyDisplay.
* **Cakupan**: `src/app/corporate/hrd/benefits/cob-claims/page.jsx`, `src/components/features/hr/benefits/ClaimSettlementDrawer.jsx`.
* **Dependency**: `FE-HRD-158`, `FE-HRD-160`
* **Acceptance criteria**:
  1. Meja kerja menampilkan kolom nomor SEP BPJS Kesehatan dengan tanda centang verifikasi online.
  2. Drawer settlement memfasilitasi opsi pendelegasian sisa ekses biaya ke pemotongan payroll Sub-11 (`S-B6`).
* **Bukti verifikasi**: Uji alur audit kuitansi dan konfirmasi settlement santunan.
* **Risiko & pemilik**: Ekses tagihan lolos tanpa penagihan; mitigasi: auto-trigger integrasi Sub-11 pada saat settlement.
* **Definition of Done**: Meja kerja audit klaim COB tuntas dan siap operasional.

### 🟡 `FE-HRD-170` — Layar `FE-HRD-179` (ESS View) — Formulir Mandiri Pengajuan Klaim COB Berlampiran Mandatory SEP BPJS
* **Slice / MVP**: `S-D3` / `MVP-38`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R24`. Laporan lama: [FE-HRD-170.md](../task/report/frontend/FE-HRD-170.md).
* **Outcome**: Formulir pengajuan mandiri di portal `/ess/claims/cob-submission` bagi pegawai nakes untuk mengajukan klaim santunan sekunder asuransi swasta/RS, wajib menginput nomor SEP BPJS Kesehatan 19 digit, kalkulator pemisahan biaya INA-CBGs vs selisih, dan uploader berkas kwitansi asli.
* **Requirement/decision**: `HRD-DEC-137`, `03-frontend-architecture.md` Bagian 21.2.6.
* **Kewenangan UI**: Formulir bertahap mandiri ESS (*step-by-step submission form*).
* **Reuse**: StepperForm, FileUpload, CurrencyInput, InformationAlert.
* **Cakupan**: `src/app/ess/claims/cob-submission/page.jsx`, `src/components/features/ess/CobClaimForm.jsx`.
* **Dependency**: `FE-HRD-158`, `FE-HRD-160`
* **Acceptance criteria**:
  1. Form memblokir tombol submit jika nomor SEP BPJS belum diisi dengan pesan peringatan `VR-BEN-16`.
  2. Kalkulator otomatis menghitung selisih santunan sekunder berdasarkan total tagihan dikurangi jaminan BPJS.
* **Bukti verifikasi**: Pengujian submit klaim COB mandiri dari antarmuka ESS staf.
* **Risiko & pemilik**: Pengajuan klaim ganda; mitigasi: pengecekan nomor SEP unik pada database.
* **Definition of Done**: Pengajuan klaim koordinasi manfaat mandiri dapat dilakukan dengan mudah oleh pegawai.

### ✅ `FE-HRD-171` — Verifikasi ujung-ke-ujung antarmuka UI/UX, linter CI/CD, build Next.js, dan hak akses peran Slice S-D3
* **Slice / MVP**: `S-D3` / `MVP-38` & Verifikasi
* **Status**: ✅ Selesai (`PASS`)
* **Outcome**: Audit komprehensif seluruh layar modul benefit (`FE-HRD-174` s.d. `FE-HRD-179`), verifikasi konsistensi token visual Quilvian, validasi linting CI/CD bebas error (`npm run lint:errors`), verifikasi kompilasi produksi standalone Next.js App Router (`npm run build`), kepatuhan akses peran RBAC, dan pemenuhan seluruh kriteria UAT `UAT-HRD-BEN-01` s.d. `UAT-HRD-BEN-05`.
* **Requirement/decision**: `03-frontend-architecture.md` Bagian 21.1 & 21.2, `04-prd-to-mvp.md` Bagian 30.4 & 30.5.
* **Kewenangan UI**: Kepatuhan penuh standar antarmuka Next.js Quilvian.
* **Reuse**: Skrip pra-deploy frontend `npm run lint:errors`, `npm run build`.
* **Cakupan**: Verifikasi seluruh antarmuka modul benefit dan portal ESS.
* **Dependency**: `FE-HRD-161`, `FE-HRD-162`, `FE-HRD-164`, `FE-HRD-165`, `FE-HRD-167`, `FE-HRD-169`, `FE-HRD-170`
* **Acceptance criteria**:
  1. Seluruh 6 layar antarmuka teruji bebas dari broken link dan error rendering.
  2. Staf umum hanya dapat mengakses portal keluarga dan klaim mandiri ESS miliknya.
  3. `npm run lint:errors` dan `npm run build` berhasil 100% dengan zero warning/error.
  4. 5 skenario UAT pada `04-prd-to-mvp.md` Bagian 30.4 tervalidasi dengan hasil memuaskan.
* **Bukti verifikasi**: Laporan lint CI/CD bersih, tangkapan layar navigasi antar peran, laporan hasil pengujian UAT.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead bersama Tim QA.
* **Definition of Done**: Antarmuka frontend modul benefit 100% siap rilis produksi.


---

## Rincian Task Frontend Slice `S-D4` — HR Service Management & Request (Sub-modul 13, Kontrak v17)

Bagian ini memuat pemecahan delivery antarmuka pengguna **Sub-Modul 13 (Slice `S-D4`)** menjadi 14 task frontend terperinci (`FE-HRD-172` s.d. `FE-HRD-185`), grafik urutan dependency pohon teks (Grafik F46 s.d. F49), serta tabel gelombang eksekusi `MVP-39` s.d. `MVP-41` & Verifikasi.

### Grafik Urutan Dependency Pohon Teks `S-D4`

#### Grafik F46 — `S-D4` `MVP-39`: Client API, Redux Slice, Dasbor Helpdesk & Pengaturan Katalog Layanan

```text
FE-HRD-172 (Client API & Redux Slice) ───┬───> FE-HRD-173 (Katalog Layanan & SLA Setup FE-HRD-181) ───> FE-HRD-174 (Template Editor Token Dinamis)
BE-HRD-193 [BE] ────────────────────────┤
FE-HRD-172 [F46] ───────────────────────┴───> FE-HRD-175 (Dasbor KPI Helpdesk FE-HRD-180) ────────────> FE-HRD-176 (Tabel Antrean SLA Countdown)
BE-HRD-195 [BE] ───────────────────────────────────────────────────────────────────────────────────────┘
```

#### Grafik F47 — `S-D4` `MVP-39`: Modal Re-assign Penugasan & Portal Mandiri Pengajuan Tiket ESS

```text
FE-HRD-176 [F46] ───┬───> FE-HRD-177 (Modal Re-assign Penugasan Analis)
BE-HRD-195 [BE] ────┤
FE-HRD-172 [F46] ───┴───> FE-HRD-178 (Portal ESS Katalog Tiles FE-HRD-182) ───> FE-HRD-179 (Wizard Multi-Step & Upload Lampiran)
BE-HRD-196 [BE] ─────────────────────────────────────────────────────────────┘
```

#### Grafik F48 — `S-D4` `MVP-40`: Drawer Pemrosesan Tiket, Utas Klarifikasi & Pemutakhiran Profil Sub-02

```text
FE-HRD-176 [F46] ───┬───> FE-HRD-180 (Drawer Pemrosesan Tiket & Chat FE-HRD-183) ───> FE-HRD-181 (Aksi Mutasi Profil Otomatis)
BE-HRD-199 [BE] ────┤
FE-HRD-174 [F46] ───┴───> FE-HRD-182 (Modal Otorisasi Surat Resmi & QR FE-HRD-184) ───> FE-HRD-183 (Modal Pencabutan Dokumen BAP)
BE-HRD-203 [BE] ────┘
```

#### Grafik F49 — `S-D4` `MVP-41` & Verifikasi: Portal Publik Verifikasi QR, PIN Gaji & Pusat Arsip E-Dossier

```text
FE-HRD-172 [F46] ───┬───> FE-HRD-184 (Portal Publik Verifikasi QR & PIN Dialog FE-HRD-185) ───┬───> FE-HRD-185 (Verifikasi Build Next.js & Linter)
BE-HRD-204 [BE] ────┤                                                                      │
FE-HRD-172 [F46] ───┼───> FE-HRD-185 (Arsip Dokumen Saya ESS & Kartu PIN FE-HRD-186) ──────┤
BE-HRD-205 [BE] ────┤                                                                      │
FE-HRD-176 [F46] ───┤                                                                      │
FE-HRD-179 [F47] ───┤                                                                      │
FE-HRD-180 [F48] ───┤                                                                      │
FE-HRD-182 [F48] ───┴──────────────────────────────────────────────────────────────────────┘
```

---

#### Tabel Gelombang Eksekusi Frontend `S-D4`

| Gelombang | Task yang Dapat Dimulai Bersamaan | Yang Menunggu Gelombang Ini |
| :---: | --- | --- |
| **1** | `FE-HRD-172` (client API & Redux slice helpdesk) | `FE-HRD-173`, `FE-HRD-175`, `FE-HRD-178`, `FE-HRD-184` |
| **2** | `FE-HRD-173` (layar katalog layanan `FE-HRD-181`), `FE-HRD-175` (dasbor KPI helpdesk `FE-HRD-180`), `FE-HRD-178` (portal tiles ESS `FE-HRD-182`) | `FE-HRD-174`, `FE-HRD-176`, `FE-HRD-179` |
| **3** | `FE-HRD-174` (editor template token dinamis), `FE-HRD-176` (antrean tiket SLA countdown), `FE-HRD-179` (wizard ESS multi-langkah & drag-and-drop) | `FE-HRD-177`, `FE-HRD-180`, `FE-HRD-182` |
| **4** | `FE-HRD-177` (modal re-assign cepat), `FE-HRD-180` (drawer tiket & chat klarifikasi `FE-HRD-183`), `FE-HRD-182` (modal otorisasi surat & QR `FE-HRD-184`), `FE-HRD-184` (portal publik verifikasi QR `FE-HRD-185`) | `FE-HRD-181`, `FE-HRD-183`, `FE-HRD-185` |
| **5** | `FE-HRD-181` (aksi mutasi profil otomatis), `FE-HRD-183` (modal revokasi ber-BAP), `FE-HRD-185` (pusat arsip e-Dossier ESS `FE-HRD-186` & verifikasi linter/build) | *(Semua task S-D4 tuntas)* |

---

### Detail Spesifikasi Task Frontend (`FE-HRD-172` s.d. `FE-HRD-185`)

#### ✅ `FE-HRD-172` — Redux Toolkit Slice `hrServiceSlice` & Endpoint Client Axios
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: ✅ PASS
* **Outcome**: Definisi state management global tiket helpdesk, timer SLA, filter antrean analis, penanganan error terpusat, dan modul Axios client terintegrasi dengan endpoint Kontrak `v17`.
* **Requirement/decision**: `03-frontend-architecture.md` Bagian 22.3.
* **Cakupan**: `src/lib/redux/slices/hrServiceSlice.js`, `src/lib/api/hrServiceApi.js`.
* **Dependency**: `BE-HRD-193`
* **Acceptance criteria**:
  1. Slice mengelola state antrean tiket, detail tiket aktif, utas percakapan, dan metrik KPI helpdesk.
  2. Modul API client mengekspos pemanggilan ke seluruh 4 tag grup endpoint Kontrak `v17`.
* **Bukti verifikasi**: Unit test Redux dispatch & asynchronous thunk API client.
* **Definition of Done**: State management helpdesk terpasang rapi dan siap dikonsumsi komponen UI.

#### 🟡 `FE-HRD-173` — Antarmuka Pengaturan Katalog Layanan SDM & Target Jam SLA (`FE-HRD-181`)
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R19`. Laporan lama: [FE-HRD-173.md](../task/report/frontend/FE-HRD-173.md).
* **Outcome**: Layanan antarmuka master kategori dan tipe layanan SDM, konfigurasi jam SLA operasional, toggle berkas wajib, dan approval atasan.
* **Requirement/decision**: `FR-HRS-01`, `03-frontend-architecture.md` Layar `FE-HRD-181`.
* **Cakupan**: `src/app/corporate/hrd/services/catalog/page.jsx`.
* **Dependency**: `FE-HRD-172`, `BE-HRD-193`
* **Acceptance criteria**:
  1. Menampilkan tabel hierarki kategori dan jenis layanan aktif.
  2. Form input jam SLA memvalidasi rentang 1 s.d. 168 jam dengan pesan error `VR-HRS-03`.
* **Bukti verifikasi**: Tangkapan layar navigasi pengaturan katalog dan penambahan jenis layanan baru.
* **Definition of Done**: Katalog layanan dapat dikonfigurasi dengan mudah oleh HR Administrator.

#### 🟡 `FE-HRD-174` — Editor Template Naskah Dinas Token Dinamis & Konfigurasi Pola Penomoran Gapless (`FE-HRD-181`)
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R19`. Laporan lama: [FE-HRD-174.md](../task/report/frontend/FE-HRD-174.md).
* **Outcome**: Editor template surat resmi berbasis variabel token dinamis (`{{NamaLengkap}}`, `{{NIP}}`, dll.) dengan pratinjau langsung (*live preview*), pola penomoran surat gapless, dan toggle proteksi finansial.
* **Requirement/decision**: `HRD-DEC-144`, `FR-HRS-02`, `FR-HRS-03`, `FR-HRS-04`.
* **Cakupan**: `src/app/corporate/hrd/services/catalog/templates/[id]/page.jsx`.
* **Dependency**: `FE-HRD-173`, `BE-HRD-194`
* **Acceptance criteria**:
  1. Editor memuat tombol penyisip token dinamis otomatis ke area teks naskah dinas.
  2. Live preview menampilkan contoh format surat dengan data simulasi pegawai.
  3. Konfigurasi pola nomor surat memvalidasi keberadaan token `[NO_URUT]` dan `[TAHUN]` (`VR-HRS-09`).
* **Bukti verifikasi**: Pengujian pembuatan template surat keterangan kerja dan verifikasi live preview.
* **Definition of Done**: Template naskah dinas resmi terstandarisasi bebas dari editor Word lepas.

#### 🟡 `FE-HRD-175` — Dasbor Meja Bantuan (Helpdesk) SDM & Kartu Metrik KPI SLA Operasional (`FE-HRD-180`)
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R19`. Laporan lama: [FE-HRD-175.md](../task/report/frontend/FE-HRD-175.md).
* **Outcome**: Tampilan dasbor ringkasan helpdesk menampilkan 4 kartu KPI performa (Tiket Aktif, Cito Kritis < 2 jam, Peringatan 75%, dan Overdue/Eskalasi) serta grafik pemenuhan SLA.
* **Requirement/decision**: `03-frontend-architecture.md` Layar `FE-HRD-180`.
* **Cakupan**: `src/app/corporate/hrd/services/tickets/page.jsx` (Region Kartu KPI).
* **Dependency**: `FE-HRD-172`, `BE-HRD-195`
* **Acceptance criteria**:
  1. Kartu KPI memperbarui angka secara real-time via REST/SignalR.
  2. Kartu Cito dan Overdue diberi aksen merah mencolok untuk memicu respon cepat supervisor.
* **Bukti verifikasi**: Tangkapan layar dasbor dengan variasi data tiket aktif dan overdue.
* **Definition of Done**: Metrik performa helpdesk tersaji visual dan informatif bagi manajemen HR.

#### 🟡 `FE-HRD-176` — Antrean Tiket Helpdesk Berjenjang 3-Tier dengan Indikator Countdown Timer SLA Real-Time (`FE-HRD-180`)
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R19`. Laporan lama: [FE-HRD-176.md](../task/report/frontend/FE-HRD-176.md).
* **Outcome**: Tabel antrean tiket helpdesk dengan filter prioritas (Cito/Standar/Khusus), indikator countdown jam tersisa, tombol ambil tiket (*claim*), dan penanda status visual (*StatusBadge*).
* **Requirement/decision**: `HRD-DEC-141`, `FR-HRS-07`, `FR-HRS-08`.
* **Cakupan**: `src/app/corporate/hrd/services/tickets/page.jsx` (Region Tabel Antrean).
* **Dependency**: `FE-HRD-175`, `BE-HRD-195`
* **Acceptance criteria**:
  1. Kolom timer menampilkan countdown detik/menit/jam dan berubah kuning pada 75% serta merah berkedip saat Overdue.
  2. Tombol "Ambil Tiket" langsung mengubah penugasan ke analis login dan status menjadi `InReview`.
* **Bukti verifikasi**: Pengujian filter prioritas Cito dan interaksi pengklaiman tiket antrean.
* **Definition of Done**: Analis HR dapat mengelola antrean pekerjaan secara tertib dan transparan.

#### 🟡 `FE-HRD-177` — Modal Re-assign Penugasan Analis SDM Cepat & Penyeimbangan Beban Kerja (`FE-HRD-180`)
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R19`. Laporan lama: [FE-HRD-177.md](../task/report/frontend/FE-HRD-177.md).
* **Outcome**: Dialog modal bagi Supervisor HR untuk mendisposisikan ulang tiket yang mendekati batas waktu atau berstatus overdue ke analis lain yang memiliki beban kerja lebih ringan.
* **Requirement/decision**: `03-frontend-architecture.md` Layar `FE-HRD-180`.
* **Cakupan**: `src/components/features/corporate/hr/services/ModalReassignTicket.jsx`.
* **Dependency**: `FE-HRD-176`, `BE-HRD-195`
* **Acceptance criteria**:
  1. Menampilkan daftar analis aktif beserta jumlah tiket yang sedang ditangani saat ini.
  2. Konfirmasi re-assign mengirimkan alert notifikasi ke analis penerima baru.
* **Bukti verifikasi**: Pengujian re-assign tiket kritis dari supervisor ke analis spesialis.
* **Definition of Done**: Penyeimbangan antrean tiket kritis berjalan fleksibel.

#### 🟡 `FE-HRD-178` — Portal ESS Mandiri: Katalog Layanan Interaktif (*Service Tiles*) & Estimasi Durasi (`FE-HRD-182`)
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R31`. Laporan lama: [FE-HRD-178.md](../task/report/frontend/FE-HRD-178.md).
* **Outcome**: Antarmuka katalog layanan visual berbasis grid kartu (*tiles*) di portal ESS pegawai, menampilkan kategori permohonan populer, syarat berkas, dan estimasi jam SLA.
* **Requirement/decision**: `FR-HRS-11`, `03-frontend-architecture.md` Layar `FE-HRD-182`.
* **Cakupan**: `src/app/portal/services/requests/page.jsx` (Region Tiles Katalog).
* **Dependency**: `FE-HRD-172`, `BE-HRD-193`
* **Acceptance criteria**:
  1. Kartu layanan menampilkan ikon intuitif, judul, dan badge durasi SLA (contoh: "SLA 24 Jam Kerja").
  2. Mengklik kartu langsung membuka formulir pengajuan tiket untuk jenis layanan terpilih.
* **Bukti verifikasi**: Navigasi pemilihan layanan mandiri dari akun staf klinis.
* **Definition of Done**: Katalog permohonan staf tersaji menarik, ramah pengguna, dan jelas.

#### 🟡 `FE-HRD-179` — Formulir Pengajuan Tiket ESS Multi-Langkah & Validasi Drag-and-Drop Lampiran 5MB (`FE-HRD-182`)
* **Slice / MVP**: `S-D4` / `MVP-39`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R31`. Laporan lama: [FE-HRD-179.md](../task/report/frontend/FE-HRD-179.md).
* **Outcome**: Wizard pengajuan tiket 3-langkah dengan komponen drag-and-drop unggah berkas, validasi ukuran file maksimal 5MB, format PDF/JPG, dan banner wajib disposisi untuk prioritas Cito.
* **Requirement/decision**: `FR-HRS-12`, `FR-HRS-13`, validasi `VR-HRS-01`, `VR-HRS-02`.
* **Cakupan**: `src/components/features/portal/services/TicketSubmissionWizard.jsx`.
* **Dependency**: `FE-HRD-178`, `BE-HRD-196`
* **Acceptance criteria**:
  1. Memblokir file melebihi 5MB atau selain format PDF/JPG/PNG dengan alert informatif.
  2. Menampilkan alert kuning tegas jika pemohon memilih prioritas Cito, mewajibkan unggahan surat disposisi medis.
  3. Submit berhasil memunculkan nomor tiket unik dan mengarahkan ke tab "Riwayat Tiket Saya".
* **Bukti verifikasi**: Uji submit tiket Standar dan pengujian error handling upload file besar.
* **Definition of Done**: Staf medis dapat mengajukan permohonan secara mandiri tanpa bingung.

#### 🟡 `FE-HRD-180` — Drawer Pemrosesan Tiket Analis, Penampil Berkas Side-by-Side & Utas Obrolan Klarifikasi (`FE-HRD-183`)
* **Slice / MVP**: `S-D4` / `MVP-40`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R31`. Laporan lama: [FE-HRD-180.md](../task/report/frontend/FE-HRD-180.md).
* **Outcome**: Panel drawer pemrosesan tiket sisi kanan dengan penampil berkas lampiran terintegrasi (*side-by-side preview*), form chat dua arah klarifikasi staf, dan toggle memo internal HR.
* **Requirement/decision**: `FR-HRS-14`, `03-frontend-architecture.md` Layar `FE-HRD-183`.
* **Cakupan**: `src/components/features/corporate/hr/services/DrawerTicketProcessing.jsx`.
* **Dependency**: `FE-HRD-176`, `BE-HRD-196`
* **Acceptance criteria**:
  1. Analis dapat memeriksa dokumen scan KTP/buku tabungan berdampingan dengan kolom isian data.
  2. Mengirim pesan klarifikasi ke staf otomatis mengubah status tiket menjadi `WaitingStaffResponse` dan mem-pause timer.
  3. Catatan internal HR diberi penanda latar belakang kuning berbeda agar tidak keliru dikirim ke staf.
* **Bukti verifikasi**: Simulasi percakapan klarifikasi berkas antara analis HR dan staf perawat.
* **Definition of Done**: Drawer pemrosesan tiket mempercepat investigasi dan komunikasi berkas permohonan.

#### 🟡 `FE-HRD-181` — Antarmuka Verifikasi Berkas Perubahan Rekening/Status & Eksekusi Pemutakhiran Profil Sub-02 (`FE-HRD-183`)
* **Slice / MVP**: `S-D4` / `MVP-40`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R31`. Laporan lama: [FE-HRD-181.md](../task/report/frontend/FE-HRD-181.md).
* **Outcome**: Formulir verifikasi permohonan ubah data master dengan tombol aksi "Setujui & Mutakhirkan Master Data", menampilkan dialog perbandingan data lama vs data baru.
* **Requirement/decision**: `HRD-DEC-143`, `FR-HRS-15`, aturan `HRS-B6`.
* **Cakupan**: `src/components/features/corporate/hr/services/ProfileChangeVerificationPanel.jsx`.
* **Dependency**: `FE-HRD-180`, `BE-HRD-199`
* **Acceptance criteria**:
  1. Dialog memunculkan tabel diff: Rekening Lama (Bank/Nomor) vs Rekening Baru yang diajukan staf.
  2. Klik "Setujui" memicu eksekusi mutasi profil otomatis ke Sub-02 dan menampilkan notifikasi sukses hijau.
* **Bukti verifikasi**: Uji eksekusi pemutakhiran rekening payroll pegawai dan verifikasi status `Resolved`.
* **Definition of Done**: Pemutakhiran data master kepegawaian tuntas tanpa intervensi re-entry data manual.

#### 🟡 `FE-HRD-182` — Modal Otorisasi Surat Resmi Pejabat, Pratinjau Live Naskah Dinas & Generator QR Code (`FE-HRD-184`)
* **Slice / MVP**: `S-D4` / `MVP-40`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R31`. Laporan lama: [FE-HRD-182.md](../task/report/frontend/FE-HRD-182.md).
* **Outcome**: Dialog modal otorisasi pejabat berwenang (Supervisor C&B / Manajer SDM) dengan pratinjau lembar surat berkop resmi RS, rincian gaji read-only Sub-10, input tanda tangan digital, dan penerbitan nomor seri gapless.
* **Requirement/decision**: `HRD-DEC-140`, `HRD-DEC-142`, `FR-HRS-16`, `FR-HRS-17`, `FR-HRS-19`.
* **Cakupan**: `src/components/features/corporate/hr/services/ModalDocumentAuthorization.jsx`.
* **Dependency**: `FE-HRD-174`, `BE-HRD-201`, `BE-HRD-203`
* **Acceptance criteria**:
  1. Menampilkan pratinjau PDF draf surat naskah dinas dengan kop resmi dan tata letak presisi.
  2. Data gaji terkunci read-only; dilarang mengedit nominal angka di formulir (`HRD-DEC-142`).
  3. Pejabat dilarang mengesahkan surat atas nama dirinya sendiri (tombol otorisasi dinonaktifkan dengan tooltip peringatan).
* **Bukti verifikasi**: Pengujian otorisasi Surat Keterangan Gaji KPR dan penerbitan nomor seri sah.
* **Definition of Done**: Surat resmi diterbitkan sah, ber-QR Code, dan siap diunduh.

#### 🟡 `FE-HRD-183` — Modal Pencabutan Keabsahan Surat Resmi (Revokasi) Ber-Input Nomor BAP (`FE-HRD-184`)
* **Slice / MVP**: `S-D4` / `MVP-40`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R31`. Laporan lama: [FE-HRD-183.md](../task/report/frontend/FE-HRD-183.md).
* **Outcome**: Dialog modal pencatatan Berita Acara Pemeriksaan (BAP) untuk mencabut keabsahan hukum surat resmi terbit yang terindikasi cacat administrasi atau fraud.
* **Requirement/decision**: `HRD-DEC-145`, `FR-HRS-25`, validasi `VR-HRS-14`.
* **Cakupan**: `src/components/features/corporate/hr/services/ModalDocumentRevocation.jsx`.
* **Dependency**: `FE-HRD-182`, `BE-HRD-205`
* **Acceptance criteria**:
  1. Input nomor BAP dan alasan pembatalan tertulis wajib diisi minimal 20 karakter.
  2. Mengonfirmasi pencabutan langsung mengubah status dokumen menjadi `Revoked`.
* **Bukti verifikasi**: Eksekusi pencabutan surat dan verifikasi perubahan status pada arsip.
* **Definition of Done**: Protokol revokasi dokumen sah terlaksana aman dan terdokumentasi.

#### 🟡 `FE-HRD-184` — Halaman Portal Publik Verifikasi Keabsahan Dokumen (`/verify/document/[hash]`) & Dialog Input 4-Digit PIN Masked Gaji (`FE-HRD-185`)
* **Slice / MVP**: `S-D4` / `MVP-41`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R31`. Laporan lama: [FE-HRD-184.md](../task/report/frontend/FE-HRD-184.md).
* **Outcome**: Halaman publik responsif tanpa sesi login yang dibuka saat pihak eksternal memindai QR Code, menampilkan banner legalitas, metadata surat, nominal gaji disamarkan, dan dialog input 4-digit PIN.
* **Requirement/decision**: `HRD-DEC-140`, `FR-HRS-22`, `FR-HRS-23`, `FR-HRS-24`.
* **Cakupan**: `src/app/verify/document/[hash]/page.jsx`.
* **Dependency**: `FE-HRD-172`, `BE-HRD-204`
* **Acceptance criteria**:
  1. Menampilkan banner hijau "SAH & BERLAKU" atau banner merah "DICABUT / TIDAK BERLAKU" jika direvokasi.
  2. Pada surat finansial, angka gaji tertutup `Rp ••••••••••`; input PIN 4-digit yang tepat membuka rincian nominal asli.
  3. Salah input PIN 3 kali memunculkan peringatan penguncian akses selama 30 menit.
* **Bukti verifikasi**: Simulasi scan QR melalui browser smartphone dan uji pembukaan PIN nominal gaji.
* **Definition of Done**: Portal publik verifikasi dokumen beroperasi aman dan melindungi kerahasiaan nakes.

#### ✅ `FE-HRD-185` — Pusat Arsip Digital Kepegawaian (Dokumen Saya ESS), Kartu PIN Rahasia & Verifikasi Ujung-ke-Ujung Slice `S-D4` (`FE-HRD-186`)
* **Slice / MVP**: `S-D4` / `MVP-41` & Verifikasi
* **Status**: ✅ PASS
* **Outcome**: Antarmuka repositori mandiri "Dokumen Saya" di portal ESS, kartu pengungkapan PIN privat berproteksi password, audit komprehensif linter CI/CD (`npm run lint:errors`), build Next.js produksi (`npm run build`), dan validasi seluruh skenario UAT.
* **Requirement/decision**: `HRD-DEC-145`, `03-frontend-architecture.md` Layar `FE-HRD-186`, `04-prd-to-mvp.md` Bagian 31.4 & 31.5.
* **Cakupan**: `src/app/portal/services/my-documents/page.jsx`.
* **Dependency**: `FE-HRD-176`, `FE-HRD-179`, `FE-HRD-180`, `FE-HRD-182`, `FE-HRD-184`
* **Acceptance criteria**:
  1. Pegawai dapat melihat dan mengunduh ulang berkas PDF resmi bertanda tangan digital miliknya kapan saja.
  2. PIN 4-digit rahasia dapat dilihat pegawai dengan memasukkan verifikasi kata sandi akun ESS.
  3. Dokumen yang direvokasi menampilkan tanda watermark visual "REVOKED".
  4. `npm run lint:errors` dan `npm run build` lulus 100% tanpa error, dan 5 skenario UAT tervalidasi.
* **Bukti verifikasi**: Laporan linting bersih, tangkapan layar unduhan e-Dossier, laporan hasil pengujian UAT.
* **Definition of Done**: Antarmuka frontend helpdesk SDM dan persuratan resmi siap rilis produksi.


---

## Bagian S-C1 — Roadmap Pengiriman Frontend Sub-Modul 14: Credentialing, License & Clinical Privilege (Slice `S-C1`, Kontrak `v18`, 5 November 2026)

Bagian ini merinci 15 task implementasi frontend terisolasi (**`FE-HRD-186` s.d. `FE-HRD-200`**) untuk **Sub-Modul 14 (Slice `S-C1`)** memakai Next.js App Router, React, Redux Toolkit, Tailwind CSS, dan Quilvian Base Components, yang mengimplementasikan antarmuka pendaftaran lisensi STR/SIP, verifikasi sumber primer (PSV) KKI/SatuSehat SDMK, meja kerja penilaian panel Mitra Bestari (*Peer Review*), sidang pleno komite, konsol pengesahan SPK-RKK oleh Direktur Utama, direktori registri wewenang SIMRS, dialog penegakan Hard-Stop bertingkat, serta radar pemantau kedaluwarsa 3 tahunan.

---

### Grafik Urutan Dependency Pohon Teks `S-C1`

#### Grafik F50 — `S-C1` `MVP-42`: Redux Slice, Portal Kredensial ESS & Dashboard PSV Staf Kredensialing
```text
FE-HRD-186 (Client API & Redux Slice) ───┬───> FE-HRD-187 (Portal Kredensial Wizard ESS FE-HRD-187) ───> FE-HRD-189 (Radar Visual SIP 5 Tahun)
BE-HRD-210 [BE] ────────────────────────┤
FE-HRD-186 [F50] ───────────────────────┴───> FE-HRD-188 (Dashboard PSV & Drawer Kemenkes FE-HRD-188) ───┘
BE-HRD-208 [BE] ────────────────────────┘
```

#### Grafik F51 — `S-C1` `MVP-43`: Master 3 Komite, Meja Mitra Bestari, Pleno & Pengesahan SPK Dirut
```text
FE-HRD-186 [F50] ───┬───> FE-HRD-190 (Katalog Whitebook 3 Komite) ───> FE-HRD-191 (Meja Asesmen Peer Review FE-HRD-189) ──┐
BE-HRD-211 [BE] ────┤                                                                                             │
FE-HRD-187 [F50] ───┴─────────────────────────────────────────────────────────────────────────────────────────────┼─> FE-HRD-192 (Sidang Pleno Komite FE-HRD-190)
BE-HRD-213 [BE] ──────────────────────────────────────────────────────────────────────────────────────────────────┤
                                                                                                                  │
FE-HRD-192 [F51] ───┬───> FE-HRD-193 (Konsol Pengesahan Dirut FE-HRD-191) ───> FE-HRD-194 (Portal Publik QR Verify) ────┘
BE-HRD-214 [BE] ────┘
```

#### Grafik F52 — `S-C1` `MVP-44`: Registri RKK, Dialog Hard-Stop SIMRS, Co-Signature & Fast-Track
```text
FE-HRD-186 [F50] ───┬───> FE-HRD-195 (Registri RKK & Direktori RS FE-HRD-192) ───┬───> FE-HRD-196 (Dialog Hard-Stop SIMRS INT-HR-69)
BE-HRD-216 [BE] ────┤                                                            │
                    │                                                            ├───> FE-HRD-197 (Modal Co-Signature 24 Jam Supervisor)
                    │                                                            │
                    │                                                            └───> FE-HRD-198 (Bypass Darurat Nyawa & Audit KMKP)
                    │
FE-HRD-193 [F51] ───┴───> FE-HRD-199 (Konsol Fast-Track FE-HRD-193 & Radar Rekredensialing FE-HRD-194)
BE-HRD-219 [BE] ────┤
BE-HRD-220 [BE] ────┘
```

#### Grafik F53 — `S-C1` Verifikasi & Standalone Build Production
```text
FE-HRD-189 [F50] ───┬────────────────────────────────────────────────────────┐
FE-HRD-194 [F51] ───┼─> FE-HRD-200 ✅ (Verifikasi Build Standalone, Linting 0 Error & UAT Acceptance S-C1)
FE-HRD-198 [F52] ───┤
FE-HRD-199 [F52] ───┤
BE-HRD-221 [BE] ────┘
```

---

### Tabel Gelombang Eksekusi Frontend `S-C1`

| Gelombang | Task ID | Target Deliverable / Output Utama | Syarat Masuk (Dependencies) |
| :---: | :--- | :--- | :--- |
| **`MVP-42`** | `FE-HRD-186`, `FE-HRD-187`, `FE-HRD-188`, `FE-HRD-189` | Redux toolkit slice, endpoint client Axios, portal self-service nakes wizard 4-langkah, dashboard verifikasi PSV staf kredensialing, drawer validasi SatuSehat SDMK, dan radar kedaluwarsa SIP 5 tahun. | `BE-HRD-208`, `BE-HRD-210` |
| **`MVP-43`** | `FE-HRD-190`, `FE-HRD-191`, `FE-HRD-192`, `FE-HRD-193`, `FE-HRD-194` | Komponen seleksi 3 komite mandiri, katalog whitebook, meja kerja Mitra Bestari (Peer Review), konsolidasi sidang pleno komite, konsol pengesahan e-Sign Direktur Utama RS, live PDF previewer, dan portal verifikasi publik ber-QR Code. | `FE-HRD-186`, `BE-HRD-211`, `BE-HRD-213`, `BE-HRD-214` |
| **`MVP-44`** | `FE-HRD-195`, `FE-HRD-196`, `FE-HRD-197`, `FE-HRD-198`, `FE-HRD-199` | Registri direktori kewenangan klinis nakes, dialog peringatan Hard-Stop di SIMRS/EMR, modal persetujuan co-signature 24 jam supervisor, tombol life-saving emergency override ber-audit KMKP, konsol fast-track penugasan cepat, dan monitoring rekredensialing 3 tahun. | `FE-HRD-193`, `BE-HRD-216`, `BE-HRD-219`, `BE-HRD-220` |
| **Verifikasi** | `FE-HRD-200` | Verifikasi integrasi Next.js App Router, audit kepatuhan linter CI/CD `npm run lint:errors` 0 error, build produksi standalone `npm run build`, dan pengujian penerimaan pengguna. | Seluruh task `FE-HRD-186` s.d. `FE-HRD-199` selesai |

---

### Detail Spesifikasi Task Frontend (`FE-HRD-186` s.d. `FE-HRD-200`)

#### ✅ `FE-HRD-186` — Redux Toolkit Slice `credentialingSlice`, Axios Client & TypeScript Interfaces
* **Slice / MVP**: `S-C1` / `MVP-42`
* **Status**: ✅ PASS
* **Outcome**: State management terpusat untuk berkas kredensial, lisensi STR/SIP, data PSV SatuSehat SDMK, penugasan peer review, registri RKK, dan audit bypass darurat, lengkap dengan TypeScript interfaces dan Axios wrapper.
* **Dependencies**: `BE-HRD-210`
* **Acceptance Criteria**: State terintegrasi dengan Redux store aplikasi, penanganan state `loading`, `idle`, `fulfilled`, dan `rejected` berjalan konsisten.
* **Verifikasi**: Build TypeScript kompilasi bersih tanpa type errors.

#### 🟡 `FE-HRD-187` — Portal Kredensial Tenaga Medis: Wizard Pendaftaran & Checklist Whitebook (`FE-HRD-187`)
* **Slice / MVP**: `S-C1` / `MVP-42`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R18`. Laporan lama: [FE-HRD-187.md](../task/report/frontend/FE-HRD-187.md).
* **Outcome**: Layar portal staf nakes pada rute `/self-service/credentials/apply` berupa formulir wizard 4 langkah (Identitas & Lisensi, Portofolio Logbook, Checklist Whitebook dengan auto-lock komite profesi, Pernyataan Etik & Submit).
* **Dependencies**: `FE-HRD-186`
* **Acceptance Criteria**: Form tervalidasi `VR-CRD-01` & `VR-CRD-04`; butir checklist whitebook terisolasi sesuai komite nakes; berkas tersimpan status `Submitted`.
* **Verifikasi**: Uji fungsional form submission & upload berkas.

#### 🟡 `FE-HRD-188` — Dashboard Verifikasi PSV Staf Kredensialing & Drawer SatuSehat SDMK (`FE-HRD-188`)
* **Slice / MVP**: `S-C1` / `MVP-42`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R18`. Laporan lama: [FE-HRD-188.md](../task/report/frontend/FE-HRD-188.md).
* **Outcome**: Meja kerja sekretariat kredensialing pada `/corporate/hrd/credentialing/psv-verification` mencakup kartu ringkasan KPI, tabel antrean verifikasi berkas, dan action drawer audit keabsahan primer ke portal KKI / SatuSehat SDMK.
* **Dependencies**: `FE-HRD-186`, `BE-HRD-208`
* **Acceptance Criteria**: Staf dapat mencatat status lolos/tolak PSV, URL rujukan bukti Kemenkes, dan tanggal audit; status berkas ter-update seketika.
* **Verifikasi**: Uji alur kerja verifikasi PSV staf.

#### 🟡 `FE-HRD-189` — Komponen Visual Radar Pemantau Kedaluwarsa SIP 5 Tahun & Peringatan Dini
* **Slice / MVP**: `S-C1` / `MVP-42`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R18`. Laporan lama: [FE-HRD-189.md](../task/report/frontend/FE-HRD-189.md).
* **Outcome**: Indikator visual progres masa berlaku SIP (Hijau > 90 hari, Kuning H-90..H-31, Oranye H-30..H-8, Merah H-7..H-0), banner peringatan fail-closed pada portal nakes, dan modal unggah perpanjangan SIP baru.
* **Dependencies**: `FE-HRD-187`, `FE-HRD-188`
* **Acceptance Criteria**: Status peringatan ter-render akurat mengikuti hari kalender; modal perpanjangan SIP memvalidasi batas 5 tahun (`VR-CRD-03`).
* **Verifikasi**: Uji visual rendering berbagai rentang tanggal kedaluwarsa.

#### 🟡 `FE-HRD-190` — Komponen Manajemen 3 Komite Mandiri & Katalog Whitebook Kompetensi
* **Slice / MVP**: `S-C1` / `MVP-43`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R18`. Laporan lama: [FE-HRD-190.md](../task/report/frontend/FE-HRD-190.md).
* **Outcome**: Modul konfigurasi master komite pada antarmuka admin untuk mengatur struktur pengurus 3 komite (Medik, Keperawatan, Nakes Lain) dan penugasan butir prosedur tindakan klinis ke dalam buku putih kompetensi.
* **Dependencies**: `FE-HRD-186`, `BE-HRD-211`
* **Acceptance Criteria**: Pengaturan komite terisolasi; penambahan tindakan klinis baru memuat derajat risiko dan kriteria supervisi baku.
* **Verifikasi**: Uji CRUD katalog whitebook komite.

#### 🟡 `FE-HRD-191` — Meja Kerja Asesmen Mitra Bestari / Peer Review Desk (`FE-HRD-189`)
* **Slice / MVP**: `S-C1` / `MVP-43`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R30`. Laporan lama: [FE-HRD-191.md](../task/report/frontend/FE-HRD-191.md).
* **Outcome**: Layar khusus reviewer Mitra Bestari pada rute `/corporate/hrd/credentialing/peer-review` untuk menelaah logbook pengalaman kasus pemohon, menetapkan rekomendasi derajat wewenang (4 Mandiri, 3 Supervisi, 1-2 Ditolak), dan menandatangani berita acara digital.
* **Dependencies**: `FE-HRD-190`
* **Acceptance Criteria**: Tombol submit rekomendasi aktif jika seluruh butir tindakan telah dinilai; sistem menolak akses reviewer jika pemohon adalah dirinya sendiri (`VR-CRD-15`).
* **Verifikasi**: Uji alur penilaian peer review.

#### 🟡 `FE-HRD-192` — Sidang Pleno Komite Kredensial & Konsolidasi Penilaian Reviewer (`FE-HRD-190`)
* **Slice / MVP**: `S-C1` / `MVP-43`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R30`. Laporan lama: [FE-HRD-192.md](../task/report/frontend/FE-HRD-192.md).
* **Outcome**: Layar sidang pengurus komite pada `/corporate/hrd/credentialing/committee-plenary` menampilkan komparasi rekomendasi Reviewer 1 vs Reviewer 2, tombol konsensus komite, catatan pembinaan etik, dan fitur generate surat rekomendasi Ketua Komite.
* **Dependencies**: `FE-HRD-191`, `BE-HRD-213`
* **Acceptance Criteria**: Matriks divergensi skor menyorot butir perbedaan pendapat antar reviewer; surat rekomendasi sah ter-generate bertanda tangan Ketua Komite.
* **Verifikasi**: Uji konsolidasi sidang pleno komite.

#### 🟡 `FE-HRD-193` — Konsol Pengesahan SPK & RKK Direktur Utama (*Director Approval Console* — `FE-HRD-191`)
* **Slice / MVP**: `S-C1` / `MVP-43`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R30`. Laporan lama: [FE-HRD-193.md](../task/report/frontend/FE-HRD-193.md).
* **Outcome**: Layar otorisasi eksekutif Direktur Utama RS pada `/corporate/hrd/credentialing/director-approval` dilengkapi live PDF viewer naskah SPK-RKK, panel validasi supervisor Level 3, paraf Direktur Pelayanan, dan modal otorisasi e-Sign Direktur Utama RS.
* **Dependencies**: `FE-HRD-192`, `BE-HRD-214`
* **Acceptance Criteria**: Live PDF merender tabel RKK rapi; e-Sign sukses mengunci dokumen SPK status `Active` dan menerbitkan token QR Code unik.
* **Verifikasi**: Uji live rendering PDF & eksekusi e-Sign Dirut.

#### 🟡 `FE-HRD-194` — Halaman Publik Responsif Verifikasi Keabsahan Dokumen SPK-RKK (`/public/credentials/verify/[qrToken]`)
* **Slice / MVP**: `S-C1` / `MVP-43`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R30`. Laporan lama: [FE-HRD-194.md](../task/report/frontend/FE-HRD-194.md).
* **Outcome**: Halaman publik tanpa login yang diakses saat kamera memindai barcode QR pada berkas fisik SPK-RKK, menampilkan status keabsahan dokumen, stempel rumah sakit, identitas dokter, masa berlaku 3 tahun, dan rincian hak klinis yang disahkan.
* **Dependencies**: `FE-HRD-193`
* **Acceptance Criteria**: Jika QR token valid dan dokumen aktif, muncul kartu hijau keabsahan; jika dokumen dicabut/kedaluwarsa, muncul banner merah pembatalan.
* **Verifikasi**: Uji pemindaian QR Code dari mobile browser.

#### 🟡 `FE-HRD-195` — Registri Kewenangan Klinis Rumah Sakit & Direktori Pencarian Nakes (`FE-HRD-192`)
* **Slice / MVP**: `S-C1` / `MVP-44`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R30`. Laporan lama: [FE-HRD-195.md](../task/report/frontend/FE-HRD-195.md).
* **Outcome**: Direktori pencarian instan kewenangan klinis pada `/corporate/hrd/credentialing/clinical-privilege-registry` untuk diakses oleh unit IGD, rawat inap, kamar operasi, dan farmasi guna memeriksa batasan kewenangan dokter/nakes bertugas.
* **Dependencies**: `FE-HRD-186`, `BE-HRD-216`
* **Acceptance Criteria**: Pencarian instan per dokter atau per tindakan merespons cepat; badge wewenang (Mandiri vs Supervisi) tampil jelas.
* **Verifikasi**: Uji pencarian & filtering registri klinis.

#### 🟡 `FE-HRD-196` — Dialog Penegakan Hard-Stop & Banner Lisensi Kedaluwarsa di SIMRS/EMR
* **Slice / MVP**: `S-C1` / `MVP-44`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Komponen atau layarnya tidak dapat dibuka dari rute mana pun. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R54`. Laporan lama: [FE-HRD-196.md](../task/report/frontend/FE-HRD-196.md).
* **Outcome**: Komponen dialog antarmuka SIMRS/EMR yang memblokir tombol order tindakan jika dokter tidak memiliki kewenangan (Level 1-2) atau SIP mati, menampilkan pesan penolakan resmi resolusi `HRD-DEC-005` & `HRD-DEC-149`.
* **Dependencies**: `FE-HRD-195`, `BE-HRD-216`
* **Acceptance Criteria**: Tombol submit dinonaktifkan keras (*Hard-Stop*); dialog konfirmasi penolakan menyajikan tautan regulasi RKK dokter.
* **Verifikasi**: Uji simulasi penolakan order tindakan di EMR.

#### 🟡 `FE-HRD-197` — Modal Persetujuan Co-Signature Supervisi & Timer 24 Jam Supervisor
* **Slice / MVP**: `S-C1` / `MVP-44`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Komponen atau layarnya tidak dapat dibuka dari rute mana pun. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R54`. Laporan lama: [FE-HRD-197.md](../task/report/frontend/FE-HRD-197.md).
* **Outcome**: Antarmuka meja kerja dokter supervisor untuk meninjau order tindakan di bawah supervisi Level 3 yang tertahan (*Pending Co-Signature*), dilengkapi timer hitung mundur 24 jam dan tombol persetujuan digital / pembatalan order.
* **Dependencies**: `FE-HRD-195`, `BE-HRD-217`
* **Acceptance Criteria**: Dokter supervisor dapat membubuhkan paraf digital persetujuan; order terbit penuh ke modul tindakan SIMRS.
* **Verifikasi**: Uji alur co-signature tindakan supervisi.

#### 🟡 `FE-HRD-198` — Komponen Life-Saving Emergency Override & Console Audit Insiden Mutu KMKP
* **Slice / MVP**: `S-C1` / `MVP-44`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R30`. Laporan lama: [FE-HRD-198.md](../task/report/frontend/FE-HRD-198.md).
* **Outcome**: Modal tombol darurat "Darurat Nyawa (Life-Saving Override)" pada antarmuka EMR dengan validasi input alasan medis cito, serta meja telaah audit insiden mutu di portal Komite Mutu & Keselamatan Pasien (KMKP) untuk menjatuhkan vonis telaah dalam 7 hari.
* **Dependencies**: `FE-HRD-196`, `BE-HRD-218`
* **Acceptance Criteria**: Bypass darurat mengizinkan eksekusi order dengan penanda merah; berkas audit otomatis masuk ke dashboard sidang KMKP.
* **Verifikasi**: Uji eksekusi override darurat & audit KMKP.

#### 🟡 `FE-HRD-199` — Konsol Penugasan Cepat Temporary & Emergency (`FE-HRD-193`) serta Radar Rekredensialing (`FE-HRD-194`)
* **Slice / MVP**: `S-C1` / `MVP-44`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R30`. Laporan lama: [FE-HRD-199.md](../task/report/frontend/FE-HRD-199.md).
* **Outcome**: Antarmuka penerbitan kewenangan sementara dokter tamu (maks 3 bln) dan darurat kebencanaan (maks 72 jam) pada rute `/corporate/hrd/credentialing/fast-track-privilege`, serta visualisasi timeline siklus 3 tahun rekredensialing dan daftar suspended nakes pada `/corporate/hrd/credentialing/recredentialing-cycle`.
* **Dependencies**: `FE-HRD-193`, `BE-HRD-219`, `BE-HRD-220`
* **Acceptance Criteria**: Fast-track privilege terbit dengan countdown jam otomatis; radar rekredensialing menampilkan status integrasi nilai OPPE Sub-15.
* **Verifikasi**: Uji penugasan cepat & monitoring siklus 3 tahun.

#### ✅ `FE-HRD-200` — Verifikasi Integrasi Menyeluruh Next.js, Linting 0 Error & UAT Acceptance `S-C1`
* **Slice / MVP**: `S-C1` / Verifikasi
* **Status**: ✅ PASS
* **Outcome**: Verifikasi kompilasi produksi Next.js App Router (`npm run build`), audit ketat linter CI/CD (`npm run lint:errors`) dengan hasil 0 error, verifikasi responsive rendering di desktop/tablet/mobile, serta verifikasi kelulusan skenario UAT 1-5.
* **Dependencies**: Seluruh task frontend `FE-HRD-186` s.d. `FE-HRD-199` dan `BE-HRD-221`
* **Acceptance Criteria**: Build standalone Next.js sukses; linter 0 error 0 warning; seluruh 8 layar baru beroperasi mulus dan aman.
* **Verifikasi**: Laporan task tracked `FE-HRD-200.md` dan output terminal npm.

---

## 18. Rencana Kerja Frontend Sub-Modul 15: OPPE, FPPE & Recredentialing (Slice `S-C1-OPPE`)

### 18.1 Grafik Urutan Dependency Frontend `S-C1-OPPE`

#### Grafik F54: Gelombang `MVP-45` (Client API, Redux Slice, Dashboard Radar OPPE & Form Peer Review Konfidensial)
```text
FE-HRD-201 (Client API & Redux Slice) ──┬──> FE-HRD-202 (Dashboard Radar OPPE Dokter FE-HRD-201)
BE-HRD-228 [BE] ───────────────────────┘
FE-HRD-201 [F54] ──> FE-HRD-203 (Konfigurasi Periode FE-HRD-202) ──> FE-HRD-204 (Modal Matriks KSM)
BE-HRD-223 [BE] ───┘
FE-HRD-201 [F54] ──> FE-HRD-205 (Form Peer Review FE-HRD-203) ──> FE-HRD-206 (Modal Disclaimer & Read-Audit)
BE-HRD-227 [BE] ───┘
```

#### Grafik F55: Gelombang `MVP-46` (Feed Pemicu FPPE, Pemantauan Proctoring & Sidang Pleno)
```text
FE-HRD-201 [F54] ──> FE-HRD-207 (Feed Pemicu FPPE FE-HRD-204) ──> FE-HRD-208 (Modal Inisiasi Proctoring)
BE-HRD-229 [BE] ───┘
FE-HRD-201 [F54] ──> FE-HRD-209 (Pemantauan Proctoring FE-HRD-205) ──> FE-HRD-210 (Modal Logbook Berpseudonim) ──┐
BE-HRD-231 [BE] ───┘                                                                                                │
                                                                                                                    ├──> FE-HRD-211 🟡 (Sidang Pleno Kesimpulan FE-HRD-206)
BE-HRD-232 [BE] ────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Grafik F56: Gelombang `MVP-47` (Kalkulator Komposit 3 Tahun, Override Diskresi & Konsol SIMRS Sync)
```text
FE-HRD-201 [F54] ──> FE-HRD-212 (Kalkulator 3 Tahun FE-HRD-207) ──> FE-HRD-213 (Modal Override Tier)
BE-HRD-233 [BE] ───┘
FE-HRD-212 [F56] ──> FE-HRD-214 🟡 (Konsol Sinkronisasi SIMRS EMR FE-HRD-208 & Modal Dispatch)
BE-HRD-235 [BE] ───┘
```

#### Grafik F57: Gelombang Verifikasi & Standalone Build Production
```text
FE-HRD-202 [F54] ──┐
FE-HRD-206 [F54] ──┼
FE-HRD-211 [F55] ──┼──> FE-HRD-215 ✅ (Verifikasi Build Standalone, Linting 0 Error & UAT Acceptance S-C1-OPPE)
FE-HRD-214 [F56] ──┤
BE-HRD-237 [BE] ───┘
```

---

### Tabel Gelombang Eksekusi Frontend `S-C1-OPPE`

| Gelombang | Task ID | Target Deliverable / Output Utama | Syarat Masuk (Dependencies) |
| :---: | :--- | :--- | :--- |
| **`MVP-45`** | `FE-HRD-201`, `FE-HRD-202`, `FE-HRD-203`, `FE-HRD-204`, `FE-HRD-205`, `FE-HRD-206` | Inisialisasi Redux state `clinicalPerformanceSlice.ts`, Axios client API Kontrak `v19`, dashboard radar chart 6 pilar ACGME/KARS dokter spesialis, antarmuka manajemen periode OPPE KSM, modal konfigurasi target ambang indikator, form evaluasi sejawat konfidensial, serta modal disclaimer hak istimewa *Peer Review Privilege* dengan perekaman *read-audit log*. | Disetujuinya Kontrak `v19` & `BE-HRD-223`, `BE-HRD-227`, `BE-HRD-228` |
| **`MVP-46`** | `FE-HRD-207`, `FE-HRD-208`, `FE-HRD-209`, `FE-HRD-210`, `FE-HRD-211` | Dashboard feed deteksi pemicu kritis FPPE real-time, modal penugasan proctoring dokter baru & prosedur baru, dashboard pemantauan target kasus proctoring, modal entri logbook kasus berpseudonim, serta meja sidang pleno kesimpulan FPPE dan perumusan rencana pembinaan klinis. | `FE-HRD-201`, `BE-HRD-229`, `BE-HRD-231`, `BE-HRD-232` |
| **`MVP-47`** | `FE-HRD-212`, `FE-HRD-213`, `FE-HRD-214` | Layanan kalkulator rapor komposit 3 tahun rekredensialing (25%-35%-40%), kartu visualisasi tier kelayakan (Hijau, Kuning, Merah), modal diskresi override Ketua Komite Medik, serta konsol monitoring sinkronisasi 4 putusan rekredensialing ke modul EMR SIMRS (`INT-HR-76`) beserta audit status lockout. | `FE-HRD-201`, `BE-HRD-233`, `BE-HRD-235` |
| **Verifikasi** | `FE-HRD-215` | Audit linting ketat `npm run lint:errors` dengan hasil 0 error, verifikasi kompilasi produksi standalone `npm run build`, audit responsive rendering di desktop/tablet/mobile, serta verifikasi kelulusan skenario `UAT-OPP-01` s.d. `UAT-OPP-05`. | Seluruh task `FE-HRD-201` s.d. `FE-HRD-214` dan `BE-HRD-237` selesai |

---

### Detail Spesifikasi Task Frontend (`FE-HRD-201` s.d. `FE-HRD-215`)

#### ✅ `FE-HRD-201` — Inisialisasi Redux Slice `clinicalPerformanceSlice.ts`, Axios Client API & DTO Types Kontrak `v19`
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: ✅ PASS
* **Outcome**: Deklarasi modul Redux state slice `clinicalPerformanceSlice.ts`, adapter Axios terisolasi untuk 24 endpoint API Kontrak `v19`, DTO interface TypeScript (indikator ACGME, periode evaluasi, draf rapor, pemicu FPPE, logbook kasus, rapor 3 tahun, status sinkronisasi SIMRS), serta middleware error handler ramah pengguna.
* **Dependencies**: Disetujuinya Kontrak `v19` & Blueprint Rev 44
* **Acceptance Criteria**: State Redux dan action creators terpasang rapi; Axios client menangani HTTP 200, 400, 403, 404, dan 502 secara graceful; types TypeScript selaras 100% dengan kontrak backend.
* **Verifikasi**: Uji unit Redux & verifikasi tipe TypeScript.

#### 🟡 `FE-HRD-202` — Dashboard Kinerja Klinis & Visualisasi Radar OPPE Dokter Spesialis (`FE-HRD-201`)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R20`. Laporan lama: [FE-HRD-202.md](../task/report/frontend/FE-HRD-202.md).
* **Outcome**: Halaman dashboard utama performa klinis pada rute `/portal/performance/clinical-oppe`, radar chart interaktif 6 area kompetensi standar ACGME/KARS, kartu nilai komposit akhir (skor 60% SIMRS + 40% Peer Review), indikator kelulusan standar, riwayat pencapaian tahun sebelumnya, serta tombol unduh rapor resmi PDF bertanda tangan digital komite.
* **Dependencies**: `FE-HRD-201`, `BE-HRD-228`
* **Acceptance Criteria**: Radar chart menampilkan 6 pilar kompetensi secara responsif; rincian breakdown nilai 60:40 tersaji transparan; unduh rapor PDF berfungsi.
* **Verifikasi**: Uji fungsional visual `AT-HRD-OPP-08` dan `UAT-OPP-01`.

#### 🟡 `FE-HRD-203` — Manajemen Periode & Pengaturan Matriks Indikator OPPE KSM (`FE-HRD-202`)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R20`. Laporan lama: [FE-HRD-203.md](../task/report/frontend/FE-HRD-203.md).
* **Outcome**: Antarmuka bagi Subkomite Mutu Profesi pada rute `/admin/performance/oppe-periods`, tabel siklus periode evaluasi tahunan/semesteran, filter status periode (*Draft*, *Active*, *EvaluationClosed*, *Finalized*), tombol aksi pembukaan periode baru, penguncian entri, dan finalisasi rapor KSM.
* **Dependencies**: `FE-HRD-201`, `BE-HRD-223`
* **Acceptance Criteria**: Admin dapat membuka periode evaluasi baru; validasi form mencegah submit jika bobot kuantitatif dan kualitatif tidak tepat 100% (`VR-OPP-01`).
* **Verifikasi**: Uji fungsional form `AT-HRD-OPP-01` dan `AT-HRD-OPP-02`.

#### 🟡 `FE-HRD-204` — Modal Pengaturan Ambang Batas & Bobot Indikator Spesifik KSM (`OppeIndicatorConfigModal`)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R20`. Laporan lama: [FE-HRD-204.md](../task/report/frontend/FE-HRD-204.md).
* **Outcome**: Komponen modal dialog interaktif untuk menyesuaikan ambang batas target numerik dan persentase kontribusi bobot indikator khusus KSM (misal: IDO Bedah < 1.5%, keterlambatan resume EMR Penyakit Dalam < 5%, kepatuhan CP Anak > 90%).
* **Dependencies**: `FE-HRD-203`
* **Acceptance Criteria**: Form modal memvalidasi tipe operator (GTE, LTE, EQ, BETWEEN); perubahan target tersimpan ke master indikator KSM.
* **Verifikasi**: Uji konfigurasi indikator spesifik KSM.

#### 🟡 `FE-HRD-205` — Formulir Evaluasi Kualitatif Peer Review Sejawat KSM (`FE-HRD-203`)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R20`. Laporan lama: [FE-HRD-205.md](../task/report/frontend/FE-HRD-205.md).
* **Outcome**: Lembar instrumen telaah sejawat pada rute `/portal/performance/peer-review-entry`, daftar dokter sejawat yang ditugaskan untuk dinilai, skala penilaian Likert 1-5 untuk domain etika profesi, komunikasi efektif SBAR, dan partisipasi audit mortalitas/morbiditas, serta validasi catatan wajib jika skor <= 2 (`VR-OPP-04`).
* **Dependencies**: `FE-HRD-201`
* **Acceptance Criteria**: Form Likert berfungsi mulus; pesan peringatan muncul dan submit terkunci jika penilai memberi nilai <= 2 tanpa catatan naratif minimal 25 karakter.
* **Verifikasi**: Uji validasi kualitatif `AT-HRD-OPP-04` dan `AT-HRD-OPP-06`.

#### 🟡 `FE-HRD-206` — Komponen Peringatan Kerahasiaan Hak Istimewa Peer Review Privilege & Modal Read-Audit Log
* **Slice / MVP**: `S-C1-OPPE` / `MVP-45`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R20`. Laporan lama: [FE-HRD-206.md](../task/report/frontend/FE-HRD-206.md).
* **Outcome**: Komponen dialog peringatan hukum *Peer Review Privilege* (`PeerReviewConfidentialityWarningModal`) yang wajib disetujui pengguna sebelum lembar ulasan kualitatif sejawat terbuka, disertai pemanggilan API seketika untuk mencatat log pembacaan (*read-audit log*) ke database forensik.
* **Dependencies**: `FE-HRD-205`, `BE-HRD-227`
* **Acceptance Criteria**: Dokumen konfidensial tertutup blur sebelum tombol "Saya Menyetujui Ketentuan Kerahasiaan" diklik; klik memicu request pencatatan audit log `PeerReviewDocumentRead`.
* **Verifikasi**: Uji privasi & keamanan audit `AT-HRD-OPP-07` dan `UAT-OPP-02`.

#### 🟡 `FE-HRD-207` — Dashboard Pusat Deteksi Pemicu Kritis & Pelacakan Kasus FPPE (`FE-HRD-204`)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-46`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R32`. Laporan lama: [FE-HRD-207.md](../task/report/frontend/FE-HRD-207.md).
* **Outcome**: Antarmuka pusat pengawasan insiden keselamatan klinis pada rute `/admin/performance/fppe-triggers`, feed real-time alert kejadian sentinel, komplikasi bedah > 2 SD, penundaan resume EMR > 48 jam > 20%, serta tabel daftar kasus penugasan FPPE berjalan.
* **Dependencies**: `FE-HRD-201`, `BE-HRD-229`
* **Acceptance Criteria**: Insiden sentinel ditandai dengan badge merah berkedip prioritas fatal; rincian pemicu menyajikan data insiden SIMRS/KMKP pendukung.
* **Verifikasi**: Uji feed deteksi insiden `AT-HRD-OPP-10`, `AT-HRD-OPP-11`, dan `UAT-OPP-03`.

#### 🟡 `FE-HRD-208` — Modal Inisiasi Penugasan FPPE & Penetapan Dokter Proctor Senior
* **Slice / MVP**: `S-C1-OPPE` / `MVP-46`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R32`. Laporan lama: [FE-HRD-208.md](../task/report/frontend/FE-HRD-208.md).
* **Outcome**: Modal dialog pembuatan berkas penugasan FPPE terarah, pemilihan pemicu insiden / rujukan etik, penetapan target kasus observasi (default: 10 kasus staf baru, 5 kasus prosedur baru), serta pemilihan Dokter Proctor senior pendamping dengan validasi masa kerja klinis.
* **Dependencies**: `FE-HRD-207`
* **Acceptance Criteria**: Form memvalidasi pengisian target kasus dan durasi observasi; penugasan FPPE sukses terbentuk dengan status `InProgress`.
* **Verifikasi**: Uji inisiasi penugasan proctoring `AT-HRD-OPP-13` dan `AT-HRD-OPP-14`.

#### 🟡 `FE-HRD-209` — Dashboard Pemantauan Proctoring Staf Baru & Prosedur Baru (`FE-HRD-205`)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-46`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R32`. Laporan lama: [FE-HRD-209.md](../task/report/frontend/FE-HRD-209.md).
* **Outcome**: Dashboard kemajuan proctoring pada rute `/admin/performance/fppe-proctoring`, kartu progres kasus (misal: 7/10 kasus terpenuhi), hitung mundur sisa hari masa observasi (3 bulan / 6 bulan), status verifikasi proctor, serta daftar logbook tindakan klinis.
* **Dependencies**: `FE-HRD-201`
* **Acceptance Criteria**: Visual progress bar menampilkan persentase ketercapaian target kasus secara akurat; warning banner muncul jika masa observasi < 14 hari namun kasus belum lengkap.
* **Verifikasi**: Uji pemantauan progres kasus proctoring `UAT-OPP-04`.

#### 🟡 `FE-HRD-210` — Modal Entri Logbook Kasus Terpseudonim & Antarmuka Verifikasi Proctor (`FppeCaseLogEntryModal`)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-46`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R32`. Laporan lama: [FE-HRD-210.md](../task/report/frontend/FE-HRD-210.md).
* **Outcome**: Modal pencatatan kasus tindakan operasi/tindakan medis terproctoring dengan input nomor rekam medis yang otomatis disamarkan (*pseudonimisasi*, `VR-OPP-11`), input tanggal tindakan, serta antarmuka verifikasi bagi Dokter Proctor untuk menilai kemandirian klinis (Level 1-4) dan mencatat observasi komplikasi.
* **Dependencies**: `FE-HRD-209`, `BE-HRD-231`
* **Acceptance Criteria**: Nomor RM pasien terenkripsi/terpseudonimkan pada antarmuka; Dokter Proctor dapat menyetujui verifikasi dengan membubuhkan tanda tangan digital.
* **Verifikasi**: Uji privasi data RM & verifikasi proctor `AT-HRD-OPP-15` dan `AT-HRD-OPP-16`.

#### 🟡 `FE-HRD-211` — Sidang Pleno Kesimpulan & Rekomendasi Wewenang FPPE (`FE-HRD-206`)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-46`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R32`. Laporan lama: [FE-HRD-211.md](../task/report/frontend/FE-HRD-211.md).
* **Outcome**: Meja kerja persidangan Subkomite Mutu Profesi pada rute `/admin/performance/fppe-conclusions`, ringkasan rekapitulasi kasus proctoring dan tingkat komplikasi, formulir penetapan putusan pleno (Lolos Penuh, Perpanjang Proctoring, Supervisi, atau Cabut Wewenang), editor rencana pembinaan (*Remediation Plan*), serta pengesahan digital Ketua Komite Medik.
* **Dependencies**: `FE-HRD-210`, `BE-HRD-232`
* **Acceptance Criteria**: Putusan pleno tersimpan sah; opsi perpanjangan dibatasi maksimal 3 bulan; status RKK dokter seketika terbarui pasca-sidang.
* **Verifikasi**: Uji sidang pleno FPPE `AT-HRD-OPP-17`, `AT-HRD-OPP-18`, dan `UAT-OPP-04`.

#### 🟡 `FE-HRD-212` — Kalkulator Rapor Komposit 3 Tahun Rekredensialing & Visualisasi Tier (`FE-HRD-207`)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-47`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R32`. Laporan lama: [FE-HRD-212.md](../task/report/frontend/FE-HRD-212.md).
* **Outcome**: Antarmuka kalkulator rapor mutu 3 tahunan pada rute `/admin/credentialing/recred-composite-scorecard`, tabel perbandingan skor OPPE tahun ke-1 (25%), tahun ke-2 (35%), dan tahun ke-3 (40%), kartu skor akhir komposit, badge visual tier kelayakan: Tier Hijau (*Fast-Track*), Tier Kuning (*Clarification*), dan Tier Merah (*Plenary Investigation*), serta ringkasan catatan pembinaan.
* **Dependencies**: `FE-HRD-201`, `BE-HRD-233`
* **Acceptance Criteria**: Komposit 3 tahun terhitung deterministik; kartu tier menampilkan rekomendasi alur sidang rekredensialing secara visual dan jelas.
* **Verifikasi**: Uji algoritma kalkulasi komposit `AT-HRD-OPP-19`, `AT-HRD-OPP-20`, dan `UAT-OPP-05`.

#### 🟡 `FE-HRD-213` — Modal Diskresi Penyesuaian Tier Kelayakan Rekredensialing (`RecredentialingTierOverrideModal`)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-47`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R32`. Laporan lama: [FE-HRD-213.md](../task/report/frontend/FE-HRD-213.md).
* **Outcome**: Modal dialog otorisasi khusus Ketua Komite Medik untuk menyesuaikan tier kelayakan kalkulator (misal menaikkan/menurunkan tier nakes), dilengkapi form wajib input alasan justifikasi klinis tertulis dan unggah berkas nota pertimbangan (`VR-OPP-19`).
* **Dependencies**: `FE-HRD-212`
* **Acceptance Criteria**: Penyesuaian tier terkunci jika kolom justifikasi kosong; event audit `RecredentialingTierOverridden` tercatat di riwayat berkas.
* **Verifikasi**: Uji diskresi otorisasi tier `AT-HRD-OPP-21`.

#### 🟡 `FE-HRD-214` — Konsol Sinkronisasi Putusan Rekredensialing ke SIMRS EMR & Audit Bypass (`FE-HRD-208`)
* **Slice / MVP**: `S-C1-OPPE` / `MVP-47`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R32`. Laporan lama: [FE-HRD-214.md](../task/report/frontend/FE-HRD-214.md).
* **Outcome**: Konsol pengendalian sinkronisasi wewenang klinis pada rute `/admin/credentialing/recred-simrs-sync`, pemantauan status transmisi 4 putusan (Perpanjang As-Is & Upgrade -> Level 4 Mandiri, Restricted -> Level 3 Supervisi co-sign 24 jam, Revoked -> Level 1-2 Hard-Stop Blokir Total), modal inspeksi payload JSON (`SimrsClinicalPrivilegeSyncModal`), tombol retry transmisi, serta log jejak audit percobaan akses yang ditolak oleh SIMRS.
* **Dependencies**: `FE-HRD-212`, `BE-HRD-235`
* **Acceptance Criteria**: Status sinkronisasi ke modul EMR SIMRS terpantau real-time; pengujian simulasi putusan Revoked memicu konfirmasi lockout Hard-Stop seketika.
* **Verifikasi**: Uji integrasi SIMRS & penegakan Hard-Stop `AT-HRD-OPP-22` dan `UAT-OPP-05`.

#### ✅ `FE-HRD-215` — Verifikasi Integrasi Menyeluruh Next.js, Linting 0 Error & UAT Acceptance `S-C1-OPPE`
* **Slice / MVP**: `S-C1-OPPE` / Verifikasi
* **Status**: ✅ PASS
* **Outcome**: Verifikasi kompilasi produksi Next.js App Router (`npm run build`), audit ketat linter CI/CD (`npm run lint:errors`) dengan hasil 0 error, verifikasi responsive rendering di desktop/tablet/mobile, serta verifikasi kelulusan skenario `UAT-OPP-01` s.d. `UAT-OPP-05`.
* **Dependencies**: Seluruh task frontend `FE-HRD-201` s.d. `FE-HRD-214` dan `BE-HRD-237`
* **Acceptance Criteria**: Build standalone Next.js sukses; linter 0 error 0 warning; seluruh 8 layar baru beroperasi mulus dan aman.
* **Verifikasi**: Laporan task tracked `FE-HRD-215.md` dan output terminal npm.


## 19. Rencana Kerja Frontend Sub-Modul 16: Learning, Training & Competency (Slice `S-C2`)

### 19.1 Grafik Urutan Dependency Frontend `S-C2`

#### Grafik F58: Gelombang `MVP-48` (Client API, Redux Slice, Dashboard Radar & Aturan Wajib STARKES)
```text
FE-HRD-216 (Client API & Redux Slice) ──┬──> FE-HRD-217 (Dashboard Pelatihan & Radar Layar FE-HRD-216)
BE-HRD-240 [BE] ───────────────────────┘
FE-HRD-216 [F58] ──> FE-HRD-218 (Konfigurasi Aturan Wajib Layar FE-HRD-217) ──> FE-HRD-219 (Modal Aturan Wajib & Ambang Nilai)
BE-HRD-239 [BE] ───┘
```

#### Grafik F59: Gelombang `MVP-49` (Manajemen Sesi Diklat, Presensi QR, Lembar Ujian OSCE & e-Sertifikat)
```text
FE-HRD-216 [F58] ──> FE-HRD-220 (Manajemen Sesi Pelatihan Layar FE-HRD-218) ──> FE-HRD-221 (Modal Presensi QR / RFID)
BE-HRD-243 [BE] ───┘
FE-HRD-216 [F58] ──> FE-HRD-222 (Lembar Ujian Teori & OSCE Layar FE-HRD-219) ──> FE-HRD-223 (Modal Rubrik OSCE & e-Sign Assessor) ──┐
BE-HRD-248 [BE] ───┘                                                                                                                  │
                                                                                                                                     ├──> FE-HRD-224 🟡 (Registri Portofolio e-Sertifikat FE-HRD-220)
BE-HRD-249 [BE] ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Grafik F60: Gelombang `MVP-50` (TNA, Kontrol Anggaran, Ikatan Dinas & SKP Plataran Sehat)
```text
FE-HRD-216 [F58] ──> FE-HRD-225 (Rencana Tahunan TNA & Pagu Anggaran Layar FE-HRD-221)
BE-HRD-245 [BE] ───┘
FE-HRD-216 [F58] ──> FE-HRD-226 (Tata Kelola Ikatan Dinas Retensi Layar FE-HRD-222) ──> FE-HRD-227 (Modal Draf Akta Ikatan Dinas)
BE-HRD-251 [BE] ───┘
FE-HRD-216 [F58] ──> FE-HRD-228 (Portofolio 50 SKP 5 Tahunan Layar FE-HRD-223) ──> FE-HRD-229 (Modal Sinkronisasi Plataran Sehat)
BE-HRD-252 [BE] ───┘
```

#### Grafik F61: Gelombang Verifikasi & Kepatuhan Arsitektur `S-C2`
```text
FE-HRD-217 [F58] ──┐
FE-HRD-224 [F59] ──┼──> FE-HRD-230 ⏳ (Verifikasi Menyeluruh Next.js App Router, Linting 0 Error & UAT Acceptance S-C2)
FE-HRD-227 [F60] ──┤
FE-HRD-229 [F60] ──┤
BE-HRD-253 [BE] ───┘
```

---

### 19.2 Tabel Gelombang Eksekusi Frontend `S-C2`

| Gelombang | Task ID | Nama Task Ringkas | Prasyarat (Dependencies) | Target Deliverables & Layar |
| :--- | :--- | :--- | :--- | :--- |
| **`MVP-48`** | `FE-HRD-216` | Redux Slice `learningDevelopmentSlice.ts`, Axios Client & DTO Types | `BE-HRD-239`, `BE-HRD-240` | Fondasi client HTTP Axios, TypeScript DTO Kontrak `v20`, store Redux |
| **`MVP-48`** | `FE-HRD-217` | Dashboard Pelatihan Pegawai & Radar Kedaluwarsa | `FE-HRD-216`, `BE-HRD-240` | Layar `FE-HRD-216` (`/hr/learning/dashboard`), radar H-90/60/30 |
| **`MVP-48`** | `FE-HRD-218` | Konfigurasi Aturan Pelatihan Wajib Akreditasi RS | `FE-HRD-216`, `BE-HRD-239` | Layar `FE-HRD-217` (`/hr/learning/mandatory-rules`), tabel katalog 5 diklat |
| **`MVP-48`** | `FE-HRD-219` | Modal Konfigurasi Aturan Pelatihan Wajib & Ambang | `FE-HRD-218` | Komponen `MandatoryTrainingRuleModal`, form validasi 24 bulan & ambang |
| **`MVP-49`** | `FE-HRD-220` | Manajemen Sesi Pelatihan, Kalender Batch & Kuota | `FE-HRD-216`, `BE-HRD-243` | Layar `FE-HRD-218` (`/hr/learning/sessions`), kalender kelas & filter batch |
| **`MVP-49`** | `FE-HRD-221` | Modal Presensi QR Code / RFID & Rekapitulasi Hadir | `FE-HRD-220`, `BE-HRD-246` | Komponen `TrainingAttendanceQrModal`, scanner check-in & hitung durasi |
| **`MVP-49`** | `FE-HRD-222` | Lembar Penilaian Asesmen Teori Post-Test & OSCE | `FE-HRD-216`, `BE-HRD-247` | Layar `FE-HRD-219` (`/hr/learning/assessments`), filter ujian nakes |
| **`MVP-49`** | `FE-HRD-223` | Modal Rubrik Checklist OSCE & Tanda Tangan Assessor | `FE-HRD-222`, `BE-HRD-248` | Komponen `OsceClinicalEvaluationModal`, canvas digital signature assessor |
| **`MVP-49`** | `FE-HRD-224` | Registri Portofolio e-Sertifikat & Arsip e-Dossier | `FE-HRD-220`, `FE-HRD-222`, `BE-HRD-249` | Layar `FE-HRD-220` (`/hr/learning/certificates`), preview PDF & unduh |
| **`MVP-50`** | `FE-HRD-225` | Rencana Program Tahunan TNA & Monitoring Pagu Anggaran | `FE-HRD-216`, `BE-HRD-242`, `BE-HRD-245` | Layar `FE-HRD-221` (`/hr/learning/annual-plans`), progress bar pagu unit |
| **`MVP-50`** | `FE-HRD-226` | Tata Kelola Kontrak Ikatan Dinas Retensi Nakes | `FE-HRD-216`, `BE-HRD-250` | Layar `FE-HRD-222` (`/hr/learning/bonds`), tabel masa retensi & status |
| **`MVP-50`** | `FE-HRD-227` | Modal Draf Akta Ikatan Dinas & Tagihan Penalti Resign | `FE-HRD-226`, `BE-HRD-251` | Komponen `TrainingBondContractModal`, rincian kalkulator denda ganti rugi |
| **`MVP-50`** | `FE-HRD-228` | Portofolio 50 SKP Nakes & Visual Progress Radial | `FE-HRD-216`, `BE-HRD-252` | Layar `FE-HRD-223` (`/hr/learning/skp-tracker`), radial chart 3 ranah SKP |
| **`MVP-50`** | `FE-HRD-229` | Modal Verifikasi ID Sertifikat Plataran Sehat Kemenkes | `FE-HRD-228`, `BE-HRD-252` | Komponen `PlataranSehatSyncModal`, tombol sinkronisasi SatuSehat SDMK |
| **Verifikasi** | `FE-HRD-230` | Verifikasi Menyeluruh Next.js, Linting 0 Error & UAT | `FE-HRD-217`, `FE-HRD-224`, `FE-HRD-227`, `FE-HRD-229`, `BE-HRD-253` | Build standalone sukses, 0 lint error, kelulusan skenario `UAT-01..05` |

---

### 19.3 Detail Spesifikasi Task Frontend (`FE-HRD-216` s.d. `FE-HRD-230`)

#### ✅ `FE-HRD-216` — Inisialisasi Redux Slice `learningDevelopmentSlice.ts`, Axios Client API & DTO Types Kontrak `v20`
* **Slice / MVP**: `S-C2` / `MVP-48`
* **Status**: ✅ PASS
* **Outcome**: Pembuatan state Redux `learningDevelopmentSlice.ts` pada folder `@/store/slices/hr/`, deklarasi antarmuka TypeScript untuk 24 DTO endpoint Kontrak `v20`, konfigurasi endpoint Axios client di `@/services/api/hr/learningService.ts`, penanganan error terpusat (`400`, `422`, `409`, `502`), serta penyiapan selektor state loading dan caching data.
* **Dependencies**: `BE-HRD-239`, `BE-HRD-240` (Kontrak API `v20`).
* **Acceptance Criteria**: Type definition TypeScript 100% cocok dengan skema Swagger Kontrak `v20`; store Redux terdaftar pada root store tanpa konflik kompilasi.
* **Verifikasi**: `npm run build` dan audit linting TypeScript.

#### 🟡 `FE-HRD-217` — Dashboard Pelatihan Pegawai, Sertifikasi & Radar Kedaluwarsa (`FE-HRD-216`)
* **Slice / MVP**: `S-C2` / `MVP-48`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-217.md](../task/report/frontend/FE-HRD-217.md).
* **Outcome**: Pembangunan layar dasbor komprehensif pada rute `/hr/learning/dashboard` (Layar `FE-HRD-216`), mencakup kartu metrik KPI kepatuhan akreditasi rumah sakit (Target 100%), visualisasi radar kedaluwarsa nakes bergradasi warna (Kuning H-90, Oranye H-60, Merah Muda H-30, Merah Menyala H-0 Overdue), penandaan staf dengan safeguard shift *buddy system*, tabel peringatan dini, serta tombol penyiaran pesan peringatan perpanjangan massal.
* **Dependencies**: `FE-HRD-216`, `BE-HRD-240`.
* **Acceptance Criteria**: Dasbor menampilkan statistik kepatuhan akreditasi secara real-time; kartu radar kedaluwarsa merespons filter unit kerja dan profesi.
* **Verifikasi**: Uji visual dashboard dan skenario radar kedaluwarsa `AT-HRD-TRN-03`, `AT-HRD-TRN-04`.

#### 🟡 `FE-HRD-218` — Konfigurasi Aturan Pelatihan Wajib Akreditasi & Penugasan Unit (`FE-HRD-217`)
* **Slice / MVP**: `S-C2` / `MVP-48`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-218.md](../task/report/frontend/FE-HRD-218.md).
* **Outcome**: Pembangunan layar pengelolaan master aturan pelatihan wajib pada rute `/hr/learning/mandatory-rules` (Layar `FE-HRD-217`), tabel interaktif daftar 5 pelatihan wajib akreditasi STARKES dan pelatihan klinis khusus ruang intensif, badge penugasan lingkup target pegawai (100% Seluruh Pegawai / Unit Khusus), kolom masa berlaku (24 Bulan), kolom grace period, sakelar keaktifan aturan, serta tombol pembuka modal konfigurasi.
* **Dependencies**: `FE-HRD-216`, `BE-HRD-239`.
* **Acceptance Criteria**: Tabel menampilkan seluruh parameter aturan pelatihan wajib; proteksi tombol hapus untuk 5 pelatihan dasar akreditasi rumah sakit (`VR-TRN-01`).
* **Verifikasi**: Uji navigasi dan render katalog aturan `AT-HRD-TRN-01`.

#### 🟡 `FE-HRD-219` — Modal Konfigurasi Aturan Pelatihan Wajib & Ambang Nilai (`MandatoryTrainingRuleModal`)
* **Slice / MVP**: `S-C2` / `MVP-48`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-219.md](../task/report/frontend/FE-HRD-219.md).
* **Outcome**: Komponen dialog modal `@/components/features/learning/MandatoryTrainingRuleModal.tsx` menggunakan `BaseFormControl` Quilvian, form input kode pelatihan, judul pelatihan, pilihan kategori dan target nakes, field numerik masa berlaku (validasi klien maks 24 bulan, `VR-TRN-02`), ambang minimal presensi (80% / 100%), passing score teori (>= 75), sakelar kewajiban simulasi OSCE dan passing score OSCE (>= 80), serta sakelar dampak safeguard shift dinas.
* **Dependencies**: `FE-HRD-218`.
* **Acceptance Criteria**: Validasi form mencegah input masa berlaku melampaui 24 bulan; pesan error muncul tepat di bawah field jika input tidak valid.
* **Verifikasi**: Uji validasi batas masa berlaku dan ambang kelulusan `AT-HRD-TRN-02`.

#### 🟡 `FE-HRD-220` — Manajemen Sesi Pelatihan, Kalender Batch Kelas & Presensi Kehadiran (`FE-HRD-218`)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-220.md](../task/report/frontend/FE-HRD-220.md).
* **Outcome**: Pembangunan layar kalender dan batch kelas pelatihan pada rute `/hr/learning/sessions` (Layar `FE-HRD-218`), tampilan kalender bulanan dan tabel list sesi diklat internal/eksternal, visualisasi sisa kuota kapasitas ruang/lab simulator (`VR-TRN-07`), badge status sesi (`Scheduled`, `RegistrationOpen`, `InProgress`, `AssessmentPhase`, `Completed`), tombol buka pendaftaran, dan tombol pintas pembuka modal absensi presensi harian.
* **Dependencies**: `FE-HRD-216`, `BE-HRD-243`.
* **Acceptance Criteria**: Batch kelas terkelompokkan rapi berdasarkan jadwal dan status; indikator kuota penuh tampil jelas dan mencegah pendaftaran berlebih.
* **Verifikasi**: Uji kalender sesi dan kuota `AT-HRD-TRN-09`.

#### 🟡 `FE-HRD-221` — Modal Presensi QR Code / RFID & Rekapitulasi Hadir Sesi (`TrainingAttendanceQrModal`)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-221.md](../task/report/frontend/FE-HRD-221.md).
* **Outcome**: Komponen modal dialog `@/components/features/learning/TrainingAttendanceQrModal.tsx` untuk menampilkan QR Code absensi dinamis per hari sesi, simulator pemindaian RFID/kartu pegawai, tabel rekapitulasi kehadiran peserta real-time, perhitungan persentase jam hadir kumulatif, serta visual badge kelayakan mengikuti ujian kelulusan (Hijau: Memenuhi Syarat >=80%/100%, Merah: Gugur Presensi, `VR-TRN-03`).
* **Dependencies**: `FE-HRD-220`, `BE-HRD-246`.
* **Acceptance Criteria**: Pemindaian presensi mencatat log secara instan; sistem menampilkan peringatan visual jika kehadiran staf klinis kurang dari 100%.
* **Verifikasi**: Uji presensi QR dan validasi kelayakan ujian `AT-HRD-TRN-12`.

#### 🟡 `FE-HRD-222` — Lembar Penilaian Asesmen Teori Post-Test & Ujian Simulasi Klinis OSCE (`FE-HRD-219`)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-222.md](../task/report/frontend/FE-HRD-222.md).
* **Outcome**: Pembangunan layar kerja penguji dan bagian Diklat pada rute `/hr/learning/assessments` (Layar `FE-HRD-219`), tabel rekapitulasi peserta sesi yang lolos presensi, kolom input cepat nilai teori post-test, penanda kelulusan teori (Passing Grade >= 75.00, `VR-TRN-04`), indikator percobaan remedial (1/2, 2/2, `VR-TRN-20`), tombol buka lembar rubrik OSCE klinis, serta tombol eksekusi penerbitan sertifikat bagi peserta yang lulus komprehensif.
* **Dependencies**: `FE-HRD-216`, `BE-HRD-247`.
* **Acceptance Criteria**: Pemisahan tegas nilai teori dan praktik; peserta yang belum lulus salah satu komponen tidak dapat diterbitkan sertifikatnya.
* **Verifikasi**: Uji lembar kerja penilaian dan alur remedial `AT-HRD-TRN-13`, `AT-HRD-TRN-15`.

#### 🟡 `FE-HRD-223` — Modal Rubrik Checklist OSCE & Tanda Tangan Digital Assessor (`OsceClinicalEvaluationModal`)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 1 dari 1 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-223.md](../task/report/frontend/FE-HRD-223.md).
* **Outcome**: Komponen modal dialog `@/components/features/learning/OsceClinicalEvaluationModal.tsx` untuk pengujian simulasi klinis langsung (misal Resusitasi Jantung Paru, Pemasangan CVC, Intubasi), menampilkan tabel rubrik checklist butir keterampilan, kalkulasi otomatis skor OSCE (Passing Grade >= 80.00, `VR-TRN-05`), field textarea umpan balik pembinaan instruktur, serta kanvas tanda tangan digital / otentikasi PIN dokter assessor penguji (`VR-TRN-06`).
* **Dependencies**: `FE-HRD-222`, `BE-HRD-248`.
* **Acceptance Criteria**: Form mengunci tombol simpan sebelum seluruh butir rubrik terisi dan tanda tangan digital terverifikasi sah.
* **Verifikasi**: Uji rubrik evaluasi OSCE dan tanda tangan penguji `AT-HRD-TRN-14`.

#### 🟡 `FE-HRD-224` — Registri Portofolio e-Sertifikat Pegawai & Verifikasi Dokumen Eksternal (`FE-HRD-220`)
* **Slice / MVP**: `S-C2` / `MVP-49`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-224.md](../task/report/frontend/FE-HRD-224.md).
* **Outcome**: Pembangunan layar portofolio sertifikat pada rute `/hr/learning/certificates` (Layar `FE-HRD-220`), tabel seluruh sertifikat kepegawaian internal dan eksternal, nomor seri registrasi baku (`SRT/HRD-TRN/...`), tanggal terbit, masa berlaku, status (`Active`, `ExpiringSoon`, `Expired`, `Revoked`), tombol pratinjau dokumen PDF di e-Dossier Sub-02 (`INT-HR-72`), tombol verifikasi keabsahan sertifikat eksternal oleh tim Diklat (`VR-TRN-19`), serta tombol unduh e-Sertifikat ber-QR Code.
* **Dependencies**: `FE-HRD-220`, `FE-HRD-222`, `BE-HRD-249`.
* **Acceptance Criteria**: Sertifikat internal terbit otomatis pasca penilaian selesai; dokumen eksternal berstatus `Unverified` hingga disetujui staf Diklat.
* **Verifikasi**: Uji registri sertifikat dan unduh PDF `AT-HRD-TRN-16`.

#### 🟡 `FE-HRD-225` — Rencana Program Tahunan TNA & Monitoring Pagu Anggaran Unit (`FE-HRD-221`)
* **Slice / MVP**: `S-C2` / `MVP-50`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-225.md](../task/report/frontend/FE-HRD-225.md).
* **Outcome**: Pembangunan layar perencanaan kerja tahunan pada rute `/hr/learning/annual-plans` (Layar `FE-HRD-221`), rekapitulasi program pelatihan tahunan berbasis TNA, ringkasan kartu anggaran (Pagu Alokasi, Anggaran Komitmen Berjalan, Realisasi Pembayaran, Sisa Saldo Pagu Unit, `VR-TRN-14`), visual progress bar realisasi anggaran departemen, tombol pengajuan rencana ke Direksi, serta tombol persetujuan eksekutif Direktur SDM/Keuangan.
* **Dependencies**: `FE-HRD-216`, `BE-HRD-242`, `BE-HRD-245`.
* **Acceptance Criteria**: Visual progress bar menampilkan sisa saldo anggaran unit secara akurat; indikator peringatan muncul saat pagu menipis (< 15%).
* **Verifikasi**: Uji monitoring pagu anggaran dan approval TNA `AT-HRD-TRN-08`, `AT-HRD-TRN-11`.

#### 🟡 `FE-HRD-226` — Tata Kelola Kontrak Ikatan Dinas Retensi Nakes & Perhitungan Denda Resign (`FE-HRD-222`)
* **Slice / MVP**: `S-C2` / `MVP-50`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-226.md](../task/report/frontend/FE-HRD-226.md).
* **Outcome**: Pembangunan layar administrasi ikatan dinas pada rute `/hr/learning/bonds` (Layar `FE-HRD-222`), tabel data staf nakes yang menjalani ikatan dinas pasca pelatihan berbiaya tinggi (> Rp 10 Juta / > 1 Bulan), kolom nomor akta perjanjian, total pembiayaan sponsor RS, masa retensi kerja (12 s.d. 36 bulan), tanggal mulai & selesai retensi, visual counter sisa bulan ikatan, badge status (`Active`, `Completed`, `BreachPenalized`), serta tombol aksi pencatatan wanprestasi resign sepihak.
* **Dependencies**: `FE-HRD-216`, `BE-HRD-250`.
* **Acceptance Criteria**: Seluruh staf dengan ikatan dinas aktif terpantau dengan counter sisa waktu yang presisi; akses terbatas untuk peran HR Legal dan Direksi.
* **Verifikasi**: Uji tabel monitoring ikatan dinas `AT-HRD-TRN-18`.

#### 🟡 `FE-HRD-227` — Modal Draf Akta Ikatan Dinas & Rincian Tagihan Penalti Resign (`TrainingBondContractModal`)
* **Slice / MVP**: `S-C2` / `MVP-50`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-227.md](../task/report/frontend/FE-HRD-227.md).
* **Outcome**: Komponen modal dialog `@/components/features/learning/TrainingBondContractModal.tsx` dengan mode ganda: (1) Pratinjau draf akta ikatan dinas, klausul retensi kerja dan tombol unggah berkas bertanda tangan meterai; (2) Kalkulator denda wanprestasi resign sepihak yang menampilkan rincian matematis pembiayaan, formula prorata sisa bulan ikatan (`VR-TRN-17`), total invoice ganti rugi yang dibebankan ke pegawai, serta tombol cetak surat tagihan penalti.
* **Dependencies**: `FE-HRD-226`, `BE-HRD-251`.
* **Acceptance Criteria**: Kalkulasi denda ganti rugi terhitung otomatis saat tanggal resign dimasukkan; cetak invoice tagihan memuat rincian klausul akta sah.
* **Verifikasi**: Uji kalkulator penalti dan cetak invoice `AT-HRD-TRN-17`, `AT-HRD-TRN-19`.

#### 🟡 `FE-HRD-228` — Portofolio 50 SKP Nakes 5 Tahunan & Visual Progress Bar Radial (`FE-HRD-223`)
* **Slice / MVP**: `S-C2` / `MVP-50`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 1 dari 1 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-228.md](../task/report/frontend/FE-HRD-228.md).
* **Outcome**: Pembangunan layar portofolio profesi nakes pada rute `/hr/learning/skp-tracker` (Layar `FE-HRD-223`), komponen radial progress chart pemenuhan target akumulasi 50 SKP dalam 5 tahun untuk perpanjangan SIP nakes (`VR-TRN-24`), rincian capaian per 3 ranah Kemenkes (Pembelajaran/Diklat, Pelayanan Klinis, Pengabdian Masyarakat, `VR-TRN-23`), tabel histori perolehan SKP, indikator verifikasi Plataran Sehat, serta tombol tambah klaim perolehan SKP mandiri.
* **Dependencies**: `FE-HRD-216`, `BE-HRD-252`.
* **Acceptance Criteria**: Radial progress chart menampilkan persentase capaian target 50 SKP dengan segmentasi warna per kategori yang informatif.
* **Verifikasi**: Uji visual progress SKP dan capaian 5 tahun `AT-HRD-TRN-20`.

#### 🟡 `FE-HRD-229` — Modal Verifikasi ID Sertifikat & Sinkronisasi Plataran Sehat Kemenkes (`PlataranSehatSyncModal`)
* **Slice / MVP**: `S-C2` / `MVP-50`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R05` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R17`. Laporan lama: [FE-HRD-229.md](../task/report/frontend/FE-HRD-229.md).
* **Outcome**: Komponen modal dialog `@/components/features/learning/PlataranSehatSyncModal.tsx` untuk input nomor registrasi sertifikat Plataran Sehat Kemenkes (`VR-TRN-22`), tombol interaktif pemicu pengecekan online ke API SatuSehat SDMK (`INT-HR-80`), penampil respon detail (Badan Akreditasi, Judul Pelatihan, Bobot SKP Sah, Tanggal Terbit), penanganan status fallback saat jaringan lambat, serta tombol konfirmasi simpan ke portofolio pegawai.
* **Dependencies**: `FE-HRD-228`, `BE-HRD-252`.
* **Acceptance Criteria**: Pengecekan API menampilkan status verifikasi instan; nomor sertifikat yang tidak valid memunculkan peringatan error yang jelas.
* **Verifikasi**: Uji sinkronisasi gateway Kemenkes `AT-HRD-TRN-21`.

#### ✅ `FE-HRD-230` — Verifikasi Integrasi Menyeluruh Next.js App Router, Linting 0 Error & UAT Acceptance `S-C2`
* **Slice / MVP**: `S-C2` / Verifikasi
* **Status**: ✅ PASS
* **Outcome**: Verifikasi kompilasi produksi Next.js App Router (`npm run build`), audit ketat linter CI/CD (`npm run lint:errors`) dengan hasil 0 error, verifikasi responsive rendering di desktop/tablet/mobile, pengujian interaksi 8 layar antarmuka baru, serta pembuktian kelulusan skenario `UAT-01` s.d. `UAT-05` secara visual dan fungsional.
* **Dependencies**: Seluruh task frontend `FE-HRD-216` s.d. `FE-HRD-229` dan task verifikasi backend `BE-HRD-253`.
* **Acceptance Criteria**: Build standalone Next.js sukses tanpa peringatan kritis; linter 0 error 0 warning; seluruh 8 layar baru dan 5 modal beroperasi konsisten.
* **Verifikasi**: Laporan task tracked `FE-HRD-230.md` dan output terminal npm.


---

## 20. Rencana Kerja Frontend Sub-Modul 17: Performance Management & Clinical Career (Slice `S-C3`)

### 20.1 Grafik Urutan Dependency Frontend `S-C3`

#### Grafik F62: Gelombang `MVP-51` (Client API, Redux Slice, Dasbor Kinerja, Katalog KPI, Sasaran & Evaluasi Berjenjang)
```text
FE-HRD-231 (Client API & Redux Slice) ──┬──> FE-HRD-232 (Dashboard Kinerja & Karier FE-HRD-231)
BE-HRD-257 [BE] ────────────────────────┘
FE-HRD-231 [F62] ──> FE-HRD-233 (Katalog Indikator KPI & BerAKHLAK FE-HRD-232)
BE-HRD-254 [BE] ───┘
FE-HRD-231 [F62] ──> FE-HRD-234 (Siklus & Cascading Sasaran FE-HRD-233) ──> FE-HRD-235 (Modal Input Sasaran & Bobot) ──┐
BE-HRD-258 [BE] ───┘                                                                                                  │
FE-HRD-231 [F62] ──> FE-HRD-236 (Portal Evaluasi Berjenjang 60:40 FE-HRD-234) ──> FE-HRD-237 (Modal Peer Feedback) ──┴──> FE-HRD-238 🟡 (Konsol Sidang Kalibrasi Manajemen FE-HRD-235)
BE-HRD-259 [BE] ───┘
```

#### Grafik F63: Gelombang `MVP-52` (Konsol Kalibrasi Manajemen, Nota Justifikasi & Tata Kelola PIP 60 Hari)
```text
FE-HRD-238 [F62] ──> FE-HRD-239 (Modal Nota Justifikasi Kalibrasi) ──> FE-HRD-240 (Tata Kelola PIP 60 Hari FE-HRD-236) ──> FE-HRD-241 🟡 (Modal Logbook Mingguan & Sidang Putusan PIP)
BE-HRD-261 [BE] ───┘                                                    BE-HRD-262 [BE] ────────────────────────────────────┘
```

#### Grafik F64: Gelombang `MVP-53` (Portofolio Clinical Ladder PK I-V, Gatekeeper & Portal Sanggah Banding SLA 7/14 Hari)
```text
FE-HRD-231 [F62] ──> FE-HRD-242 (Portofolio Clinical Ladder PK I-V FE-HRD-237)
BE-HRD-264 [BE] ───┘
FE-HRD-231 [F62] ──> FE-HRD-243 (Portal Sanggah Banding SLA 7/14 Hari FE-HRD-238) ──> FE-HRD-244 🟡 (Modal Putusan Majelis Banding Direktur SDM)
BE-HRD-267 [BE] ───┘
```

#### Grafik F65: Gelombang Verifikasi & Kepatuhan Arsitektur Frontend `S-C3`
```text
FE-HRD-232 [F62] ──┐
FE-HRD-241 [F63] ──┼──> FE-HRD-245 ⏳ (Verifikasi Menyeluruh Next.js App Router, Linting 0 Error & UAT Acceptance S-C3)
FE-HRD-242 [F64] ──┤
FE-HRD-244 [F64] ──┤
BE-HRD-269 [BE] ───┘
```

```text
Legenda:
──>    : Prasyarat langsung (kiri harus selesai sebelum kanan dapat dimulai)
├──>   : Percabangan dependensi
[...]  : Cermin task dari grafik gelombang sebelumnya atau dari roadmap backend [BE]
â ³    : Task aktif berstatus Menunggu Prasyarat (Waiting Dependencies)
```

---

### 20.2 Tabel Urutan Gelombang Eksekusi Frontend `S-C3`

| Gelombang | ID Task | Nama Task | Prasyarat Langsung | Lingkup Deliverable |
| :--- | :--- | :--- | :--- | :--- |
| **`MVP-51`** | `FE-HRD-231` | Redux Slice State, Base Types & Service API Client Kontrak v21 | - | Slice Redux `performanceManagementSlice.ts`, TypeScript interfaces, Axios API client terintegrasi 24 endpoint Kontrak v21. |
| **`MVP-51`** | `FE-HRD-232` | Dashboard Kinerja & Karier Klinis Nakes (`FE-HRD-231`) | `FE-HRD-231`, `BE-HRD-257` [BE] | Layar `/hr/performance/dashboard`, metrik ringkasan siklus aktif, radar rata-rata skor unit, feed status lembar evaluasi, countdown batas waktu. |
| **`MVP-51`** | `FE-HRD-233` | Katalog Indikator KPI & Core Values BerAKHLAK (`FE-HRD-232`) | `FE-HRD-231`, `BE-HRD-254` [BE] | Layar `/hr/performance/kpi-catalogs`, kamus indikator hasil kerja kuantitatif, rubrik perilaku 7 nilai BerAKHLAK skala likert 1-5, filter kategori nakes/non-nakes. |
| **`MVP-51`** | `FE-HRD-234` | Siklus Kinerja & Cascading Sasaran Mutu Unit (`FE-HRD-233`) | `FE-HRD-231`, `BE-HRD-258` [BE] | Layar `/hr/performance/cycles`, daftar periode siklus evaluasi, visualisasi pohon cascading sasaran mutu unit ke sasaran kerja pegawai, progress status persetujuan atasan. |
| **`MVP-51`** | `FE-HRD-235` | Modal Input Sasaran Kerja Pegawai & Bobot Target (`KpiGoalFormModal`) | `FE-HRD-234` | Komponen modal dialog input butir KPI, penetapan target angka/persentase, validasi interaktif total akumulasi bobot tepat 100% (`VR-PRF-04`). |
| **`MVP-51`** | `FE-HRD-236` | Portal Lembar Penilaian Berjenjang Tiga Tingkat (`FE-HRD-234`) | `FE-HRD-231`, `BE-HRD-259` [BE] | Layar `/hr/performance/appraisals`, antarmuka dual-role (Pegawai untuk self-assessment & Atasan untuk evaluasi resmi 60:40), slider perilaku BerAKHLAK, kalkulator nilai komposit live. |
| **`MVP-51`** | `FE-HRD-237` | Modal Peer Review Sejawat & Umpan Balik Kualitatif (`PeerReviewFeedbackModal`) | `FE-HRD-236` | Komponen modal input ulasan sejawat, form observasi kualitatif kekuatan dan area pengembangan, pemisahan data IDP non-skor dari nilai resmi (`VR-PRF-10`). |
| **`MVP-52`** | `FE-HRD-238` | Konsol Sidang Kalibrasi Manajemen & Guiding Distribution (`FE-HRD-235`) | `FE-HRD-236`, `BE-HRD-261` [BE] | Layar `/hr/performance/calibration`, konsol pleno komite kalibrasi direksi, diagram distribusi frekuensi nilai aktual vs pedoman fleksibel (10-15% A, 70-80% B, 5-10% C/D), tombol rilis serentak ke ESS. |
| **`MVP-52`** | `FE-HRD-239` | Modal Nota Justifikasi Kalibrasi Manajemen (`CalibrationJustificationModal`) | `FE-HRD-238` | Komponen modal penyesuaian skor, field skor awal, input skor revisi, textarea wajib nota justifikasi tertulis minimal 50 karakter (`VR-PRF-12`). |
| **`MVP-52`** | `FE-HRD-240` | Tata Kelola PIP 60 Hari Kalender & Logbook Mingguan Mentor (`FE-HRD-236`) | `FE-HRD-239`, `BE-HRD-262` [BE] | Layar `/hr/performance/pip`, dasbor pemantauan kasus pembinaan staf skor < 65, timeline 60 hari kalender, indikator status pembekuan mutasi/promosi, tabel logbook mingguan 1-8. |
| **`MVP-52`** | `FE-HRD-241` | Modal Logbook Mingguan & Sidang Putusan PIP (`PipWeeklyLogbookModal`) | `FE-HRD-240`, `BE-HRD-262` [BE] | Komponen modal pencatatan catatan mingguan mentor, upload bukti mentoring, panel sidang hari ke-60 (pilihan Lulus, Perpanjangan 30 hari, atau Eskalasi HR Legal). |
| **`MVP-53`** | `FE-HRD-242` | Portofolio Kenaikan Jenjang Klinis Perawat/Nakes PK I-V (`FE-HRD-237`) | `FE-HRD-231`, `BE-HRD-264` [BE] | Layar `/hr/performance/clinical-ladder`, registri kenaikan jenjang nakes, verifikasi badge gatekeeper prasyarat KPI >= 75 & sertifikat Sub-16, viewer portofolio logbook komite. |
| **`MVP-53`** | `FE-HRD-243` | Portal Pengajuan Sanggah Banding Nilai SLA 7/14 Hari (`FE-HRD-238`) | `FE-HRD-231`, `BE-HRD-267` [BE] | Layar `/hr/performance/disputes`, antarmuka pengajuan sanggahan staf di ESS dengan countdown SLA H+7, upload berkas bukti pembanding, pemantauan progress mediasi klarifikasi. |
| **`MVP-53`** | `FE-HRD-244` | Modal Putusan Majelis Banding Direktur SDM (`DisputeResolutionModal`) | `FE-HRD-243` | Komponen modal sidang majelis banding, formulir amar putusan direktur SDM (Diterima/Ditolak), penetapan nilai final terkoreksi, penegakan batas waktu SLA 14 hari kalender. |
| **Verifikasi** | `FE-HRD-245` | Verifikasi Menyeluruh Next.js App Router, Linting 0 Error & UAT Acceptance `S-C3` | `FE-HRD-232`, `FE-HRD-241`, `FE-HRD-242`, `FE-HRD-244`, `BE-HRD-269` [BE] | Audit linting `npm run lint:errors`, uji build produksi `npm run build`, verifikasi integrasi UI dengan API backend, eksekusi 22 test case `AT-HRD-PRF-01` s.d. `AT-HRD-PRF-22`, dan 5 skenario UAT. |

---

### 20.3 Rincian Spesifikasi Task Frontend `S-C3`

#### ✅ `FE-HRD-231` — Redux Slice State, Base Types & Service API Client Kontrak v21
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: ✅ Selesai (PASS - Verifikasi Mandiri & Build 0 Error)
* **Outcome**: Pembuatan Redux Toolkit slice `@/lib/redux/slices/performanceManagementSlice.ts`, pendefinisian antarmuka TypeScript untuk seluruh 10 entitas Performance & Clinical Career, pembuatan service client Axios `@/services/performanceManagementService.ts` yang mengikat 24 endpoint API Kontrak `v21`, penanganan caching terpusat, serta penanganan error response berstandar RFC 7807.
* **Dependencies**: Tidak ada (Task pondasi Frontend `S-C3`).
* **Acceptance Criteria**: State Redux terintegrasi ke store utama; service Axios mengeksekusi request HTTP ke seluruh 24 endpoint secara type-safe; penanganan error 400/404/422 menampilkan toast notification yang deskriptif.
* **Verifikasi**: Uji isolasi state management Redux pada laporan task `FE-HRD-231.md`.

#### 🟡 `FE-HRD-232` — Dashboard Kinerja & Karier Klinis Nakes (`FE-HRD-231`)
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-232.md](../task/report/frontend/FE-HRD-232.md).
* **Outcome**: Pembangunan layar dasbor utama pada rute `/hr/performance/dashboard` (Layar `FE-HRD-231`), menampilkan kartu metrik ringkasan siklus penilaian aktif, widget radar rata-rata capaian KPI per unit kerja/instalasi, diagram sebaran nilai komposit organisasi, feed aktivitas terbaru evaluasi kinerja, kartu peringatan kasus PIP aktif, dan tombol akses cepat ke katalog sasaran mutu.
* **Dependencies**: `FE-HRD-231`, `BE-HRD-257` [BE].
* **Acceptance Criteria**: Komponen visual ter-render responsif; metrik kartu selaras dengan data backend; badge peringatan PIP tampil jika terdapat kasus pembinaan aktif.
* **Verifikasi**: Uji visual dan data binding dasbor kinerja `AT-HRD-PRF-01`.

#### 🟡 `FE-HRD-233` — Katalog Indikator KPI & Core Values BerAKHLAK (`FE-HRD-232`)
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-233.md](../task/report/frontend/FE-HRD-233.md).
* **Outcome**: Pembangunan layar katalog indikator kinerja pada rute `/hr/performance/kpi-catalogs` (Layar `FE-HRD-232`), tabel master indikator kuantitatif rumah sakit (indikator pelayanan medis, keperawatan, penunjang, dan umum), kamus definisi operasional, formula perhitungan indikator, serta rubrik deskriptor 7 butir Core Values BerAKHLAK skala 1-5 dengan contoh perilaku spesifik lingkungan RS.
* **Dependencies**: `FE-HRD-231`, `BE-HRD-254` [BE].
* **Acceptance Criteria**: Tabel menampilkan seluruh katalog indikator kuantitatif dan Core Values BerAKHLAK; fitur pencarian dan filter per profesi berfungsi instan tanpa lag.
* **Verifikasi**: Uji katalog indikator dan rubrik BerAKHLAK `AT-HRD-PRF-02`.

#### 🟡 `FE-HRD-234` — Siklus Kinerja & Cascading Sasaran Mutu Unit (`FE-HRD-233`)
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-234.md](../task/report/frontend/FE-HRD-234.md).
* **Outcome**: Pembangunan layar pengelolaan periode siklus kinerja pada rute `/hr/performance/cycles` (Layar `FE-HRD-233`), visualisasi hirarki pohon (*cascading tree view*) penurunan sasaran mutu dari target direksi -> instalasi/unit -> individu staf, status persetujuan atasan langsung (`Draft`, `Submitted`, `Approved`, `Locked`), dan tombol pemicu pembukaan modal input sasaran kerja.
* **Dependencies**: `FE-HRD-231`, `BE-HRD-258` [BE].
* **Acceptance Criteria**: Pohon cascading ter-render dinamis; status persetujuan atasan menampilkan badge status Quilvian; tombol input sasaran memicu pembukaan modal form.
* **Verifikasi**: Uji visualisasi cascading target sasaran mutu `AT-HRD-PRF-03`.

#### 🟡 `FE-HRD-235` — Modal Input Sasaran Kerja Pegawai & Bobot Target (`KpiGoalFormModal`)
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-235.md](../task/report/frontend/FE-HRD-235.md).
* **Outcome**: Komponen dialog modal `@/components/features/performance/KpiGoalFormModal.tsx` menggunakan `BaseFormControl` Quilvian, formulir pemilihan indikator KPI dari katalog, pengisian target angka/persentase, field numerik bobot persentase, penghitung akumulasi bobot live yang menampilkan peringatan warna merah jika total != 100.00% (`VR-PRF-04`), dan pembatasan input maksimal 10 butir KPI per pegawai (`VR-PRF-03`).
* **Dependencies**: `FE-HRD-234`.
* **Acceptance Criteria**: Form memblokir tombol submit jika akumulasi total bobot tidak sama persis dengan 100.00%; pesan kesalahan validasi tampil jelas di bawah field.
* **Verifikasi**: Uji form input sasaran dan validasi total bobot `AT-HRD-PRF-04`.

#### 🟡 `FE-HRD-236` — Portal Lembar Penilaian Berjenjang Tiga Tingkat (`FE-HRD-234`)
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-236.md](../task/report/frontend/FE-HRD-236.md).
* **Outcome**: Pembangunan layar penilaian kinerja resmi pada rute `/hr/performance/appraisals` (Layar `FE-HRD-234`), antarmuka adaptif dual-mode: Mode Staf Mandiri (*Self-Assessment*) dan Mode Atasan Langsung (Penilaian Karu & Penelaahan Kabid). Dilengkapi tabel realisasi target 60% KPI hasil kerja, kontrol slider nilai perilaku 40% Core Values BerAKHLAK (skala likert 1-5), kalkulator live nilai komposit dan predikat kinerja, serta alur penguncian bertahap tanpa bypass wewenang.
* **Dependencies**: `FE-HRD-231`, `BE-HRD-259` [BE].
* **Acceptance Criteria**: Atasan tidak dapat mengisi penilaian sebelum staf merampungkan self-assessment; kalkulator live memperbarui skor komposit 60:40 secara real-time saat slider BerAKHLAK digeser; tombol submit memicu konfirmasi tanda tangan digital.
* **Verifikasi**: Uji lembar evaluasi berjenjang 60:40 dan kalkulasi skor `AT-HRD-PRF-05`, `AT-HRD-PRF-06`, `AT-HRD-PRF-07`.

#### 🟡 `FE-HRD-237` — Modal Peer Review Sejawat & Umpan Balik Kualitatif (`PeerReviewFeedbackModal`)
* **Slice / MVP**: `S-C3` / `MVP-51`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-237.md](../task/report/frontend/FE-HRD-237.md).
* **Outcome**: Komponen dialog modal `@/components/features/performance/PeerReviewFeedbackModal.tsx`, formulir pengisian umpan balik rekan sejawat setara untuk penyusunan *Individual Development Plan* (IDP), textarea catatan observasi kekuatan (*Strengths*) dan area yang perlu ditingkatkan (*Improvement Areas*), sakelar opsi pengiriman anonim, serta label informasi penegasan bahwa ulasan ini tidak mempengaruhi skor komposit resmi pegawai (`VR-PRF-10`).
* **Dependencies**: `FE-HRD-236`.
* **Acceptance Criteria**: Form menyimpan ulasan rekan sejawat; label penjelas non-skor tampil mencolok; skor komposit pada lembar evaluasi terbukti tidak bergeser.
* **Verifikasi**: Uji pengumpulan umpan balik peer review IDP `AT-HRD-PRF-08`.

#### 🟡 `FE-HRD-238` — Konsol Sidang Kalibrasi Manajemen & Guiding Distribution (`FE-HRD-235`)
* **Slice / MVP**: `S-C3` / `MVP-52`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-238.md](../task/report/frontend/FE-HRD-238.md).
* **Outcome**: Pembangunan konsol sidang pleno kalibrasi pada rute `/hr/performance/calibration` (Layar `FE-HRD-235`), diagram visual kurva sebaran nilai komposit aktual seluruh pegawai rumah sakit terhadap pedoman sebaran fleksibel (*Guiding Distribution*: 10-15% A, 70-80% B, 5-10% C/D), tabel interaktif daftar seluruh pegawai beserta skor awal dan skor rekomendasi, tombol buka modal nota justifikasi override nilai, dan tombol aksi pengesahan pleno rilis nilai serentak ke portal ESS.
* **Dependencies**: `FE-HRD-236`, `BE-HRD-261` [BE].
* **Acceptance Criteria**: Diagram distribusi kurva fleksibel ter-render dinamis; deviasi sebaran di luar pedoman ditandai dengan warna highlight peringatan; rilis nilai serentak memerlukan konfirmasi PIN otorisasi direktur SDM.
* **Verifikasi**: Uji konsol sidang kalibrasi dan kurva guiding distribution `AT-HRD-PRF-09`, `AT-HRD-PRF-10`.

#### 🟡 `FE-HRD-239` — Modal Nota Justifikasi Kalibrasi Manajemen (`CalibrationJustificationModal`)
* **Slice / MVP**: `S-C3` / `MVP-52`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-239.md](../task/report/frontend/FE-HRD-239.md).
* **Outcome**: Komponen dialog modal `@/components/features/performance/CalibrationJustificationModal.tsx`, antarmuka penyesuaian skor oleh komite kalibrasi direksi, field skor awal (read-only), input skor hasil penyesuaian kalibrasi, textarea wajib nota justifikasi tertulis minimal 50 karakter (`VR-PRF-12`), serta pencatatan nama pimpinan sidang penanggung jawab koreksi.
* **Dependencies**: `FE-HRD-238`.
* **Acceptance Criteria**: Tombol simpan terkunci (*disabled*) jika panjang teks nota justifikasi < 50 karakter (`VR-PRF-13`); audit trail perubahan skor tercatat permanen.
* **Verifikasi**: Uji validasi nota justifikasi override nilai `AT-HRD-PRF-11`.

#### 🟡 `FE-HRD-240` — Tata Kelola PIP 60 Hari Kalender & Logbook Mingguan Mentor (`FE-HRD-236`)
* **Slice / MVP**: `S-C3` / `MVP-52`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 3 dari 3 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-240.md](../task/report/frontend/FE-HRD-240.md).
* **Outcome**: Pembangunan layar tata kelola program pembinaan kinerja pada rute `/hr/performance/pip` (Layar `FE-HRD-236`), dasbor pemantauan kasus aktif pegawai dengan skor komposit < 65.00, visual progress bar timeline 60 hari kalender, kartu profil mentor pendamping, badge peringatan merah pembekuan hak mutasi/promosi jabatan (`VR-PRF-16`), tabel daftar 8 sesi logbook mingguan, serta tombol buka modal logbook.
* **Dependencies**: `FE-HRD-239`, `BE-HRD-262` [BE].
* **Acceptance Criteria**: Seluruh staf dengan nilai < 65 otomatis muncul pada daftar PIP; timeline 60 hari menghitung sisa hari pembinaan secara akurat; indikator pembekuan mutasi aktif.
* **Verifikasi**: Uji dasbor tata kelola PIP 60 hari `AT-HRD-PRF-12`, `AT-HRD-PRF-13`.

#### 🟡 `FE-HRD-241` — Modal Logbook Mingguan & Sidang Putusan PIP (`PipWeeklyLogbookModal`)
* **Slice / MVP**: `S-C3` / `MVP-52`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-241.md](../task/report/frontend/FE-HRD-241.md).
* **Outcome**: Komponen dialog modal `@/components/features/performance/PipWeeklyLogbookModal.tsx`, formulir pengisian catatan mingguan mentor pembimbing (minimal 20 karakter, `VR-PRF-15`), tombol upload berkas bukti coaching, serta panel khusus sidang evaluasi hari ke-60 yang menyediakan 3 tombol radio putusan resmi: Lulus (kembali normal), Perpanjangan (maksimal 30 hari), atau Eskalasi HR Legal Hubungan Industrial.
* **Dependencies**: `FE-HRD-240`, `BE-HRD-262` [BE].
* **Acceptance Criteria**: Validasi form mewajibkan catatan mentor; panel putusan hari ke-60 hanya aktif jika seluruh 8 sesi logbook mingguan telah terisi lengkap.
* **Verifikasi**: Uji pengisian logbook mentor dan penegakan putusan PIP `AT-HRD-PRF-14`, `AT-HRD-PRF-15`.

#### 🟡 `FE-HRD-242` — Portofolio Kenaikan Jenjang Klinis Perawat/Nakes PK I-V (`FE-HRD-237`)
* **Slice / MVP**: `S-C3` / `MVP-53`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-242.md](../task/report/frontend/FE-HRD-242.md).
* **Outcome**: Pembangunan layar portofolio kenaikan jenjang karier profesional perawat klinis dan nakes pada rute `/hr/performance/clinical-ladder` (Layar `FE-HRD-237`), registri kenaikan jenjang (PK I s.d. PK V), visual badge gatekeeper verifikasi prasyarat administratif (Skor Kinerja >= 75.00 `VR-PRF-17`, Sertifikat Diklat/OSCE Sub-16 `VR-PRF-18`, Bebas Sentinel Sub-15 `VR-PRF-19`), viewer berkas logbook tindakan klinis, formulir rekomendasi Komite Keperawatan, dan form penerbitan SK Direktur Utama.
* **Dependencies**: `FE-HRD-231`, `BE-HRD-264` [BE].
* **Acceptance Criteria**: Tombol pengajuan terkunci jika salah satu prasyarat gatekeeper merah/tidak terpenuhi; viewer dokumen portofolio klinis berfungsi mulus.
* **Verifikasi**: Uji portal jenjang karier klinis PK I-V dan gatekeeper mutu `AT-HRD-PRF-16`, `AT-HRD-PRF-17`, `AT-HRD-PRF-18`, `AT-HRD-PRF-19`, `AT-HRD-PRF-20`.

#### 🟡 `FE-HRD-243` — Portal Pengajuan Sanggah Banding Nilai SLA 7/14 Hari (`FE-HRD-238`)
* **Slice / MVP**: `S-C3` / `MVP-53`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-243.md](../task/report/frontend/FE-HRD-243.md).
* **Outcome**: Pembangunan layar portal sanggah banding nilai kinerja pada rute `/hr/performance/disputes` (Layar `FE-HRD-238`), antarmuka pegawai di ESS untuk mengajukan sanggahan nilai resmi pasca pengumuman kalibrasi, countdown visual batas waktu pengajuan (H+7 hari kalender, `VR-PRF-21`), form alasan keberatan, komponen upload dokumen bukti pembanding (`VR-PRF-22`), tabel pemantauan proses mediasi klarifikasi oleh HR Performance, serta timeline batas penyelesaian putusan majelis 14 hari kalender.
* **Dependencies**: `FE-HRD-231`, `BE-HRD-267` [BE].
* **Acceptance Criteria**: Form pengajuan terkunci otomatis dan menampilkan badge merah "Masa Sanggah Telah Berakhir" jika waktu telah melampaui H+7; upload bukti pembanding wajib disertakan sebelum submit.
* **Verifikasi**: Uji portal sanggah banding dan penolakan otomatis H+7 `AT-HRD-PRF-21`.

#### 🟡 `FE-HRD-244` — Modal Putusan Majelis Banding Direktur SDM (`DisputeResolutionModal`)
* **Slice / MVP**: `S-C3` / `MVP-53`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R04` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R16`. Laporan lama: [FE-HRD-244.md](../task/report/frontend/FE-HRD-244.md).
* **Outcome**: Komponen dialog modal `@/components/features/performance/DisputeResolutionModal.tsx`, antarmuka pimpinan sidang majelis banding Direktur SDM, ringkasan perbandingan nilai awal vs bukti keberatan staf, input amar putusan resmi majelis (Diterima dengan Perbaikan / Ditolak), input nilai komposit akhir terkoreksi, textarea pertimbangan majelis, serta penegakan batas waktu SLA penerbitan putusan maksimal 14 hari kalender (`VR-PRF-24`).
* **Dependencies**: `FE-HRD-243`.
* **Acceptance Criteria**: Putusan majelis tersimpan dengan tanda tangan digital Direktur SDM; nilai komposit pegawai di ESS otomatis terbarui sesuai amar putusan dan status lembar evaluasi kembali normal `Final`.
* **Verifikasi**: Uji penetapan putusan majelis banding `AT-HRD-PRF-22`.

#### ✅ `FE-HRD-245` — Verifikasi Menyeluruh Next.js App Router, Linting 0 Error & UAT Acceptance `S-C3`
* **Slice / MVP**: `S-C3` / Verifikasi
* **Status**: ✅ Selesai (PASS - Verifikasi Mandiri & Build 0 Error)
* **Outcome**: Eksekusi audit integritas frontend Next.js: pemeriksaan linting `npm run lint:errors` menghasilkan 0 error, build produksi `npm run build` sukses tanpa warning kritis, validasi konsistensi design tokens dan base components Quilvian, verifikasi kelancaran alur kerja UI ujung-ke-ujung (E2E), serta pengujian penerimaan terhadap 5 skenario UAT utama Sub-Modul 17.
* **Dependencies**: Seluruh task frontend `FE-HRD-231` s.d. `FE-HRD-244` dan task verifikasi backend `BE-HRD-269`.
* **Acceptance Criteria**: Linting lulus 0 error; build produksi Next.js standalone sukses; seluruh 8 layar dan 5 modal dialog beroperasi mulus; 5 skenario UAT terverifikasi lulus.
* **Verifikasi**: Laporan task tracked `FE-HRD-245.md` dan output terminal npm.


---

## 21. Rencana Kerja Frontend Sub-Modul 18: Hubungan Industrial & Terminasi (Slice `S-C5`)

### 21.1 Grafik Urutan Dependency Frontend `S-C5`

#### Grafik F66: Gelombang `MVP-54` (Redux Slice, API Client, Navigasi, Dasbor, Laporan Insiden Just Culture & Sanksi SP 1-3)
```text
FE-HRD-246 (Redux Slice, Types & Axios Service) ──┬──> FE-HRD-247 (Sidebar Navigasi, Breadcrumbs & RBAC Guards) ──> FE-HRD-248 (Dasbor Hubungan Industrial & Radar Clearance)
                                                  └──> FE-HRD-249 (Layar Insiden Disiplin & Just Culture) ──┬──> FE-HRD-250 (Modal Berita Acara Sidang BAP)
                                                                                                            └──> FE-HRD-251 (Layar Penerbitan SP 1-3 PP 35/2021) ──> FE-HRD-252 🟡 (Modal Sanggah Banding Sanksi SP)
```

#### Grafik F67: Gelombang `MVP-55` (Skorsing Upah Penuh & Pengamanan SIMRS, Pengaduan ESS, Whistleblowing Rahasia & Forum Bipartit)
```text
FE-HRD-248 [F66] ──┬──> FE-HRD-253 (Layar Skorsing Upah Penuh Paid Suspension & Status SIMRS INT-HR-88)
                   ├──> FE-HRD-254 (Layar Pengaduan Staf Grievance ESS & Notulen Mediasi SLA 14 Hari)
                   ├──> FE-HRD-255 (Modal & Portal Whistleblower Terenkripsi Asimetris Anti-Retaliasi)
                   └──> FE-HRD-256 🟡 (Layar Notulen & Dokumentasi Risalah Forum Bipartit RS)
```

#### Grafik F68: Gelombang `MVP-56` (Pengajuan PHK/Resign 30 Hari, Kalkulator Pesangon PP 35/2021 & Exit Clearance Multi-Departemen Pemblokir Payroll)
```text
FE-HRD-253 [F67] ──┬──> FE-HRD-257 (Layar Pengajuan PHK/Resign 30 Hari & Radar Kontrak PKWT) ──> FE-HRD-258 (Kalkulator Pesangon Deterministik PP 35/2021 & Lembar Rincian)
                   └──> FE-HRD-259 🟡 (Pusat Kendali Exit Clearance Multi-Departemen 5 Unit)
```

#### Grafik F69: Gelombang Verifikasi & Uji Integrasi Antarmuka End-to-End
```text
FE-HRD-258 [F68] ──┬──> FE-HRD-260 🟡 (Modal Signoff Kliring Unit, Indikator Gembok Payroll Final Sub-10 INT-HR-87 & Verifikasi E2E)
FE-HRD-259 [F68] ──┘
```

---

### 21.2 Tabel Urutan Gelombang Eksekusi Frontend `S-C5`

| Gelombang Eksekusi | Task ID | Deskripsi Singkat Pekerjaan Frontend | Prasyarat (Dependencies) | Target MVP |
|---|---|---|---|---|
| **Gelombang 1** | `FE-HRD-246` | Redux State Slice `employeeRelationSlice.ts`, Axios Services & Types Definition | `BE-HRD-272` [BE] | `MVP-54` |
| **Gelombang 1** | `FE-HRD-247` | Sidebar Menu Navigasi, Breadcrumbs & RBAC Route Guards `/hrd/relations` | `FE-HRD-246` | `MVP-54` |
| **Gelombang 1** | `FE-HRD-248` | Layar Dasbor Hubungan Industrial, Radar Kedaluwarsa SP & Status Exit Clearance | `FE-HRD-247` | `MVP-54` |
| **Gelombang 2** | `FE-HRD-249` | Layar Pelaporan Insiden Disiplin, Filter Just Culture RS & Validasi KMKP | `FE-HRD-246`, `BE-HRD-273` [BE] | `MVP-54` |
| **Gelombang 2** | `FE-HRD-250` | Komponen Dialog Modal Berita Acara Pemeriksaan (BAP) & Unggah Bukti Sidang | `FE-HRD-249`, `BE-HRD-274` [BE] | `MVP-54` |
| **Gelombang 2** | `FE-HRD-251` | Layar Penerbitan & Monitoring Surat Peringatan (SP 1-3 Masa 6 Bulan PP 35/2021) | `FE-HRD-249`, `BE-HRD-275` [BE] | `MVP-54` |
| **Gelombang 2** | `FE-HRD-252` | Komponen Modal Sanggah Banding Sanksi SP & Risalah Putusan Sidang Direksi | `FE-HRD-251` | `MVP-54` |
| **Gelombang 3** | `FE-HRD-253` | Layar Pengelolaan Skorsing Investigasi (*Paid Suspension*) & Status Akses SIMRS | `FE-HRD-248`, `BE-HRD-277` [BE] | `MVP-55` |
| **Gelombang 3** | `FE-HRD-254` | Layar Pengaduan Keluhan Staf (Grievance ESS) & Notulen Mediasi SLA 14 Hari | `FE-HRD-248`, `BE-HRD-279` [BE] | `MVP-55` |
| **Gelombang 3** | `FE-HRD-255` | Komponen Modal & Portal Whistleblower Terenkripsi Asimetris Anti-Retaliasi | `FE-HRD-248`, `BE-HRD-280` [BE] | `MVP-55` |
| **Gelombang 3** | `FE-HRD-256` | Layar Manajemen Notulen & Risalah Forum Perundingan Bipartit RS | `FE-HRD-248`, `BE-HRD-281` [BE] | `MVP-55` |
| **Gelombang 4** | `FE-HRD-257` | Layar Pengajuan Terminasi Kerja (Resign 30 Hari, Kontrak, Pensiun, SP3) | `FE-HRD-253`, `BE-HRD-282` [BE] | `MVP-56` |
| **Gelombang 4** | `FE-HRD-258` | Komponen Kalkulator Pesangon Deterministik PP 35/2021 & Lembar Rincian Finansial | `FE-HRD-257`, `BE-HRD-283` [BE] | `MVP-56` |
| **Gelombang 4** | `FE-HRD-259` | Layar Pusat Kendali Exit Clearance Multi-Departemen 5 Unit Kerja | `FE-HRD-253`, `BE-HRD-284` [BE] | `MVP-56` |
| **Verifikasi** | `FE-HRD-260` | Modal Signoff Kliring Unit, Indikator Gembok Payroll Final Sub-10 & Uji Linter Build | `FE-HRD-258`, `FE-HRD-259`, `BE-HRD-285` [BE] | Verifikasi |

---

### 21.3 Rincian Spesifikasi Task Frontend `S-C5`

#### ✅ `FE-HRD-246` — Redux State Slice `employeeRelationSlice.ts`, Axios Services & Types Definition
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: ✅ PASS
* **Outcome**: Pembuatan Redux slice `employeeRelationSlice.ts`, tipe TypeScript lengkap untuk 10 entitas transaksi hubungan industrial dan exit clearance, Axios API client yang terhubung ke 24 endpoint Kontrak `v22`, serta interceptor penanganan error HTTP 422 dan HTTP 403 khusus kasus whistleblower.
* **Dependencies**: `BE-HRD-272` [BE]
* **Acceptance Criteria**: State store Redux terkonfigurasi bersih; helper fungsi fetching API siap dipanggil oleh seluruh layar; zero compilation type error.
* **Verifikasi**: Build inspeksi TypeScript.

#### ✅ `FE-HRD-247` — Sidebar Menu Navigasi, Breadcrumbs & RBAC Route Guards `/hrd/relations`
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: ✅ PASS
* **Outcome**: Penambahan butir menu navigasi pada sidebar aplikasi di bawah grup `Hubungan Industrial & Terminasi`, penataan breadcrumb navigasi berjenjang, dan penerapan route guard proteksi hak akses berbasis RBAC (`Hrd.Disciplinary.*`, `Hrd.Grievance.*`, `Hrd.Separation.*`, `Hrd.ExitClearance.*`).
* **Dependencies**: `FE-HRD-246`
* **Acceptance Criteria**: Seluruh rute terproteksi; user tanpa permission menerima peringatan atau dialihkan ke halaman Unauthorized yang ramah pengguna.
* **Verifikasi**: Pengujian navigasi dan evaluasi permission guard.

#### 🟡 `FE-HRD-248` — Layar Dasbor Hubungan Industrial & Radar Exit Clearance (`/hrd/relations/dashboard`)
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-248.md](../task/report/frontend/FE-HRD-248.md).
* **Outcome**: Pembangunan layar dasbor eksekutif (`FE-HRD-246`) pada rute `/hrd/relations/dashboard`. Menyajikan metrik ringkasan kartu (SP Aktif, Kasus Skorsing, Tiket Grievance, Offboarding Berjalan), tabel radar peringatan SP yang akan kedaluwarsa dalam 14 hari ke depan (H-14), grafik donat sebaran pelanggaran Just Culture (Human Error vs At-Risk vs Reckless), serta daftar proses exit clearance kritis yang mendekati tanggal keluar.
* **Dependencies**: `FE-HRD-247`
* **Acceptance Criteria**: Data statistik termuat secara asinkron dari backend; indikator visual badge status berfungsi konsisten dengan design token Quilvian.
* **Verifikasi**: Uji tampilan dasbor pada `AT-HRD-EMP-01`.

#### 🟡 `FE-HRD-249` — Layar Pelaporan Insiden Disiplin, Filter Just Culture RS & Validasi KMKP (`/hrd/relations/incidents`)
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-249.md](../task/report/frontend/FE-HRD-249.md).
* **Outcome**: Pembangunan layar daftar dan pelaporan insiden tata tertib (`FE-HRD-247`) pada rute `/hrd/relations/incidents`. Form pelaporan memuat toggle insiden pelayanan medis pasien yang secara kondisional memunculkan input wajib nomor registrasi audit keselamatan pasien KMKP (`KmKpAuditReferenceNumber`, `VR-EMP-02`). Dilengkapi panel klasifikasi Just Culture dan pencegahan otomatis penerbitan SP untuk kategori Human Error.
* **Dependencies**: `FE-HRD-246`, `BE-HRD-273` [BE]
* **Acceptance Criteria**: Form insiden menolak submit jika kategori klinis tidak menyertakan nomor audit KMKP; kategori Human Error memunculkan banner pengalihan ke pembinaan/diklat Sub-16.
* **Verifikasi**: Uji form pelaporan insiden Just Culture `AT-HRD-EMP-02`, `AT-HRD-EMP-03`.

#### 🟡 `FE-HRD-250` — Komponen Dialog Modal Berita Acara Pemeriksaan (BAP) & Unggah Bukti Sidang (`BapRecordModal`)
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-250.md](../task/report/frontend/FE-HRD-250.md).
* **Outcome**: Pembuatan modal dialog digitalisasi Berita Acara Pemeriksaan (`BapRecordModal.tsx`). Menyediakan formulir input tanggal sidang klarifikasi, multi-select nama tim pemeriksa (minimal 2 orang), textarea rangkuman saksi, komponen upload dokumen bukti sidang / CCTV, input pembelaan tertulis staf, dan radio-button pilihan rekomendasi sanksi disiplin.
* **Dependencies**: `FE-HRD-249`, `BE-HRD-274` [BE]
* **Acceptance Criteria**: Modal memvalidasi input wajib pembelaan staf sebelum tombol simpan aktif; dokumen bukti terunggah sukses ke storage RS.
* **Verifikasi**: Uji input BAP pada `AT-HRD-EMP-04`.

#### 🟡 `FE-HRD-251` — Layar Penerbitan & Monitoring Surat Peringatan (SP 1-3 Masa 6 Bulan PP 35/2021) (`/hrd/relations/sanctions`)
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-251.md](../task/report/frontend/FE-HRD-251.md).
* **Outcome**: Pembangunan layar registri Surat Peringatan (`FE-HRD-248`) pada rute `/hrd/relations/sanctions`. Menampilkan riwayat sanksi masa lalu staf dari e-Dossier Sub-02 (`INT-HR-85`), otomatisasi pengisian tanggal kedaluwarsa 6 bulan kalender (`ValidUntil`), validasi kenaikan sanksi berjenjang, opsi tombol darurat penerbitan langsung SP 3 untuk pelanggaran berat berizin Direktur SDM, serta tombol aksi cetak lembar PDF resmi SP bertanda tangan elektronik.
* **Dependencies**: `FE-HRD-249`, `BE-HRD-275` [BE]
* **Acceptance Criteria**: Form penerbitan SP otomatis mengunci `ValidUntil` persis 6 bulan kalender dari `IssuedDate`; badge status sanksi (`Active`, `Expired`, `RevokedOnAppeal`) tampil jelas.
* **Verifikasi**: Uji penerbitan sanksi SP pada `AT-HRD-EMP-05`, `AT-HRD-EMP-08`, `AT-HRD-EMP-09`.

#### 🟡 `FE-HRD-252` — Komponen Modal Sanggah Banding Sanksi SP & Risalah Putusan Sidang Direksi (`SanctionAppealModal`)
* **Slice / MVP**: `S-C5` / `MVP-54`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-252.md](../task/report/frontend/FE-HRD-252.md).
* **Outcome**: Pembuatan modal dialog sanggah banding sanksi (`SanctionAppealModal.tsx`). Menyediakan antarmuka bagi staf untuk mengajukan permohonan banding tertulis maksimal 7 hari kerja sejak sanksi diserahkan, lampiran dokumen bukti sanggahan, serta form pencatatan amar putusan sidang banding Direksi (Ditolak / Diterima & Sanksi Dicabut).
* **Dependencies**: `FE-HRD-251`
* **Acceptance Criteria**: Form sanggahan menolak pengajuan yang melewati batas 7 hari kerja; putusan pengabulan banding otomatis mengubah status sanksi menjadi `RevokedOnAppeal`.
* **Verifikasi**: Uji alur sanggah banding pada `AT-HRD-EMP-10`.

#### 🟡 `FE-HRD-253` — Layar Pengelolaan Skorsing Investigasi (*Paid Suspension*) & Status Akses SIMRS (`/hrd/relations/suspensions`)
* **Slice / MVP**: `S-C5` / `MVP-55`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-253.md](../task/report/frontend/FE-HRD-253.md).
* **Outcome**: Pembangunan layar manajemen skorsing pegawai (`FE-HRD-249`) pada rute `/hrd/relations/suspensions`. Menampilkan daftar pegawai dalam masa skorsing investigasi, indikator progres waktu (Hari ke-N dari batas maksimal 180 hari), banner konfirmasi perlindungan upah penuh 100% di Sub-10 Payroll sesuai UU Cipta Kerja, serta saklar status pemblokiran real-time login SSO SIMRS/EMR dan kartu akses fisik gedung (`INT-HR-88`).
* **Dependencies**: `FE-HRD-248`, `BE-HRD-277` [BE]
* **Acceptance Criteria**: Status pemblokiran akun SIMRS terpantau aktif; durasi skorsing menampilkan peringatan jika mendekati 150 hari.
* **Verifikasi**: Uji pemantauan skorsing pada `AT-HRD-EMP-11`, `AT-HRD-EMP-12`.

#### 🟡 `FE-HRD-254` — Layar Pengaduan Keluhan Staf (Grievance ESS) & Notulen Mediasi SLA 14 Hari (`/hrd/relations/grievance`)
* **Slice / MVP**: `S-C5` / `MVP-55`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-254.md](../task/report/frontend/FE-HRD-254.md).
* **Outcome**: Pembangunan portal pengaduan hubungan kerja pegawai (`FE-HRD-250`) pada rute `/hrd/relations/grievance`. Antarmuka mandiri bagi staf untuk menyampaikan keluhan seputar shift, beban kerja, atau perselisihan interpersonal; pelacak status tiket interaktif; penghitung mundur batas waktu SLA penanganan 14 hari kerja; serta formulir notulen hasil mediasi klarifikasi damai oleh HR Relations.
* **Dependencies**: `FE-HRD-248`, `BE-HRD-279` [BE]
* **Acceptance Criteria**: Pegawai dapat memantau status tiket keluhannya secara transparan; countdown SLA menampilkan warna kuning/merah saat mendekati batas 14 hari kerja.
* **Verifikasi**: Uji tiket keluhan pada `AT-HRD-EMP-13`.

#### 🟡 `FE-HRD-255` — Komponen Modal & Portal Whistleblower Terenkripsi Asimetris Anti-Retaliasi (`WhistleblowerModal`)
* **Slice / MVP**: `S-C5` / `MVP-55`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-255.md](../task/report/frontend/FE-HRD-255.md).
* **Outcome**: Pembuatan komponen formulir pengaduan rahasia (`WhistleblowerModal.tsx`). Menampilkan opsi pelaporan `Anonim` tanpa jejak akun, textarea kronologi dugaan kecurangan yang dienkripsi asimetris pada sisi browser sebelum dikirim ke server, pernyataan resmi perlindungan anti-retaliasi, serta kotak tanda terima tiket berpasangan kode pelacak unik.
* **Dependencies**: `FE-HRD-248`, `BE-HRD-280` [BE]
* **Acceptance Criteria**: Form melakukan enkripsi payload sebelum transmisi; pengguna non-Kepatuhan tidak dapat menemukan menu atau data tiket whistleblower.
* **Verifikasi**: Uji keamanan dan enkripsi whistleblower pada `AT-HRD-EMP-14`, `AT-HRD-EMP-15`.

#### 🟡 `FE-HRD-256` — Layar Manajemen Notulen & Risalah Forum Perundingan Bipartit RS (`/hrd/relations/bipartite`)
* **Slice / MVP**: `S-C5` / `MVP-55`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-256.md](../task/report/frontend/FE-HRD-256.md).
* **Outcome**: Pembangunan layar administrasi forum Bipartit (`FE-HRD-251`) pada rute `/hrd/relations/bipartite`. Menampilkan jadwal perundingan perselisihan hak/kepentingan, formulir pencatatan daftar hadir perwakilan manajemen dan serikat buruh, editor naskah risalah rapat (*Minutes of Meeting*), serta modul pengunggahan berkas naskah Perjanjian Bersama (PB) berformat PDF.
* **Dependencies**: `FE-HRD-248`, `BE-HRD-281` [BE]
* **Acceptance Criteria**: Risalah rapat tersimpan dengan aman; dokumen Perjanjian Bersama PDF dapat diunduh dan dilihat oleh pihak yang berwenang.
* **Verifikasi**: Uji administrasi Bipartit pada `AT-HRD-EMP-16`.

#### 🟡 `FE-HRD-257` — Layar Pengajuan Terminasi Kerja (Resign 30 Hari, Kontrak, Pensiun, SP3) & Radar PKWT (`/hrd/separations`)
* **Slice / MVP**: `S-C5` / `MVP-56`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-257.md](../task/report/frontend/FE-HRD-257.md).
* **Outcome**: Pembangunan layar manajemen pengakhiran kerja (`FE-HRD-252`) pada rute `/hrd/separations`. Formulir pengajuan resign sukarela dengan validasi pemberitahuan minimal 30 hari kalender (*one month notice*, `VR-EMP-19`), pencatatan terminasi pensiun/kontrak/SP3, serta tabel radar pemantau kontrak PKWT yang akan berakhir pada H-60 dan H-30.
* **Dependencies**: `FE-HRD-253`, `BE-HRD-282` [BE]
* **Acceptance Criteria**: Pengajuan resign sukarela < 30 hari memunculkan peringatan validasi; radar kontrak PKWT menampilkan sisa hari aktif secara akurat.
* **Verifikasi**: Uji pengajuan terminasi pada `AT-HRD-EMP-17`.

#### 🟡 `FE-HRD-258` — Komponen Kalkulator Pesangon Deterministik PP 35/2021 & Lembar Rincian Finansial (`SeveranceCalculatorCard`)
* **Slice / MVP**: `S-C5` / `MVP-56`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-258.md](../task/report/frontend/FE-HRD-258.md).
* **Outcome**: Pembuatan komponen kalkulator pesangon interaktif (`SeveranceCalculatorCard.tsx`). Menampilkan masa kerja efektif yang ditarik otomatis dari `HireDate` Sub-02, rincian transparansi formula PP 35/2021 (Uang Pesangon, UPMK, UPH cuti tahunan aktif, Uang Pisah, dan kompensasi PKWT), estimasi potongan PPh 21 Final pesangon, total hak bersih (*net payout*), serta status kunci otorisasi Direktur Keuangan.
* **Dependencies**: `FE-HRD-257`, `BE-HRD-283` [BE]
* **Acceptance Criteria**: Nilai pesangon terhitung instan saat alasan terminasi dipilih; tombol sinkronisasi ke payroll berstatus disabled hingga Direktur Keuangan memberikan approval digital.
* **Verifikasi**: Uji kalkulator pesangon pada `AT-HRD-EMP-18`, `AT-HRD-EMP-19`, `AT-HRD-EMP-20`.

#### 🟡 `FE-HRD-259` — Layar Pusat Kendali Exit Clearance Multi-Departemen 5 Unit (`/hrd/separations/exit-clearance`)
* **Slice / MVP**: `S-C5` / `MVP-56`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-259.md](../task/report/frontend/FE-HRD-259.md).
* **Outcome**: Pembangunan layar kendali kliring keluar pegawai (`FE-HRD-253`) pada rute `/hrd/separations/exit-clearance`. Papan kendali visual matriks checklist serah terima 5 departemen (Unit Kerja, Umum/Aset, Farmasi/Alkes, Keuangan, IT/SIMRS) dengan status progress bar penyelesaian kliring dan rincian barang dinas yang belum diserahkan.
* **Dependencies**: `FE-HRD-253`, `BE-HRD-284` [BE]
* **Acceptance Criteria**: Matriks 5 departemen menampilkan status real-time; item yang belum diserahkan ditandai dengan warna merah tegas.
* **Verifikasi**: Uji papan kendali exit clearance pada `AT-HRD-EMP-21`.

#### 🟡 `FE-HRD-260` — Modal Signoff Kliring Unit, Indikator Gembok Payroll Final Sub-10 & Uji Linter Build
* **Slice / MVP**: `S-C5` / Verifikasi
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R03` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R15`. Laporan lama: [FE-HRD-260.md](../task/report/frontend/FE-HRD-260.md).
* **Outcome**: Pembuatan modal dialog persetujuan kliring per departemen (`DepartmentClearanceModal.tsx`), indikator visual gembok keamanan payroll `INT-HR-87` (menampilkan badge merah "LOCKED" selama checklist belum tuntas 100%), tombol aksi finalisasi exit clearance, serta eksekusi audit menyeluruh frontend Next.js: linting `npm run lint:errors` lulus 0 error dan kompilasi build produksi `npm run build` sukses tanpa warning kritis.
* **Dependencies**: `FE-HRD-258`, `FE-HRD-259`, `BE-HRD-285` [BE]
* **Acceptance Criteria**: Gembok payroll final hanya dapat dibuka jika seluruh 5 departemen telah berstatus `Approved`; `npm run lint:errors` dan `npm run build` lulus 100%.
* **Verifikasi**: Uji hard-block payroll `AT-HRD-EMP-22` dan laporan task tracked `FE-HRD-260.md`.


---

## 22. Rencana Pengiriman Frontend `S-C6`: Occupational Health & Staff Safety (K3RS & Medical Check-Up)

Bagian ini memetakan perencanaan teknis pelaksanaan antarmuka frontend Next.js App Router untuk **Sub-Modul 19 (Occupational Health & Staff Safety / K3RS & MCU — Slice `S-C6`)** menjadi 15 task terukur (`FE-HRD-261` s.d. `FE-HRD-275`) yang mencakup implementasi 8 layar utama (`FE-HRD-261` s.d. `FE-HRD-268`), 5 modal interaktif, widget tombol darurat tertusuk jarum mobile ESS, dasbor radar imunitas nakes STARKES, gembok visual kelaikan kerja pada modul roster Sub-06, dan generator formulir klaim BPJS Ketenagakerjaan JKK.

---

### 22.1 Grafik Urutan Ketergantungan Frontend `S-C6` (Pohon Teks)

#### Grafik F70: Fondasi State Redux, API Client, Komponen Visual K3RS & Dasbor Utama
```text
FE-HRD-261 ⏳ (Setup Types, Redux Slice & Axios Client Kontrak v23)
   │
   └─> FE-HRD-262 🟡 (Komponen Visual: Badge Status, Alert Kerahasiaan Medis & Kelaikan)
          │
          └─> FE-HRD-263 🟡 (Layar Dasbor Utama K3RS Rumah Sakit /hrd/occupational-health)
```

#### Grafik F71: Portal Rekam Medis Staf, Modal Fitness to Work & Manajemen Restriksi
```text
FE-HRD-263 [F70] ─┬─> FE-HRD-264 🟡 (Layar Rekam Kesehatan Staf & Evaluasi Fitness to Work)
                  │      │
                  │      └─> FE-HRD-265 🟡 (Modal Pengesahan Kelaikan Kerja Dokter Hiperkes)
                  │             │
                  │             └─> FE-HRD-266 🟡 (Layar Manajemen Pembatasan Tugas Medis Work Restriction)
                  │                    │
                  │                    └─> FE-HRD-267 🟡 (Modal Asesmen Return-to-Work RTW & Pembuka Gembok Roster)
```

#### Grafik F72: Konsol Tanggap Darurat Tertusuk Jarum, Widget Panik ESS & Radar Imunisasi
```text
FE-HRD-263 [F70] ─┬─> FE-HRD-268 🟡 (Layar Konsol Tanggap Darurat Pajanan Darah & Tertusuk Jarum)
                  │      │
                  │      └─> FE-HRD-269 🟡 (Widget Tombol Panik Darurat Tertusuk Jarum Mobile ESS Staf)
                  │
                  └─> FE-HRD-270 🟡 (Layar Radar Imunisasi & Vaksinasi Nakes STARKES KPS)
```

#### Grafik F73: Surveilans Bahaya Ruangan, MCU Berkala, Kecelakaan Kerja & Verifikasi E2E
```text
FE-HRD-263 [F70] ─┬─> FE-HRD-271 🟡 (Layar Surveilans Paparan Bahaya Ruangan Khusus TLD Radiasi)
                  ├─> FE-HRD-272 🟡 (Layar Pusat Penjadwalan & Pelaksanaan MCU Berkala STARKES)
                  ├─> FE-HRD-273 🟡 (Layar Registrasi Kecelakaan Kerja KAK/PAK & Olah TKP)
                  │      │
                  │      └─> FE-HRD-274 🟡 (Modal Generator Ekspor Formulir Klaim JKK BPJS Tahap I-II)
                  │
FE-HRD-265 [F71] ─┤
FE-HRD-267 [F71] ─┤
FE-HRD-269 [F72] ─┼─> FE-HRD-275 ⏳ (Integrasi Navigasi Sidebar K3RS, Indikator Hard-Stop Roster & Verifikasi E2E)
FE-HRD-270 [F72] ─┤
FE-HRD-271 [F73] ─┤
FE-HRD-272 [F73] ─┤
FE-HRD-274 [F73] ─┘
```

---

### 22.2 Tabel Urutan Gelombang Eksekusi Frontend `S-C6`

| Gelombang Eksekusi | Task ID | Deskripsi Singkat Pekerjaan Frontend | Prasyarat (Dependencies) | Target MVP |
|---|---|---|---|---|
| **Gelombang 1** | `FE-HRD-261` | Setup TypeScript Interfaces, Redux Slice `occupationalHealthSlice.ts` & Axios Client | None | `MVP-57` |
| **Gelombang 1** | `FE-HRD-262` | Komponen Visual K3RS: StatusBadge Kelaikan, Alert Kerahasiaan Medis & Indikator | `FE-HRD-261` | `MVP-57` |
| **Gelombang 2** | `FE-HRD-263` | Layar Dasbor Utama K3RS Rumah Sakit (`FE-HRD-261`) pada `/hrd/occupational-health` | `FE-HRD-262`, `BE-HRD-289` [BE] | `MVP-57` |
| **Gelombang 2** | `FE-HRD-264` | Layar Rekam Kesehatan Staf & Evaluasi Fitness to Work (`FE-HRD-262`) | `FE-HRD-263`, `BE-HRD-289` [BE] | `MVP-57` |
| **Gelombang 3** | `FE-HRD-265` | Modal Dialog Pengesahan Kelaikan Kerja Dokter Hiperkes (`FitnessToWorkModal.tsx`) | `FE-HRD-264`, `BE-HRD-290` [BE] | `MVP-57` |
| **Gelombang 3** | `FE-HRD-266` | Layar Manajemen Pembatasan Tugas Medis (*Work Restriction*) (`FE-HRD-263`) | `FE-HRD-265`, `BE-HRD-291` [BE] | `MVP-57` |
| **Gelombang 3** | `FE-HRD-267` | Modal Asesmen Kembali Bekerja (*Return-to-Work*) & Pelepasan Hard-Stop Roster | `FE-HRD-266`, `BE-HRD-293` [BE] | `MVP-57` |
| **Gelombang 4** | `FE-HRD-268` | Layar Konsol Tanggap Darurat Pajanan Darah & Tertusuk Jarum (`FE-HRD-264`) | `FE-HRD-263`, `BE-HRD-294` [BE] | `MVP-58` |
| **Gelombang 4** | `FE-HRD-269` | Widget Tombol Panik Darurat Tertusuk Jarum Mobile ESS Staf (`NeedleStickPanicButton.tsx`)| `FE-HRD-268`, `BE-HRD-294` [BE] | `MVP-58` |
| **Gelombang 4** | `FE-HRD-270` | Layar Radar Imunisasi & Vaksinasi Tenaga Medis (`FE-HRD-265`) | `FE-HRD-263`, `BE-HRD-297` [BE] | `MVP-58` |
| **Gelombang 5** | `FE-HRD-271` | Layar Surveilans Paparan Bahaya Ruangan Khusus TLD Radiasi (`FE-HRD-266`) | `FE-HRD-263`, `BE-HRD-298` [BE] | `MVP-58` |
| **Gelombang 5** | `FE-HRD-272` | Layar Pusat Penjadwalan & Pelaksanaan MCU Berkala STARKES (`FE-HRD-267`) | `FE-HRD-263`, `BE-HRD-299` [BE] | `MVP-59` |
| **Gelombang 5** | `FE-HRD-273` | Layar Registrasi Kecelakaan Kerja KAK/PAK & Olah TKP (`FE-HRD-268`) | `FE-HRD-263`, `BE-HRD-300` [BE] | `MVP-59` |
| **Gelombang 5** | `FE-HRD-274` | Modal Generator Ekspor Formulir Klaim JKK BPJS Ketenagakerjaan Tahap I-II | `FE-HRD-273`, `BE-HRD-300` [BE] | `MVP-59` |
| **Gelombang 6** | `FE-HRD-275` | Integrasi Navigasi Sidebar K3RS, Indikator Hard-Stop Roster Sub-06 & Uji Linter Build E2E | `FE-HRD-265`, `FE-HRD-267`, `FE-HRD-269`, `FE-HRD-270`, `FE-HRD-271`, `FE-HRD-272`, `FE-HRD-274`, `BE-HRD-301` [BE] | Verifikasi |

---

### 22.3 Rincian Kartu Spesifikasi Task Frontend `S-C6`

#### ✅ `FE-HRD-261` — Setup TypeScript Interfaces, Redux Slice `occupationalHealthSlice.ts` & Axios Client
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: ✅ PASS
* **Outcome**: Penyiapan tipe data TypeScript untuk 10 entitas K3RS, enumerasi status (`FitnessToWorkStatus`, `WorkRestrictionStatus`, dll.), konfigurasi Redux Toolkit slice (`occupationalHealthSlice.ts`) dengan async thunks untuk memanggil 24 endpoint API Kontrak v23, serta integrasi Axios client dengan interceptor token otentikasi JWT.
* **Dependencies**: None
* **Acceptance Criteria**: State Redux terinisialisasi bersih; seluruh types dan interfaces sinkron 1:1 dengan DTOs backend Kontrak v23.
* **Verifikasi**: Uji tipe data TypeScript dan eksekusi reducer Redux.

#### 🟡 `FE-HRD-262` — Komponen Visual K3RS: StatusBadge Kelaikan, Alert Kerahasiaan Medis & Indikator
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R02` ✅, layar belum diverifikasi ulang. 1 dari 1 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-262.md](../task/report/frontend/FE-HRD-262.md).
* **Outcome**: Pembuatan komponen atomik UI K3RS berbasis design token Quilvian: `FitnessStatusBadge` (hijau untuk `Fit`, kuning untuk `Fit with Restriction`, merah untuk `Unfit`), `MedicalConfidentialityAlert` (banner informasi peringatan kerahasiaan rekam medis UU PDP), dan `ImmunityIndicatorCard` (grafik visual kadar titer antibodi Anti-HBs).
* **Dependencies**: `FE-HRD-261`
* **Acceptance Criteria**: Komponen visual merender warna dan label status secara akurat tanpa styling manual hardcoded.
* **Verifikasi**: Uji visual komponen atomik K3RS.

#### 🟡 `FE-HRD-263` — Layar Dasbor Utama K3RS Rumah Sakit (`FE-HRD-261`) pada `/hrd/occupational-health`
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R02` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-263.md](../task/report/frontend/FE-HRD-263.md).
* **Outcome**: Pembangunan layar dasbor komprehensif K3RS (`FE-HRD-261`). Menyajikan kartu ringkasan kelaikan kerja rumah sakit (Total Staf, Laik Penuh, Dalam Restriksi Tugas, Tidak Laik Sementara), radar insiden tertusuk jarum aktif dalam masa pemantauan serial 6 bulan, persentase kepatuhan MCU tahunan akreditasi STARKES KPS, serta tombol pintas tanggap darurat pajanan.
* **Dependencies**: `FE-HRD-262`, `BE-HRD-289` [BE]
* **Acceptance Criteria**: Metrik agregat K3RS tampil real-time; role HR Admin hanya melihat metrik operasional tanpa rincian penyakit klinis (`HRD-DEC-176`).
* **Verifikasi**: Uji dasbor utama K3RS `AT-HRD-OHS-02`.

#### 🟡 `FE-HRD-264` — Layar Rekam Kesehatan Staf & Evaluasi Fitness to Work (`FE-HRD-262`)
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R02` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-264.md](../task/report/frontend/FE-HRD-264.md).
* **Outcome**: Pembangunan layar rekam kesehatan pegawai (`FE-HRD-262`) pada rute `/hrd/occupational-health/records`. Menampilkan tabel riwayat pemeriksaan kesehatan staf, filter berdasarkan unit kerja/ruangan risiko, serta tab rincian yang membedakan tampilan staf/dokter (rekam klinis lengkap terenkripsi) dan tampilan HR (ringkasan operasional).
* **Dependencies**: `FE-HRD-263`, `BE-HRD-289` [BE]
* **Acceptance Criteria**: Akses login pegawai mandiri menampilkan riwayat MCU pribadinya; akun HR dibatasi pada kesimpulan kelaikan saja.
* **Verifikasi**: Uji akses mandiri staf di ESS `AT-HRD-OHS-03`.

#### 🟡 `FE-HRD-265` — Modal Dialog Pengesahan Kelaikan Kerja Dokter Hiperkes (`FitnessToWorkModal.tsx`)
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R02` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-265.md](../task/report/frontend/FE-HRD-265.md).
* **Outcome**: Pembuatan modal dialog interaktif bagi dokter pemeriksa untuk mengesahkan status kelayakan kerja staf (`FitnessToWorkModal.tsx`). Menampilkan input status kelaikan, kolom alasan medis terenkripsi, input nomor lisensi/sertifikat Hiperkes dokter penilai (`VR-OHS-03`), dan konfirmasi penandatanganan digital resmi.
* **Dependencies**: `FE-HRD-264`, `BE-HRD-290` [BE]
* **Acceptance Criteria**: Tombol simpan terkunci jika nomor lisensi Hiperkes kosong; status menjadi imutabel setelah disahkan.
* **Verifikasi**: Uji modal penetapan kelaikan kerja `AT-HRD-OHS-04`.

#### 🟡 `FE-HRD-266` — Layar Manajemen Pembatasan Tugas Medis (*Work Restriction*) (`FE-HRD-263`)
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R02` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-266.md](../task/report/frontend/FE-HRD-266.md).
* **Outcome**: Pembangunan layar manajemen restriksi tugas staf (`FE-HRD-263`) pada rute `/hrd/occupational-health/restrictions`. Menampilkan daftar staf yang sedang memiliki surat pembatasan tugas dinas aktif, kategori tugas terlarang (shift malam, beban berat, ruang isolasi), tanggal kedaluwarsa restriksi, dan indikator gembok hard-stop yang terpasang di modul roster Sub-06 (`INT-HR-89`).
* **Dependencies**: `FE-HRD-265`, `BE-HRD-291` [BE]
* **Acceptance Criteria**: Tabel menampilkan sisa hari masa restriksi; indikator gembok hard-stop roster Sub-06 aktif menyala.
* **Verifikasi**: Uji layar pembatasan tugas dinas `AT-HRD-OHS-05`.

#### 🟡 `FE-HRD-267` — Modal Asesmen Kembali Bekerja (*Return-to-Work*) & Pelepasan Hard-Stop Roster
* **Slice / MVP**: `S-C6` / `MVP-57`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R02` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-267.md](../task/report/frontend/FE-HRD-267.md).
* **Outcome**: Pembuatan modal dialog evaluasi kesiapan kembali bertugas penuh (`ReturnToWorkModal.tsx`). Dokter K3RS mengisi checklist evaluasi fisik/motorik, kesimpulan kelaikan tugas normal, dan menerbitkan surat pemulihan yang secara instan melepaskan status gembok hard-stop roster di Sub-06 (`VR-OHS-07`, `INT-HR-89`).
* **Dependencies**: `FE-HRD-266`, `BE-HRD-293` [BE]
* **Acceptance Criteria**: Pelepasan restriksi wajib menyertakan hasil asesmen RTW; badge kelaikan staf berubah kembali hijau "Fit".
* **Verifikasi**: Uji alur asesmen RTW `AT-HRD-OHS-07`.

#### 🟡 `FE-HRD-268` — Layar Konsol Tanggap Darurat Pajanan Darah & Tertusuk Jarum (`FE-HRD-264`)
* **Slice / MVP**: `S-C6` / `MVP-58`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R02` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-268.md](../task/report/frontend/FE-HRD-268.md).
* **Outcome**: Pembangunan layar ruang kendali tanggap darurat tertusuk jarum (`FE-HRD-264`) pada rute `/hrd/occupational-health/needle-stick`. Menampilkan antrean tiket darurat cito, indikator hitung mundur waktu emas terapi PPP (< 4 jam), identitas pasien sumber, dan matriks pemantauan surveilans serial darah pada Bulan ke-1, Bulan ke-3, dan Bulan ke-6.
* **Dependencies**: `FE-HRD-263`, `BE-HRD-294` [BE]
* **Acceptance Criteria**: Tiket darurat baru berkedip dengan alarm merah; timer hitung mundur PPP berjalan akurat sejak jam insiden.
* **Verifikasi**: Uji konsol tanggap darurat pajanan `AT-HRD-OHS-09`.

#### 🟡 `FE-HRD-269` — Widget Tombol Panik Darurat Tertusuk Jarum Mobile ESS Staf (`NeedleStickPanicButton.tsx`)
* **Slice / MVP**: `S-C6` / `MVP-58`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R02` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-269.md](../task/report/frontend/FE-HRD-269.md).
* **Outcome**: Pembuatan widget tombol panik cito berukuran besar pada portal ESS mobile staf rumah sakit (`NeedleStickPanicButton.tsx`). Menyediakan formulir lapor cepat 3 langkah: jam kejadian, lokasi ruangan, dan status identitas pasien sumber, yang secara instan mengirimkan notifikasi darurat sirene ke posko K3RS/PPI dalam < 5 detik (`INT-HR-91`).
* **Dependencies**: `FE-HRD-268`, `BE-HRD-294` [BE]
* **Acceptance Criteria**: Pelaporan selesai dalam < 30 detik oleh korban; notifikasi darurat terkirim seketika ke konsol K3RS.
* **Verifikasi**: Uji tombol darurat panik ESS `AT-HRD-OHS-08`.

#### 🟡 `FE-HRD-270` — Layar Radar Imunisasi & Vaksinasi Tenaga Medis (`FE-HRD-265`)
* **Slice / MVP**: `S-C6` / `MVP-58`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R02` ✅, layar belum diverifikasi ulang. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-270.md](../task/report/frontend/FE-HRD-270.md).
* **Outcome**: Pembangunan layar radar imunisasi nakes (`FE-HRD-265`) pada rute `/hrd/occupational-health/vaccinations`. Menampilkan tabel buku vaksinasi elektronik staf, log nomor batch & pabrikan vaksin, pelacak visual kadar titer antibodi Anti-HBs kuantitatif, serta penanda status staf yang membutuhkan suntikan vaksinasi booster Hepatitis B karena titer < 10 mIU/mL (`VR-OHS-15`).
* **Dependencies**: `FE-HRD-263`, `BE-HRD-297` [BE]
* **Acceptance Criteria**: Titer di bawah 10 mIU/mL ditandai badge peringatan merah; tombol buat rujukan booster dapat diklik satu sentuhan.
* **Verifikasi**: Uji radar kepatuhan imunitas nakes `AT-HRD-OHS-15`.

#### 🟡 `FE-HRD-271` — Layar Surveilans Paparan Bahaya Ruangan Khusus TLD Radiasi (`FE-HRD-266`)
* **Slice / MVP**: `S-C6` / `MVP-58`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memanggil API, tetapi sebelum audit menampilkan data contoh dan jawaban berhasil palsu saat API gagal; service-nya sudah diperbaiki `FE-HRD-R02` ✅, layar belum diverifikasi ulang. 1 dari 1 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-271.md](../task/report/frontend/FE-HRD-271.md).
* **Outcome**: Pembangunan layar pemantauan bahaya okupasional ruangan (`FE-HRD-266`) pada rute `/hrd/occupational-health/surveillance`. Menampilkan grafik tren dosis radiasi badge TLD triwulanan staf radiologi/kateterisasi, peringatan batas dosis kumulatif tahunan Bapeten (> 20 mSv), serta rekaman hasil audiometri dan spirometri staf unit berisiko tinggi.
* **Dependencies**: `FE-HRD-263`, `BE-HRD-298` [BE]
* **Acceptance Criteria**: Dosis radiasi di atas batas aman memunculkan banner peringatan merah bahaya radiasi dan rekomendasi rotasi kerja.
* **Verifikasi**: Uji pemantauan dosis radiasi TLD `AT-HRD-OHS-17`.

#### 🟡 `FE-HRD-272` — Layar Pusat Penjadwalan & Pelaksanaan MCU Berkala STARKES (`FE-HRD-267`)
* **Slice / MVP**: `S-C6` / `MVP-59`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-272.md](../task/report/frontend/FE-HRD-272.md).
* **Outcome**: Pembangunan layar pusat MCU berkala staf (`FE-HRD-267`) pada rute `/hrd/occupational-health/mcu`. Kalender penjadwalan pemeriksaan kesehatan tahunan, penapisan paket otomatis berbasis risiko ruangan kerja staf, dasbor progres kehadiran pegawai, dan generator ekspor laporan pemenuhan standar akreditasi rumah sakit STARKES instrumen KPS.
* **Dependencies**: `FE-HRD-263`, `BE-HRD-299` [BE]
* **Acceptance Criteria**: Paket MCU tersaring otomatis sesuai profil risiko ruangan; tombol ekspor dokumen kepatuhan STARKES KPS menghasilkan dokumen resmi.
* **Verifikasi**: Uji penapisan MCU berbasis risiko `AT-HRD-OHS-12` dan ekspor laporan STARKES `AT-HRD-OHS-22`.

#### 🟡 `FE-HRD-273` — Layar Registrasi Kecelakaan Kerja KAK/PAK & Olah TKP (`FE-HRD-268`)
* **Slice / MVP**: `S-C6` / `MVP-59`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-273.md](../task/report/frontend/FE-HRD-273.md).
* **Outcome**: Pembangunan layar pencatatan kecelakaan kerja (`FE-HRD-268`) pada rute `/hrd/occupational-health/injuries`. Formulir registrasi insiden kecelakaan fisik/penyakit akibat kerja, galeri foto tempat kejadian perkara (TKP), input saksi, matriks investigasi akar masalah keselamatan K3RS (CAPA), dan pelacakan hari kerja hilang (*Lost Time Injury* - LTI).
* **Dependencies**: `FE-HRD-263`, `BE-HRD-300` [BE]
* **Acceptance Criteria**: Form investigasi memuat minimal 2 investigator K3RS; LTI terhitung sinkron dengan hari izin sakit.
* **Verifikasi**: Uji registrasi kecelakaan kerja `AT-HRD-OHS-19` dan sinkronisasi LTI `AT-HRD-OHS-20`.

#### 🟡 `FE-HRD-274` — Modal Generator Ekspor Formulir Klaim JKK BPJS Ketenagakerjaan Tahap I-II
* **Slice / MVP**: `S-C6` / `MVP-59`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 1 dari 1 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R14`. Laporan lama: [FE-HRD-274.md](../task/report/frontend/FE-HRD-274.md).
* **Outcome**: Pembuatan modal dialog pembuatan berkas klaim BPJS JKK (`BPJSClaimExportModal.tsx`). Menampilkan generator dokumen formulir pelaporan Tahap I berstandar BPJS Ketenagakerjaan dengan batas waktu pelaporan < 2x24 jam (`VR-OHS-18`), serta kompilasi lampiran kwitansi asli dan resume medis dokter untuk klaim santunan Tahap II.
* **Dependencies**: `FE-HRD-273`, `BE-HRD-300` [BE]
* **Acceptance Criteria**: Berkas PDF Formulir JKK Tahap I terisi lengkap otomatis dan siap diserahkan ke BPJS Ketenagakerjaan.
* **Verifikasi**: Uji generator formulir klaim JKK BPJS `AT-HRD-OHS-18` dan `AT-HRD-OHS-21`.

#### ✅ `FE-HRD-275` — Integrasi Navigasi Sidebar K3RS, Indikator Hard-Stop Roster Sub-06 & Uji Linter Build E2E
* **Slice / MVP**: `S-C6` / Verifikasi
* **Status**: ✅ PASS
* **Outcome**: Pendaftaran rute dan menu navigasi K3RS pada sidebar utama modul HRD Next.js App Router, integrasi badge peringatan kelaikan medis pada modul penyusunan roster Sub-06 (menampilkan tooltip restriksi dinas), serta eksekusi audit kualitas frontend secara menyeluruh: validasi linting `npm run lint:errors` lulus 0 error dan kompilasi build produksi `npm run build` sukses 100%.
* **Dependencies**: Seluruh task frontend `FE-HRD-261` s.d. `FE-HRD-274` serta task backend `BE-HRD-301` [BE]
* **Acceptance Criteria**: Menu K3RS tampil rapi sesuai permission user; gembok restriksi muncul di layar roster Sub-06; `npm run lint:errors` dan `npm run build` lulus tanpa error.
* **Verifikasi**: Uji hard-stop roster `AT-HRD-OHS-06` dan laporan task tracked `FE-HRD-275.md`.


---

## 23. S-A7-V2: Employee Self-Service (ESS) & Kotak Masuk Persetujuan Terpadu (Unified Approval Inbox V2 Hardening)

### Grafik Urutan Dependency Frontend Slice S-A7-V2

#### Grafik F74 — Fondasi Redux Store, Dasbor Kotak Masuk Persetujuan & Filter Tab (`FE-HRD-276` s.d. `FE-HRD-279`)

```text
BE-HRD-305 ⏳ [BE] ─> FE-HRD-276 ⏳ ─> FE-HRD-277 🟡 ─┬─> FE-HRD-278 🟡
                                                      │
                                                      └─> FE-HRD-279 🟡
```

#### Grafik F75 — Lembar Rincian Permohonan, Drawer Riwayat & Modal Aksi Tri-State (`FE-HRD-280` s.d. `FE-HRD-283`)

```text
FE-HRD-277 🟡 [F74] ─┬─> FE-HRD-280 🟡 ─┬─> FE-HRD-281 🟡
BE-HRD-309 ⏳ [BE]  ─┘                 ├─> FE-HRD-282 🟡
                                        │
                                        └─> FE-HRD-283 🟡
```

#### Grafik F76 — Tata Kelola Delegasi Plh, Matriks Alur & Modal Bulk Approval PIN (`FE-HRD-284` s.d. `FE-HRD-287`)

```text
FE-HRD-277 🟡 [F74] ─┬─> FE-HRD-284 🟡 ─> FE-HRD-285 🟡
BE-HRD-307 ⏳ [BE]  ─┘
FE-HRD-278 🟡 [F74] ─┬─> FE-HRD-286 🟡
BE-HRD-312 ⏳ [BE]  ─┘
FE-HRD-277 🟡 [F74] ─┬─> FE-HRD-287 🟡
BE-HRD-303 ⏳ [BE]  ─┤
BE-HRD-310 ⏳ [BE]  ─┘
```

#### Grafik F77 — Portal Mandiri ESS, Pelacak Riwayat, Lembar Resubmit Revisi & Audit E2E (`FE-HRD-288` s.d. `FE-HRD-290`)

```text
FE-HRD-276 ⏳ [F74] ─┬─> FE-HRD-288 🟡 ─┬─> FE-HRD-289 🟡
BE-HRD-304 ⏳ [BE]  ─┘                 │
                                        └─> FE-HRD-290 ⏳
FE-HRD-276 ⏳ s.d. FE-HRD-289 🟡 ─────────> FE-HRD-290 ⏳
BE-HRD-317 ⏳ [BE] ───────────────────────┘
```

---

### Tabel Gelombang Eksekusi Frontend Slice S-A7-V2

| Gelombang | Target Cakupan Layar & Komponen UI | Task Frontend | Prasyarat Backend Terkait |
|---|---|---|---|
| **MVP-60** | Redux Store Terpadu, Dasbor Kotak Masuk Persetujuan, Filter Tab Antrean & Kartu Metrik SLA | `FE-HRD-276`, `FE-HRD-277`, `FE-HRD-278`, `FE-HRD-279` | `BE-HRD-305` (Inbox Query Service) |
| **MVP-61** | Lembar Detail Permohonan, Stepper Visual Alur, Modal Persetujuan PIN & Modal Tolak/Revisi | `FE-HRD-280`, `FE-HRD-281`, `FE-HRD-282`, `FE-HRD-283` | `BE-HRD-309` (Approval Action Service) |
| **MVP-62** | Pengaturan Delegasi Plh, Modal Kuasa Digital, Modal Persetujuan Massal & Matriks Builder | `FE-HRD-284`, `FE-HRD-285`, `FE-HRD-286`, `FE-HRD-287` | `BE-HRD-307`, `BE-HRD-312` |
| **Verifikasi** | Portal Mandiri Staf ESS, Pelacak Riwayat, Lembar Resubmit Draf, Prompt Plh & Audit Linting Build | `FE-HRD-288`, `FE-HRD-289`, `FE-HRD-290` | `BE-HRD-304`, `BE-HRD-317` |

---

### Rincian 15 Task Frontend Slice S-A7-V2 (`FE-HRD-276` s.d. `FE-HRD-290`)

#### ✅ `FE-HRD-276` — State Management Redux Toolkit `unifiedApprovalSlice.ts` & Sinkronisasi Token Auth
* **Slice / MVP**: `S-A7-V2` / `MVP-60`
* **Status**: ✅ PASS
* **Outcome**: Pembuatan Redux slice terpusat `@/lib/store/slices/unifiedApprovalSlice.ts` untuk mengelola state global: daftar antrean permohonan kotak masuk, filter aktif (tipe request, status, urgensi), metrik counter pending/urgent H-4 SLA, detail tiket terpilih, riwayat komentar, dan state seleksi centang persetujuan massal bersyarat.
* **Dependencies**: `BE-HRD-305` [BE]
* **Acceptance Criteria**: State Redux tersimpan konsisten; counter pending dan urgent terupdate otomatis saat aksi dilakukan.
* **Verifikasi**: Uji state management Redux `AT-HRD-WF-03`.

#### 🟡 `FE-HRD-277` — Layar Utama Kotak Masuk Persetujuan Terpadu (`/hrd/approvals/inbox` - `FE-HRD-276`)
* **Slice / MVP**: `S-A7-V2` / `MVP-60`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R21`. Laporan lama: [FE-HRD-277.md](../task/report/frontend/FE-HRD-277.md).
* **Outcome**: Pembangunan layar dasbor utama kotak masuk persetujuan atasan (`/hrd/approvals/inbox`). Menampilkan tabel antrean permohonan terpadu lintas 15 modul HRD dengan kolom nomor tiket, nama pemohon, jabatan, unit kerja, tipe request, ringkasan permohonan, tanggal pengajuan, hitung mundur batas SLA, penanda status delegasi (*Delegated Badge*), dan tombol aksi cepat.
* **Dependencies**: `FE-HRD-276`, `BE-HRD-305` [BE]
* **Acceptance Criteria**: Seluruh permohonan pending tampil terpadu dalam 1 tabel; paginasi dan sorting tanggal/urgensi bekerja responsif.
* **Verifikasi**: Uji tampilan kotak masuk terpadu `AT-HRD-WF-03`.

#### 🟡 `FE-HRD-278` — Komponen Tab Filter Antrean Kotak Masuk (*Pending*, *Urgent H-4*, *Delegated*, *Approved*)
* **Slice / MVP**: `S-A7-V2` / `MVP-60`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R21`. Laporan lama: [FE-HRD-278.md](../task/report/frontend/FE-HRD-278.md).
* **Outcome**: Pembuatan komponen segmented tab filter antrean kotak masuk: (1) *Semua Pending*; (2) *Mendesak (H-4 SLA)* dengan badge merah berkedip; (3) *Sebagai Plh (Delegated)* untuk tiket yang dilimpahkan dari pejabat lain; dan (4) *Riwayat Persetujuan*. Dilengkapi dropdown filter per kategori permohonan (Cuti, Lembur, Tukar Shift, Restitusi, Pinjaman, Resign, Diklat).
* **Dependencies**: `FE-HRD-277`
* **Acceptance Criteria**: Perpindahan tab menyaring data secara instan; filter kategori request bekerja akurat.
* **Verifikasi**: Uji fungsionalitas tab filter kotak masuk `AT-HRD-WF-03`.

#### 🟡 `FE-HRD-279` — Kartu Rangkuman Metrik Kotak Masuk & Penanda Badge SLA Realtime
* **Slice / MVP**: `S-A7-V2` / `MVP-60`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R21`. Laporan lama: [FE-HRD-279.md](../task/report/frontend/FE-HRD-279.md).
* **Outcome**: Pembuatan 4 kartu metrik analitik di bagian atas dasbor persetujuan: (1) Total Menunggu Persetujuan; (2) Tiket Kritis Mendekati SLA (< 4 Jam); (3) Wewenang Delegasi Plh Aktif; dan (4) Kepatuhan SLA Departemen (%). Dilengkapi visual timer hitung mundur realtime yang berubah warna dari hijau (> 12 jam), kuning (4-12 jam), hingga merah berkedip (< 4 jam / overdue).
* **Dependencies**: `FE-HRD-277`
* **Acceptance Criteria**: Kartu metrik menampilkan agregat akurat; indikator warna SLA berubah dinamis sesuai sisa durasi.
* **Verifikasi**: Uji visualisasi timer SLA dan kartu metrik `AT-HRD-WF-10`.

#### 🟡 `FE-HRD-280` — Layar Detail & Riwayat Permohonan Lengkap (`/hrd/approvals/[id]` - `FE-HRD-277`)
* **Slice / MVP**: `S-A7-V2` / `MVP-61`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R21`. Laporan lama: [FE-HRD-280.md](../task/report/frontend/FE-HRD-280.md).
* **Outcome**: Pembangunan layar lembar rincian permohonan persetujuan (`/hrd/approvals/[id]`). Menyajikan profil pemohon, detail formulir transaksi asli, viewer dokumen lampiran pendukung (surat sakit dokter, struk kuitansi, formulir pelatihan), kartu indikator risiko hijau/merah, serta panel thread percakapan klarifikasi antara pemohon dan approver.
* **Dependencies**: `FE-HRD-277`, `BE-HRD-309` [BE]
* **Acceptance Criteria**: Data permohonan dan dokumen lampiran tampil utuh; tombol otorisasi hanya aktif bagi approver sah/Plh.
* **Verifikasi**: Uji lembar detail permohonan `AT-HRD-WF-07`.

#### 🟡 `FE-HRD-281` — Visual Alur Tahapan Persetujuan & Kartu Indikator Risiko Hijau/Merah
* **Slice / MVP**: `S-A7-V2` / `MVP-61`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R21`. Laporan lama: [FE-HRD-281.md](../task/report/frontend/FE-HRD-281.md).
* **Outcome**: Pembuatan komponen horizontal stepper visual yang menampilkan alur rantai persetujuan (Step 1 -> Step 2 -> Disetujui Final), mencantumkan nama pejabat approver, jabatan, status (Selesai, Sedang Berjalan, Menunggu), dan timestamp keputusan. Dilengkapi kartu validasi risiko yang menampilkan indikator kepatuhan: saldo cuti, bentrok roster shift, dan tap absensi.
* **Dependencies**: `FE-HRD-280`
* **Acceptance Criteria**: Stepper tahapan persetujuan merefleksikan posisi tiket saat ini; indikator risiko menampilkan warna hijau/merah sesuai validasi.
* **Verifikasi**: Uji visual alur tahapan dan kartu risiko `AT-HRD-WF-02`.

#### 🟡 `FE-HRD-282` — Modal Dialog Persetujuan Tunggal & Verifikasi PIN Otorisasi
* **Slice / MVP**: `S-A7-V2` / `MVP-61`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R21`. Laporan lama: [FE-HRD-282.md](../task/report/frontend/FE-HRD-282.md).
* **Outcome**: Pembuatan modal dialog konfirmasi persetujuan (*ApprovalConfirmationModal.tsx*). Menampilkan ringkasan dampak persetujuan (misal: "Saldo cuti pegawai akan dipotong 3 hari"), kolom catatan persetujuan opsional, dan input PIN otorisasi keamanan 6-digit pejabat approver sebelum transaksi disahkan ke backend (`VR-WF-10`).
* **Dependencies**: `FE-HRD-280`
* **Acceptance Criteria**: Persetujuan hanya diproses setelah verifikasi PIN sah; modal menutup dan status tiket berubah disetujui.
* **Verifikasi**: Uji modal persetujuan tunggal ber-PIN `AT-HRD-WF-12`.

#### 🟡 `FE-HRD-283` — Modal Dialog Penolakan Berkas (Wajib Min 15 Karakter) & Modal Pengembalian Revisi
* **Slice / MVP**: `S-A7-V2` / `MVP-61`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R21`. Laporan lama: [FE-HRD-283.md](../task/report/frontend/FE-HRD-283.md).
* **Outcome**: Pembuatan dua modal dialog: (1) *RejectRequestModal.tsx* untuk penolakan permohonan dengan validasi ketat teks alasan penolakan wajib diisi minimal 15 karakter (`VR-WF-05`) dengan counter karakter realtime; dan (2) *ReturnForRevisionModal.tsx* untuk mengembalikan permohonan ke staf pemohon disertai instruksi perbaikan data yang jelas (`VR-WF-06`).
* **Dependencies**: `FE-HRD-280`
* **Acceptance Criteria**: Tombol konfirmasi penolakan disabled jika teks < 15 karakter; modal pengembalian revisi mengirimkan draf kembali ke pemohon.
* **Verifikasi**: Uji validasi alasan penolakan minimal 15 karakter `AT-HRD-WF-07`.

#### 🟡 `FE-HRD-284` — Layar Pengaturan Pendelegasian Wewenang Plh (`/hrd/approvals/delegation` - `FE-HRD-278`)
* **Slice / MVP**: `S-A7-V2` / `MVP-62`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 1 dari 1 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R33`. Laporan lama: [FE-HRD-284.md](../task/report/frontend/FE-HRD-284.md).
* **Outcome**: Pembangunan layar manajemen pendelegasian wewenang persetujuan (`/hrd/approvals/delegation`). Menampilkan tabel surat kuasa delegasi aktif, nama pejabat penerima kuasa (Plh), rentang tanggal berlaku, jenis kategori wewenang yang dialihkan, riwayat delegasi masa lalu, serta tombol pencabutan wewenang cito sebelum batas waktu berakhir.
* **Dependencies**: `FE-HRD-277`, `BE-HRD-307` [BE]
* **Acceptance Criteria**: Pejabat dapat melihat status delegasi yang dibuatnya dan wewenang Plh yang sedang diembannya.
* **Verifikasi**: Uji layar manajemen pendelegasian Plh `AT-HRD-WF-05`.

#### 🟡 `FE-HRD-285` — Modal Surat Kuasa Digital Penunjukan Plh & Pemilihan Pengganti Sah
* **Slice / MVP**: `S-A7-V2` / `MVP-62`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R33`. Laporan lama: [FE-HRD-285.md](../task/report/frontend/FE-HRD-285.md).
* **Outcome**: Pembuatan modal dialog penunjukan Pelaksana Harian (*CreateDelegationModal.tsx*). Menampilkan dropdown pemilihan staf pengganti yang tersaring otomatis hanya untuk pejabat setara atau 1 tingkat di bawahnya (`VR-WF-09`), date-range picker tanggal mulai dan tanggal selesai, checklist pilihan jenis permohonan (Semua / Parsial), dan input alasan penunjukan (cuti/dinas luar).
* **Dependencies**: `FE-HRD-284`
* **Acceptance Criteria**: Dropdown hanya menampilkan staf pengganti yang sah; validasi menolak tanggal mulai di masa lampau.
* **Verifikasi**: Uji modal pembuatan surat kuasa Plh `AT-HRD-WF-05`.

#### 🟡 `FE-HRD-286` — Modal Persetujuan Massal Bersyarat (*Conditional Bulk Approval Modal*) & Filter Guard Item Berisiko
* **Slice / MVP**: `S-A7-V2` / `MVP-62`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R33`. Laporan lama: [FE-HRD-286.md](../task/report/frontend/FE-HRD-286.md).
* **Outcome**: Pembuatan modal dialog persetujuan massal (*BulkApprovalModal.tsx*). Menampilkan daftar ringkasan tiket yang terpilih, memvalidasi dan secara otomatis menyaring keluar item berisiko/bentrok atau bertanda kuning/merah (`VR-WF-07`), menyajikan total nominal finansial atau hari cuti akumulatif, dan meminta 1 kali input PIN otorisasi keamanan pejabat untuk mengeksekusi batch.
* **Dependencies**: `FE-HRD-278`, `BE-HRD-312` [BE]
* **Acceptance Criteria**: Item berisiko otomatis dikecualikan dari persetujuan massal; laporan hasil batch ditampilkan transparan pasca eksekusi.
* **Verifikasi**: Uji modal persetujuan massal bersyarat `AT-HRD-WF-11`.

#### 🟡 `FE-HRD-287` — Layar Konfigurasi Matriks Alur Persetujuan (`/hrd/approvals/matrix` - `FE-HRD-279`) & Konsol Monitoring Overdue (`FE-HRD-283`)
* **Slice / MVP**: `S-A7-V2` / `MVP-62`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R33`. Laporan lama: [FE-HRD-287.md](../task/report/frontend/FE-HRD-287.md).
* **Outcome**: Pembangunan dua layar administratif: (1) Visual Approval Matrix Builder (`/hrd/approvals/matrix`) bagi HR Admin untuk merancang alur persetujuan multi-level per unit kerja, plafon nominal, dan durasi SLA (jam); dan (2) Konsol Monitoring Tiket Overdue (`/hrd/approvals/overdue`) bagi Manajemen SDM untuk memantau penumpukan antrean macet dan melakukan intervensi eskalasi manual.
* **Dependencies**: `FE-HRD-277`, `BE-HRD-303` [BE], `BE-HRD-310` [BE]
* **Acceptance Criteria**: Matrix builder menyimpan konfigurasi alur persetujuan dinamis; konsol overdue menampilkan radar tiket macet.
* **Verifikasi**: Uji konfigurasi matriks alur persetujuan `AT-HRD-WF-02` dan monitoring overdue `AT-HRD-WF-09`.

#### 🟡 `FE-HRD-288` — Layar Portal Layanan Mandiri Karyawan ESS (`/hrd/self-service` - `FE-HRD-280`) & Menu Cepat Permohonan
* **Slice / MVP**: `S-A7-V2` / Verifikasi
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R33`. Laporan lama: [FE-HRD-288.md](../task/report/frontend/FE-HRD-288.md).
* **Outcome**: Pembangunan portal beranda layanan mandiri pegawai (`/hrd/self-service`). Menampilkan kartu saldo hak staf (sisa cuti tahunan, kuota lembur bulan berjalan, plafon benefit medis), kartu menu cepat pengajuan mandiri (Ajukan Cuti, Rencana Lembur, Tukar Shift, Klaim Restitusi, Pinjaman, Resign, Surat HR), serta widget ringkasan status 3 permohonan terakhir staf.
* **Dependencies**: `FE-HRD-276`, `BE-HRD-304` [BE]
* **Acceptance Criteria**: Portal ESS menyajikan data hak pegawai secara personal dan terisolasi; menu cepat membuka formulir pengajuan sah.
* **Verifikasi**: Uji portal beranda ESS mandiri `AT-HRD-WF-01`.

#### 🟡 `FE-HRD-289` — Layar Pelacak Riwayat Permohonan Mandiri (`FE-HRD-281`) & Lembar Perbaikan Draf Revisi (`FE-HRD-282`)
* **Slice / MVP**: `S-A7-V2` / Verifikasi
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R33`. Laporan lama: [FE-HRD-289.md](../task/report/frontend/FE-HRD-289.md).
* **Outcome**: Pembangunan dua layar pelacak ESS: (1) Layar Pelacak Riwayat Permohonan (`/hrd/self-service/my-requests`) dengan status tracker visual dan tombol unduh surat/bukti persetujuan PDF; dan (2) Layar Pengeditan Draf Revisi (`/hrd/self-service/revisions/[id]`) untuk permohonan yang dikembalikan atasan (*Returned*), menampilkan drawer catatan kekurangan dari atasan, fasilitas unggah ulang lampiran, dan tombol *Resubmit Permohonan* (`VR-WF-06`).
* **Dependencies**: `FE-HRD-288`
* **Acceptance Criteria**: Staf dapat memantau posisi tiketnya di pohon approver; formulir revisi mengizinkan pengeditan dan pengajuan ulang draf.
* **Verifikasi**: Uji pelacak riwayat ESS dan alur resubmit draf perbaikan `AT-HRD-WF-16`.

#### ✅ `FE-HRD-290` — Pendaftaran Navigasi Sidebar, Prompt Otomasi Plh Cuti (`INT-HR-94`) & Uji Build Linting Standalone
* **Slice / MVP**: `S-A7-V2` / Verifikasi
* **Status**: ✅ PASS
* **Outcome**: Pendaftaran seluruh rute menu navigasi kotak masuk persetujuan dan portal ESS pada sidebar utama aplikasi Next.js App Router, integrasi prompt modal otomatis pembuatan delegasi Plh saat pengajuan cuti pejabat disetujui di modul Sub-08 (`INT-HR-94`), serta eksekusi audit kualitas frontend secara menyeluruh: validasi linting `npm run lint:errors` lulus 0 error dan kompilasi build produksi `npm run build` sukses 100%.
* **Dependencies**: Seluruh task frontend `FE-HRD-276` s.d. `FE-HRD-289` serta task backend `BE-HRD-317` [BE]
* **Acceptance Criteria**: Menu approvals dan self-service tampil rapi sesuai permission akun; prompt Plh muncul saat cuti disetujui; `npm run lint:errors` dan `npm run build` lulus tanpa error.
* **Verifikasi**: Uji integrasi navigasi, prompt delegasi cuti `AT-HRD-WF-19`, dan laporan task tracked `FE-HRD-290.md`.


---

## 24. S-D5: Perjalanan Dinas & Manajemen Biaya Operasional (Business Travel & Expense Management)

### Grafik Urutan Dependency Frontend Slice S-D5

#### Grafik F78 — Fondasi Redux Store, Dasbor Perjalanan Dinas & Matriks Tarif (`FE-HRD-291` s.d. `FE-HRD-294`)

```text
BE-HRD-319 ⏳ [BE] ─> FE-HRD-291 ⏳ ─> FE-HRD-292 🟡 ─┬─> FE-HRD-293 🟡
                                                      │
                                                      └─> FE-HRD-294 🟡
```

#### Grafik F79 — Formulir Multi-Step SPPD, Pengajuan Uang Muka & Voucher Kasir (`FE-HRD-295` s.d. `FE-HRD-298`)

```text
FE-HRD-292 🟡 [F78] ─┬─> FE-HRD-295 🟡 ─┬─> FE-HRD-296 🟡 ─> FE-HRD-297 🟡
BE-HRD-320 ⏳ [BE]  ─┘                 │
                                        └─> FE-HRD-298 🟡
BE-HRD-324 ⏳ [BE]  ──────────────────────────────────────┘
```

#### Grafik F80 — Lembar Rekonsiliasi Settlement, Modal Deklarasi Darurat & Radar Overdue (`FE-HRD-299` s.d. `FE-HRD-302`)

```text
FE-HRD-295 🟡 [F79] ─┬─> FE-HRD-299 🟡 ─┬─> FE-HRD-300 🟡 ─> FE-HRD-301 🟡
BE-HRD-326 ⏳ [BE]  ─┘                 │
                                        └─> FE-HRD-302 🟡
BE-HRD-327 ⏳ [BE]  ───────────────────────┘
```

#### Grafik F81 — Portal Klaim Biaya Non-Trip, Integrasi Roster Sub-06 & Audit Linting/Build E2E (`FE-HRD-303` s.d. `FE-HRD-305`)

```text
FE-HRD-291 ⏳ [F78] ─┬─> FE-HRD-303 🟡 ─> FE-HRD-304 🟡 ─┬─> FE-HRD-305 ⏳
BE-HRD-330 ⏳ [BE]  ─┘                                  │
FE-HRD-291 ⏳ s.d. FE-HRD-304 🟡 ─────────────────────────┤
BE-HRD-333 ⏳ [BE]  ─────────────────────────────────────┘
```

---

### Tabel Gelombang Eksekusi Frontend Slice S-D5

| Gelombang | Target Cakupan Layar & Komponen UI | Task Frontend | Prasyarat Backend Terkait |
|---|---|---|---|
| **MVP-63** | Redux Store Terpadu, Dasbor Utama Perjalanan Dinas, Kartu Analitik Anggaran & Matriks Tarif | `FE-HRD-291`, `FE-HRD-292`, `FE-HRD-293`, `FE-HRD-294` | `BE-HRD-319` (Travel Rates Service) |
| **MVP-64** | Pusat Penugasan SPPD, Formulir Pengajuan Multi-Step, Kalkulator Per Diem & Konsol Uang Muka | `FE-HRD-295`, `FE-HRD-296`, `FE-HRD-297`, `FE-HRD-298` | `BE-HRD-320`, `BE-HRD-324` |
| **MVP-65** | Lembar Rekonsiliasi Settlement, Modal Deklarasi Darurat, Modal Setor Balik Kas & Radar Overdue | `FE-HRD-299`, `FE-HRD-300`, `FE-HRD-301`, `FE-HRD-302` | `BE-HRD-326`, `BE-HRD-327` |
| **Verifikasi** | Portal Klaim Biaya Non-Trip, Modal Upload Struk, Integrasi Navigasi & Uji Build Linter E2E | `FE-HRD-303`, `FE-HRD-304`, `FE-HRD-305` | `BE-HRD-330`, `BE-HRD-333` |

---

### Rincian 15 Task Frontend Slice S-D5 (`FE-HRD-291` s.d. `FE-HRD-305`)

#### ✅ `FE-HRD-291` — State Management Redux Toolkit `businessTravelSlice.ts` & Sinkronisasi Token
* **Slice / MVP**: `S-D5` / `MVP-63`
* **Status**: ✅ PASS
* **Outcome**: Pembuatan Redux slice terpusat `@/lib/store/slices/businessTravelSlice.ts` untuk mengelola state global: daftar antrean SPPD dinas, filter rute & status, kalkulator tarif per diem harian, pengajuan uang muka kasir, lembar kerja rekonsiliasi settlement, dan antrean klaim biaya operasional non-trip.
* **Dependencies**: `BE-HRD-319` [BE]
* **Acceptance Criteria**: State Redux tersimpan konsisten; kalkulasi tarif per diem terhitung dinamis saat zona atau grade berubah.
* **Verifikasi**: Uji state management Redux `AT-HRD-TRV-02`.

#### 🟡 `FE-HRD-292` — Layar Dasbor Utama Perjalanan Dinas & Manajemen Biaya (`/hrd/travel/dashboard` - `FE-HRD-291`)
* **Slice / MVP**: `S-D5` / `MVP-63`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 1 dari 1 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R22`. Laporan lama: [FE-HRD-292.md](../task/report/frontend/FE-HRD-292.md).
* **Outcome**: Pembangunan layar dasbor komprehensif perjalanan dinas dan biaya (`/hrd/travel/dashboard`). Menampilkan kartu metrik real-time: Total SPPD Aktif Bulan Berjalan, Pagu Realisasi Anggaran Unit Cost Center (%), Antrean Settlement Menunggu Penyerahan (< 7 Hari), dan Klaim Biaya Non-Trip Pending Kasir.
* **Dependencies**: `FE-HRD-291`, `BE-HRD-319` [BE]
* **Acceptance Criteria**: Dasbor menyajikan metrik agregat anggaran dan aktivitas kedinasan secara responsif dan akurat.
* **Verifikasi**: Uji tampilan dasbor perjalanan dinas `AT-HRD-TRV-01`.

#### 🟡 `FE-HRD-293` — Kartu Analitik Realisasi Anggaran Unit Cost Center & Radar Settlement Aktif
* **Slice / MVP**: `S-D5` / `MVP-63`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R22`. Laporan lama: [FE-HRD-293.md](../task/report/frontend/FE-HRD-293.md).
* **Outcome**: Pembuatan komponen visual grafis tren serapan pagu anggaran dinas per unit kerja (Departemen Bedah, Rawat Inap, IGD, Farmasi) dan radar kartu penanda batas waktu settlement pegawai dengan status warna hijau (hari 1-3), kuning (hari 4-7), dan merah berkedip (> 7 hari / overdue).
* **Dependencies**: `FE-HRD-292`
* **Acceptance Criteria**: Kartu analitik merefleksikan sisa pagu anggaran unit cost center; indikator warna radar berjalan presisi.
* **Verifikasi**: Uji visualisasi analitik anggaran cost center `AT-HRD-TRV-21`.

#### 🟡 `FE-HRD-294` — Layar Matriks Tarif Lumpsum Uang Harian Per Diem & Zonasi Wilayah (`/hrd/travel/rates-and-policies` - `FE-HRD-297`)
* **Slice / MVP**: `S-D5` / `MVP-63`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R22`. Laporan lama: [FE-HRD-294.md](../task/report/frontend/FE-HRD-294.md).
* **Outcome**: Pembangunan layar pengaturan kebijakan dan matriks tarif uang harian dinas (`/hrd/travel/rates-and-policies`). Menampilkan tabel tarif baku lumpsum per diem per kombinasi zonasi wilayah (Zona 1-3) dan golongan grade nakes (Gol I-V), batas plafon kelas hotel berbintang, dan form editor update tarif bagi HR Administrator.
* **Dependencies**: `FE-HRD-292`
* **Acceptance Criteria**: Tabel tarif per diem tampil transparan; pengubahan tarif hanya dapat dilakukan oleh role HR Admin.
* **Verifikasi**: Uji layar matriks tarif per diem `AT-HRD-TRV-02`.

#### 🟡 `FE-HRD-295` — Layar Pusat Penugasan & Riwayat SPPD Pegawai (`/hrd/travel/requests` - `FE-HRD-292`)
* **Slice / MVP**: `S-D5` / `MVP-64`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R22`. Laporan lama: [FE-HRD-295.md](../task/report/frontend/FE-HRD-295.md).
* **Outcome**: Pembangunan layar pusat pengelolaan seluruh perjalanan dinas nakes (`/hrd/travel/requests`). Menampilkan tabel riwayat SPPD dengan kolom nomor tiket, nama pemohon, jabatan, kota tujuan, tanggal keberangkatan/kepulangan, badge status alur kerja, dan tombol aksi cetak berkas PDF Surat Tugas resmi ber-barcode.
* **Dependencies**: `FE-HRD-292`, `BE-HRD-320` [BE]
* **Acceptance Criteria**: Tabel riwayat SPPD mendukung sorting dan filtering status; tombol cetak surat tugas menghasilkan dokumen sah.
* **Verifikasi**: Uji layar pusat riwayat SPPD `AT-HRD-TRV-01`.

#### 🟡 `FE-HRD-296` — Layar Formulir Pengajuan SPPD Mandiri Multi-Step (`/hrd/travel/requests/create` - `FE-HRD-293`)
* **Slice / MVP**: `S-D5` / `MVP-64`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R22`. Laporan lama: [FE-HRD-296.md](../task/report/frontend/FE-HRD-296.md).
* **Outcome**: Pembangunan wizard formulir multi-step pengajuan SPPD mandiri di ESS: (1) Langkah 1: Input kota rute, tanggal dinas, agenda penugasan, dan upload surat undangan simposium/CME; (2) Langkah 2: Pilihan moda transportasi dan hotel sesuai batas plafon kelas; (3) Langkah 3: Kalkulasi otomatis uang harian lumpsum dan pengajuan uang muka kerja (maks 80%); (4) Langkah 4: Ringkasan estimasi total biaya dan tombol *Kirim SPPD*.
* **Dependencies**: `FE-HRD-295`, `BE-HRD-320` [BE]
* **Acceptance Criteria**: Wizard memvalidasi kelengkapan berkas di tiap tahapan; input tanggal tervalidasi bebas bentrok cuti.
* **Verifikasi**: Uji formulir multi-step pengajuan SPPD mandiri `AT-HRD-TRV-01` dan pencegahan bentrok cuti `AT-HRD-TRV-20`.

#### 🟡 `FE-HRD-297` — Komponen Kalkulator Otomatis Lumpsum Uang Harian & Pilihan Hotel Sesuai Plafon Grade
* **Slice / MVP**: `S-D5` / `MVP-64`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R22`. Laporan lama: [FE-HRD-297.md](../task/report/frontend/FE-HRD-297.md).
* **Outcome**: Pembuatan widget kalkulator cerdas yang secara instan menghitung jumlah hari dinas inklusif (`TravelDayCount`) dan mengalikan tarif per diem harian sesuai zonasi kota tujuan dan grade nakes pemohon. Menampilkan batas plafon hotel (misal: Bintang 3 untuk Staf, Bintang 4 untuk Dokter Spesialis/Manajer) dan memberikan peringatan jika estimasi hotel melampaui plafon.
* **Dependencies**: `FE-HRD-296`
* **Acceptance Criteria**: Kalkulator menghitung estimasi secara akurat tanpa input manual tarif; peringatan plafon hotel muncul saat over-budget.
* **Verifikasi**: Uji kalkulator otomatis per diem dan plafon hotel `AT-HRD-TRV-02`.

#### 🟡 `FE-HRD-298` — Layar Konsol Manajemen Uang Muka Kerja (Advance) (`/hrd/travel/advances` - `FE-HRD-294`) & Modal Pencairan Kasir
* **Slice / MVP**: `S-D5` / `MVP-64`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R34`. Laporan lama: [FE-HRD-298.md](../task/report/frontend/FE-HRD-298.md).
* **Outcome**: Pembangunan layar kendali uang muka kerja dinas (`/hrd/travel/advances`). Menampilkan tabel pengajuan uang muka yang disetujui, tombol verifikasi 2-pintu Finance, generator voucher kasir cito, dan modal konfirmasi pencairan dana kasir (*AdvanceDisbursementModal.tsx*) lengkap dengan input nomor referensi transfer bank / nomor voucher kasir.
* **Dependencies**: `FE-HRD-295`, `BE-HRD-324` [BE]
* **Acceptance Criteria**: Tabel uang muka menyajikan status pencairan real-time; modal pencairan mencatat nomor referensi kasir sah.
* **Verifikasi**: Uji konsol manajemen uang muka dan modal pencairan kasir `AT-HRD-TRV-05`.

#### 🟡 `FE-HRD-299` — Layar Lembar Rekonsiliasi & Settlement Pengeluaran Riil (`/hrd/travel/settlements` - `FE-HRD-295`)
* **Slice / MVP**: `S-D5` / `MVP-65`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R34`. Laporan lama: [FE-HRD-299.md](../task/report/frontend/FE-HRD-299.md).
* **Outcome**: Pembangunan layar lembar kerja rekonsiliasi pertanggungjawaban dinas (`/hrd/travel/settlements`). Staf mengisi rincian pengeluaran riil, mengunggah foto invoice hotel dan tiket pesawat boarding pass, dan sistem secara otomatis menghitung selisih kas: Kurang Bayar (*Underpaid* - RS bayar staf) atau Lebih Bayar (*Overpaid* - Staf setor balik kasir).
* **Dependencies**: `FE-HRD-295`, `BE-HRD-326` [BE]
* **Acceptance Criteria**: Lembar komparasi menghitung selisih uang muka vs aktual dengan presisi; dokumen lampiran dapat dipratinjau jelas.
* **Verifikasi**: Uji lembar kerja rekonsiliasi settlement `AT-HRD-TRV-15` dan `AT-HRD-TRV-16`.

#### 🟡 `FE-HRD-300` — Modal Formulir Deklarasi Biaya Darurat Tanpa Kuitansi (*MissingReceiptDeclarationModal.tsx*)
* **Slice / MVP**: `S-D5` / `MVP-65`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R34`. Laporan lama: [FE-HRD-300.md](../task/report/frontend/FE-HRD-300.md).
* **Outcome**: Pembuatan modal dialog deklarasi biaya darurat tanpa kuitansi resmi (*MissingReceiptDeclarationModal.tsx*). Staf memasukkan jenis pengeluaran darurat (ojek pelosok, sewa perahu evakuasi pasien), nominal biaya (maksimal Rp 150.000), penjelasan kronologi darurat tertulis (minimal 15 karakter), dan konfirmasi persetujuan atasan langsung (`VR-TRV-08`, `VR-TRV-09`).
* **Dependencies**: `FE-HRD-299`, `BE-HRD-327` [BE]
* **Acceptance Criteria**: Modal memblokir pengajuan di atas Rp 150.000 atau jika akumulasi melebihi 10% total klaim; alasan darurat terekam jelas.
* **Verifikasi**: Uji modal deklarasi biaya darurat tanpa nota `AT-HRD-TRV-12` dan `AT-HRD-TRV-13`.

#### 🟡 `FE-HRD-301` — Modal Tanda Terima Setor Balik Kasir Kelebihan Uang Muka (*RefundSettlementModal.tsx*) & Bukti Kurang Bayar
* **Slice / MVP**: `S-D5` / `MVP-65`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R34`. Laporan lama: [FE-HRD-301.md](../task/report/frontend/FE-HRD-301.md).
* **Outcome**: Pembuatan dua modal pendukung transaksi kas akhir: (1) *RefundSettlementModal.tsx* untuk pencatatan tanda terima pengembalian sisa uang muka lebih bayar ke Kasir RS dengan nomor bukti setor; dan (2) *UnderpaidPaymentModal.tsx* untuk konfirmasi pencairan transfer selisih kurang bayar dari Kasir RS ke staf.
* **Dependencies**: `FE-HRD-300`
* **Acceptance Criteria**: Transaksi setor balik tercatat rapi dengan nomor tanda terima kasir; status settlement berubah `Settled`.
* **Verifikasi**: Uji modal penyelesaian transaksi kas settlement `AT-HRD-TRV-16`.

#### 🟡 `FE-HRD-302` — Layar Radar Pemantau Keterlambatan Settlement Overdue (`/hrd/travel/overdue-settlements` - `FE-HRD-298`)
* **Slice / MVP**: `S-D5` / `MVP-65`
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 2 dari 2 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R34`. Laporan lama: [FE-HRD-302.md](../task/report/frontend/FE-HRD-302.md).
* **Outcome**: Pembangunan konsol radar pengawasan keterlambatan pertanggungjawaban dinas (`/hrd/travel/overdue-settlements`). Menampilkan daftar staf yang menunggak settlement (> 7 hari), penanda status pembekuan SPPD (> 14 hari), dan panel simulasi pemotongan otomatis pada slip gaji bulan berjalan bagi tunggakan > 30 hari (`INT-HR-99`).
* **Dependencies**: `FE-HRD-299`
* **Acceptance Criteria**: Radar menampilkan daftar tunggakan secara transparan; tombol simulasi potong payroll menampilkan nominal akurat.
* **Verifikasi**: Uji konsol monitoring settlement overdue `AT-HRD-TRV-10` dan `AT-HRD-TRV-11`.

#### 🟡 `FE-HRD-303` — Layar Portal Klaim Pengeluaran Operasional Non-Trip (`/hrd/travel/expenses` - `FE-HRD-296`)
* **Slice / MVP**: `S-D5` / Verifikasi
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 1 dari 1 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R34`. Laporan lama: [FE-HRD-303.md](../task/report/frontend/FE-HRD-303.md).
* **Outcome**: Pembangunan layar pengelolaan klaim penggantian biaya operasional langsung non-trip staf (`/hrd/travel/expenses`). Menampilkan tabel riwayat klaim non-trip, status verifikasi kuitansi oleh Finance, status pencairan kasir, dan tombol cepat formulir klaim pembelian darurat (logistik cito, konsumsi rapat, kurir lab).
* **Dependencies**: `FE-HRD-291`, `BE-HRD-330` [BE]
* **Acceptance Criteria**: Klaim operasional non-trip terkelola mandiri tanpa memerlukan surat tugas perjalanan dinas luar kota.
* **Verifikasi**: Uji portal klaim pengeluaran non-trip `AT-HRD-TRV-17`.

#### 🟡 `FE-HRD-304` — Modal Upload Struk Bukti Kasir & Drawer Pratinjau Faktur Pajak / Boarding Pass
* **Slice / MVP**: `S-D5` / Verifikasi
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 11 November 2026 lewat audit [frontend-unfinished-work.md](frontend-unfinished-work.md) (`FE-HRD-R01`); semula ✅. Layar memakai data contoh di kode dan tidak memanggil API. 1 dari 1 acceptance criteria belum terbukti berjalan. Dikerjakan ulang lewat `FE-HRD-R34`. Laporan lama: [FE-HRD-304.md](../task/report/frontend/FE-HRD-304.md).
* **Outcome**: Pembuatan modal dialog pengunggahan struk nota kuitansi (*ExpenseReceiptUploadModal.tsx*) dengan fitur kompresi gambar otomatis, pemindaian tanggal struk dalam bulan berjalan, dan drawer penampil dokumen pendukung (pratinjau faktur pajak hotel, tiket boarding pass penerbangan, invoice resmi toko rekanan).
* **Dependencies**: `FE-HRD-303`
* **Acceptance Criteria**: Berkas gambar/PDF terunggah cepat (< 5 detik) dan dapat dipratinjau utuh oleh Verifikator Keuangan RS.
* **Verifikasi**: Uji modal upload struk dan pratinjau dokumen kasir `AT-HRD-TRV-18`.

#### ✅ `FE-HRD-305` — Integrasi Navigasi Sidebar, Penanda Indikator Hard-Stop Roster Sub-06 & Uji Linter Build Standalone
* **Slice / MVP**: `S-D5` / Verifikasi
* **Status**: ✅ PASS
* **Outcome**: Pendaftaran seluruh rute menu navigasi Perjalanan Dinas & Biaya pada sidebar utama modul HRD Next.js App Router, integrasi badge peringatan penugasan dinas luar kota pada modul penyusunan roster dinas Sub-06 (memicu tooltip hard-stop jadwal bangsal), serta eksekusi audit kualitas frontend menyeluruh: validasi linter `npm run lint:errors` lulus 0 error dan kompilasi build produksi `npm run build` sukses 100%.
* **Dependencies**: Seluruh task frontend `FE-HRD-291` s.d. `FE-HRD-304` serta task backend `BE-HRD-333` [BE]
* **Acceptance Criteria**: Menu travel & expenses tampil rapi sesuai hak akses; indikator hard-stop aktif di roster Sub-06; `npm run lint:errors` dan `npm run build` lulus tanpa error.
* **Verifikasi**: Uji integrasi navigasi, indikator gembok roster `AT-HRD-TRV-06`, dan laporan task tracked `FE-HRD-305.md`.
